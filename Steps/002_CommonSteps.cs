using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.BusinessLogic.System;
using LoyaltyAutoTest.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using FluentAssertions;

using TechTalk.SpecFlow;
using System.Xml;
using System.Xml.Linq;


namespace LoyaltyAutoTest.Steps
{
    [Binding]
    public sealed class CommonStpes : StepsAbstract
    {

        private readonly ScenarioContext _scenarioContext;


        PrePaidService service;


        public CommonStpes(ScenarioContext scenarioContext) : base()
        {
            _scenarioContext = scenarioContext;

        }




        [Given(@"i have a loyalty program")]
        public void GivenIHaveALoyaltyProgram()
        {
            if (FeatureContext.Current.Keys.Contains("testThread") == false)
            {
                StateManager.BuildLoyaltyAccount(FeatureContext.Current);
            }
            AddTestThreadToScenarioContext(_scenarioContext);

        }



        [Given(@"I'm the owner of Card ""(.*)""")]
        public void GivenIMTheOwnerOfCard(string cardnumber)
        {
            this.testThread.setCardNumber(cardnumber);
        }


        [Given(@"I'm at Site (.*)")]
        public void GivenIMAtSite(int siteNum)
        {
            this.testThread.setSite(siteNum);

        }




        [Then(@"I Get An Exception of type ""(.*)""")]
        public void ThenIGetAnExceptionOfType(string ExceptionKey)
        {
            if (this.testThread.IsException())
            {
                Utils.ValidateExceptionByKey(ExceptionKey, this.testThread.ExceptionRaised);
            }
            else
            {
                throw new Exception("No Exception occurred, expected : " + ExceptionKey);
            }

        }

        [Then(@"I Get An Exception of type ""([^""]*)"" with message '([^']*)'")]
        public void ThenIGetAnExceptionOfTypeWithMessage(string ExceptionKey, string ExceptionMessage)
        {
            if (this.testThread.IsException())
            {
                Utils.ValidateExceptionByKeyAndMessage(ExceptionKey, ExceptionMessage,  testThread.ExceptionRaised);
            }
            else
            {
                throw new Exception("No Exception occurred, expected : " + ExceptionKey);
            }
        }



        [Given(@"transaction channel is ""([^""]*)""")]
        public void GivenTransactionChannelIs(string Channel)
        {
            Channel = Channel.ToLower();
            this.testThread.setJoinChannel(Channel);
        }




    }

}