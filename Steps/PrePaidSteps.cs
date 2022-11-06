using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.BusinessLogic.System;
using LoyaltyAutoTest.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using LoyaltyAutoTest.BusinessLogic.Services;
using System.Threading;
using LoyaltyAutoTest.BusinessLogic.Common;

namespace LoyaltyAutoTest.Steps
{
    [Binding]
    public sealed class PrePaid : StepsAbstract
    {

        private readonly ScenarioContext _scenarioContext;

        #region constructor and init setup
        public PrePaid(ScenarioContext scenarioContext) : base()
        {
            _scenarioContext = scenarioContext;
        }


        #endregion

        #region Steps

        [Given(@"PrePaid Series is Configured to require CVV is ""([^""]*)""")]
        public void GivenPrePaidSeriesIsConfiguredToRequireCVVIs(string CvvMode)
        {
            bool flag = Utils.convertOnOffToBool(CvvMode);
            this.testThread.currentSeries.SetConfiguration("IsCvvRequired", flag);
        }

        [Given(@"I Dont have the CVV number")]
        public void GivenIDontHaveTheCVVNumber()
        {
            this.testThread.CVV = "";
        }

        [Given(@"I have a wrong CVV code")]
        public void GivenIHaveAWrongCVVCode()
        {
            this.testThread.CVV = "432423";
        }

        [Given(@"I have the correct CVV code")]
        public void GivenIHaveTheCorrectCVVCode()
        {
            this.testThread.CVV = this.testThread.currentSeries.GetCvvForCard();
        }


        [When(@"I dont Type the Cvv Code for payment")]
        public void WhenIDontTypeTheCvvCodeForPayment()
        {
            this.testThread.CVV = "";
        }

        [When(@"I Type the correct Cvv code for payment")]
        public void WhenITypeTheCorrectCvvCodeForPayment()
        {
            this.testThread.CVV = this.testThread.currentSeries.GetCvvForCard();
        }


        [When(@"I Type the wrong Cvv Code for payment")]
        public void WhenITypeTheWrongCvvCodeForPayment()
        {
            this.testThread.CVV = "42432423";
        }


        [Then(@"i am running the pre paid activity report and all actions are listed")]
        public void ThenIAmRunningThePrePaidActivityReportAndAllActionsAreListed()
        {
            //these are the steps from the test
            List<PrePaidActionsTableModel> Commands = (List<PrePaidActionsTableModel>)_scenarioContext["PrePaidCommandsTable"];

            //this is the report we need to check
            var service = new PrePaidService(testThread);
            List<PrePaidTransactionReportModel> myReport = service.getPrePaidTransactionsReport();

            //starting the last line - the report is in DESC sort
            int ReportLineIndex = myReport.Count;
            foreach (var command in Commands)
            {
                PrePaidTransactionReportModel ReportRow = myReport[ReportLineIndex - 1];
                switch (command.Action)
                {
                    case "load":
                        ReportRow.Action.Should().Be("Load");
                        ReportRow.TotalValue.Should().Be(command.Amount);
                        ReportRow.Value.Should().Be(command.Amount);

                        break;
                    case "pay":
                        ReportRow.Action.Should().Be("Redeem");
                        ReportRow.TotalValue.Should().Be(command.Amount * -1);
                        ReportRow.Value.Should().Be(command.Amount * -1);
                        break;
                }

                //ReportRow.BusinessId.Should().Be();
                ReportRow.CumulativeSum.Should().Be(command.Balance);

                ReportLineIndex--;
            }

        }

        [When(@"I do the following prepaid actions")]
        public void WhenIDoTheFollowingPrepaidActions(Table table)
        {
            var Commands = table.CreateSet<PrePaidActionsTableModel>();
            foreach (var command in Commands)
            {
                Thread.Sleep(500);
                SetSite(command.Site);

                switch (command.Action)
                {
                    case "load":
                        WhenILoadTheCardWith(command.Amount);
                        break;
                    case "pay":
                        WhenIPay(command.Amount);
                        break;
                }

                ThenTheBalanceOfTheCardIs(command.Balance);
            }

            _scenarioContext.Add("PrePaidCommandsTable", Commands);

        }

        [When(@"I do the following prepaid actions with Bonus")]
        public void WhenIDoTheFollowingPrepaidActionsWithBonus(Table table)
        {
            var Commands = table.CreateSet<PrePaidActionsTableWithBonusModel>();
            foreach (var command in Commands)
            {
                Thread.Sleep(500);
                SetSite(command.Site);

                switch (command.Action)
                {
                    case "load":
                        GivenIConfigureThePrePaidSeriesToHaveBonusOf(command.Bonus);
                        WhenILoadTheCardWith(command.Amount);
                        break;
                    case "pay":
                        WhenIPay(command.Amount);
                        break;
                }

                ThenTheBalanceOfTheCardIs(command.Balance);
            }
        }

