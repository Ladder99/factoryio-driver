using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.factoryio.collectors
{
    public class BasicRemote01: Collector
    {
        private Dictionary<(string, string), dynamic> _mapNames = new Dictionary<(string, string), dynamic>();
        
        public BasicRemote01(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            //TODO: scene changes/updates
            
            try
            {
                dynamic tags = await machine["platform"].GetTagsAsync();
                
                foreach (JObject o in tags)
                {
                    var key = (o.GetValue("name").ToString(), o.GetValue("kind").ToString());
                    machine.ApplyVeneer(typeof(factoryio.veneers.Memory), $"{o.GetValue("kind").ToString()}/{o.GetValue("name").ToString()}");
                    _mapNames.Add(key, new { });
                }
                
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
                dynamic tags = await machine["platform"].GetTagsAsync();
                
                foreach (JObject o in tags)
                {
                    var key = (o.GetValue("name").ToString(), o.GetValue("kind").ToString());
                    
                    if (!_mapNames.ContainsKey(key))
                        continue;
                    
                    var value = new
                    {
                        type = o.GetValue("type").ToString(),
                        name = o.GetValue("name").ToString(),
                        address = (int)o.GetValue("address"),
                        kind = o.GetValue("kind").ToString(),
                        value = o.GetValue("value")
                    };
                    
                    await machine.PeelVeneerAsync($"{o.GetValue("kind").ToString()}/{o.GetValue("name").ToString()}", value);
                    
                    _mapNames[key] = value;
                }

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