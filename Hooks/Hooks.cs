using LoyaltyAutoTest.BusinessLogic;
using LoyaltyAutoTest.Steps;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace LoyaltyAutoTest.Hooks
{
    [Binding]
    public sealed class Hooks
    {

        [AfterFeature]
        public static void FeatureCleanUp(FeatureContext featureContext)
        {
            StateManager.DestroyAccount(featureContext);
        }


        [AfterScenario]
        public void ScenarioCleanUp(ScenarioContext scenarioContext)
        {
            TestThread currentTestThread = (TestThread)scenarioContext["testThread"];
            currentTestThread.CleanUp();
        }

        //[AfterStep]
        //public void DontThrowError(ScenarioContext scenarioContext)
        //{

        //    //if (scenarioContext.StepContext.StepInfo.StepDefinitionType == StepDefinitionType.Then)
        //    //{
        //    //    if (scenarioContext.ScenarioInfo.Tags.Contains("DontThrowError")) return;

        //    //    TestThread currentTestThread = (TestThread)scenarioContext["testThread"];
        //    //    if (currentTestThread.ExceptionRaised != null)
        //    //    {
        //    //        throw currentTestThread.ExceptionRaised;
        //    //    }
        //    //}
        //}



    }
}
