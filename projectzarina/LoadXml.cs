using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace projectzarina
{
    class LoadXml
    {
        public static string LoadScreenshotPath()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("UserSettings.xml");
            xmlDoc.DocumentElement.SelectSingleNode("SettingData/ScreenshotPath");
            string ScreenshotPath = xmlDoc.InnerText;
            return ScreenshotPath;
        }

        public static string LoadLastUpdatedFile()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("UserSettings.xml");
            xmlDoc.DocumentElement.SelectSingleNode("SettingData/LastCreatedFile");
            string lastCreatedFile = xmlDoc.InnerText;
            return lastCreatedFile;
        }
    }
}
