using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Consumption;
using Microsoft.Azure.Management.Consumption.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace consumption_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureConsumptionApiController : ControllerBase
    {
        private static async Task<string> GetAccessTokenAsync(string TenantId, string ClientId, string ClientSecret)
        {
            var tenantId = TenantId;
            var clientId = ClientId;
            var clientSecret = ClientSecret;
            var resource = "https://management.azure.com/";

            var context = new AuthenticationContext("https://login.microsoftonline.com/" + tenantId);
            var credential = new ClientCredential(clientId, clientSecret);
            var result = await context.AcquireTokenAsync(resource, credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }

        [HttpGet]
        public async Task<ActionResult<UsageDetail>> GetUsage( string subscriptionId,string TenantId,string ClientId,string ClientSecret)
        {
            var accessToken = await GetAccessTokenAsync(TenantId,ClientId,ClientSecret);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            
            string apiVersion = "2019-10-01";
           // string resourceGroup = "your-resource-group";

            string url = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.Consumption/usageDetails?api-version={apiVersion}";
           // string url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Consumption/usageDetails?api-version={apiVersion}";
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
               
                var json = await response.Content.ReadAsStringAsync();
                var usageDetails = JsonConvert.DeserializeObject<UsageDetail>(json);

                return Ok(usageDetails);
            }
            else
            {
                return Ok($"Failed to retrieve usage details: {response.StatusCode} {response.ReasonPhrase}");
            }
            //var credentials = new TokenCredentials(accessToken);

            //var consumptionManagementClient = new ConsumptionManagementClient(credentials)
            //{
            //    SubscriptionId = subscriptionId

            //};

            //var usage = await consumptionManagementClient.UsageDetails.ListAsync();
            //return Ok(usage);
        }





    }
}
