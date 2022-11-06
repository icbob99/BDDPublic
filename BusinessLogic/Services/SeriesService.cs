using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.BusinessLogic.System;

using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace LoyaltyAutoTest.Services
{

    public class SeriesService
    {

        public TestThread CurrentThread;

        private int? _CardTypeId;
        public int CardTypeId
        {
            get
            {
                if (_CardTypeId == null)
                {
                    _CardTypeId = GetPrePaidSeriesCardTypeId();

                }

                return _CardTypeId.Value;

            }

        }
        public SeriesService(TestThread currentTestThread)
        {
            this.CurrentThread = currentTestThread;
        }


        internal string GetCvvForCard()
        {
            string CardNumber = this.CurrentThread.CardNumber;
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, CurrentThread);
            dynamic jBody = new JObject();
            jBody.cardNumber = CardNumber;

            var res = api.SendApiCommand("qa/get-card-cvv", HttpMethod.Post, jBody);
            return res["ResponseData"]["cvv"].ToString();
        }


        internal void GeneratePrePaidCards(int NumberOfCards)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, CurrentThread);

            dynamic jBody = new JObject();
            jBody.accountGuid = this.CurrentThread.currentAccount.AccountGUID;
            jBody.cardTypeId = this.CardTypeId;
            jBody.quantity = NumberOfCards;

            var res = api.SendApiCommand("ofc/generate-cards", HttpMethod.Post, jBody);
        }

        internal void ChangeCardStatus (int cardId, bool active)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, CurrentThread);
            dynamic jBody = new JObject();
            jBody.accountGuid = this.CurrentThread.currentAccount.AccountGUID;
            jBody.cardId = cardId;
            jBody.active = active;
                
            var res = api.SendApiCommand("ofc/customer/change-card-status", HttpMethod.Put, jBody);
        }

        private int GetPrePaidSeriesCardTypeId()
        {
            return GetSeriesConfigurationKey("CardTypeId").ToInt32();
        }

        public string GetSeriesConfigurationKey(string Key)
        {
            TLGQ myQuery = new TLGQ(this.CurrentThread);
            JObject res = myQuery.Get("[tlgq].[View_CardType]", "[" + Key + "]", "[CanLoadMoney]=1", "");
            string sValue = res["ResponseData"]["TLGQ"][0][Key].ToString();
            return sValue;
        }

        internal void SetConfiguration(string SeriesConfigurationKey, object value)
        {
            var json = new JObject();
            json.Add("CardTypeId", CardTypeId);

            if (value is bool)
            {
                json.Add(SeriesConfigurationKey, value.ToBoolean().ToInt32());
            }
            else if (value is decimal)
            {
                json.Add(SeriesConfigurationKey, value.ToDecimal());
            }
            else
            {
                json.Add(SeriesConfigurationKey, value.ToString());
            }

            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);

            TLGQ myQuery = new TLGQ(this.CurrentThread);
            var res = myQuery.Update("CardType", json);

            this.CurrentThread.IsSeriesConfigurationChanged = true;
        }

        internal void ResotreDefaultConfiguration()
        {
            var json = GetDefaultSettings();
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);

            TLGQ myQuery = new TLGQ(this.CurrentThread);
            var res = myQuery.Update("CardType", json);
        }

        private JObject GetDefaultSettings()
        {
            JObject json = new JObject();
            json.Add("CardTypeId", CardTypeId);
            json.Add("AccountId", CurrentThread.currentAccount.AccountId);

            if (IsPrePaid())
            {
                json.Add("IsGeneralCustomerCanCreateInvoice", 1);
                json.Add("IsCvvRequired", 0);
                json.Add("BonusPercentage", 0m);
                json.Add("IsCardRechargeable", 1);
                json.Add("SingleChargeLimit", 1000m);
                json.Add("CardBalanceLimit", 5000m);
                json.Add("MinimumLoadingAmount", 0);
                json.Add("EnablePartialRedemptions", 1);
            }

            return json;
        }

        private bool IsPrePaid()
        {
            string CanLoadMoney = GetSeriesConfigurationKey("CanLoadMoney");
            return CanLoadMoney.ToBoolean();
        }
    }




}
