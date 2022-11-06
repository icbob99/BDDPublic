using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.BusinessLogic.Services;
using LoyaltyAutoTest.BusinessLogic.Services.Infrastructure;
using LoyaltyAutoTest.BusinessLogic.System;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace LoyaltyAutoTest.Services
{

    class PrePaidService
    {
        public string CardNumber {  get; private set; }

        public TestThread currentTestThread;

        public PrePaidService(TestThread currentTestThread)
        {
            this.currentTestThread = currentTestThread;
            this.CardNumber = this.currentTestThread.CardNumber;
        }


        internal void clearHistory()
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, currentTestThread);

            dynamic jBody = new JObject();
            jBody.cardNumber = CardNumber;

            api.SendApiCommand("ofc/customer/clear-card", HttpMethod.Post, jBody);
        }

        internal JObject getCardBalance()
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread); ;

            dynamic jBody = new JObject();
            jBody.cardNumber = CardNumber;
            if (string.IsNullOrEmpty(currentTestThread.CVV) == false)
            {
                jBody.cvv = currentTestThread.CVV;
            }


            JObject res = api.SendApiCommand("pos/get-money-balance", HttpMethod.Post, jBody);
            return res;

        }

        private string GetCurrentJoinChannel()
        {
            return this.currentTestThread.CurrentJoinChannel.JoinChannelGUID;
        }

        internal JObject loadCard(decimal loadAmount)
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread);

            dynamic jBody = new JObject();
            jBody.amount = loadAmount;
            jBody.orderId = currentTestThread.OrderInScope;
            jBody.cardNumber = currentTestThread.CardNumber;
            if (string.IsNullOrEmpty(currentTestThread.CVV) == false)
            {
                jBody.cvv = currentTestThread.CVV;
            }

            JObject res = api.SendApiCommand("pos/load-money", HttpMethod.Post, jBody);
            return res;
        }



        internal JObject Pay(decimal amount)
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread);

            dynamic jBody = new JObject();
            jBody.amount = amount;
            jBody.orderId = currentTestThread.OrderInScope;
            jBody.cardNumber = currentTestThread.CardNumber;
            if (string.IsNullOrEmpty(currentTestThread.CVV) == false)
            {
                jBody.cvv = currentTestThread.CVV;
            }
            JObject res = api.SendApiCommand("pos/pay-money", HttpMethod.Post, jBody);
            return res;
        }

        internal JObject CancelLoadMoney(string referenceId)
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread);

            dynamic jBody = new JObject();
            jBody.referenceId = referenceId;

            JObject res = api.SendApiCommand("pos/cancel-load-money", HttpMethod.Post, jBody);
            return res;
        }

        internal JObject CancelPayMoney(string referenceId)
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread);

            dynamic jBody = new JObject();
            jBody.referenceId = referenceId;

            JObject res = api.SendApiCommand("pos/cancel-pay-money", HttpMethod.Post, jBody);
            return res;
        }

        internal JObject getCustomer()
        {
            ApiHandler api = new ApiHandler("il", GetCurrentJoinChannel(), currentTestThread);

            dynamic jBody = new JObject();
            jBody.query = this.CardNumber;

            JObject res = api.SendApiCommand("pos/get-customer", HttpMethod.Post, jBody);
            return res;
        }

        internal List<PrePaidTransactionReportModel> getPrePaidTransactionsReport()
        {
            JObject jCustomer = getCustomer();
            int CardId = jCustomer.GetElementFromJson("customer/cardId").Value.ToInt32();

            TLGQ myQuery = new TLGQ(this.currentTestThread);
            JObject res = myQuery.Get("[tlgq].[View_PrePaidTransactions]", "", "[CardId] = " + CardId, "[TimeStamp] desc");

            var data = res["ResponseData"]["TLGQ"];
            List<PrePaidTransactionReportModel> ArrayResult = data.ToObject<List<PrePaidTransactionReportModel>>();

            return ArrayResult;

        }



        internal void VerifyCancelPaymentResult(JObject result)
        {
            ComponenetsValidators.VerifyPaymentTransactionObject(result, PaymentTransactionMode.CancelPay, 0, 0); ;
            ComponenetsValidators.VerifyCustomerObject(result);
            ComponenetsValidators.VerifyBalanceObject(result);

        }

        internal void VerifyPaymentResult(JObject result, decimal Amount)
        {
            ComponenetsValidators.VerifyCustomerObject(result);
            /// TODO:
            /// Multipass does not have Configuration section. This is a bug and it should be fixed in refactoring process
            //ComponenetsValidators.VerifyConfigurationObject(result);
            ComponenetsValidators.VerifyBalanceObject(result);
            ComponenetsValidators.VerifyPaymentTransactionObject(result, PaymentTransactionMode.Pay, Amount, 0m); ;
        }

        internal void VerifyGetBalanceResult(JObject result, decimal balance)
        {
            result.ShouldHave("balance", balance);
            result.ShouldExists("cardNumber");
            result.ShouldExists("firstName");
            result.ShouldExists("lastName");
            // ComponenetsValidators.VerifyConfigurationObject(result);
        }

        internal void VerifyGetBalanceOfGetCustomer(JObject result, decimal balance)
        {
            ComponenetsValidators.VerifyCustomerObject(result);
            ComponenetsValidators.VerifyBalanceObjectForPrePaid(result, balance);
            ComponenetsValidators.VerifyConfigurationObject(result);
        }

        internal void VerifyLoadResult(JObject result, decimal Amount)
        {
            ComponenetsValidators.VerifyCustomerObject(result);
            ComponenetsValidators.VerifyBalanceObject(result);
            /// TODO:
            /// Multipass does not have Configuration section. This is a bug and it should be fixed in refactoring process
            //ComponenetsValidators.VerifyConfigurationObject(result);
            /// TODO:
            /// Multipass returns "payment" instead of "load" on LoadCard action
            if (this.currentTestThread.IsMultipass)
                ComponenetsValidators.VerifyPaymentTransactionObject(result, PaymentTransactionMode.Pay, Amount, 0m);
            else
                ComponenetsValidators.VerifyPaymentTransactionObject(result, PaymentTransactionMode.Load, Amount, 0m);

        }

        internal void VerifyCancelLoad(JObject result)
        {
            ComponenetsValidators.VerifyCustomerObject(result);
            ComponenetsValidators.VerifyBalanceObject(result);
            ComponenetsValidators.VerifyPaymentTransactionObject(result, PaymentTransactionMode.CancelLoad, 0m, 0m);
        }

        internal void VerifyPaymentTransaction(JObject response, decimal amount, decimal bonusDiscount)
        {
            ComponenetsValidators.VerifyPaymentTransactionObject(response, PaymentTransactionMode.Pay, amount, bonusDiscount);
        }
    }
}
