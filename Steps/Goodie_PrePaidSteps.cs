using LoyaltyAutoTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace LoyaltyAutoTest.Steps
{
    [Binding]
    internal sealed class Goodie_PrePaidSteps : StepsAbstract
    {
        #region constructor
        private readonly ScenarioContext _scenarioContext;

        public Goodie_PrePaidSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }
        #endregion

        [Given(@"the site is configured for Goodie")]
        public void GivenTheSiteIsConfiguredForGoodie()
        {
            this.testThread.currentAccount.setConfiguration("IsExternalPaymentProvider", true);
            this.testThread.currentAccount.setConfiguration("ExternalPaymentProviderURL", "https://www.goodi.co.il/Interfaces/Tabit");
        }

        [Given(@"I topup Goodie amount")]
        public void GivenITopupGoodieAmount()
        {
            decimal balance = GetCardBalance();
            if (balance < 10m)
            {
                PrePaidService myPrePaid = new PrePaidService(this.testThread);
                myPrePaid.currentTestThread.OpenOrder();
                try
                {
                    myPrePaid.Pay(-10m);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    myPrePaid.currentTestThread.CloseOrder();
                }
            }
        }
        [When(@"I Pay more (.*)\$ than the card balance")]
        public void WhenIPayMoreThanTheCardBalance(decimal amount)
        {
            decimal balance = GetCardBalance();

            PrePaidService myPrePaid = new PrePaidService(this.testThread);
            testThread.OpenOrder();
            try
            {
                myPrePaid.Pay(balance + amount);
            }
            catch (Exception ex)
            {
                this.testThread.ExceptionRaised = ex;
            }
            finally
            {
                testThread.CloseOrder();
            }
        }

    }
}
