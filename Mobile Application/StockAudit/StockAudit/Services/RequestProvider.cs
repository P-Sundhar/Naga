using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StockAudit.Services
{
    public class RequestProvider
    {
        private readonly JsonSerializerSettings _serializerSettings;
        private string _baseUrl;

        public RequestProvider()
        {
            this._serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore
            };
            this._serializerSettings.Converters.Add((JsonConverter)new StringEnumConverter());
        }

        public void SetBaseUrl(string url)
        {
            this._baseUrl = url;
        }

        public async Task<string> GetAsync(string uri, string token = "")
        {
            var response = string.Empty;
            try
            {

                var httpClient = this.CreateHttpClient(token);
                HttpResponseMessage awaiter = await httpClient.GetAsync(uri);
                await HandleResponse(awaiter, uri, null, "", "");
                response = await awaiter.Content.ReadAsStringAsync();
                return response;
            }
            catch (Exception Ex)
            {
                return response;
            }
        }

        public async Task<TResult> GetAsync<TResult>(string uri, string token = "")
        {
            string async = await this.GetAsync(uri, token);
            if (!string.IsNullOrEmpty(async))
                return JsonConvert.DeserializeObject<TResult>(async, this._serializerSettings);
            if (!(typeof(TResult) == typeof(string)))
                return (TResult)Activator.CreateInstance(typeof(TResult));
            return (TResult)Activator.CreateInstance(typeof(string), new object[1]
            {
        (object) "".ToCharArray()
            });
        }

        public async Task<string> PostAsync(string uri, object data, string token = "", string header = "")
        {
            var response = string.Empty;
            var httpClient = this.CreateHttpClient(token);

            if (!string.IsNullOrEmpty(header))
                this.AddHeaderParameter(httpClient, header);

            string output = JsonConvert.SerializeObject(data);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(data));
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage awaiter = await httpClient.PostAsync(uri, (HttpContent)stringContent);
            await HandleResponse(awaiter, uri, data, token, header);
            response = await awaiter.Content.ReadAsStringAsync();
            return response;
        }

        public async Task<TResult> PostAsync<TResult>(string uri, object data, string token = "", string header = "")
        {
            string str = await this.PostAsync(uri, data, token, header);
            if (!string.IsNullOrEmpty(str))
                return JsonConvert.DeserializeObject<TResult>(str, this._serializerSettings);
            if (!(typeof(TResult) == typeof(string)))
                return (TResult)Activator.CreateInstance(typeof(TResult));
            return (TResult)Activator.CreateInstance(typeof(string), new object[1]
            {
        (object) "".ToCharArray()
            });
        }

        public async Task<TResult> PostAsync<TResult>(string uri, string data, string clientId, string clientSecret)
        {
            HttpClient httpClient = this.CreateHttpClient(string.Empty);
            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                this.AddBasicAuthenticationHeader(httpClient, clientId, clientSecret);
            StringContent stringContent = new StringContent(data);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await httpClient.PostAsync(uri, stringContent);

            await HandleResponse(response, uri, null, "", "");

            string responseData = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));
        }

        public async Task<TResult> PutAsync<TResult>(string uri, TResult data, string token = "", string header = "")
        {
            HttpClient httpClient = this.CreateHttpClient(token);
            if (!string.IsNullOrEmpty(header))
                this.AddHeaderParameter(httpClient, header);
            string serialized = await Task.Run(() => JsonConvert.SerializeObject(data, _serializerSettings));
            HttpResponseMessage response = await httpClient.PutAsync(uri, new StringContent(serialized, Encoding.UTF8, "application/json"));

            await HandleResponse(response, uri, null, "", "");

            string responseData = await response.Content.ReadAsStringAsync();

            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData, _serializerSettings));


        }

        public async Task DeleteAsync(string uri, string token = "")
        {
            HttpResponseMessage httpResponseMessage = await this.CreateHttpClient(token).DeleteAsync(uri);
        }

        private HttpClient CreateHttpClient(string token = "")
        {
            HttpClient httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(this._baseUrl))
                httpClient.BaseAddress = new Uri(this._baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        private void AddHeaderParameter(HttpClient httpClient, string parameter)
        {
            if (httpClient == null || string.IsNullOrEmpty(parameter))
                return;
            httpClient.DefaultRequestHeaders.Add(parameter, Guid.NewGuid().ToString());
        }

        private void AddBasicAuthenticationHeader(HttpClient httpClient, string clientId, string clientSecret)
        {
            if (httpClient == null || string.IsNullOrWhiteSpace(clientId))
                return;
            string.IsNullOrWhiteSpace(clientSecret);
        }

        private async Task HandleResponse(HttpResponseMessage response, string uri, object data, string token = "", string header = "")
        {
            if (!response.IsSuccessStatusCode)
            {
                //throw new HttpRequestExceptionEx(response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }

        private async Task HandleError()
        {
            
        }


    }
}
