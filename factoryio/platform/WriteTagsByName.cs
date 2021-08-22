using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace l99.driver.factoryio
{
    public partial class Platform
    {
        public async Task<dynamic> WriteTagsByNameAsync(string tag_array)
        {
            var request = new RestRequest("tag/values/by-name", Method.PUT, DataFormat.Json);
            request.AddParameter("application/json", tag_array, ParameterType.RequestBody);
            var response = await _machine.Client.ExecuteAsync(request);

            return JArray.Parse(response.Content);
        }
    }
}