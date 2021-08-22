namespace l99.driver.factoryio
{
    public class FactoryioRemoteEndpoint
    {
        public string URI => _uri;

        private string _uri = "http://localhost:7410";
        
        public short ConnectionTimeout => _connectionTimeout;

        private short _connectionTimeout = 3;

        public FactoryioRemoteEndpoint(string uri, short connectionTimeout)
        {
            _uri = uri;
            _connectionTimeout = connectionTimeout;
        }
    }
}