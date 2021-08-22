using l99.driver.@base;

namespace l99.driver.factoryio
{
    public class FactoryioLocalMachine : Machine
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
        
        public FactoryioLocalMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            
            this["cfg"] = cfg;
        }
    }
}