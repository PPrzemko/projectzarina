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
using Path = System.IO.Path;

namespace projectzarina
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TextScreenshotPath.Text = LoadXml.LoadScreenshotPath();



            try
            {
                string watchedPath = LoadXml.LoadScreenshotPath();
                // watchedPath = @"C:\Program Files (x86)\Steam\userdata\2263d59406\760\remote\381210\screenshots\test\amk\fsdfsf";
                FileSystemWatcher watcher = new FileSystemWatcher(watchedPath);
                watcher.Filter = "*.jpg";

                watcher.Created += new FileSystemEventHandler(Watcher_Created);

                watcher.EnableRaisingEvents = true;
            }
            catch (System.ArgumentException e)
            {
                Console.WriteLine("Something went wrong.");
                Console.WriteLine(e.Message);
            }



        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PathSelect();
        }


        private void BtnSaveSettings(object sender, RoutedEventArgs e)
        {
            SaveUserSettings();
        }



        public void PathSelect()
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();

            openFileDlg.SelectedPath = @"C:\Program Files (x86)\Steam\userdata\";
            openFileDlg.ShowDialog();
            var result = openFileDlg.SelectedPath;
            TextScreenshotPath.Text = result;

        }

      
        public void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string filename = Path.GetFileName(e.FullPath);
            Console.WriteLine("Erstellt: " + filename);
            SendPost(filename);
        }

        private readonly HttpClient client = new HttpClient();
        private async void SendPost(string file)
        {
            string url = "http://zarina.visualstatic.net/zarina/api/forms/upload?application=E1eJ3E4whf2mGC5aMdQ2L5CIUHPW9n33";


            //read file into upfilebytes array
            var upfilebytes = File.ReadAllBytes(LoadXml.LoadScreenshotPath() + file);
            
            //create new HttpClient and MultipartFormDataContent and add our file, and StudentId
            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(upfilebytes);
            content.Add(baContent, "file", "uploadedImage.jpg");


            //upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content);

            //read response result as a string async into json var
            var responsestr = response.Content.ReadAsStringAsync().Result;

            //debug
            Console.WriteLine(responsestr);

        }

        public void SaveUserSettings()
        {
            try
            {
                SettingData userData = new SettingData();
                userData.ScreenshotPath = TextScreenshotPath.Text + @"\";
                SaveXml.SaveData(userData, "UserSettings.xml");
                SaveMessage.Visibility = Visibility.Visible;
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
