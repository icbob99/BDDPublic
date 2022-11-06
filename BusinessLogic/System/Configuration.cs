using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LoyaltyAutoTest.BusinessLogic.System
{
    public static class ConfigurationHandler
    {

        private static IConfigurationRoot _AppConfig = null;
        public static IConfigurationRoot AppConfig
        {
            get
            {
                if (_AppConfig == null)
                {
                    _AppConfig = GetConfiguration();
                }
                return _AppConfig;
            }
        }


        public static IConfigurationRoot GetConfiguration()
        {

            var Builder = new ConfigurationBuilder();

            Builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            Builder.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);

            return Builder.Build();

        }

        public static string Key(string key)
        {
            return AppConfig[key];
        }

    }
}
