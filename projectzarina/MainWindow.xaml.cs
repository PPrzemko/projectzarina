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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace projectzarina {
    
    public partial class MainWindow : Window {

        protected string application = "MdhfE1eJ2L59n3mG3EPWQ23CIw4C5aUH";
        protected bool restartRequired = false;


        /// <summary>
        /// MainWindow
        /// </summary>
        public MainWindow() {
            try
            {
                InitializeComponent();

                // get ScreenshotPath
                var SettingXML = new Settings();
                int Notification = Int16.Parse(SettingXML.getValue("Notification"));
                if (Notification == 1)
                {
                    NotificationCheckbox.IsChecked = true;
                }

                createFSW();

            }catch (Exception ex){
                this.LogError(ex);}
        }


      /// <summary>
      /// 
      /// </summary>
        private void createFSW() {
            try{
                Settings SettingXML = new Settings();

                var screenshotPath = SettingXML.getValue("ScreenshotPath");
                string path = screenshotPath + @"\";
                TextScreenshotPath.Text = path;

                Console.WriteLine("first check of scP: " + path);
            
                // listing on path for new file. try for wrong path argument
            
                FileSystemWatcher watcher = new FileSystemWatcher(path);

                watcher.Filter = "*.jpg";
                watcher.Created += new FileSystemEventHandler(OnCreated);

                watcher.EnableRaisingEvents = true;

                // DEBUG
                // Console.WriteLine("new fsw on: " + path);
            } catch (System.ArgumentException ex) {
                // DEBUG
                // Console.WriteLine(ex.Message);
                this.LogError(ex);
            }


        }



       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"> source of event.</param>
        /// <param name="e"> contains event data.</param>
        //DateTime lastRead = DateTime.MinValue;
        public void OnCreated(object sender, FileSystemEventArgs e) {
            try { 
                if(this.restartRequired) return;
            

                Settings xml = new Settings();

                string filename = System.IO.Path.GetFileName(e.FullPath);
                Console.WriteLine("Erstellt: " + filename);
                Console.WriteLine("FullPath: " + e.FullPath);

                string fullPath = e.FullPath;
                uploadImage(filename, fullPath);
            }catch (Exception ex){
                this.LogError(ex);
            }
        }

        /// <summary>
        /// Set Path function on MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assignPath(object sender, RoutedEventArgs e) {
            try{
                System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();

                //openFileDlg.SelectedPath = @"C:\Program Files (x86)\Steam\userdata\226359406\760\remote\381210\screenshots";
                openFileDlg.ShowDialog();
                var result = openFileDlg.SelectedPath;
                TextScreenshotPath.Text = result;
            }catch(Exception ex){ 
                this.LogError(ex);}
        }

        /// <summary>
        /// Saves ScreenshotPath and notificationconfig to XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveScreenshotPath(object sender, RoutedEventArgs e)
        {
            try{ 
                var SettingXML = new Settings();
                int XMLNotification = Int16.Parse(SettingXML.getValue("Notification"));
                string currentXMLpath = SettingXML.getValue("ScreenshotPath") + @"\";

                string screenshotPath = TextScreenshotPath.Text;

                /*
                    * Save Screenshot path and Notification status. set output accordingly
                    * 
                    * 
                    */
                if (currentXMLpath != screenshotPath)
                {
                    SettingXML.updateValue("ScreenshotPath", screenshotPath);
                    createFSW();
                    this.restartRequired = true;
                    Button_SaveSettings.IsEnabled = false;
                    nameassignPath.IsEnabled = false;
                    NotificationCheckbox.IsEnabled = false;
                    SaveMessage.Content = "Path has been updated. Please restart Application";
                }
                else
                {
                    SaveMessage.Content = "There is nothing to save1";
                }


                if (NotificationCheckbox.IsChecked == true & XMLNotification == 0)
                {
                    SettingXML.updateValue("Notification", "1");
                    if (currentXMLpath != screenshotPath)
                    {
                        SaveMessage.Content = "You will now receive Notification. Path has been updated. Please restart Application";
                    }
                    else
                    {
                        SaveMessage.Content = "You will now receive Notification.";
                    }

                }
                else if (NotificationCheckbox.IsChecked == false & XMLNotification == 1)
                {
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


            }catch (Exception ex){
            this.LogError(ex);}
        }
            





        /// <summary>
        /// Image Upload
        /// </summary>
        /// <param name="file"> filename from OnCreated(from FSW) </param>
        /// <param name="fullPath"> Gets Path from OnCreated</param>
        private async void uploadImage(string file, string fullPath) {
            try{ 
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

                string time = DateTime.Now.ToString("HH:mm tt");

                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText("-----------------------------" + Environment.NewLine)));
                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText( time + ":  " + file + " has been submitted to your statistics" + Environment.NewLine)));
                // DEBUG
                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText( result + Environment.NewLine)));
                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText("-----------------------------" + Environment.NewLine)));
                await test.Dispatcher.BeginInvoke((Action)(() => test.ScrollToEnd()));
            }catch (Exception ex){
                this.LogError(ex);}
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutButton(object sender, RoutedEventArgs e) {
            logout();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">args will be passed when starting this program</param>
        private async void logout() {
            try{
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
            }catch (Exception ex){
                this.LogError(ex);}
        }




        // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            try{
                if (e.LeftButton == MouseButtonState.Pressed) {
                    DragMove();
                }
            }catch (Exception ex){
                this.LogError(ex);}
        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e) {
            try{
                this.Close();
            }catch (Exception ex){
                this.LogError(ex);}
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            try{
                WindowState = WindowState.Minimized;
             }catch (Exception ex){
                this.LogError(ex);}
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
            string path = @"ErrorLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        private void NotificationCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("NotInUse");
        }

        private void MyAccountbutton(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("NotInUse");
            System.Diagnostics.Process.Start("https://zarina.visualstatic.net/my-statistics");
        }

        private void Statsbutton(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
        }
    }
}
