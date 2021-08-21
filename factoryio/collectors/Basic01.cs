using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngineIO;
using l99.driver.@base;

namespace l99.driver.factoryio.collectors
{
    public class Basic01 : Collector
    {
        private List<string> _mapNames = new List<string>();
        private Dictionary<string,dynamic> _mapInputs = new Dictionary<string, dynamic>();
        private Dictionary<string,dynamic> _mapOutputs = new Dictionary<string, dynamic>();
        private Dictionary<string,dynamic> _mapMemories = new Dictionary<string, dynamic>();
        
        public Basic01(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }

        public override async Task<dynamic?> InitializeAsync()
        {
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

                foreach (var memory_name in _mapNames)
                {
                    machine.ApplyVeneer(typeof(factoryio.veneers.Memory), memory_name);
                }
                
                machine.VeneersApplied = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        private void setMemory(dynamic memory)
        {
            switch (memory.direction)
            {
                case ("INPUT"):
                    if (_mapInputs.ContainsKey(memory.name))
                        _mapInputs[memory.name] = memory;
                    else
                        _mapInputs.Add(memory.name, memory);
                    break;
                case ("OUTPUT"):
                    if (_mapOutputs.ContainsKey(memory.name))
                        _mapOutputs[memory.name] = memory;
                    else
                        _mapOutputs.Add(memory.name, memory);
                    break;
                case ("MEMORY"):
                    if (_mapMemories.ContainsKey(memory.name))
                        _mapMemories[memory.name] = memory;
                    else
                        _mapMemories.Add(memory.name, memory);
                    break;
            }
        }
        
        private void processMemories(MemoriesChangedEventArgs args, Action<dynamic> memoryAction)
        {
            var prop_infos = args.GetType().GetProperties();

            foreach (var prop_info in prop_infos)
            {
                var prop_value = (dynamic)prop_info.GetValue(args);
                
                if (prop_value.Length == 0)
                    continue;

                foreach (var memory in prop_value)
                {
                    var memory_value = new
                    {
                        name = memory.Name,
                        address = memory.Address,
                        direction = memory.MemoryType.ToString().ToUpper(),
                        value = memory.Value
                    };
                    
                    if(memoryAction != null)
                        memoryAction(memory_value);

                    setMemory(memory_value);
                }
            }
        }
        
        private void fioNameChange(MemoryMap sender, MemoriesChangedEventArgs args)
        {
            processMemories(args, (memory) =>
            {
                if(!_mapNames.Contains(memory.name))
                    _mapNames.Add(memory.name);
            });
        }
        
        private void fioValueChange(MemoryMap sender, MemoriesChangedEventArgs args)
        {
            processMemories(args, null);
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                MemoryMap.Instance.Update();
                
                foreach(var kv in _mapInputs)
                {
                    await machine.PeelVeneerAsync(kv.Key, kv.Value);
                }
                
                foreach(var kv in _mapInputs)
                {
                    await machine.PeelVeneerAsync(kv.Key, kv.Value);
                }
                
                foreach(var kv in _mapInputs)
                {
                    await machine.PeelVeneerAsync(kv.Key, kv.Value);
                }
                
                _mapInputs.Clear();
                _mapOutputs.Clear();
                _mapMemories.Clear();
                
                LastSuccess = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }
        
    }
}