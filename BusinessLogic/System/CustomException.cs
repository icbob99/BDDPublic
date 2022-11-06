using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    internal class CustomException : Exception
    {
        public JObject ResponseData = new JObject();
        public string Key = "";
        public string ExceptionMessage = "";
        public string MethodUrl = "";
        public int statusCode;
        internal string InnerMessage;

        public CustomException(JObject TblExceptionResult, string methodUrl, int statusCode) : base(BuildMessage(TblExceptionResult, methodUrl, statusCode))
        {
            this.MethodUrl = methodUrl;
            this.statusCode = statusCode;

            this.Key = TblExceptionResult["Key"].ToString();
            this.ResponseData = TblExceptionResult;
            this.InnerMessage = TblExceptionResult["Message"].ToString();

            ExceptionMessage = BuildMessage(TblExceptionResult, methodUrl, statusCode);
        }

        private static string BuildMessage(JObject TblExceptionResult, string MethodUrl, int statusCode)
        {
            var Key = TblExceptionResult["Key"].ToString();
            string Message = TblExceptionResult["Message"].ToString();

            return "on Method : " + MethodUrl + ", API Results an ErrorCode : " + statusCode + " [" + Message + "] / with Key : [" + Key + "], Process stopped";
        }
    }
}
