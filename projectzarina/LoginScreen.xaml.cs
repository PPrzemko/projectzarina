using System;
using System.Collections.Generic;
using System.Linq;
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

namespace projectzarina
{
    /// <summary>
    /// Interaktionslogik für LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        public LoginScreen()
        {
            InitializeComponent();
        }

        private void emailorusername_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void password_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string user = emailorusername.Text;
            string passwd = password.Password;
            SendPost(user, passwd);
           
        }

        private readonly HttpClient client = new HttpClient();
        private async void SendPost(string user, string passwd)
        {
            string url = "http://zarina.visualstatic.net/zarina/api/auth/signin?application=E1eJ3E4whf2mGC5aMdQ2L5CIUHPW9n33";


            //create new HttpClient and MultipartFormDataContent and add our file, and StudentId
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
                {
                    { "user", user },
                    { "passwd", passwd }
                };

            var content = new FormUrlEncodedContent(values);

            //upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content);

            //read response result as a string async into json var
            var responsestr = response.Content.ReadAsStringAsync().Result;

            //debug
            Console.WriteLine(responsestr);

            dynamic magic = JsonConvert.DeserializeObject(responsestr);
            string token = magic.unique_token; 
            

            try
            {
                SettingData userData = new SettingData();
                userData.token = token;
                userData.ScreenshotPath = LoadXml.LoadScreenshotPath();
                Console.WriteLine(token);
                Console.WriteLine(LoadXml.LoadScreenshotPath());
                SaveXml.SaveData(userData, "UserSettings.xml");
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            






            //{"success":"true","message":"Valid login attempt.","unique_token":"65b476da358aaad8a078c8bc13d7a74d","valid_until":"2021-06-21","created":"2021-05-22"}
        }
    }
}
