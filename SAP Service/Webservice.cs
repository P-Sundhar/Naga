using ModernHttpClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.PeerToPeer;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NagaSapService
{
    public class Webservice
    {
        string RspData = "";
        IEnumerable<string> cookies = new List<string>();
        CookieContainer cookieJar = new CookieContainer();
        string token = "";
        public async Task<string> WebApiPostRequest(string Mthd, string ApiInput)
        {
            RspData = "";
            try
            {
                var url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + Mthd;
                var ipData = new StringContent(ApiInput, Encoding.UTF8, "application/json");
                
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse resp;
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SAPUserName"].ToString(),
                    ConfigurationManager.AppSettings["SAPPassword"].ToString());
                req.Credentials = credentials;
                req.Method = "GET";
                req.Headers.Add("X-CSRF-Token", "Fetch");
                this.cookieJar = new CookieContainer();
                req.CookieContainer = this.cookieJar;
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (System.Net.WebException ex)
                {
                    return ex.Message.ToString();
                }
                catch (Exception ex)
                {
                    return ex.Message.ToString();
                }
                this.token = resp.Headers.Get("X-CSRF-Token");
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", ConfigurationManager.AppSettings["SAPUserName"].ToString(), ConfigurationManager.AppSettings["SAPPassword"].ToString()))));
                    client.DefaultRequestHeaders.Add("X-CSRF-Token", this.token);
                    client.DefaultRequestHeaders.Add("ContentType", "application/json");
                    cookieContainer.Add(client.BaseAddress, resp.Cookies);

                    HttpResponseMessage response = await client.PostAsync(client.BaseAddress, ipData).ConfigureAwait(continueOnCapturedContext: false);
                    if (response.IsSuccessStatusCode)
                    {
                        var receivedData = await response.Content.ReadAsStringAsync();
                        return receivedData;
                    }
                    else
                        return response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        public async Task<string> WebApiGetRequest(string Mthd)
        {
            RspData = "";
            try
            {
                var url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + Mthd;
                using (HttpClient httpClient = new HttpClient(new NativeMessageHandler()))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                               .GetBytes(ConfigurationManager.AppSettings["SAPUserName"].ToString() + ":" + ConfigurationManager.AppSettings["SAPPassword"].ToString())));
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        RspData = "Server Connect Failed ..";
                        return RspData;
                    }
                    var json = await response.Content.ReadAsStringAsync();
                    RspData = json;
                    return RspData;
                }
            }
            catch (Exception ex)
            {
                RspData = "Exception in ApiRequest";
                return RspData;
            }
        }
        public async Task<string> WebApiGetCsrfToken(string Mthd)
        {
            RspData = "";
            try
            {
                

                var url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + Mthd;
                using (HttpClient httpClient = new HttpClient(new NativeMessageHandler()))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                               .GetBytes(ConfigurationManager.AppSettings["SAPUserName"].ToString() + ":" + ConfigurationManager.AppSettings["SAPPassword"].ToString())));
                    httpClient.DefaultRequestHeaders.Add("x-csrf-token", "fetch");
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        RspData = "Server Connect Failed ..";
                        return RspData;
                    }
                    var json = response.Headers.GetValues("X-CSRF-TOKEN").FirstOrDefault();
                    cookies = response.Headers.GetValues("Set-Cookie");
                    RspData = json;
                    return RspData;
                }
            }
            catch (Exception ex)
            {
                RspData = "Exception in ApiRequest";
                return RspData;
            }
        }
    }
}
