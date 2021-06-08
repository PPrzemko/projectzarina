using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Xml;
using System.Reflection;
using System.Net;
using Windows.UI.Xaml.Input;

namespace projectzarina {

    public partial class LoginScreen : Window {

        private string application = "MdhfE1eJ2L59n3mG3EPWQ23CIw4C5aUH";
        private string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public LoginScreen() {
            try {
                InitializeComponent();

                // Checks if user has valid token in UserSettings.xml
                validateLogin();

                var SettingXML = new Settings();

                /**
                 * Checks for UserSettings.xml creates new one if there is none
                 */
                if(!File.Exists("UserSettings.xml")) {
                    /** 
                     * Removed old XML because of insufficient rights
                     * System.IO.Directory.CreateDirectory(@"C:\Users\Public");
                     * string gems = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                     */

                    XDocument doc123 = new XDocument(
                            new XElement("SettingData",
                                new XElement("token"),
                                new XElement("ScreenshotPath"),
                                new XElement("Notification"),
                                new XElement("InDev")
                            )
                        );

                    doc123.Save("UserSettings.xml");
                    SettingXML.updateValue("Notification", "1");
                    SettingXML.updateValue("InDev", "0");

                }

                /** 
                 * Automatic Updater
                 */
                string url = string.Empty;
                int InDevStatus = 0;
                InDevStatus = Int16.Parse(SettingXML.getValue("InDev"));
                if(InDevStatus == 1){
                    url = "https://update.visualstatic.net/api/zarina/indev";
                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                    using(HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                    using(StreamReader reader = new StreamReader(stream)) {
                        currentVersion = reader.ReadToEnd();
                    }
                    if(assemblyVersion == currentVersion) {
                        UpdateStatus.Text = "SOFTWARE IS UP TO DATE";

                    } else if(assemblyVersion != currentVersion) {
                        UpdateStatus.Text = " INDEV SOFTWARE IS OUTDATED. PLEASE UPDATE TO " + currentVersion;
                    }  else {
                        UpdateStatus.Text = "SOFTWARE VERSION COULD NOT BE CHECKED";
                    }
                } else{
                    url = "https://update.visualstatic.net/api/zarina/stable";

                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                    using(HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                    using(StreamReader reader = new StreamReader(stream)) {
                        currentVersion = reader.ReadToEnd();
                    }

                    if(assemblyVersion == currentVersion){
                        UpdateStatus.Text = "SOFTWARE IS UP TO DATE";
                    } else if(assemblyVersion != currentVersion){
                        UpdateStatus.Text = "SOFTWARE IS OUTDATED. PLEASE UPDATE TO " + currentVersion;
                    } else {
                        UpdateStatus.Text = "SOFTWARE VERSION COULD NOT BE CHECKED";
                    }
                }




                // relpace update 
                // File.Move("projectzarina.exe", "projectzarina.tmp");
                // File.Delete("projectzarina.tmp");

            } catch(Exception ex){
                this.LogError(ex);
            }

        }



        /// <summary>
        /// User Button click login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                string user = emailorusername.Text;
                string passwd = password.Password;
                postLogin(user, passwd);

                // Console.WriteLine("test" + user + passwd); -- DEBUG
            } catch (Exception ex){
                this.LogError(ex);
            }
        }


        private async void validateLogin() {
            try {
                if (File.Exists("UserSettings.xml")) {
                    var SettingXML = new Settings();
                    string token = SettingXML.getValue("token");

                    if(token != "") {
                        if (Int16.Parse(token) == 0){
                            var MainWindow = new MainWindow();
                            MainWindow.Show();
                            this.Close();
                        }else { 
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

                            try {
                                dynamic json = JsonConvert.DeserializeObject(result);
                                if(json.success == "true") {
                                    var MainWindow = new MainWindow();
                                    MainWindow.Show();
                                    this.Close();
                                }
                            } catch(Exception) {
                                ErrorBox.Text = "API is offline";
                            }
                        }
                    }

                }
            } catch(Exception ex){
                this.LogError(ex);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwd"></param>
        private async void postLogin(string user, string passwd) {
            try {
                string url = "https://zarina.visualstatic.net/api/auth/signin?application=" + application;

                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string> {
                    { "user", user },
                    { "passwd", passwd }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
                var result = response.Content.ReadAsStringAsync().Result;


                // try catch server fehler 500
                dynamic json = JsonConvert.DeserializeObject(result);

                if(json.success == "true") {
                    string token = json.unique_token;

                    var SettingXML = new Settings();
                    SettingXML.updateValue("token", token);
                    Console.WriteLine("token: " + token);
                    var MainWindow = new MainWindow();
                    MainWindow.Show();
                    this.Close();
                } else {
                    string feedback = json.errorMessage;
                }
            } catch (Exception ex) {
                    this.LogError(ex);
            }

        }


        // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            try {
                if(e.LeftButton == MouseButtonState.Pressed) {
                    DragMove();
                }
            } catch(Exception ex) {
                this.LogError(ex);
            }
        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e) {
            try {
                this.Close();
            } catch(Exception ex) {
                this.LogError(ex);
            }
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            try {
                WindowState = WindowState.Minimized;
            } catch (Exception ex) {
                this.LogError(ex);
            }
        }

        /// <summary>
        /// catches every exception and logs it to a file
        /// </summary>
        /// <param name="ex"> handed from any catch Exception</param>
        private void LogError(Exception ex) {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;

            string path = @"ErrorLogLogin.txt";
            using(StreamWriter writer = new StreamWriter(path, true)) {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CONTINUEASGUEST(object sender, RoutedEventArgs e) {
            var SettingXML = new Settings();
            SettingXML.updateValue("token", "0");
            var MainWindow = new MainWindow();
            MainWindow.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