        [When(@"I do the following prepaid actions with on Redeem Bonus")]
        public void WhenIDoTheFollowingPrepaidActionsWithOnRedeemBonus(Table table)
        {
            var Commands = table.CreateSet<PrePaidActionsTableWithOnRedeemBonusModel>();
            foreach (var command in Commands)
            {
                Thread.Sleep(500);
                SetSite(command.Site);

                switch (command.Action)
                {
                    case "load":
                        GivenIConfigureThePrePaidSeriesToHaveBonusOf(command.Bonus);
                        WhenILoadTheCardWith(command.Amount);
                        break;
                    case "pay":
                        WhenIPay(command.Amount);
                        var service = new PrePaidService(this.testThread);

                        if (testThread.currentAccount.IsPrePaidBonusOnRedeem())
                        {
                            JObject response = (JObject)_scenarioContext["lastResponseData"];

                            service.VerifyPaymentTransaction(response, command.PaidAmount, command.BonusDiscount);
                        }
                        break;
                }

                ThenTheBalanceOfTheCardIs(command.Balance);
            }
        }

        private void SetSite(int site)
        {
            this.testThread.setSite(site);
        }

        [Then(@"I Get an error with balance of (.*)\$ and card rechargable is ""([^""]*)""")]
        public void ThenIGetAnErrorWithBalanceOfAndCardRechargableIs(Decimal desiredBalance, string CardRechargeableMode)
        {

            bool flag = Utils.convertOnOffToBool(CardRechargeableMode);

            if (this.testThread.IsException())
            {
                var ex = this.testThread.ExceptionRaised;
                Utils.ValidateExceptionByKey("CustomInsufficientFunds", ex);

                var CustomEx = (CustomException)ex;
                CustomEx.ResponseData.ShouldHave("currentBalance", desiredBalance);
                CustomEx.ResponseData.ShouldHave("isCardRechargeable", flag);

            }
        }


        [Given(@"Account Configuration of PrePaid rounding is set to ""(.*)""")]
        public void GivenAccountConfigurationOfPrePaidRoundingIsSetTo(string mode)
        {
            bool flag = Utils.convertOnOffToBool(mode);
            this.testThread.currentAccount.setConfiguration("IsRoundPrepaidBalance", flag);

        }

        [Given(@"PrePaid Series is Configured to allow money load is ""(.*)""")]
        public void GivenPrePaidSeriesIsConfiguredToAllowMoneyLoadIs(string mode)
        {
            bool flag = Utils.convertOnOffToBool(mode);
            this.testThread.currentSeries.SetConfiguration("CanLoadMoney", flag);

        }

        [Given(@"I configure the account to give bonus '([^']*)'")]
        public void GivenIConfigureTheAccountToGiveBonus(PrepaidBonusType bonusType)
        {
            SetPrepaidBonusType(bonusType);


        }

        [When(@"I configure the account to give bonus '([^']*)'")]
        public void WhenIConfigureTheAccountToGiveBonus(PrepaidBonusType bonusType)
        {
            try
            {
                SetPrepaidBonusType(bonusType);

            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }

        }

        private void SetPrepaidBonusType(PrepaidBonusType bonusType)
        {
            testThread.currentAccount.setConfiguration("PrepaidBonusTypeId", ((int)bonusType).ToString());
        }

        [Given(@"I configure the Pre Paid Series to have Bonus of (.*)%")]
        [When(@"I configure the Pre Paid Series to have Bonus of (.*)%")]
        public void GivenIConfigureThePrePaidSeriesToHaveBonusOf(decimal bonusPercent)
        {
            testThread.currentSeries.SetConfiguration("BonusPercentage", bonusPercent);
        }



        [Given(@"PrePaid Series is Configured to have minimum load of (.*)\$")]
        public void GivenPrePaidSeriesIsConfiguredToHaveMinimumLoadOf(decimal amount)
        {

            this.testThread.currentSeries.SetConfiguration("MinimumLoadingAmount", amount);
        }

        [Given(@"PrePaid Series is Configured to have a single charge limit of (.*)\$")]
        public void GivenPrePaidSeriesIsConfiguredToHaveASingleChargeLimitOf(decimal amount)
        {
            testThread.currentSeries.SetConfiguration("SingleChargeLimit", amount);
        }

        [Given(@"PrePaid Series is Configured to have a card balance limit of (.*)\$")]
        public void GivenPrePaidSeriesIsConfiguredToHaveACardBalanceLimitOf(decimal amount)
        {
            testThread.currentSeries.SetConfiguration("CardBalanceLimit", amount);
        }

