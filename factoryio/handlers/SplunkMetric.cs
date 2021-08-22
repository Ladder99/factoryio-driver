﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.factoryio.handlers
{
    public class SplunkMetric: Handler
    {
        private int _counter = 0;
        
        public SplunkMetric(Machine machine) : base(machine)
        {
            
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            var payload = new
            {
                time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                @event = "metric",
                host = veneers.Machine.Id,
                fields = new
                {
                    metric_name = veneer.LastArrivedValue.name,
                    _value = veneer.LastArrivedValue.value,
                    veneer.LastArrivedValue.direction,
                    veneer.LastArrivedValue.address
                }
            };
                
            Console.WriteLine(
                _counter++ + " > " +
                JObject.FromObject(payload).ToString()
            );

            return payload;
        }
        
        protected override async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            if (onChange == null)
                return;
            
            Regex regex = new Regex("[^a-zA-Z0-9]");
            var sanitizedName = regex.Replace(veneer.Name, "_");
            
            var topic = $"factoryio/{veneers.Machine.Id}/splunk/{sanitizedName}";
            string payload = JObject.FromObject(onChange).ToString();
            await veneers.Machine.Broker.PublishChangeAsync(topic, payload);
        }
        
        protected override async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            
        }
    }
}