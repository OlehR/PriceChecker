//-----------------------------------------------------------------------------------
// FILENAME: MainForm.cs
//
// Copyright © 2011 Motorola Solutions, Inc. All rights reserved.
//
// DESCRIPTION: The source file for the MainForm.
//
// ----------------------------------------------------------------------------------
//  
//	This sample demonstrates the usage of the EMDK for .NET API Symbol.Barcode 
//   in order to access the functionality of the barcode scanner. Please note the 
//   fact that this sample covers only the most basic operations associated with 
//   the barcode scanner, so illustrates the usage of only a subset of the complete 
//   API available.
//	
// ----------------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using Symbol.Exceptions;
using System.IO;
using System.Runtime.InteropServices;
using Symbol.Barcode2;
using System.Net;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace PriceChecker
{
    /// <summary>
    /// Class MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {

     
        
        private System.Windows.Forms.Timer timer = null;
        

        //System.EventHandler myActivateHandler = null;
        enum Status
        { On, Off, Starting };
        
        // The constants defined for the string table entries.
        private int CountClick=0;
        private int CountWait = 0;
        private Status StatusScaner = Status.On;

        private API myScanSampleAPI = null;
        private Barcode2.OnScanHandler myScanNotifyHandler = null;
        //private Barcode2.OnStatusHandler myStatusNotifyHandler = null;

        private EventHandler myFormActivatedEventHandler = null;
        private EventHandler myFormDeactivatedEventHandler = null;
        private Label BarCode;
        private TextBox NameText;
        private Label Price;
        private string CodeShop = "000000009";
        private string ServiceUrl = "http://1CSRV/utppsu/ws/ws1.1cws";
        private int TimeOut = 30;
        // The flag to track whether the Barcode object has been initialized or not.
        private bool isBarcodeInitiated = false;
//        PowerNotifications cPowerNotifications = new PowerNotifications();
        public MainForm()
        {

            //Save the current cursor.
            Cursor savedCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

                        //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            ConfigFile cFile = new ConfigFile(null);

            CodeShop = cFile.GetAppSetting("CodeShop");
            ServiceUrl = cFile.GetAppSetting("ServiceUrl");
            TimeOut = Convert.ToInt32(cFile.GetAppSetting("TimeOut"));

            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            this.timer.Enabled=true;
            Cursor.Current = savedCursor;
            
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BarCode = new System.Windows.Forms.Label();
            this.NameText = new System.Windows.Forms.TextBox();
            this.Price = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BarCode
            // 
            this.BarCode.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.BarCode.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Bold);
            this.BarCode.Location = new System.Drawing.Point(44, 8);
            this.BarCode.Name = "BarCode";
            this.BarCode.Size = new System.Drawing.Size(266, 36);
            this.BarCode.TextAlign = System.Drawing.ContentAlignment.TopRight;
            
            // 
            // NameText
            // 
            this.NameText.Enabled = false;
            this.NameText.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular);
            this.NameText.Location = new System.Drawing.Point(5, 47);
            this.NameText.Multiline = true;
            this.NameText.Name = "NameText";
            this.NameText.Size = new System.Drawing.Size(309, 146);
            this.NameText.TabIndex = 1;
            // 
            // Price
            // 
            this.Price.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold);
            this.Price.ForeColor = System.Drawing.SystemColors.Highlight;
            this.Price.Location = new System.Drawing.Point(6, 193);
            this.Price.Name = "Price";
            this.Price.Size = new System.Drawing.Size(304, 37);
            this.Price.Text = "0.00";
            this.Price.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(318, 244);
            this.Controls.Add(this.Price);
            this.Controls.Add(this.NameText);
            this.Controls.Add(this.BarCode);
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "PriceChecker";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            MainForm mainForm = new MainForm();
            Application.Run(mainForm);
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            //this.myActivateHandler = new System.EventHandler(this.listViewMain_ItemActivate);
//            this.listViewMain.ItemActivate += this.myActivateHandler;

            // Add MainMenu if Pocket PC
            if (Symbol.Win32.PlatformType.IndexOf("PocketPC") != -1)
            {
                this.Menu = new MainMenu();
            }

            // Initialize the ScanSampleAPI reference.
            this.myScanSampleAPI = new API();

            this.isBarcodeInitiated = this.myScanSampleAPI.InitBarcode();

            if (!(this.isBarcodeInitiated))// If the Barcode object has not been initialized
            {
                // Display a message & exit the application.
                MessageBox.Show(Resources.GetString("AppExitMsg"));
                Application.Exit();
            }
            else // If the Barcode object has been initialized
            {
                // Clear the statusbar where subsequent status information would be displayed.
                //statusBar.Text = "";

                // Attach a scan notification handler.
                this.myScanNotifyHandler = new Barcode2.OnScanHandler(myBarcode2_ScanNotify);
                myScanSampleAPI.AttachScanNotify(myScanNotifyHandler);

                // Attach a status notification handler.
                //this.myStatusNotifyHandler = new Barcode2.OnStatusHandler(myBarcode2_StatusNotify);
                //myScanSampleAPI.AttachStatusNotify(myStatusNotifyHandler);
            }

            

            // Ensure that the keyboard focus is set on a control.
            
            myFormActivatedEventHandler = new EventHandler(MainForm_Activated);
            myFormDeactivatedEventHandler = new EventHandler(MainForm_Deactivate);
            this.Activated += myFormActivatedEventHandler;
            this.Deactivate += myFormDeactivatedEventHandler;
            FullScreen.StartFullScreen(this);
        }

        [DllImport("Coredll.dll")]
        extern static void GwesPowerOffSystem();


        const string BASE_POWER_HIVE = @"System\CurrentControlSet\Control\Power\State";
        string[] _powerStateNames = {"Full Power","Power Savings","Standby","Sleep Mode","Power Off"};

        Regex _targetRegistryValue = new Regex("(DEFAULT)|(^.*:$)", RegexOptions.IgnoreCase);
       
        string[] GetPowerStateList()
        {
            RegistryKey powerStateKey = Registry.LocalMachine.OpenSubKey(BASE_POWER_HIVE);
            return powerStateKey.GetSubKeyNames();
        }


        string[][] GetPowerStateInfo(string stateName)
        {
            RegistryKey stateInformationKey = Registry.LocalMachine.OpenSubKey(String.Format(@"{0}\{1}", BASE_POWER_HIVE, stateName));
            string[] valueList = stateInformationKey.GetValueNames();
            List<string[]> StateInfo = new List<string[]>();
            for (int i = 0; i < valueList.Length; ++i)
            {
                string currentValue = valueList[i];
                if (_targetRegistryValue.IsMatch(currentValue))
                {
                    StateInfo.Add(new string[] { valueList[i], _powerStateNames[(int)stateInformationKey.GetValue(currentValue)] });
                }
            }
            return StateInfo.ToArray();
        }

        public enum CEDEVICE_POWER_STATE : int
        {
            PwrDeviceUnspecified = -1,
            //Full On: full power,  full functionality
            D0 = 0,
            /// <summary>
            /// Low Power On: fully functional at low power/performance
            /// </summary>
            D1 = 1,
            /// <summary>
            /// Standby: partially powered with automatic wake
            /// </summary>
            D2 = 2,
            /// <summary>
            /// Sleep: partially powered with device initiated wake
            /// </summary>
            D3 = 3,
            /// <summary>
            /// Off: unpowered
            /// </summary>
            D4 = 4,
            PwrDeviceMaximum
        }
        [Flags()]
        public enum DevicePowerFlags
        {
            None = 0,
            /// <summary>
            /// Specifies the name of the device whose power should be maintained at or above the DeviceState level.
            /// </summary>
            POWER_NAME = 0x00000001,
            /// <summary>
            /// Indicates that the requirement should be enforced even during a system suspend.
            /// </summary>
            POWER_FORCE = 0x00001000,
            POWER_DUMPDW = 0x00002000
        }



        [DllImport("CoreDLL")]
        public static extern int GetDevicePower(string device, DevicePowerFlags flags, out CEDEVICE_POWER_STATE PowerState);



             /// <summary>
        /// Stop the timer and reset index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            CEDEVICE_POWER_STATE currentPowerState;

            GetDevicePower("Display", DevicePowerFlags.POWER_NAME, out currentPowerState);
            CountWait++;
            if (StatusScaner == Status.On)
            {                
                if (CountWait % 5 == 0)
                    CountClick = 0;
            }

            if (StatusScaner == Status.On)
            {
                if (CountWait>0 && CountWait % 15 == 0)
                {
                    BarCode.Text = "";
                    NameText.Text="Піднесіть товар для сканування";
                    Price.Text = "";
                }

            }

            if (StatusScaner == Status.Starting)
            {
                if (CountWait <= 5)
                    NameText.Text = "Йде підготовка обладнання=>" + (5 - CountWait).ToString();
                else
                {
                    StatusScaner = Status.On;
                    myScanSampleAPI.StartScan(true);
                    NameText.Text = "Піднесіть товар для сканування";
                    CountWait = 0;
                }

            }
            //var r = GetPowerStateInfo("Suspend");
            if (CountWait>0 && (CountWait % TimeOut) == 0)
            {
                myScanSampleAPI.StopScan();
                GwesPowerOffSystem();
                //CountWait = 0;
                StatusScaner = Status.Off;
                NameText.Text = "Для розблокування натисніть клавішу";
            }


            
        }

        /// <summary>
        /// Go to the selected item and expand it if possible
        /// </summary>
       

        /// <summary>
        /// The handler called when resizing MainForm. The UI is re-calculated
        /// and adjusted based on the dimentions of the screen.
        /// </summary>
        private void MainForm_Resize(object sender, System.EventArgs e)
        {
           

        }

        /// <summary>
        /// Adjust the listViewMain dimensions, mainly the column widths.
        /// </summary>
     
        /// <summary>
        /// Read notification handler.
        /// </summary>
        private void myBarcode2_ScanNotify(ScanDataCollection scanDataCollection)
        {
            // Get ScanData
            ScanData scanData = scanDataCollection.GetFirst;

            switch (scanData.Result)
            {
                case Results.SUCCESS:

             
                 // Handle the data from this read & submit the next read.
                   
                   this.BarCode.Text = scanData.Text;
                   this.BarCode.Refresh(); 
                   try
                   {
                     var res= Post(ServiceUrl, @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
<soap:Body><GetInfoForScancode xmlns=""vopak"">
<Scancode>" + scanData.Text + @"</Scancode>
<CodeOfShop>" +CodeShop+@"</CodeOfShop>
</GetInfoForScancode>
</soap:Body>
</soap:Envelope>");
                     res = res.Substring(res.IndexOf(@"-instance"">") + 11);
                     res = res.Substring(0,res.IndexOf("</m:return>"));   
                     var varRes = res.Split(new char[]{';'});
                     NameText.Text= varRes.Length>0 ? varRes[0] :"";
                     NameText.Refresh();
                     Price.Text = varRes.Length > 1 ? varRes[1] : "0.00";
                     Price.Refresh();
                     CountWait = 0;
                   }
                   catch(Exception ex)
                   {
                       NameText.Text = ex.Message;
                   }
                   System.Threading.Thread.Sleep(1000);
                   this.myScanSampleAPI.StartScan(true);
                    break;

                case Results.E_SCN_READTIMEOUT:


                        this.myScanSampleAPI.StartScan(true);
                    break;

                case Results.CANCELED:

                    break;

                case Results.E_SCN_DEVICEFAILURE:

                    this.myScanSampleAPI.StopScan();
                    this.myScanSampleAPI.StartScan(true);
                    break;

                default:

                    string sMsg = "Read Failed\n"
                        + "Result = "
                        + (scanData.Result).ToString();

                    

                    if (scanData.Result == Results.E_SCN_READINCOMPATIBLE)
                    {
                        // If the failure is E_SCN_READINCOMPATIBLE, exit the application.

                        MessageBox.Show(Resources.GetString("AppExitMsg"), Resources.GetString("Failure"));

                        this.Close();
                        return;
                    }

                    break;
            }
        }





        private void MainForm_Activated(object sender, EventArgs e)
        {

            this.myScanSampleAPI.StartScan(true);
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
                myScanSampleAPI.StopScan();

        }


        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FullScreen.StopFullScreen(this);

            if (isBarcodeInitiated)
            {
                myScanSampleAPI.TermBarcode();


                //Remove Form event handlers
                this.Activated -= myFormActivatedEventHandler;
                this.Deactivate -= myFormDeactivatedEventHandler;
            }
        }
        /*
        private const int LVM_GETITEMPOSITION = (0x1010);
        private const int LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1037;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036;
        private const int LVS_EX_GRIDLINES = 0x1;



        [DllImport("coredll.dll")]
        private static extern int SendMessageW(int hWnd, uint wMsg, int wParam, ref Point lParam);

        [DllImport("coredll.dll")]
        private static extern int SendMessageW(int hWnd, int wMsg, int wParam, int lParam);
        */
        private string Post(string Url, string varData)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);


            var data = Encoding.ASCII.GetBytes(varData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = varData.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            var response = (HttpWebResponse)request.GetResponse();


            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();

            return responseString;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (StatusScaner == Status.Off)
            {
                MainForm_KeyDown(null, null);
            }
            if (StatusScaner == Status.On)
            {
                CountClick++;
                if (CountClick > 4)
                    this.Close();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (StatusScaner == Status.Off)
            {
                StatusScaner = Status.Starting;
                CountWait = 0;
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            MainForm_KeyDown(sender, null);
        }

        


    }


   

}