        [Given(@"I set PrePaid Series to be not rechargeable")]
        [When(@"I set PrePaid Series to be not rechargeable")]
        public void GivenISetPrePaidSeriesToBeNotRechargeable()
        {
            this.testThread.currentSeries.SetConfiguration("IsCardRechargeable", false);
        }

        [Given(@"I set PrePaid Series to forbid partial payment")]
        [When(@"I set PrePaid Series to forbid partial payment")]
        public void GivenISetPrePaidSeriesToForbidPartialPayment()
        {
            testThread.currentSeries.SetConfiguration("EnablePartialRedemptions", 0);
        }

        [When(@"I set PrePaid Series to allow partial payment")]
        public void WhenISetPrePaidSeriesToAllowPartialPayment()
        {
            testThread.currentSeries.SetConfiguration("EnablePartialRedemptions", 1);
        }

        [Given(@"I clear the card history")]
        [When(@"I clear the card history")]
        public void WhenIClearTheCardHistory()
        {
            var service = new PrePaidService(this.testThread);
            service.clearHistory();
        }

        [When(@"I Pay (.*)\$ And Exception Collect Is ""([^""]*)""")]
        public void WhenIPayAndExceptionCollectIs(decimal Amount, string ExceptionCollectMode)
        {
            bool IsExceptionCollect = Utils.convertOnOffToBool(ExceptionCollectMode);
            PrePaidService myPrePaid = new PrePaidService(this.testThread);
            testThread.OpenOrder();
            try
            {
                JObject res = myPrePaid.Pay(Amount);

                _scenarioContext["pay_money_referenceId"] = res["ResponseData"]["paymentTransaction"]["referenceId"].ToString();
                _scenarioContext["last_customerId"] = res["ResponseData"]["customer"]["cardNumber"].ToString();
                _scenarioContext["lastResponseData"] = res;

                if (this.testThread.currentAccount.IsPrePaidBonusOnLoad())
                {
                    myPrePaid.VerifyPaymentResult(res, Amount);
                }

            }
            catch (Exception ex)
            {
                if (IsExceptionCollect)
                {

                    this.testThread.ExceptionRaised = ex;
                }
                else { throw ex; }
            }
            finally
            {
                testThread.CloseOrder();
            }

        }


        [When(@"I Pay (.*)\$")]
        public void WhenIPay(decimal Amount)
        {
            WhenIPayAndExceptionCollectIs(Amount, "off");
        }

        [When(@"I load the card with (.*)\$ And Exception Collect Is ""([^""]*)""")]
        public void WhenILoadTheCardWithAndExceptionCollectIs(decimal loadAmount, string ExceptionCollectMode)
        {
            bool IsExceptionCollect = Utils.convertOnOffToBool(ExceptionCollectMode);

            try
            {
                JObject result = new JObject();
                testThread.OpenOrder();
                var service = new PrePaidService(this.testThread);
                result = service.loadCard(loadAmount);

                service.VerifyLoadResult(result, loadAmount);

                _scenarioContext["load_money_referenceId"] = result["ResponseData"]["paymentTransaction"]["referenceId"].ToString();
                _scenarioContext["last_customerId"] = result["ResponseData"]["customer"]["cardNumber"].ToString();
                _scenarioContext["lastResponseData"] = result;
            }
            catch (Exception ex)
            {
                if (IsExceptionCollect)
                {
                    this.testThread.ExceptionRaised = ex;
                    return;
                }
                else { throw; }
            }
            finally
            {
                this.testThread.CloseOrder();
            }

        }


        [When(@"I load the card with (.*)\$")]
        public void WhenILoadTheCardWith(decimal loadAmount)
        {
            WhenILoadTheCardWithAndExceptionCollectIs(loadAmount, "off");

        }

        [When(@"I cancel last load money")]
        public void WhenICancelLastLoadMoney()
        {
            var service = new PrePaidService(testThread);

            JObject result;
            try
            {
                string referenceId = (string)_scenarioContext["load_money_referenceId"];
                result = service.CancelLoadMoney(referenceId);
                service.VerifyCancelLoad(result);
            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }

        }

        [When(@"I cancel load money by reference ""([^""]*)""")]
        public void WhenICancelLoadMoney(string referenceId)
        {
            var service = new PrePaidService(testThread);

            JObject result;
            try
            {
                result = service.CancelLoadMoney(referenceId);
                service.VerifyCancelLoad(result);
            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }

        }

        [When(@"I set PrePaid Series to be rechargeable")]
        public void WhenISetPrePaidSeriesToBeRechargeable()
        {
            this.testThread.currentSeries.SetConfiguration("IsCardRechargeable", true);
        }

