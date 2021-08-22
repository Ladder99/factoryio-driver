using l99.driver.@base;
using RestSharp;

namespace l99.driver.factoryio
{
    public class FactoryioRemoteMachine : Machine
    {
        public override string ToString()
        {
            return new
            {
                Id,
                _factoryioRemoteEndpoint.URI,
                _factoryioRemoteEndpoint.ConnectionTimeout
            }.ToString();
        }

        public override dynamic Info
        {
            get
            {
                return new
                {
                    _id = id,
                    _factoryioRemoteEndpoint.URI,
                    _factoryioRemoteEndpoint.ConnectionTimeout
                };
            }
        }
        
        public FactoryioRemoteEndpoint FactoryioRemoteEndpoint
        {
            get => _factoryioRemoteEndpoint;
        }
        
        private FactoryioRemoteEndpoint _factoryioRemoteEndpoint;

        public RestClient Client
        {
            get => _client;
        }

        private RestClient _client;
        
        public FactoryioRemoteMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            
            this["cfg"] = cfg;
            this["platform"] = new Platform(this);
            
            _factoryioRemoteEndpoint = new FactoryioRemoteEndpoint(cfg.type["net_uri"], (short)cfg.type["net_timeout_s"]);

            _client = new RestClient($"{cfg.type["net_uri"]}/api");
            _client.Timeout = cfg.type["net_timeout_s"] * 1000;
        }
    }
}