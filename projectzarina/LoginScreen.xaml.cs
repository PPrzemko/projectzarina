using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace projectzarina {

    public partial class LoginScreen : Window {
        
        protected string application = "E1eJ3E4whf2mGC5aMdQ2L5CIUHPW9n33";

        public LoginScreen() {
            InitializeComponent();

            // Prüfen, ob Benutzer (noch) angemeldet ist.
            validateLogin();




            // connection check
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://zarina.visualstatic.net");
            request.Timeout = 15000;
            request.Method = "HEAD"; // As per Lasse's comment
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    bool lol = response.StatusCode == HttpStatusCode.OK;
                    //Console.WriteLine(lol);
                    ErrorBox.Text = "";
                }
            }
            catch (WebException)
            {
                ErrorBox.Text = "Seams like our servers are down. Check your internet connection or contact support";
            }

        }

        // Benutzer klickt auf "Login"
        private void Button_Click(object sender, RoutedEventArgs e) {
            string user = emailorusername.Text;
            string passwd = password.Password;
            postLogin(user, passwd);
            //Console.WriteLine("test" + user + passwd);
        }


        // DragnDrop funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e){
            if(e.LeftButton == MouseButtonState.Pressed){
                DragMove();
            }

        }




private async void validateLogin() {

            var Config = new Settings();
            string token = Config.getValue("token");

            if(token != "") {
                // "Angemeldet" => Anmeldung vorher prüfen mittels Validierung (auth/validate)
                string url = "http://zarina.visualstatic.net/zarina/api/auth/validate?application=" + application;

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

                if(json.success == "true") {
                    // Jo, Token ist noch valide.
                    // Weiterleitung auf MainWindow.cs, da noch angemeldet.
                    var MainWindow = new MainWindow();
                    MainWindow.Show();
                    this.Close();
                }
                // ANDERNFALLS: No, Session Key existiert nicht mehr, User muss abgemeldet sein.
                // LoginScreen verbleibt weiterhin offen, gibt dem User die Möglichkeit sich nochmal zu authentifizieren oder als Gast fortzufahren.
                    
            }

        }

        private async void postLogin(string user, string passwd) {
            string url = "http://zarina.visualstatic.net/zarina/api/auth/signin?application=" + application;

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
                // Jo, hat funktioniert
                string token = json.unique_token;
                
                var Config = new Settings();
                Config.updateValue("token", token);
                Console.WriteLine("token: " + token);
                // Wenn halt richtig ist amk, muss dann auf MainWindow umgeschalten werden.
                // Umschalten auf MainWindow.cs
                var MainWindow = new MainWindow();
                MainWindow.Show();
                this.Close();

            } else {
                // No, hat nicht funktioniert
                // Im Programm anzeigen lassen, dass etwas nicht funktioniert hat.
                string feedback = json.errorMessage;

            }

            // {"success":"true","message":"Valid login attempt.","unique_token":"65b476da358aaad8a078c8bc13d7a74d","valid_until":"2021-06-21","created":"2021-05-22"}
        }


        // Close Button on LoginPage
        private void ExitProgramm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
