using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectzarina
{
    public class SettingData
    {
        private string txtdata1;
        private string txtdata2;

        public string ScreenshotPath
        {
            get { return txtdata1; }
            set { txtdata1 = value; }
        }
        public string token
        {
            get { return txtdata2; }
            set { txtdata2 = value; }
        }
    }
}
