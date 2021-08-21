using l99.driver.@base;

namespace factoryio
{
    public class FactoryioMachine : Machine
    {
        public override string ToString()
        {
            return new
            {
                Id
            }.ToString();
        }

        public override dynamic Info
        {
            get
            {
                return new
                {
                    _id = id
                };
            }
        }
        
        public FactoryioMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            
            this["cfg"] = cfg;
            this["data"] = cfg.type["data"];
            //this["platform"] = new Platform(this);
        }
    }
}