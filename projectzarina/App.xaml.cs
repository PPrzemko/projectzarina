using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace projectzarina
{
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        /*
         * Stops user from opening more than one window
         * @param StartupEventArgs e - defines the startup event args
         */
        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "projectzarina";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if(!createdNew){
                //app is already running! Exiting the application  
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
