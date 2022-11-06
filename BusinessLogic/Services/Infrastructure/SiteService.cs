using LoyaltyAutoTest.BusinessLogic.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyAutoTest.BusinessLogic.Services
{
    public class SiteService
    {
        public string SiteName;
        public string SiteId;
        public int SiteNum;
        public TestThread CurrentThread { get; }


        public SiteService(int SiteNum, TestThread CurrentThread)
        {
            this.CurrentThread = CurrentThread;
            this.SiteNum = SiteNum;
            this.SiteId = Utils.GetConfigKey("Site_" + SiteNum);
        }
     
    }
}
