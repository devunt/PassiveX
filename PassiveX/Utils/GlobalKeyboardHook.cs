using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PassiveX.Utils
{
    internal static class GlobalKeyboardHook
    {
        public delegate void GlobalKeyDownEventHandler(int keyCode);
        public static event GlobalKeyDownEventHandler OnGlobalKeyDown;

        private static readonly IntPtr CurrentModuleHandle;
        private static readonly NativeMethods.LowLevelKeyBoardProc CallbackMethod;
        private static IntPtr _keyboardHookId;

        static GlobalKeyboardHook()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                CurrentModuleHandle = NativeMethods.GetModuleHandle(currentModule.ModuleName);
            }

            CallbackMethod = Callback;
        }

        public static void Install()
        {
            if (_keyboardHookId == IntPtr.Zero)
            {
                _keyboardHookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, CallbackMethod, CurrentModuleHandle, 0);
                Log.I($"Global keyboard hook installed. (ID: {_keyboardHookId})");
            }
        }

        public static void Remove()
        {
            if (_keyboardHookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_keyboardHookId);
                Log.I($"Global keyboard hook removed. (ID: {_keyboardHookId})");
                _keyboardHookId = IntPtr.Zero;
            }
        }

        private static IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WH_KEYDOWN)
            {
                if (OnGlobalKeyDown != null)
                {
                    var keyCode = Marshal.ReadInt32(lParam);
                    OnGlobalKeyDown.Invoke(keyCode);
                }
            }

            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
