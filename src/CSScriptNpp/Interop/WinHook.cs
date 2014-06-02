using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CSScriptNpp
{
    public class WinHook<T> : LocalWindowsHook, IDisposable where T : new()
    {
        static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }

        protected WinHook()
            : base(HookType.WH_DEBUG)
        {
            m_filterFunc = this.Proc;
        }

        ~WinHook()
        {
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            if (IsInstalled)
                Uninstall();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        protected void Install(HookType type)
        {
            base.m_hookType = type;
            base.Install();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected int Proc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == 0) //Win32.HC_ACTION
                if (HandleHookEvent(wParam, lParam))
                    return 1;

            return CallNextHookEx(m_hhook, code, wParam, lParam);
        }

        virtual protected bool HandleHookEvent(IntPtr wParam, IntPtr lParam)
        {
            throw new NotSupportedException();
        }
    }

    public struct Modifiers
    {
        public bool IsCtrl;
        public bool IsShift;
        public bool IsAlt;
    }

    public partial class KeyInterceptor : WinHook<KeyInterceptor>
    {
        [DllImport("USER32.dll")]
        static extern short GetKeyState(int nVirtKey);

        public static bool IsPressed(Keys key)
        {
            const int KEY_PRESSED = 0x8000;
            return Convert.ToBoolean(GetKeyState((int)key) & KEY_PRESSED);
        }

        public static Modifiers GetModifiers()
        {
            return new Modifiers
            {
                IsCtrl = KeyInterceptor.IsPressed(Keys.ControlKey),
                IsShift = KeyInterceptor.IsPressed(Keys.ShiftKey),
                IsAlt = KeyInterceptor.IsPressed(Keys.Menu)
            };
        }

        public delegate void KeyDownHandler(Keys key, int repeatCount, ref bool handled);

        public List<int> KeysToIntercept = new List<int>();

        public new void Install()
        {
            base.Install(HookType.WH_KEYBOARD);
        }

        public event KeyDownHandler KeyDown;

        public void Add(params Keys[] keys)
        {
            foreach (int key in keys)
                KeysToIntercept.Add(key);
        }

        public void Remove(params Keys[] keys)
        {
            foreach (int key in keys)
            {
                //ignore for now as anyway the extra invoke will not do any harm 
                //but eventually it needs to be ref counting based
                //KeysToIntercept.RemoveAll(k => k == key);
            }
        }

        public const int KF_UP = 0x8000;
        public const long KB_TRANSITION_FLAG = 0x80000000;

        override protected bool HandleHookEvent(IntPtr wParam, IntPtr lParam)
        {
            int key = (int)wParam;
            int context = (int)lParam;

            if (KeysToIntercept.Contains(key))
            {
                bool down = ((context & KB_TRANSITION_FLAG) != KB_TRANSITION_FLAG);
                int repeatCount = (context & 0xFF00);
                if (down && KeyDown != null)
                {
                    bool handled = false;
                    KeyDown((Keys)key, repeatCount, ref handled);
                    return handled;
                }
            }
            return false;
        }
    }

    public partial class KeyInterceptor //CS-Script plugin specific functionality
    {
        public static bool IsShortcutPressed(ShortcutKey key)
        {
            Keys expectedKey = (Keys)key._key;
            bool expectedAlt = (key._isAlt != 0);
            bool expectedCtrl = (key._isCtrl != 0);
            bool expectedShift = (key._isShift != 0);

            if (!KeyInterceptor.IsPressed(expectedKey))
                return false;

            if (KeyInterceptor.IsPressed(Keys.ControlKey) == expectedCtrl)
                return false;

            if (KeyInterceptor.IsPressed(Keys.ShiftKey) == expectedShift)
                return false;

            if (KeyInterceptor.IsPressed(Keys.Menu) == expectedAlt)
                return false;

            return true;
        }
    }
}