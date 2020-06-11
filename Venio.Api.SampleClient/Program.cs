using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Venio.Api.SampleClient {
    static class Program {
#if DEBUG
        const string ACCESS_TOKEN_URI = "https://dev.tks.co.th/identityserver2";
        const string BASE_URI = "https://veniocrm.azure-api.net/test";
#else
        const string ACCESS_TOKEN_URI = "https://login.gofive.co.th";
        const string BASE_URI = "https://veniocrm.azure-api.net";
#endif
        static HttpClient client = new HttpClient();
        static void Main() {
#if DEBUG
            string clientId = "7CD36778-713C-4BA1-AC3A-C50A43009D6E";
            string clientSecret = "AtopWREgYmiNQUEneNervIKE";
#else
            string clientId = "";
            string clientSecret = "";
#endif
            string subscriptionKey = "d976fbc3ddae47eabfb9f3d0b98b4831"; //Visit the developer profile area to manage your subscription and subscription keys https://veniocrm.developer.azure-api.net
            string uri = $"{BASE_URI}/v1/Customers/filter?skip=0&pageLength=10";
            if (string.IsNullOrEmpty(subscriptionKey)) throw new ArgumentNullException("Subscription key is required.");
            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{subscriptionKey}");
            string accessToken = GetAccessToken(clientId, clientSecret).Result;
            client.DefaultRequestHeaders.Add($"Authorization", $"bearer {accessToken}");
            // Customer list
            string customers = MakeRequest(HttpMethod.Post, uri, accessToken, subscriptionKey, new {
                type = 11300
            }).Result;
            Console.WriteLine(JObject.Parse(customers).ToString(Newtonsoft.Json.Formatting.Indented));
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadLine();
        }

        static async Task<string> GetAccessToken(string clientId, string clientSecret) {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException("Client id and secret are required.");

            string scope = "Venio2.API";
            HttpResponseMessage response;
            string accessToken = null;
            using (var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId),
                new KeyValuePair<string,string>("client_secret", clientSecret),
                new KeyValuePair<string,string>("scope", scope)
            })) {
                response = await client.PostAsync($"{ACCESS_TOKEN_URI}/connect/token", content);
                string jsonString = await response.Content.ReadAsStringAsync();
                accessToken = JObject.Parse(jsonString)["access_token"].ToString();
            }
            return accessToken;
        }

        static async Task<string> MakeRequest(HttpMethod method, string uri, string accessToken, string subscriptionKey, object body) {
            string jsonResult = string.Empty;
            HttpResponseMessage response;

            // Request body
            string jsonContent = JObject.FromObject(body).ToString(Newtonsoft.Json.Formatting.None);
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json")) {
                HttpRequestMessage request = new HttpRequestMessage(method, uri) {
                    Content = content
                };
                response = client.SendAsync(request).Result;
                jsonResult = await response.Content.ReadAsStringAsync();
            }
            return jsonResult;
        }
    }
}