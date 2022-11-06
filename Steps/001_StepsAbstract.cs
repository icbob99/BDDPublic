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

    public class StepsAbstract
    {
        private TestThread _testThread;
        public TestThread testThread
        {
            get
            {
                if (_testThread == null)
                {
                    _testThread = FeatureContext.Current["testThread"] as TestThread;
                }
                return _testThread;
            }
        }


        public StepsAbstract()
        {
            
        }



        public void AddTestThreadToScenarioContext(ScenarioContext _scenarioContext)
        {

            _scenarioContext.Add("testThread", this.testThread);
        }



        #region static services



        internal static TestThread GetTestThread()
        {
            return FeatureContext.Current["testThread"] as TestThread;
        }




        #endregion


        #region Private functions


        internal decimal GetCardBalance()
        {
            PrePaidService myPrePaid = new PrePaidService(this.testThread);
            var result = myPrePaid.getCardBalance();
            var balance = result["ResponseData"]["balance"].ToDecimal();
            myPrePaid.VerifyGetBalanceResult(result, balance);

            return balance;
        }


        #endregion
    }
}
