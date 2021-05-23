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

        protected string application = "E1eJ3E4whf2mGC5aMdQ2L5CIUHPW9n33";

        public MainWindow() {

            InitializeComponent();
            var Config = new Settings();
            var screenshotPath = Config.getValue("ScreenshotPath");

            TextScreenshotPath.Text = screenshotPath;

            try {
                FileSystemWatcher watcher = new FileSystemWatcher(screenshotPath);
                watcher.Filter = "*.jpg";
                watcher.Created += new FileSystemEventHandler(Watcher_Created);
                watcher.EnableRaisingEvents = true;
            } catch (System.ArgumentException e) {
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
            string screenshotPath = TextScreenshotPath.Text + @"\";

            var Config = new Settings();
            Config.updateValue("ScreenshotPath", screenshotPath);
        }




        /**
         * Autmatic Image upload after FSW
         */
        private async void uploadImage(string file) {

            string url = "http://45.10.62.120:8070/zarina/api/forms/upload?application=" + application;

            var Config = new Settings();
            var screenshotPath = Config.getValue("ScreenshotPath");
            var token = Config.getValue("token");


            var values = new Dictionary<string, string> {
                { "token", token },
            };

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
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Your Stats have been updated")
                .AddText(file + " has been uploaded")
                .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 5, your TFM must be net5.0-windows10.0.17763.0 or greater

        }






        /**
         * Button to logout and delete Session Token in XML and DB
         */
        private void logoutButton(object sender, RoutedEventArgs e) {
            logout();
        }

        private async void logout() {
            var Config = new Settings();
            string token = Config.getValue("token");

            if(token != "") {

                // token in DB löschen
                string url = "http://zarina.visualstatic.net/zarina/api/auth/destroy?application=" + application;

                HttpClient client = new HttpClient();
                var values = new Dictionary<string, string> {
                    { "token", token },
                };

                var content = new FormUrlEncodedContent(values);
                await client.PostAsync(url, content);

                // Weiterleitung auf Login & Token in XML löschen
                Config.updateValue("token", "");

                var LoginScreen = new LoginScreen();
                LoginScreen.Show();
                this.Close();

            }
        }


                            // notification test 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Requires Microsoft.Toolkit.Uwp.Notifications NuGet package version 7.0 or greater
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Andrew sent you a picture")
                .AddText("Check this out, The Enchantments in Washington!")
                .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 5, your TFM must be net5.0-windows10.0.17763.0 or greater
        }
    }
}
