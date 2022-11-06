using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.BusinessLogic.System;
using LoyaltyAutoTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace LoyaltyAutoTest.Steps
{
    public static class StateManager
    {
        /// <summary>
        /// init a feature
        /// creates a new loyalty account
        /// init ROS token and init a new test thread
        /// </summary>
        /// <param name="featureContext"></param>
        internal static void BuildLoyaltyAccount(FeatureContext featureContext)
        {
            CleanUpPreviousKeys(featureContext);
            string token = HandleRosToken(featureContext);
            string AccountGuid = HandleAccountCreation();
            AccountService.CreateBusinessInAccount(AccountGuid);
            HandleTestThreadCreation(featureContext, token, AccountGuid);
        }


        private static void CleanUpPreviousKeys(FeatureContext featureContext)
        {
            if (featureContext.Keys.Contains("testThread"))
            {
                featureContext.Remove("testThread");
            }
        }


        /// <summary>
        /// init the test thread instance and stores it on the featureContext
        /// </summary>
        /// <param name="featureContext"></param>
        /// <param name="token"></param>
        /// <param name="AccountGuid"></param>
        internal static void HandleTestThreadCreation(FeatureContext featureContext, string RosToken, string AccountGuid)
        {
            var testThread = new TestThread(AccountGuid, RosToken);
            featureContext.Add("testThread", testThread);
        }

        /// <summary>
        /// creates a new account on TBL
        /// </summary>
        /// <returns>AccountGUID</returns>
        internal static string HandleAccountCreation()
        {
            string AccountName = "QA_Auto_PP_" + Utils.getRandomNumber(1000, 10000);
            string AccountGuid = AccountService.CreateNewAccount(AccountName);
            return AccountGuid;
        }

        /// <summary>
        /// generate a new ROS token and stores it on the featureContext
        /// </summary>
        /// <param name="featureContext"></param>
        /// <returns></returns>
        internal static string HandleRosToken(FeatureContext featureContext)
        {
            if (featureContext.Keys.Contains("rosToken"))
            {
                return featureContext["rosToken"].ToString();
            }

            RosHandler ros = new RosHandler();
            string token = ros.GetToken();
            featureContext.Add("rosToken", token);
            return token;
        }

        /// <summary>
        /// destory the account on TBL
        /// </summary>
        /// <param name="featureContext"></param>
        internal static void DestroyAccount(FeatureContext featureContext)
        {
            var testThread = featureContext["testThread"] as TestThread;
            testThread.currentAccount.Destroy();
            featureContext["testThread"] = null;
        }
    }
}
