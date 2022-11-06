using LoyaltyAutoTest.BusinessLogic.Services;
using LoyaltyAutoTest.BusinessLogic.System;
using LoyaltyAutoTest.Services;
using System;

namespace LoyaltyAutoTest.BusinessLogic
{
    public class TestThread
    {

        internal string CardNumber { get; private set; } = "0";
        public bool IsMultipass { get; internal set; } = false;

        public string CVV = String.Empty;

        public bool IsSeriesConfigurationChanged = false;

        public AccountService currentAccount;
        public SiteService currentSite;
        public string ROSToken = "";
        public string env = "il";
        public string OrderInScope = "";
        public SeriesService currentSeries;
        public Exception ExceptionRaised;
        public JoinChannel CurrentJoinChannel;

        public TestThread(string AccountGuid, string ROSToken)
        {
            this.ROSToken = ROSToken;
            currentAccount = new AccountService(AccountGuid, this);
            setJoinChannel("pad"); //default
        }


        internal void setCardNumber(string cardnumber)
        {
            this.CardNumber = cardnumber;
            this.currentSeries = new SeriesService(this);

        }

        internal void setSite(int SiteNum)
        {
            currentSite = new SiteService(SiteNum, this);
        }

        /// <summary>
        /// creates a random order id and sets in the testThread scope [OrderInScope]
        /// </summary>
        internal void OpenOrder()
        {
            OrderInScope = Utils.getRandomObjectId();
        }

        /// <summary>
        /// clears the [OrderInScope] field
        /// </summary>
        internal void CloseOrder()
        {
            OrderInScope = "";
        }

        internal bool IsException()
        {
            return (this.ExceptionRaised != null);
        }

        internal void CleanUp()
        {
            if (IsSeriesConfigurationChanged)
            {
                this.currentSeries.ResotreDefaultConfiguration();
                this.IsSeriesConfigurationChanged = false;
            }

            if (this.IsException())
            {
                this.ExceptionRaised = null;
            }

        }

        internal void setJoinChannel(string channel)
        {
            this.CurrentJoinChannel = new JoinChannel(channel);
        }
    }



}
