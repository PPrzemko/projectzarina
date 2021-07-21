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
using System.Net;
using System.Reflection;

namespace projectzarina {
    
    public partial class MainWindow : Window {

        protected string application = "MdhfE1eJ2L59n3mG3EPWQ23CIw4C5aUH";
        protected bool restartRequired = false;
        protected int lol = 0;
        protected int AutoRemMessage = 0;
        private string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// MainWindow
        /// </summary>
        public MainWindow(string output) {
            try
            {
                InitializeComponent();
                OutputToConsole(output, true);
                var SettingXML = new Settings();


                // get and check token
                string token = SettingXML.getValue("token");

                if (String.Compare(token, "0") == 0)
                {
                    LogoutTxt.Content = "Login";

                }




                // get ScreenshotPath

                int Notification = Int16.Parse(SettingXML.getValue("Notification"));
                int AutoRem = Int16.Parse(SettingXML.getValue("AutoRem"));
                if (Notification == 1)
                {
                    NotificationCheckbox.IsChecked = true;
                }
                if (AutoRem == 1)
                {
                    AutoRemChk.IsChecked = true;
                }

                createFSW();



                // Software version check output to activity log
                string url = string.Empty;
                int InDevStatus = 0;
                InDevStatus = Int16.Parse(SettingXML.getValue("InDev"));
                if (InDevStatus == 1)
                {
                    url = "https://update.visualstatic.net/api/zarina-portable/indev";
                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        currentVersion = reader.ReadToEnd();
                    }
                    if (assemblyVersion == currentVersion)
                    {
                        OutputToConsole("INDEV SOFTWARE IS UP TO DATE", true);

                    }
                    else if (assemblyVersion != currentVersion)
                    {
                        OutputToConsole("INDEV SOFTWARE IS OUTDATED. PLEASE UPDATE TO " + currentVersion, true);
                    }
                    else
                    {
                        OutputToConsole("SOFTWARE VERSION COULD NOT BE CHECKED", true);
                    }
                }
                else
                {
                    url = "https://update.visualstatic.net/api/zarina-portable/stable";

                    string currentVersion = string.Empty;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        currentVersion = reader.ReadToEnd();
                    }

                    if (assemblyVersion == currentVersion)
                    {
                        OutputToConsole("SOFTWARE IS UP TO DATE", true);
                    }
                    else if (assemblyVersion != currentVersion)
                    {
                        OutputToConsole("SOFTWARE IS OUTDATED.PLEASE UPDATE TO " + currentVersion, true);
                    }
                    else
                    {
                        OutputToConsole("SOFTWARE VERSION COULD NOT BE CHECKED", true);

                    }
                }






                }catch (Exception ex){
                LogError(ex);}
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
                LogError(ex);
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
                LogError(ex);
            }
        }

        /// <summary>
        /// Set Path function on MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assignPath(object sender, RoutedEventArgs e) {
            try{
                
                var openFolder = new CommonOpenFileDialog();
                openFolder.AllowNonFileSystemItems = true;
                openFolder.Multiselect = true;
                openFolder.IsFolderPicker = true;
                openFolder.Title = "Please select the Steam Screeshot folder";

               

                if (openFolder.ShowDialog() == CommonFileDialogResult.Ok){
                    TextScreenshotPath.Text = openFolder.FileName;
                }else{
                    //MessageBox.Show("No Folder selected");
                    OutputToConsole("No Folder selcted", true);
                    return;
                }
                


                /*
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                Console.WriteLine(result);
                TextScreenshotPath.Text = result; */


                /*
                 * old path selector
                System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
                //openFileDlg.SelectedPath = @"C:\Program Files (x86)\Steam\userdata\226359406\760\remote\381210\screenshots";
                openFileDlg.ShowDialog();
                var result = openFileDlg.SelectedPath;
                TextScreenshotPath.Text = result;
                */
            }
            catch(Exception ex){ 
                LogError(ex);}
        }

        /// <summary>
        /// Saves ScreenshotPath and notificationconfig to XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveScreenshotPath(object sender, RoutedEventArgs e){
            try{
                
                var config = new Settings();
                int XMLNotification = Int16.Parse(config.getValue("Notification"));
                string currentXMLpath = config.getValue("ScreenshotPath") + @"\";
                int CurrentAutoRem = Int16.Parse(config.getValue("AutoRem"));

                string screenshotPath = TextScreenshotPath.Text;

                /*
                    * Save Screenshot path and Notification status. set output accordingly
                    * 
                    * 
                    */
                int SaveCounter = 0;

                if (AutoRemChk.IsChecked == true & CurrentAutoRem == 0)
                {
                    config.updateValue("AutoRem", "1");
                    OutputToConsole("Successful submitted pictures will now automatically be deleted.", true);
                }
                else if (AutoRemChk.IsChecked == false & CurrentAutoRem == 1)
                {
                    config.updateValue("AutoRem", "0");
                    OutputToConsole("Successful submitted pictures will no longer be deleted.", true);
                }
                else
                {
                    SaveCounter = SaveCounter + 1;
                }




                if (NotificationCheckbox.IsChecked == true & XMLNotification == 0)
                {
                    config.updateValue("Notification", "1");
                    OutputToConsole("You will now receive Notification.", true);
                    

                }
                else if (NotificationCheckbox.IsChecked == false & XMLNotification == 1)
                {
                    config.updateValue("Notification", "0");
                    OutputToConsole("You will no longer receive Notification.", true);

                }
                else
                {
                    SaveCounter = SaveCounter + 1;
                }



                // Path update
                if (currentXMLpath != screenshotPath)
                {
                    config.updateValue("ScreenshotPath", screenshotPath);
                    createFSW();
                    this.restartRequired = true;
                    Button_SaveSettings.IsEnabled = false;
                    nameassignPath.IsEnabled = false;
                    NotificationCheckbox.IsEnabled = false;
                    OutputToConsole("Path has been updated.", true);
                    var MainWindow = new MainWindow("Path has been updated.");
                    MainWindow.Show();
                    this.Close();
                }
                else
                {
                    SaveCounter = SaveCounter + 1;
                }

                // Display message if nothing is to save
                if (SaveCounter == 3){
                    OutputToConsole("There is nothing to save", true);
                    SaveCounter = 0;
                }


            }
            catch (Exception ex){
            LogError(ex);}
        }





        /// <summary>
        /// Image Upload
        /// </summary>
        /// <param name="file"> filename from OnCreated(from FSW) </param>
        /// <param name="fullPath"> Gets Path from OnCreated</param>
        private async void uploadImage(string file, string fullPath) {
            try{
                OutputToConsole("-----------------------------", true);
                OutputToConsole(file + " wurde erstellt", true);
                string url = "https://zarina.visualstatic.net/api/forms/upload?application=" + application;

                var config = new Settings();
                string token = config.getValue("token");


                // read file into upfilebytes array 

                Console.WriteLine("token: " + token);
                Console.WriteLine("FullPath 2: " + fullPath);
                await Task.Delay(3000);
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



                string time = DateTime.Now.ToString("HH:mm tt");

                // DEBUG
               



                /// await test.Dispatcher.BeginInvoke((Action)(() => test.ScrollToEnd()));
                /// 



                dynamic json = JsonConvert.DeserializeObject(result);
                // Notifcation
                if (json.invalid == "false")
                {
                    if (token != "")
                    {
                        if (String.Compare(token, "0") == 0)
                        {
                            new ToastContentBuilder()
                            .AddText("Public Stats have been updated")
                            .AddText(file + " has been uploaded")
                            .Show();

                            OutputToConsole(time + ":  " + file + " has been submitted", true);
                        }
                        else
                        {
                            new ToastContentBuilder()
                            .AddText("Your personalized Profile Stats have been updated")
                            .AddText(file + " has been uploaded")
                            .Show();

                            OutputToConsole(time + ":  " + file + " has been submitted to your statistics", true);

                        }
                    }
                }




                OutputToConsole(result, true);
                OutputToConsole("-----------------------------", true);





                int notification = Int16.Parse(config.getValue("Notification"));
                int AutoRem = Int16.Parse(config.getValue("AutoRem"));
                string path = config.getValue("ScreenshotPath");


               
                if (AutoRem == 1) { 
                    if (json.invalid == "false"){
                        await Task.Delay(3000);


                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            if(File.Exists(path+ @"\thumbnails\" + file))
                            {
                                File.Delete(path + @"\thumbnails\" + file);
                            }
                        }

                        await Task.Delay(1000);

         
                       OutputToConsole("Screenshot successfully deleted.", true);
                   
                   


                        await Task.Delay(1000);
                        if (File.Exists(fullPath))
                        {
                            OutputToConsole("an error occurred", true);
                            OutputToConsole(fullPath + "couldn't be deleted", true);
                        }
                        if (File.Exists(path + @"\thumbnails\" + file))
                        {
                            OutputToConsole("an error occurred", true);
                            OutputToConsole(path + @"\thumbnails\" + file + "couldn't be deleted", true);
                        }


                    }
                    else{
                            OutputToConsole(" !!! : Picture is not an Endgame Screenshot", true);
                        }


                   
                }
                else
                {
                    /// autorem not enabled
                    Random rnd = new Random();
                    int randomint = rnd.Next(0, 10);
                    this.AutoRemMessage = AutoRemMessage + 1;
                    if (AutoRemMessage >= 2 & randomint == 8)
                    {
                        OutputToConsole("TIP: Make sure to activate auto remove screenshots to save space on your hard drive", true);
                    }
                }


        



                /*
               if (notification == 1)
                                {
                                    new ToastContentBuilder()
                                        .AddArgument("action", "viewConversation")
                                        .AddArgument("conversationId", 9813)
                                        .AddText("Your Stats have been updated")
                                        .AddText(file + " has been uploaded")
                                        .Show();
                                }
               




                  if (token != "")
                {
                    if (String.Compare(token, "0") == 0)
                    {








                */

            }
            catch (Exception ex){
                LogError(ex);}
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
                
                if (String.Compare(token, "0") != 0 ) { 
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
                else if (String.Compare(token, "0") == 0)
                {
                    // Weiterleitung auf Login & Token in XML löschen
                    SettingXML.updateValue("token", "");

                    var LoginScreen = new LoginScreen();
                    LoginScreen.Show();
                    this.Close();
                }
            }
            catch (Exception ex){
                LogError(ex);}
        }


        /// <summary>
        /// Function to display text in Activity log
        /// </summary>
        /// <param name="content"> content to display in activity log</param>
        /// <param name="scroll">scrolls to end if true</param>
        private async void OutputToConsole(String content, bool scroll){
            if (scroll == false){
                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText(content + Environment.NewLine)));
            }
            else if (scroll == true){
                await test.Dispatcher.BeginInvoke((Action)(() => test.AppendText(content + Environment.NewLine)));
                await test.Dispatcher.BeginInvoke((Action)(() => test.ScrollToEnd()));
            }
        }

        /// <summary>
        /// catches every exception and logs it to a file
        /// </summary>
        /// <param name="ex"> handed from any catch Exception</param>
        private void LogError(Exception ex){
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
        private void AutoRemChk_Checked(object sender, RoutedEventArgs e)
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
            
        }
         // Dragon Drop Funktion
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            try{
                if (e.LeftButton == MouseButtonState.Pressed) {
                    DragMove();
                }
            }catch (Exception ex){
                LogError(ex);}
        }

        // Exit Button
        private void ExitProgramm_Click(object sender, RoutedEventArgs e) {
            try{
                this.Close();
            }catch (Exception ex){
                LogError(ex);}
        }

        // Minimize Button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            try{
                WindowState = WindowState.Minimized;
             }catch (Exception ex){
                LogError(ex);}
        }

        private void ButtonLOL_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(this.lol);
            this.lol = this.lol + 1;
            if (this.lol == 10)
            {
                System.Diagnostics.Process.Start("https://youtu.be/dQw4w9WgXcQ");
                this.lol = 0;
            }
        }

      
    }
}
