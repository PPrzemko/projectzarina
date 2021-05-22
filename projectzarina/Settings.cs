using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace projectzarina {
    class Settings {

        private string path = "UserSettings.xml";
        private string parent = "/SettingData/";

        public Settings() {}

        public string getValue(string node) {
            try {

                var xdoc = XDocument.Load(this.path);
                var result = xdoc.XPathSelectElement("/" + this.parent + node);

                if(result != null) {
                    return (string) result;
                } else return "false";

            } catch(Exception ev) {
                string error = "getValue(): " + ev.Message;
                Console.WriteLine(error);
                return error;
            }
        }

        public void updateValue(string node, string value) {
            try {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(this.path);
                xmlDoc.DocumentElement.SelectSingleNode(this.parent + node).InnerText = value;
                xmlDoc.Save(path);
            } catch(Exception ev) {
                Console.WriteLine("updateValue(): " + ev.Message);
            }
        }


    }
}