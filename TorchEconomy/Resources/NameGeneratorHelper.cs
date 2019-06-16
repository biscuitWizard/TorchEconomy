using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TorchEconomy.Data.Types;

namespace TorchEconomy.Resources
{
    public static class NameGeneratorHelper
    {
        public static string GetIndustryName(IndustryTypeEnum industryType)
        {
            var manifestResource = "";
            switch (industryType)
            {
                case IndustryTypeEnum.Consumer:
                    manifestResource = "TorchEconomy.Resources.Corporations.Consumer.txt";
                    break;
                case IndustryTypeEnum.Military:
                    manifestResource = "TorchEconomy.Resources.Corporations.Military.txt";
                    break;
                case IndustryTypeEnum.Research:
                    manifestResource = "TorchEconomy.Resources.Corporations.Research.txt";
                    break;
                case IndustryTypeEnum.Industrial:
                    manifestResource = "TorchEconomy.Resources.Corporations.Industrial.txt";
                    break;
            }

            using (Stream stream = Assembly
                .GetAssembly(typeof(EconomyPlugin))
                .GetManifestResourceStream(manifestResource)) 
            {
                using (var reader = new StreamReader(stream))
                {
                    var choices = reader.ReadToEnd().Split('\n');
                    return choices.OrderBy(g => Guid.NewGuid()).First();
                }
            }
        }

        public static string GetName()
        {
            using (Stream stream = Assembly
                .GetAssembly(typeof(EconomyPlugin))
                .GetManifestResourceStream("TorchEconomy.Resources.Names.txt")) 
            {
                using (var reader = new StreamReader(stream))
                {
                    var choices = reader.ReadToEnd().Split('\n');
                    return choices.OrderBy(g => Guid.NewGuid()).First();
                }
            }
        }
    }
}