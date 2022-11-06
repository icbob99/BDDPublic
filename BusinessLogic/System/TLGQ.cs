using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace LoyaltyAutoTest.BusinessLogic.System
{

    public class TLGQ
    {

        private TestThread testThread;

        public TLGQ(TestThread testThread)
        {
            this.testThread = testThread;
        }

        public JObject Get(string target, string projection, string filter, string order)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.testThread);
            dynamic jBody = new JObject();
            jBody.target = target;
            jBody.projection = projection;
            if (!string.IsNullOrEmpty(filter))
                jBody.filter = filter;
            if (!string.IsNullOrEmpty(order))
                jBody.order = order;
            JObject res = api.SendApiCommand("tlgq/get", HttpMethod.Post, jBody);
            return res;
        }


        public JObject Update(string target, JObject data)
        {
            dynamic jBody = new JObject();
            jBody.target = target;
            jBody.data = new JArray() { data };

            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.testThread);
            JObject res = api.SendApiCommand("tlgq/update", HttpMethod.Put, jBody);
            return res;
        }




    }
}
