using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    class RosHandler
    {
        private readonly HttpClient s_client = new();
        string TabitAccountClientId;
        string RosUrl;

        public RosHandler()
        {
            this.TabitAccountClientId = Utils.GetConfigKey("TabitAccountClientId");
            this.RosUrl = Utils.GetConfigKey("RosUrl");
        }

        public string GetToken()
        {
            var requestObject = new
            {
                grant_type = "client_credentials",
                client_id = TabitAccountClientId
            };
            var requestBody = JsonSerializer.Serialize(requestObject);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{RosUrl}/oauth2/token"), 
                Content = content
            };

            var response = s_client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
           
            var jResult = JObject.Parse(response.Content.ReadAsStringAsync().Result); // need to replace with system.text - not urgent you can still use this one
            Console.WriteLine("**** ROS TOKEN INIT SUCCESS *** ");
            return jResult["access_token"].ToString(); 
        }
    }
}

