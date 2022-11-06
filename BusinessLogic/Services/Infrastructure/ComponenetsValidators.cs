using FluentAssertions;
using LoyaltyAutoTest.BusinessLogic.System;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using TechTalk.SpecFlow.CommonModels;

namespace LoyaltyAutoTest.BusinessLogic.Services.Infrastructure
{
    public enum PaymentTransactionMode
    {
        Pay,
        Load,
        CancelLoad,
        CancelPay
    }

    internal class ComponenetsValidators
    {

        internal static void VerifyBalanceObject(JObject Response)
        {
            XElement xBalance = Response.GetElementFromJson("balance");
            xBalance.Should().HaveElement("prePaid");
            xBalance.Should().HaveElement("moneyCompensation");
            xBalance.Should().HaveElement("points");
        }
        internal static void VerifyBalanceObjectForPrePaid(JObject Response, decimal PrePaidBalance)
        {
            XElement xBalance = Response.GetElementFromJson("balance");

            xBalance.Should().HaveElement("prePaid");
            xBalance.Element("prePaid").Value.ToDecimal().Should().Be(PrePaidBalance);

            VerifyBalanceObject(Response);
        }

        internal static void VerifyConfigurationObject(JObject Response)
        {
            XElement xConfigurations = Response.GetElementFromJson("configurations");

            xConfigurations.Should().HaveElement("accountSettings");
            XElement xAccountSettings = xConfigurations.XPathSelectElement("accountSettings");

            xAccountSettings.Element("pointsNickName").Value.Should().NotBeNullOrEmpty();

            xAccountSettings.Should().HaveElement("isShowPointsIssueOnBill");
            xAccountSettings.Element("isShowPointsIssueOnBill").Value.Should().ContainAny("true", "false");

            xAccountSettings.Should().HaveElement("isShowPointsBalanceInPos");
            xAccountSettings.Element("isShowPointsBalanceInPos").Value.Should().ContainAny("true", "false");

            xAccountSettings.Should().HaveElement("isShowIdNumberInPos");
            xAccountSettings.Element("isShowIdNumberInPos").Value.Should().ContainAny("true", "false");

            xAccountSettings.Should().HaveElement("isShowBirthdateInPos");
            xAccountSettings.Element("isShowBirthdateInPos").Value.Should().ContainAny("true", "false");

            xAccountSettings.Should().HaveElement("isPrintShortNumber");
            xAccountSettings.Element("isPrintShortNumber").Value.Should().ContainAny("true", "false");

            xConfigurations.Should().HaveElement("cardType");
            XElement xCardType = xConfigurations.XPathSelectElement("cardType");

            xCardType.Should().HaveElement("cardTypeId");
            xCardType.Element("cardTypeId").Value.Should().NotBeNullOrEmpty();

            xCardType.Should().HaveElement("name");
            xCardType.Element("name").Value.Should().NotBeNullOrEmpty();

            xCardType.Should().HaveElement("description");

            xCardType.Should().HaveElement("isPayToJoin");
            xCardType.Element("isPayToJoin").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("canLoadMoney");
            xCardType.Element("canLoadMoney").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("isCvvRequired");
            xCardType.Element("isCvvRequired").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("isCardRechargeable");
            xCardType.Element("isCardRechargeable").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("isRequire2WayAuth");
            xCardType.Element("isRequire2WayAuth").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("isIncludedInMarketing");
            xCardType.Element("isIncludedInMarketing").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("isGeneralCustomerCanCreateInvoice");
            xCardType.Element("isGeneralCustomerCanCreateInvoice").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("expiredMemberCanRedeemPoints");
            xCardType.Element("expiredMemberCanRedeemPoints").Value.Should().ContainAny("true", "false");

            xCardType.Should().HaveElement("singleChargeLimit");
            xCardType.Element("singleChargeLimit").Value.Should().NotBeNullOrEmpty();

            xCardType.Should().HaveElement("cardBalanceLimit");
            xCardType.Element("cardBalanceLimit").Value.Should().NotBeNullOrEmpty();

            xCardType.Should().HaveElement("bonusPercentage");
            xCardType.Element("bonusPercentage").Value.Should().NotBeNullOrEmpty();

            xCardType.Should().HaveElement("minimumLoadingAmount");
            xCardType.Element("minimumLoadingAmount").Value.Should().NotBeNullOrEmpty();

        }

        internal static void VerifyCustomerObject(JObject Response)
        {
            XElement xCustomer = Response.GetElementFromJson("customer");

            xCustomer.Should().HaveElement("PrintMessage");
            xCustomer.Element("PrintMessage").Value.Should().NotBeNullOrEmpty();

            xCustomer.Should().HaveElement("cardNumber");
            xCustomer.Element("cardNumber").Value.Should().NotBeNullOrEmpty();

            xCustomer.Should().HaveElement("cardId");
            xCustomer.Element("cardId").Value.Should().NotBeNullOrEmpty();

            xCustomer.Should().HaveElement("fullName");

            xCustomer.Should().HaveElement("customerId");
            xCustomer.Element("customerId").Value.Should().NotBeNullOrEmpty();

            xCustomer.Should().HaveElement("isLoyaltyCustomer");
            xCustomer.Element("isLoyaltyCustomer").Value.Should().ContainAny("true", "false");


            xCustomer.Should().HaveElement("isGeneral");
            xCustomer.Element("isGeneral").Value.Should().ContainAny("true", "false");


            //xCustomer.Should().HaveElement("workflow");
            //xCustomer.Element("workflow").Value.Should().ContainAny("none", "MustPayMembershipFee", "ExpiredSoon", "CustomerExpired");

            //xCustomer.Should().HaveElement("accountName");
            //xCustomer.Element("accountName").Value.Should().NotBeNullOrEmpty();


        }

        internal static void VerifyPaymentTransactionObject(JObject Response, PaymentTransactionMode Mode, decimal Amount, decimal BonusDiscount)
        {
            XElement xPaymentTransaction = Response.GetElementFromJson("paymentTransaction");

            xPaymentTransaction.Should().HaveElement("value");
            xPaymentTransaction.Should().HaveElement("bonusDiscount");
            xPaymentTransaction.Should().HaveElement("transactionType");
            xPaymentTransaction.Should().HaveElement("referenceId");
            xPaymentTransaction.Should().HaveElement("referenceType");

            switch (Mode)
            {
                case PaymentTransactionMode.Pay:
                    xPaymentTransaction.Element("value").Value.ToDecimal().Should().Be(Amount);
                    xPaymentTransaction.Element("bonusDiscount").Value.ToDecimal().Should().Be(BonusDiscount);
                    xPaymentTransaction.Element("transactionType").Value.Should().Be("payment");

                    break;
                case PaymentTransactionMode.Load:
                    xPaymentTransaction.Element("value").Value.ToDouble().Should().BePositive();
                    xPaymentTransaction.Element("transactionType").Value.Should().Be("load");
                    break;
                case PaymentTransactionMode.CancelLoad:
                    xPaymentTransaction.Element("value").Value.ToDouble().Should().BeNegative();
                    xPaymentTransaction.Element("transactionType").Value.Should().Be("cancel-load");

                    break;
                case PaymentTransactionMode.CancelPay:

                    xPaymentTransaction.Element("value").Value.ToDouble().Should().BeNegative();
                    xPaymentTransaction.Element("transactionType").Value.Should().Be("cancel-payment");
                    break;
                default:
                    break;
            }
        }
    }
}
