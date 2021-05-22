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

namespace projectzarina {
    
    public partial class MainWindow : Window {

        protected string application = "E1eJ3E4whf2mGC5aMdQ2L5CIUHPW9n33";

        public MainWindow() {

            InitializeComponent();
            var Config = new Settings();
            var screenshotPath = Config.getValue("ScreenshotPath");

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

        private void saveSettings(object sender, RoutedEventArgs e) {
            string screenshotPath = TextScreenshotPath.Text + @"\";
            
            var Config = new Settings();
            Config.updateValue("ScreenshotPath", screenshotPath);

        }

        private readonly HttpClient client = new HttpClient();
        private async void uploadImage(string file) {

            string url = "http://zarina.visualstatic.net/zarina/api/forms/upload" + application;

            var Config = new Settings();
            var screenshotPath = Config.getValue("ScreenshotPath");

            // read file into upfilebytes array
            var upfilebytes = File.ReadAllBytes(screenshotPath + file);
            
            // create new HttpClient and MultipartFormDataContent and add our file, and StudentId
            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(upfilebytes);
            content.Add(baContent, "file", "uploadedImage.jpg");


            // upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content);

            // read response result as a string async into json var
            var result = response.Content.ReadAsStringAsync().Result;

            // DEBUG
            Console.WriteLine(result);

        }

        private void Button_Click(object sender, RoutedEventArgs e){
            logout();
        }


        private async void logout()
        {

            var Config = new Settings();
            string token = Config.getValue("token");

            if (token != "")
            {
                // "Angemeldet" => Anmeldung vorher prüfen mittels Validierung (auth/validate)
                string url = "http://zarina.visualstatic.net/zarina/api/auth/destroy?application=" + application;

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


                dynamic json = JsonConvert.DeserializeObject(result);

                if (json.success == "true")
                {
                    // Jo, Token ist noch valide.
                    // Weiterleitung auf MainWindow.cs, da noch angemeldet.
                    var LoginScreen = new LoginScreen();
                    LoginScreen.Show();
                    this.Close();

                    Config.updateValue("token", "");
                }
                // ANDERNFALLS: No, Session Key existiert nicht mehr, User muss abgemeldet sein.
                // LoginScreen verbleibt weiterhin offen, gibt dem User die Möglichkeit sich nochmal zu authentifizieren oder als Gast fortzufahren.





                
            }

        }





    }
}