        [When(@"I cancel last payment")]
        public void WhenICancelLastPayment()
        {
            PrePaidService myPrePaid = new PrePaidService(this.testThread);
            myPrePaid.currentTestThread.OpenOrder();
            try
            {
                string referenceId = (string)_scenarioContext["pay_money_referenceId"];
                JObject res = myPrePaid.CancelPayMoney(referenceId);
                myPrePaid.VerifyCancelPaymentResult(res);
            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }
            finally
            {
                myPrePaid.currentTestThread.CloseOrder();
            }
        }

        [When(@"I cancel payment by reference ""([^""]*)""")]
        public void WhenICancelPaymentByReference(string referenceId)
        {
            PrePaidService myPrePaid = new PrePaidService(this.testThread);
            myPrePaid.currentTestThread.OpenOrder();
            try
            {
                JObject res = myPrePaid.CancelPayMoney(referenceId);
                myPrePaid.VerifyCancelPaymentResult(res);
            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }
            finally
            {
                myPrePaid.currentTestThread.CloseOrder();
            }
        }

        [When(@"I block my card")]
        public void WhenIBlockMyCard()
        {
            int CardId = GetCardId();

            ChangeCardStatusTo(CardId, "not active");

            _scenarioContext["CardId"] = CardId;
        }

        [When(@"I get customer")]
        public void WhenIGetCustomer()
        {
            try
            {
                GetCardId();
            }
            catch (Exception ex)
            {
                testThread.ExceptionRaised = ex;
                return;
            }
        }


        [When(@"I unblock my card")]
        public void WhenIUnblockMyCard()
        {
            ChangeCardStatusTo((int)_scenarioContext["CardId"], "active");
        }

        [When(@"I block the Account")]
        public void WhenIBlockAccount()
        {
            testThread.currentAccount.setConfiguration("IsActive", false);
        }

        [When(@"I unblock the Account")]
        public void WhenIUnblockTheAccount()
        {
            testThread.currentAccount.setConfiguration("IsActive", true);
        }

        [When(@"I block the Site")]
        public void WhenIBlockTheSite()
        {
            
            testThread.currentAccount.BlockSite(testThread.currentSite);
        }

        [When(@"I unblock the Site")]
        public void WhenIUnblockTheSite()
        {
            testThread.currentAccount.UnBlockSite(testThread.currentSite);
        }


        [Then(@"the balance of the card is (.*)\$")]
        public void ThenTheBalanceOfTheCardIs(decimal balance)
        {
            var service = new PrePaidService(this.testThread);
            JObject result;

            result = service.getCardBalance();
            service.VerifyGetBalanceResult(result, balance);

            result = service.getCustomer();
            service.VerifyGetBalanceOfGetCustomer(result, balance);

        }

        [Then(@"Pre Paid Bonus Type is '([^']*)'")]
        public void ThenPrePaidBonusTypeIs(PrepaidBonusType bonusType)
        {
            testThread.currentAccount.ValidateConfiguration("PrepaidBonusTypeId", ((int)bonusType).ToString());
        }


        [Then(@"In order to let other tests run, I reset the series to allow money load ""(.*)""")]
        public void ThenInOrderToLetOtherTestsRunIResetTheSeriesToAllowMoneyLoad(string mode)
        {
            bool flag = Utils.convertOnOffToBool(mode);
            this.testThread.currentSeries.SetConfiguration("CanLoadMoney", true);
        }


        #endregion

        #region Preset and cleanup hooks

        [Given(@"I have a loyalty program with (.*) new pre paid cards")]
        public void GivenIHaveALoyaltyProgramWithNewPrePaidCards(int NumOfCards)
        {
            if (FeatureContext.Current.Keys.Contains("testThread") == false)
            {
                StateManager.BuildLoyaltyAccount(FeatureContext.Current);
                GeneratePrePaidCards(NumOfCards);
            }

            AddTestThreadToScenarioContext(_scenarioContext);
        }

        #endregion

        #region private
        private static void GeneratePrePaidCards(int NumberOfCards)
        {
            TestThread testThread = GetTestThread();
            SeriesService series = new SeriesService(testThread);
            series.GeneratePrePaidCards(NumberOfCards);

        }

        private void ChangeCardStatusTo(int CardId, string status)
        {
            bool cardIsActive = status.ToLower().Equals("active");
            TestThread testThread = GetTestThread();
            SeriesService series = new SeriesService(testThread);

            series.ChangeCardStatus(CardId, cardIsActive);
        }

        private int GetCardId()
        {
            var service = new PrePaidService(this.testThread);
            JObject jCustomer = service.getCustomer();
            int CardId = jCustomer.GetElementFromJson("customer/cardId").Value.ToInt32();
            return CardId;
        }


        #endregion
    }


}
