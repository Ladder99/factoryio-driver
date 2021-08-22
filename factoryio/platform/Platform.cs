using NLog;

namespace l99.driver.factoryio
{
    public partial class Platform
    {
        private ILogger _logger;
        
        private FactoryioRemoteMachine _machine;
        
        public Platform(FactoryioRemoteMachine machine)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _machine = machine;
        }
    }
}