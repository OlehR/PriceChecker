using System;
//using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;


[Flags()]
public enum FullScreenFlags : int
{
    SwHide = 0,
    ShowTaskbar = 0x1,
    HideTaskbar = 0x2,
    ShowSipButton = 0x4,
    HideSipButton = 0x8,
    SwRestore = 9,
    ShowStartIcon = 0x10,
    HideStartIcon = 0x20

}
static class FullScreen
{

    #region Win32 API Calls

    /// <summary>
    /// The GetCapture function retrieves a handle to the window 
    /// </summary>
    /// <returns></returns>
    [DllImport("coredll.dll")]
    private static extern IntPtr GetCapture();

    /// <summary>
    /// The SetCapture function sets the mouse capture to
    /// the specified window belonging to the current thread
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    [DllImport("coredll.dll")]
    private static extern IntPtr SetCapture(IntPtr hWnd);

    /// <summary>
    /// This function can be used to take over certain areas of the screen
    ///  It is used to modify the taskbar, Input Panel button,
    /// or Start menu icon.
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    [DllImport("aygshell.dll", SetLastError = true)]
    private static extern bool SHFullScreen(IntPtr hwnd, int state);

    /// <summary>
    /// The function retrieves the handle to the top-level 
    /// window whose class name and window name match 
    /// the specified strings. This function does not search child windows.
    /// </summary>
    /// <param name="lpClass"></param>
    /// <param name="lpWindow"></param>
    /// <returns></returns>
    [DllImport("coredll.dll", SetLastError = true)]
    private static extern IntPtr FindWindowW(string lpClass, string
    lpWindow);

    /// <summary>
    /// changes the position and dimensions of the specified window
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="l"></param>
    /// <param name="repaint"></param>
    /// <returns></returns>
    [DllImport("coredll.dll", SetLastError = true)]
    private static extern IntPtr MoveWindow(IntPtr hwnd, int x, int y, int
    w, int l, int repaint);
    #endregion

    #region General Methods

    /// <summary>
    /// obtain the window handle of a .Net window or control
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static IntPtr GetHWnd(Control c)
    {
        IntPtr hOldWnd = GetCapture();
        c.Capture = true;
        IntPtr hWnd = GetCapture();
        c.Capture = false;
        SetCapture(hOldWnd);
        return hWnd;



    }

    /// <summary>
    /// Set Full Screen Mode
    /// </summary>
    /// <param name="form"></param>
    public static void StartFullScreen(Form form)
    {
        //if not Pocket Pc platform
        if (!Platform.PlatformDetection.IsPocketPC())
        {
            //Set Full Screen For Windows CE Device

            //Normalize windows state
            form.WindowState = FormWindowState.Normal;

            IntPtr iptr = form.Handle;
            SHFullScreen(iptr, (int)FullScreenFlags.HideStartIcon);

            //detect taskbar height
            int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;

            // move the viewing window north taskbar height to get rid of the command 
            //bar 
            MoveWindow(iptr, 0, -taskbarHeight, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height + taskbarHeight, 1);


            // move the task bar south taskbar height so that its not visible anylonger 
            IntPtr iptrTB = FindWindowW("HHTaskBar", null);
            MoveWindow(iptrTB, 0, Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width, taskbarHeight, 1);
        }
        else //pocket pc platform
        {
            //Set Full Screen For Pocket Pc Device
            form.Menu = null;
            form.ControlBox = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.WindowState = FormWindowState.Maximized;
            form.Text = string.Empty;
        }

    }


    /// <summary>
    /// Stop Full Screen Mode
    /// </summary>
    /// <param name="form"></param>
    public static void StopFullScreen(Form form)
    {
        //if windows ce return window and taskbar to his original place
        if (!Platform.PlatformDetection.IsPocketPC())
        {
            IntPtr iptr = form.Handle;

            SHFullScreen(iptr, (int)FullScreenFlags.ShowStartIcon);

            //detect taskbar height
            int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;

            MoveWindow(iptr, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - taskbarHeight, 1);

            IntPtr iptrTB = FindWindowW("HHTaskBar", null);
            MoveWindow(iptrTB, 0, Screen.PrimaryScreen.Bounds.Height - taskbarHeight, Screen.PrimaryScreen.Bounds.Width, taskbarHeight, 1);

        }

    }
}

    



    #endregion



namespace Platform
{
    internal partial class PInvoke
    {
        #region Invoke  and Enums
        /// <summary>
        /// to find whether you are running on a Smartphone or a Pocket PC
        /// </summary>
        /// <param name="uiAction"></param>
        /// <param name="uiParam"></param>
        /// <param name="pvParam"></param>
        /// <param name="fWinIni"></param>
        /// <returns></returns>
        [DllImport("Coredll.dll", EntryPoint = "SystemParametersInfoW", CharSet = CharSet.Unicode)]
        static extern int SystemParametersInfo4Strings(uint uiAction, uint uiParam, StringBuilder pvParam, uint fWinIni);


        public enum SystemParametersInfoActions : uint
        {
            SPI_GETPLATFORMTYPE = 257, // this is used elsewhere for Smartphone/PocketPC detection

        }
        #endregion

        /// <summary>
        /// Get Platform Type: Pocket Pc or Windows CE
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformType()
        {
            StringBuilder platformType = new StringBuilder(50);
            if (SystemParametersInfo4Strings((uint)SystemParametersInfoActions.SPI_GETPLATFORMTYPE,
                (uint)platformType.Capacity, platformType, 0) == 0)
                throw new Exception("Error getting platform type.");
            return platformType.ToString();
        }
    }

    /// <summary>
    /// Platform Detections methods
    /// </summary>
    internal partial class PlatformDetection
    {
        public static bool IsSmartphone()
        {
            return PInvoke.GetPlatformType() == "SmartPhone";
        }
        public static bool IsPocketPC()
        {
            return PInvoke.GetPlatformType() == "PocketPC";
        }
    }


}
