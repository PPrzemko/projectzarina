﻿using System;
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
using System.Threading.Tasks;

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
                                new XElement("InDev"),
                                new XElement("AutoRem")
                            )
                        );

                    doc123.Save("UserSettings.xml");
                    SettingXML.updateValue("Notification", "1");
                    SettingXML.updateValue("InDev", "0");
                    SettingXML.updateValue("AutoRem", "0");


                }

                /** 
                 * Automatic Version Check
                 */
                string url = string.Empty;
                int InDevStatus = 0;
                InDevStatus = Int16.Parse(SettingXML.getValue("InDev"));
                if(InDevStatus == 1){
                    url = "https://update.visualstatic.net/api/zarina-portable/indev";
                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                    using(HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                    using(StreamReader reader = new StreamReader(stream)) {
                        currentVersion = reader.ReadToEnd();
                    }
                    if(assemblyVersion == currentVersion) {
                        UpdateStatusBox.Text = "INDEV SOFTWARE IS UP TO DATE";

                    } else if(assemblyVersion != currentVersion) {
                        UpdateStatusBox.Text = " INDEV SOFTWARE IS OUTDATED. PLEASE UPDATE TO " + currentVersion;
                        DownloadBtn.Visibility = Visibility.Visible;
                    }  else {
                        UpdateStatusBox.Text = "SOFTWARE VERSION COULD NOT BE CHECKED";
                        DownloadBtn.Visibility = Visibility.Visible;
                    }
                } else{
                    url = "https://update.visualstatic.net/api/zarina-portable/stable";

                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                    using(HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                    using(StreamReader reader = new StreamReader(stream)) {
                        currentVersion = reader.ReadToEnd();
                    }

                    if(assemblyVersion == currentVersion){
                        UpdateStatusBox.Text = "SOFTWARE IS UP TO DATE";
                    } else if(assemblyVersion != currentVersion){
                        UpdateStatusBox.Text = "SOFTWARE IS OUTDATED. PLEASE UPDATE TO " + currentVersion;
                        DownloadBtn.Visibility = Visibility.Visible;
                    } else {
                        UpdateStatusBox.Text = "SOFTWARE VERSION COULD NOT BE CHECKED";
                        DownloadBtn.Visibility = Visibility.Visible;
                    }
                }




                // relpace update 
                // File.Move("projectzarina.exe", "projectzarina.tmp");
                // File.Delete("projectzarina.tmp");

            } catch(Exception ex){
                LogError(ex);
            }

        }



        /// <summary>
        /// User Button click login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                GIF(true);

                string user = emailorusername.Text;
                string passwd = password.Password;
                postLogin(user, passwd);

                // Console.WriteLine("test" + user + passwd); -- DEBUG
            } catch (Exception ex){
                LogError(ex);
            }
        }


        private async void validateLogin() {
            try {
                if (File.Exists("UserSettings.xml")) {
                    var SettingXML = new Settings();
                    

                    string token = SettingXML.getValue("token");
                    if (token != "") {
                        if (String.Compare(token, "0") == 0 ){
                            GIF(true);
                            var MainWindow = new MainWindow("Auto: Logged in as Guest");
                            MainWindow.Show();
                            this.Close();
                        }else {

                            GIF(true);
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
                                    var MainWindow = new MainWindow("Auto: Logged in as " + token);
                                    MainWindow.Show();
                                    this.Close();
                                }
                                else{
                                    imgCircle.Visibility = Visibility.Visible;
                                }
                            } catch(Exception) {
                                ErrorBox.Text = "API is offline.";
                            }
                        }
                    }

                }
                else{
                    
                }
            } catch(Exception ex){
                LogError(ex);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"> Username from Textbox</param>
        /// <param name="passwd"> Password from Passwordbox</param>
        private async void postLogin(string user, string passwd) {
            try {
                if (File.Exists("UserSettings.xml")){

                    //delay
                    Random rnd = new Random();
                    int rnddelay = rnd.Next(600, 800);
                    await Task.Delay(rnddelay);


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

                    if (json.success == "true"){
                        string token = json.unique_token;

                        var SettingXML = new Settings();
                        SettingXML.updateValue("token", token);
                        Console.WriteLine("token: " + token);
                        var MainWindow = new MainWindow("Logged in as " + user);
                        MainWindow.Show();
                        this.Close();
                    }
                    else{
                        string feedback = json.errorMessage;
                        GIF(false);
                        emailorusername.Text = user;
                        password.Password = "";
                        ErrorBox.Text = "Try Again";
                        /// emailorusername.BorderBrush = System.Windows.Media.Brushes.Red;
                        /// password.BorderBrush = System.Windows.Media.Brushes.Red;
                    }
                }
                else{
                    // if Usersettings.xml is not found = error
                    MessageBox.Show("Do not start this application in a Zip archive");
                    this.Close();
                }
            } catch (Exception ex) {
                    LogError(ex);
                    GIF(false);
                    emailorusername.Text = user;
                    password.Password = "";
                    ErrorBox.Text = "Try Again";
               
            }

        }

        private async void GuestLogin()
        {
            try {
                /// display loading gif
                GIF(true);


                /// delay
                Random rnd = new Random();
                int rnddelay = rnd.Next(500, 650);
                await Task.Delay(rnddelay);


                if (File.Exists("UserSettings.xml"))
                {
                    var SettingXML = new Settings();
                    SettingXML.updateValue("token", "0");
                    var MainWindow = new MainWindow("Logged in as Guest");
                    MainWindow.Show();
                    this.Close();
                }
                else
                {
                    // if Usersettings.xml is not found = error
                    MessageBox.Show("Do not start this application in a Zip archive");
                    this.Close();
                }
            }catch(Exception ex) {
                LogError(ex);
                }
}







        // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            try {
                if(e.LeftButton == MouseButtonState.Pressed) {
                    DragMove();
                }
            } catch(Exception ex) {
                LogError(ex);
            }
        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e) {
            try {
                this.Close();
            } catch(Exception ex) {
                LogError(ex);
            }
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            try {
                WindowState = WindowState.Minimized;
            } catch (Exception ex) {
                LogError(ex);
            }
        }

    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CONTINUEASGUEST(object sender, RoutedEventArgs e) {
            GuestLogin();
            }






        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://zarina.visualstatic.net/downloads");
        }


        /// <summary>
        /// catches every exception and logs it to a file
        /// </summary>
        /// <param name="ex"> handed from any catch Exception</param>
        private void LogError(Exception ex)
        {
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
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }


        private void GIF(bool loading)
        {
            if (loading == true)
            {
                // display loading gif and hide other fields
                imgCircle.Visibility = Visibility.Visible;
                emailorusernametxt.Visibility = Visibility.Hidden;
                emailorusername.Visibility = Visibility.Hidden;
                passwordtxt.Visibility = Visibility.Hidden;
                password.Visibility = Visibility.Hidden;
                SignInBtn.Visibility = Visibility.Hidden;
                GuestBtn.Visibility = Visibility.Hidden;
            }
            else if(loading == false)
            {
                imgCircle.Visibility = Visibility.Collapsed;
                emailorusernametxt.Visibility = Visibility.Visible;
                emailorusername.Visibility = Visibility.Visible;
                passwordtxt.Visibility = Visibility.Visible;
                password.Visibility = Visibility.Visible;
                SignInBtn.Visibility = Visibility.Visible;
                GuestBtn.Visibility = Visibility.Visible;
            }
        }








    }
}
