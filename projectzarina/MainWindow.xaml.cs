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

        public MainWindow() {

            InitializeComponent();

            // get ScreenshotPath
            var SettingXML = new Settings();
            var screenshotPath = SettingXML.getValue("ScreenshotPath");
            TextScreenshotPath.Text = screenshotPath;
            var Notification = SettingXML.getValue("Notification");
            int Notification2 = Int16.Parse(Notification);
            if (Notification2 == 1){
                NotificationCheckbox.IsChecked = true;
            }
                // listing on path for new file. try for wrong path argument
                try
                {
                FileSystemWatcher watcher = new FileSystemWatcher(screenshotPath);
                watcher.Filter = "*.jpg";
                watcher.Created += new FileSystemEventHandler(Watcher_Created);
                watcher.EnableRaisingEvents = true;
            }catch (System.ArgumentException e) {
                Console.WriteLine(e.Message);
            }

        }

        public void Watcher_Created(object sender, FileSystemEventArgs e) {

            string filename = System.IO.Path.GetFileName(e.FullPath);
            Console.WriteLine("Erstellt: " + filename);
            uploadImage(filename);
        }

        private void assignPath(object sender, RoutedEventArgs e) {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();

            openFileDlg.SelectedPath = @"C:\Program Files (x86)\Steam\userdata\";
            openFileDlg.ShowDialog();
            var result = openFileDlg.SelectedPath;
            TextScreenshotPath.Text = result;
        }

        /**
         * Button to save the path for the Steam screenshots
         */
        private void saveScreenshotPath(object sender, RoutedEventArgs e) {

            // SaveScreenshotPath
            string screenshotPath = TextScreenshotPath.Text + @"\";
            var SettingXML = new Settings();
            SettingXML.updateValue("ScreenshotPath", screenshotPath);

            // Save Notification Status
            if(NotificationCheckbox.IsChecked == true){
                SettingXML.updateValue("Notification", "1");
            }
            else if(NotificationCheckbox.IsChecked == false){
                SettingXML.updateValue("Notification", "0");
            }
        }




        /**
         * Autmatic Image upload after FSW
         */
        private async void uploadImage(string file) {
            
            string url = "https://zarina.visualstatic.net/api/forms/upload?application=" + application;

            var SettingXML = new Settings();
            var screenshotPath = SettingXML.getValue("ScreenshotPath");
            var token = SettingXML.getValue("token");

            // dictionary mit einem eintrag hinbekommen 
            // fehler : =c9398accfb4bd3673b8e84e78f1c2c76
            var values = new Dictionary<string, string> {
                            { "lol", token },

                        };
          
            //Console.WriteLine("das hier token amk " + content);

            

            // read file into upfilebytes array
            var upfilebytes = File.ReadAllBytes(screenshotPath + file);

            // create new HttpClient combine picture and string into single content "codeproject.com/Questions/1228835/How-to-post-file-and-data-to-api-using-httpclient"
            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(upfilebytes);
            HttpContent Dictionary = new FormUrlEncodedContent(values);

            content.Add(baContent, "file", "uploadedImage.jpg");
            content.Add(Dictionary, "token");

           

            // upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content);
            Console.WriteLine(response);
            // read response result as a string async into json var
            var result = response.Content.ReadAsStringAsync().Result;

            // DEBUG
            Console.WriteLine(result);



            //Windows notification

            string notification = SettingXML.getValue("Notification");
            int notification2 = Int16.Parse(notification);
            if (notification2 == 1) {
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
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }

        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        
        
        





    }
}
