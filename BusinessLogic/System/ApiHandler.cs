using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    public class ApiHandler
    {
        public string baseUrl = Utils.GetConfigKey("LoyaltyUrl");
        private string env { get; }
        public string JoinChannelGUID { get; }
        public string AccountGUID { get; }
        public string RosToken { get; }

        private TestThread CurrentThread;

        private readonly HttpClient s_client = new();

        #region Constructors

        /// <summary>
        /// use this constructor for any account related command
        /// </summary>
        /// <param name="env"></param>
        /// <param name="JoinChannelGUID"></param>
        /// <param name="CurrentThread"></param>
        public ApiHandler(string env, string JoinChannelGUID, TestThread CurrentThread)
        {
            this.env = env;
            this.JoinChannelGUID = JoinChannelGUID;
            if (CurrentThread.currentAccount != null)
            {
                this.AccountGUID = CurrentThread.currentAccount.AccountGUID;
            }
            this.RosToken = CurrentThread.ROSToken;
            this.CurrentThread = CurrentThread;

        }

        /// <summary>
        /// use this constructor for any higher level actions. above account 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="JoinChannelGUID"></param>
        public ApiHandler(string env, string JoinChannelGUID)
        {
            this.env = env;
            this.JoinChannelGUID = JoinChannelGUID;
            RosHandler ros = new RosHandler();
            this.RosToken = ros.GetToken();
        }

        #endregion

        internal JObject SendApiCommand(string MethodUrl, HttpMethod Method)
        {
            return SendApiCommand(MethodUrl, Method, new Dictionary<string, object>());
        }

        internal JObject SendApiCommand(string MethodUrl, HttpMethod Method, JObject body)
        {
            return SendApiCommand(MethodUrl, Method, new Dictionary<string, object>(), body);
        }

        internal JObject SendApiCommand(string MethodUrl, HttpMethod Method, JObject body, Dictionary<string, object> HeaderValues)
        {
            return SendApiCommand(MethodUrl, Method, HeaderValues, body);
        }


        private JObject SendApiCommand(string MethodUrl, HttpMethod httpMethod, Dictionary<string, object> HeaderValues, JObject body = null)
        {
            HttpRequestMessage request = BuildRequest(MethodUrl, httpMethod, body);

            HandleConstantHeaders(request);

            HandleCustomHeaders(HeaderValues, request);

            PrintRequestInformation(request);

            return Execute(request);

        }
        

        private JObject Execute(HttpRequestMessage request)
        {
            try
            {
                var response = s_client.SendAsync(request).GetAwaiter().GetResult();
                
                string result = response.Content.ReadAsStringAsync().Result;
                PrintResult(response.StatusCode, result);

                if (!response.IsSuccessStatusCode)
                {
                    int statusCode = (int)response.StatusCode;

                    if (statusCode >= 500)
                    {
                        throw new Exception($"on Method : {request.RequestUri}, API Results an ErrorCode : {statusCode}, Process stopped");
                    }
                    else
                    {
                        JObject res = JObject.Parse(result);
                        throw new CustomException(res, request.RequestUri.ToString(), statusCode);
                    }
                }

                var jResult = JObject.Parse(result); // need to replace with system.text - not urgent you can still use this one
                return jResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("<--Result Error : " + ex.Message);
                throw ;
            }
        }

        private HttpRequestMessage BuildRequest(string MethodUrl, HttpMethod httpMethod, JObject body)
        {
            var requestObject = body == null ? new JObject() : body;
            var requestBody = requestObject.ToString();//JsonSerializer.Serialize(requestObject);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri($"{baseUrl}{MethodUrl}"),
                Content = content
            };
            return request;
        }
        private void HandleCustomHeaders(Dictionary<string, object> HeaderValues, HttpRequestMessage request)
        {
            foreach (var header in HeaderValues)
            {
                if (header.Value != null)
                    request.Headers.Add(header.Key, header.Value.ToString());
            }
        }

        private void HandleConstantHeaders(HttpRequestMessage request)
        {
            //request.Headers.Add("Content-Type", "application/json");
            request.Headers.Add("Authorization", string.Concat("Bearer ", this.RosToken));
            request.Headers.Add("env", env);
            request.Headers.Add("JoinChannelGuid", JoinChannelGUID);

            if (!string.IsNullOrEmpty(AccountGUID))
            {
                request.Headers.Add("AccountGuid", AccountGUID);
            }

            if (CurrentThread != null)
            {
                if (CurrentThread.currentSite != null)
                {
                    request.Headers.Add("siteId", CurrentThread.currentSite.SiteId);
                }
            }
        }

        private void PrintRequestInformation(HttpRequestMessage request)
        {
            string Method = request.Method.ToString();
            string url = request.RequestUri.ToString();

            Console.WriteLine("-->[" + Method + "]" + url);

            Console.WriteLine("**BODY**" + Environment.NewLine + request.Content.ReadAsStringAsync().Result + Environment.NewLine + "**End of BODY**");
        }

        private void PrintResult(HttpStatusCode httpStatusCode, string result)
        {
            Console.WriteLine($"<--Result status[{httpStatusCode}]{Environment.NewLine}{result}");
        }

    }
}
