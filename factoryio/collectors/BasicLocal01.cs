using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using EngineIO;
using l99.driver.@base;
using MQTTnet;
using Newtonsoft.Json.Linq;

namespace l99.driver.factoryio.collectors
{
    public class BasicLocal01 : Collector
    {
        private string _subTopic = "factoryio/fio/io";
        private Dictionary<(string, string), dynamic> _mapNames = new Dictionary<(string, string), dynamic>();
        private Dictionary<(string, string), dynamic> _mapValues = new Dictionary<(string, string), dynamic>();
        
        public BasicLocal01(Machine machine, object cfg) : base(machine, cfg)
        {
            _subTopic = ((dynamic)cfg).strategy["sub_topic"];
        }

        ~BasicLocal01()
        {
            MemoryMap.Instance.Dispose();
        }
        
        private void trackMemory(dynamic memory)
        {
            if (_mapValues.ContainsKey((memory.name, memory.kind)))
                _mapValues[(memory.name, memory.kind)] = memory;
            else
                _mapValues.Add((memory.name, memory.kind), (object) memory);   //TODO: why is boxing required
        }
        
        private void processMemories(MemoriesChangedEventArgs args, Action<dynamic> memoryAction)
        {
            var pis = args.GetType().GetProperties();

            foreach (var pi in pis)
            {
                var pivs = (dynamic)pi.GetValue(args);
                
                if (pivs.Length == 0)
                    continue;

                foreach (var piv in pivs)
                {
                    var memory = new
                    {
                        type = piv.GetType().Name.Replace("Memory",""),
                        name = piv.Name,
                        address = piv.Address,
                        kind = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(piv.MemoryType.ToString().ToLower()),
                        value = piv.Value
                    };

                    memoryAction?.Invoke(memory);

                    trackMemory(memory);
                }
            }
        }
        
        private void fioNameChange(MemoryMap sender, MemoriesChangedEventArgs args)
        {
            processMemories(args, (memory) =>
            {
                if(!_mapNames.ContainsKey((memory.name,memory.kind)))
                    _mapNames.Add((memory.name, memory.kind), new { });
            });
        }
        
        private void fioValueChange(MemoryMap sender, MemoriesChangedEventArgs args)
        {
            processMemories(args, null);
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            //TODO: scene changes/updates
            
            try
            {
                MemoryMap.Instance.InputsNameChanged += fioNameChange;
                MemoryMap.Instance.OutputsNameChanged += fioNameChange;
                MemoryMap.Instance.MemoriesNameChanged += fioNameChange;
                
                MemoryMap.Instance.InputsValueChanged += fioValueChange;
                MemoryMap.Instance.OutputsValueChanged += fioValueChange;
                MemoryMap.Instance.MemoriesValueChanged += fioValueChange;
                
                MemoryMap.Instance.Update();

                MemoryMap.Instance.InputsNameChanged -= fioNameChange;
                MemoryMap.Instance.OutputsNameChanged -= fioNameChange;
                MemoryMap.Instance.MemoriesNameChanged -= fioNameChange;

                foreach (var kv in _mapNames)
                {
                    machine.ApplyVeneer(typeof(factoryio.veneers.Memory), $"{kv.Key.Item2}/{kv.Key.Item1}");
                }
                
                await machine.Broker.SubscribeAsync(_subTopic, incomingMessage);
                
                machine.VeneersApplied = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                MemoryMap.Instance.Update();

                foreach (var kv in _mapValues)
                {
                    //TODO: handle items previously not veneered
                    if (!_mapNames.ContainsKey(kv.Key))
                        continue;
                    
                    await machine.PeelVeneerAsync($"{kv.Key.Item2}/{kv.Key.Item1}", kv.Value);

                    _mapNames[kv.Key] = kv.Value;
                }

                _mapValues.Clear();

                LastSuccess = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }

        private async Task incomingMessage(string topic, string payload, ushort qos, bool retain)
        {
            /*
                [
                    { "name": "Conveyor", "value": true, "kind": "output" }
                ]
             */
            
            try
            {
                var tags = JArray.Parse(payload);

                foreach (JObject tag in tags)
                {
                    updateTag(tag);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"[{machine.Id}] Incoming MQTT payload processing failed.");
            }

            void updateTag(JObject tag)
            {
                try
                {
                    MemoryType kind = MemoryType.Output;

                    try { kind = Enum.Parse<MemoryType>(tag.GetValue("kind").ToString(), true); }
                    catch { }

                    var key = (tag.GetValue("name").ToString(),
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kind.ToString().ToLower()));
                    
                    if (!_mapNames.ContainsKey(key))
                        return;

                    string get_method = $"Get{_mapNames[key].type}";
                    
                    // get method
                        //MemoryMap.Instance.Get
                    // invoke
                    // update value
                    // store Memory<>  until next update ?
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"[{machine.Id}] Incoming MQTT tag processing failed.");
                }
            }
        }
    }
}