using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyAutoTest.BusinessLogic.Common
{
    public enum PrepaidBonusType
    {
        /* 
            select  pb.Alias + '=' + CAST(pb.PrePaidBonusTypeId as nvarchar(max)) + ', //' + Name
            from PrePaidBonusType pb 
        */
        OnLoad = 1, 
        OnRedeem = 2, 
    }
}
