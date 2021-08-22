using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace l99.driver.factoryio
{
    public partial class Platform
    {
        public async Task<dynamic> GetTagsAsync()
        {
            var request = new RestRequest("tags", DataFormat.Json);
            var response = await _machine.Client.ExecuteGetAsync(request);

            return JArray.Parse(response.Content);
        }
    }
}