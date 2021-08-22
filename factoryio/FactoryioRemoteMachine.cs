using l99.driver.@base;

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

        public FactoryioRemoteMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            
            this["cfg"] = cfg;
            
            _factoryioRemoteEndpoint = new FactoryioRemoteEndpoint(cfg.type["net_uri"], (short)cfg.type["net_timeout_s"]);
        }
    }
}