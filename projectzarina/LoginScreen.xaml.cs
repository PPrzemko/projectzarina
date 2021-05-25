using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace projectzarina {

    public partial class LoginScreen : Window {
        
        protected string application = "MdhfE1eJ2L59n3mG3EPWQ23CIw4C5aUH";

        public LoginScreen() {

            InitializeComponent();

            // Prüfen, ob Benutzer (noch) angemeldet ist
            validateLogin();
            /*       
            // connection check
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://bing.com");
            request.Timeout = 15000;
            request.Method = "HEAD";
            try {
                using(HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                    // Funktioniert.
                }
            } catch(WebException) {
                ErrorBox.Text = "Seems like our servers are down. Check your internet connection or contact support";
            }
            */


            // Checks for XML if not there creates it 
            //string gems = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!File.Exists("UserSettings.xml"))
            {
                //System.IO.Directory.CreateDirectory(@"C:\Users\Public");

                XDocument doc123 = new XDocument(
                                             new XElement("SettingData",
                                             new XElement("token"),
                                             new XElement("ScreenshotPath")
                                             ));
                doc123.Save("UserSettings.xml");

                

            }
           
        }
       

        void OpenNewOrRestoreWindow<T>() where T : Window, new()
        {
            bool isWindowOpen = false;

            foreach (Window w in Application.Current.Windows)
            {
                if (w is T)
                {
                    isWindowOpen = true;
                    w.Activate();
                }
            }

            if (!isWindowOpen)
            {
                T newwindow = new T();
                newwindow.Show();
            }
        }





        // Benutzer klickt auf "Login"
        private void Button_Click(object sender, RoutedEventArgs e) {
            string user = emailorusername.Text;
            string passwd = password.Password;
            postLogin(user, passwd);
            // Console.WriteLine("test" + user + passwd);
        }




        private async void validateLogin() {
            if (File.Exists("UserSettings.xml")){
                var Config = new Settings();
                string token = Config.getValue("token");

                if (token != "")
                {
                    Console.WriteLine("token: " + token);
                    // "Angemeldet" => Anmeldung vorher prüfen mittels Validierung (auth/validate)
                    string url = "https://zarina.visualstatic.net/api/auth/validate?application=" + application;

                    HttpClient client = new HttpClient();
                    var values = new Dictionary<string, string> {
                    { "token", token },
                };

                    // DEBUG
                    // Console.WriteLine("token: " + token);

                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync(url, content);
                    var result = response.Content.ReadAsStringAsync().Result;

                    // DEBUG
                    // Console.WriteLine("result: " + result);

                    // TryCatch if API bwoke

                    try
                    {
                        dynamic json = JsonConvert.DeserializeObject(result);
                        if (json.success == "true")
                        {
                            var MainWindow = new MainWindow();
                            MainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            ErrorBox.Text = "API is offline";
                        }
                    }
                    catch (Exception)
                    {
                        ErrorBox.Text = "API is offline";
                    }

             }
                // ANDERNFALLS: No, Session Key existiert nicht mehr, User muss abgemeldet sein.
                // LoginScreen verbleibt weiterhin offen, gibt dem User die Möglichkeit sich nochmal zu authentifizieren oder als Gast fortzufahren.

            }

        }


        private async void postLogin(string user, string passwd) {
            string url = "https://zarina.visualstatic.net/api/auth/signin?application=" + application;

            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string> {
                { "user", user },
                { "passwd", passwd }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
            var result = response.Content.ReadAsStringAsync().Result;

            dynamic json = JsonConvert.DeserializeObject(result);

            if(json.success == "true") {
                string token = json.unique_token;
                
                var Config = new Settings();
                Config.updateValue("token", token);
                Console.WriteLine("token: " + token);
                var MainWindow = new MainWindow();
                MainWindow.Show();
                this.Close();

            } else {
                string feedback = json.errorMessage;
                ErrorBox.Text = "API is offline";
            }

            // {"success":"true","message":"Valid login attempt.","unique_token":"65b476da358aaad8a078c8bc13d7a74d","valid_until":"2021-06-21","created":"2021-05-22"}
        }


        // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }
}
