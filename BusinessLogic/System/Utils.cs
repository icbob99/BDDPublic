using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Configuration;


namespace LoyaltyAutoTest.BusinessLogic.System
{
    class Utils
    {
        public static void ValidateExceptionByKey(string ExceptionKey, Exception ex)
        {
            if (ex is CustomException)
            {
                var CustomEx = ex as CustomException;
                if (CustomEx.Key != ExceptionKey)
                {
                    throw new Exception($"couldnt find the error : {ExceptionKey}, instead we got an error : [{CustomEx.Key}] with Message: {CustomEx.Message}");
                }
            }

        }

        internal static void ValidateExceptionByKeyAndMessage(string ExceptionKey, string exceptionMessage, Exception ex)
        {
            if (ex is CustomException)
            {
                var CustomEx = ex as CustomException;
                if (CustomEx.Key != ExceptionKey)
                {
                    throw new Exception($"couldnt find the error : {ExceptionKey}, instead we got an error : [{CustomEx.Key}] with Message: {CustomEx.Message}");
                }

                if (CustomEx.InnerMessage != exceptionMessage)
                {
                    throw new Exception($"couldnt find the error : {ExceptionKey} with message: {exceptionMessage}, instead we got an error : [{CustomEx.Key}] with Message: {CustomEx.InnerMessage}");
                }

            }
        }

        public static int getRandomNumber(int Min, int Max)
        {
            Random r = new Random();
            int Rand = r.Next(Min, Max);
            return Rand;

        }


        public static string GetConfigKey(string Key)
        {
            return ConfigurationHandler.Key(Key);

            //return ConfigurationManager.AppSettings[Key].ToString();
        }

        public static string getRandomObjectId()
        {
            Random random = new Random();
            int length = 24;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static dynamic GetJsonAsObject(JObject json)
        {
            return JsonConvert.DeserializeObject<ExpandoObject>(json.ToString(), new ExpandoObjectConverter());
        }

        internal static bool convertOnOffToBool(string mode)
        {
            if (mode.ToLower() == "on") return true;
            if (mode.ToLower() == "off") return false;
            throw new Exception("cannot convertOnOffToBool mode : " + mode);
        }

       
    }
}
