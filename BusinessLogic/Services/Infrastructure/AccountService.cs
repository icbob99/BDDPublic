using LoyaltyAutoTest.BusinessLogic.System;
using System;
using Newtonsoft.Json.Linq;
using LoyaltyAutoTest.BusinessLogic;
using FluentAssertions;
using LoyaltyAutoTest.BusinessLogic.Common;
using System.Net.Http;
using LoyaltyAutoTest.BusinessLogic.Services;

namespace LoyaltyAutoTest.Services
{
    public class AccountService
    {
        public string AccountGUID { get; }
        private string AccountName = "";
        public int AccountId = 0;

        public TestThread CurrentThread { get; }

        private JObject jAccount;


        public AccountService(string AccountGuid, TestThread CurrentThread)
        {
            this.AccountGUID = AccountGuid;
            this.CurrentThread = CurrentThread;
            initAccount();
        }

        private void initAccount()
        {
            InitAccountSettings();
            this.AccountName = GetAccountColumn("Name");
            this.AccountId = GetAccountColumn("AccountId").ToInt32();
        }

        private void InitAccountSettings()
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);
            this.jAccount = api.SendApiCommand("ofc/account/" + this.AccountGUID + "/settings", HttpMethod.Get);
        }

        internal static void CreateBusinessInAccount(string accountGuid)
        {
            BuildSingleBusiness(1, accountGuid);
            BuildSingleBusiness(2, accountGuid);
        }

        private static void BuildSingleBusiness(int SiteNum, string accountGuid)
        {
            JObject site = new JObject();
            site.Add("siteId", ConfigurationHandler.Key("Site_" + SiteNum));
            site.Add("name", "Branch #" + SiteNum);

            dynamic json = new JObject();
            json.sites = new JArray() { site };

            ApiHandler api = new ApiHandler("il", JoinChannels.Office);
            api.SendApiCommand("ofc/account/" + accountGuid + "/business", HttpMethod.Post, json);


        }

        public string GetAccountColumn(string key)
        {
            return this.jAccount["ResponseData"]["Account"][0][key].ToString();
        }

        public string GetAccountSettingsColumn(string key)
        {
            return this.jAccount["ResponseData"]["AccountSettings"][0][key].ToString();
        }


        #region Account Creation

        internal static string CreateNewAccount(string accountName)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office);
            dynamic json = new JObject();
            json.name = accountName;
            json.defaultClubName = "loyalty program";
            json.defaultPrepaidName = "pre paid";
            json.accountTypeId = 4;
            json.siteId = Utils.GetConfigKey("HQ_SiteId");

            var res = api.SendApiCommand("ofc/account", HttpMethod.Post, json);
            return res["ResponseData"]["accountGuid"].ToString();
        }

        internal void Destroy()
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office);
            dynamic json = new JObject();
            json.accountId = this.AccountId;

            var res = api.SendApiCommand("qa/destroy-account", HttpMethod.Post, json);
        }

        internal void setConfiguration(string AccountSettingsKey, bool value)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);

            string sJson = "{ \"" + AccountSettingsKey + "\": " + value.ToInt32() + "}";
            JObject json = JObject.Parse(sJson);


            var res = api.SendApiCommand("ofc/account/settings", HttpMethod.Put, json);

            InitAccountSettings();
        }

        internal void setConfiguration(string AccountSettingsKey, string value)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);

            string sJson = "{ \"" + AccountSettingsKey + "\": '" + value + "'}";
            JObject json = JObject.Parse(sJson);


            var res = api.SendApiCommand("ofc/account/settings", HttpMethod.Put, json);

            InitAccountSettings();
        }

        internal void ValidateConfiguration(string AccountSettingsKey, string expectedAccountSettingsValue)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);
            jAccount = api.SendApiCommand("ofc/account/" + this.AccountGUID + "/settings", HttpMethod.Get);
            string actualAccountSettingValue = GetAccountSettingsColumn(AccountSettingsKey);
            expectedAccountSettingsValue.Should().BeEquivalentTo(actualAccountSettingValue);
        }

        internal bool IsPrePaidBonusOnLoad()
        {
            string sPrepaidBonusTypeId = this.GetAccountSettingsColumn("PrepaidBonusTypeId");
            if ((PrepaidBonusType)Enum.Parse(typeof(PrepaidBonusType), sPrepaidBonusTypeId) == PrepaidBonusType.OnLoad)
            {
                return true;
            }

            return false;
        }

        internal bool IsPrePaidBonusOnRedeem()
        {
            string sPrepaidBonusTypeId = this.GetAccountSettingsColumn("PrepaidBonusTypeId");
            return (PrepaidBonusType)Enum.Parse(typeof(PrepaidBonusType), sPrepaidBonusTypeId) == PrepaidBonusType.OnRedeem;
        }

        internal void BlockSite(SiteService currentSite)
        {
            SetBusinessConfiguration(currentSite, "IsActive", false);
        }

        internal void UnBlockSite(SiteService currentSite)
        {
         
            SetBusinessConfiguration(currentSite, "IsActive", true);
        }


        #endregion

        private JObject SetBusinessConfiguration(SiteService currentSite, string BusinessConfigurationKey, bool value)
        {
            ApiHandler api = new ApiHandler("il", JoinChannels.Office, this.CurrentThread);

            dynamic json = new JObject();

            json.Key = BusinessConfigurationKey;
            json.Value = value.ToInt32();

            var res = api.SendApiCommand($"qa/business/{currentSite.SiteId}/settings", HttpMethod.Put, json);
            return res;
        }

    }
}
