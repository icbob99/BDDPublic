using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyAutoTest.BusinessLogic.Services
{
    internal class PrePaidActionsTableModel
    {
        public string Action;
        public int Site;
        public decimal Amount;
        public decimal Balance;
    }

    internal class PrePaidActionsTableWithBonusModel
    {
        public string Action;
        public decimal Bonus;
        public int Site;
        public decimal Amount;
        public decimal Balance;

    }

    internal class PrePaidTransactionReportModel
    {
        public string Action;
        public decimal? Bonus;
        public int BusinessId;
        public decimal CumulativeSum;
        public decimal TotalValue;
        public decimal Value;
    }

    internal class PrePaidActionsTableWithOnRedeemBonusModel
    {
        public string? Action;
        public decimal Bonus;
        public int Site;
        public decimal Amount;
        public decimal Balance;
        public decimal PaidAmount;
        public decimal BonusDiscount;
    }
}
