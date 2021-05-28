using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;

namespace projectzarina {

    public partial class MainWindow : Window {

        protected string application = "MdhfE1eJ2L59n3mG3EPWQ23CIw4C5aUH";
        protected bool restartRequired = false;

        public MainWindow() {

            InitializeComponent();

            // get ScreenshotPath
            var SettingXML = new Settings();
            

            int Notification = Int16.Parse(SettingXML.getValue("Notification"));
            if (Notification == 1){
                NotificationCheckbox.IsChecked = true;
            }



            createFSW();

        }

        private void createFSW() {
            
            Settings SettingXML = new Settings();

            var screenshotPath = SettingXML.getValue("ScreenshotPath");
            string path = screenshotPath + @"\";
            TextScreenshotPath.Text = path;

            Console.WriteLine("first check of scP: " + path);
            
            // listing on path for new file. try for wrong path argument
            try {
                FileSystemWatcher watcher = new FileSystemWatcher(path);

                watcher.Filter = "*.jpg";
                watcher.Created += new FileSystemEventHandler(OnCreated);

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("new fsw on: " + path);
            } catch (System.ArgumentException e) {
                Console.WriteLine(e.Message);
            }


        }


        DateTime lastRead = DateTime.MinValue;
        public void OnCreated(object sender, FileSystemEventArgs e) {
            
            if(this.restartRequired) return;
            

            Settings xml = new Settings();

            string filename = System.IO.Path.GetFileName(e.FullPath);
            Console.WriteLine("Erstellt: " + filename);
            Console.WriteLine("FullPath: " + e.FullPath);

            string fullPath = e.FullPath;
            uploadImage(filename, fullPath);

            // else discard the (duplicated) OnChanged event            
        }

        private void assignPath(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();

            //openFileDlg.SelectedPath = @"C:\Program Files (x86)\Steam\userdata\226359406\760\remote\381210\screenshots";
            openFileDlg.ShowDialog();
            var result = openFileDlg.SelectedPath;
            TextScreenshotPath.Text = result;
        }

        /**
         * Button to save the path for the Steam screenshots
         */
        private void saveScreenshotPath(object sender, RoutedEventArgs e) {
            var SettingXML = new Settings();
            int XMLNotification = Int16.Parse(SettingXML.getValue("Notification"));
            string currentXMLpath = SettingXML.getValue("ScreenshotPath") + @"\";

            string screenshotPath = TextScreenshotPath.Text;

            /*
             * Save Screenshot path and Notification status. set output accordingly
             * 
             * 
             */
            if (currentXMLpath != screenshotPath){
                SettingXML.updateValue("ScreenshotPath", screenshotPath);
                createFSW();
                this.restartRequired = true;
                Button_SaveSettings.IsEnabled = false;
                nameassignPath.IsEnabled = false;
                NotificationCheckbox.IsEnabled = false;
                SaveMessage.Content = "Path has been updated. Please restart Application";
            }
            else{
                SaveMessage.Content = "There is nothing to save1";
            }


            if (NotificationCheckbox.IsChecked == true & XMLNotification == 0){
                SettingXML.updateValue("Notification", "1");
                if (currentXMLpath != screenshotPath){
                    SaveMessage.Content = "You will now receive Notification. Path has been updated. Please restart Application";
                }
                else{
                    SaveMessage.Content = "You will now receive Notification.";
                }
                   
            }else if (NotificationCheckbox.IsChecked == false & XMLNotification == 1){
                SettingXML.updateValue("Notification", "0");
                    if (currentXMLpath != screenshotPath)
                    {
                        SaveMessage.Content = "You will no longer receive Notification. Path has been updated. Please restart Application";
                    }
                    else
                    {
                        SaveMessage.Content = "You will no longer receive Notification.";
                    }
            }
            /* else if(currentXMLpath != screenshotPath)
            {
                SaveMessage.Content = "There is nothing to save2";
            } */











        }

        


        /**
         * Autmatic Image upload after FSW
         */
        private async void uploadImage(string file, string fullPath) {
            
            string url = "https://zarina.visualstatic.net/api/forms/upload?application=" + application;

            var config = new Settings();
            string token = config.getValue("token");


            // read file into upfilebytes array 

            Console.WriteLine("token: " + token);
            Console.WriteLine("FullPath 2: " + fullPath);
                
            var upfilebytes = File.ReadAllBytes(fullPath);

            // create new HttpClient combine picture and string into single content "codeproject.com/Questions/1228835/How-to-post-file-and-data-to-api-using-httpclient"
            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(upfilebytes);
            StringContent stringContent = new StringContent(token);

            content.Add(baContent, "file", "uploadedImage.jpg");
            content.Add(stringContent, "token");


            // upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content);
            Console.WriteLine(response);
            // read response result as a string async into json var
            var result = response.Content.ReadAsStringAsync().Result;

            // DEBUG
            // Console.WriteLine(result);



            // Windows Notification

            int notification = Int16.Parse(config.getValue("Notification"));
            if(notification == 1) {
                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText("Your Stats have been updated")
                    .AddText(file + " has been uploaded")
                    .Show();
            }
            
        }

       




        /**
         * Button to logout and delete Session Token in XML and DB
         */
        private void logoutButton(object sender, RoutedEventArgs e) {
            logout();
        }

        private async void logout() {
            var SettingXML = new Settings();
            string token = SettingXML.getValue("token");

            if(token != "") {

                // delete token in db
                string url = "https://zarina.visualstatic.net/api/auth/destroy?application=" + application;

                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string> {
                    { "token", token },
                };

                var content = new FormUrlEncodedContent(values);
                await client.PostAsync(url, content);

                // Weiterleitung auf Login & Token in XML löschen
                SettingXML.updateValue("token", "");

                var LoginScreen = new LoginScreen();
                LoginScreen.Show();
                this.Close();

            }
        }




        // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
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
