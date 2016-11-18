using CSScriptIntellisense.Interop;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSScriptIntellisense
{
    class MemberInfoPopupManager : PopupManager<MemberInfoPanel>
    {
        MouseMonitor hook = new MouseMonitor();

        public MemberInfoPopupManager(Action popupRequest)
            : base(popupRequest)
        {
            PopupRequest = popupRequest;
            hook.MouseMove += hook_MouseMove;
            hook.Install();
        }

        public bool Simple
        {
            get
            {
                return popupForm != null && popupForm.Simple;
            }
        }

        const UInt32 WM_SIZE = 0x0005;
        const UInt32 WM_MOVE = 0x0003;

        bool simple = false;
        int? lastMethodStartPos;

        public void TriggerPopup(bool simple, int methodStartPos, string[] data)
        {
            try
            {
                this.simple = simple;
                lastMethodStartPos = methodStartPos;

                base.Popup(
                    form => //on opening
                    {
                        if (!simple)
                            form.LeftBottomCorner = Npp.GetCaretScreenLocation();
                        else
                            form.LeftBottomCorner = Cursor.Position;

                        form.Simple = simple;
                        form.AddData(data);

                        KeyInterceptor.Instance.Add(Keys.Escape);
                        KeyInterceptor.Instance.KeyDown += Instance_KeyDown;

                        if (!simple)
                        {
                            KeyInterceptor.Instance.Add(Keys.Down, Keys.Up, Keys.Escape, Keys.Enter, Keys.Delete, Keys.Back);

                            form.KeyPress += (sender, e) =>
                            {
                                NppEditor.ProcessKeyPress(e.KeyChar);
                                Plugin.OnCharTyped(e.KeyChar);
                                CheckIfNeedsClosing();
                            };

                            form.KeyDown += (sender, e) =>
                            {
                                if (e.KeyCode == Keys.Delete)
                                    NppEditor.ProcessDeleteKeyDown();
                            };

                            form.ProcessMethodOverloadHint(NppEditor.GetMethodOverloadHint(methodStartPos));

                            Task.Factory.StartNew(() =>
                                {
                                    Rectangle rect = Npp.GetClientRect();

                                    while (popupForm != null)
                                    {
                                        try
                                        {
                                            Npp.GrabFocus();
                                            var newRect = Npp.GetClientRect();
                                            if (rect != newRect) //if NPP moved, resized close the popup
                                            {
                                                base.Close();
                                                return;
                                            }
                                            Thread.Sleep(500);
                                        }
                                        catch
                                        {
                                            base.Close();
                                            return;
                                        }
                                    }
                                });
                        }
                    },

                    form => //on closing
                    {
                        KeyInterceptor.Instance.KeyDown -= Instance_KeyDown;
                    });
            }
            catch { }
        }

        void Instance_KeyDown(Keys key, int repeatCount, ref bool handled)
        {
            if (key == Keys.Down || key == Keys.Up || key == Keys.Enter)
            {
                if (popupForm != null)
                    handled = true;
            }

            try
            {
                popupForm.kbdHook_KeyDown(key, repeatCount);
                CheckIfNeedsClosing();
            }
            catch { }
        }

        public void CheckIfNeedsClosing()
        {
            if (IsShowing && !simple && lastMethodStartPos.HasValue)
            {
                int methodStartPos = lastMethodStartPos.Value;

                string text;
                popupForm.ProcessMethodOverloadHint(NppEditor.GetMethodOverloadHint(methodStartPos, out text));

                int currentPos = Npp.GetCaretPosition();
                if (currentPos <= methodStartPos) //user removed/substituted method token as the result of keyboard input
                {
                    base.Close();
                }
                else if (text != null && text[text.Length - 1] == ')')
                {
                    string typedArgs = text;
                    if (NRefactoryExtensions.AreBracketsClosed(typedArgs))
                    {
                        base.Close();
                    }
                }
            }
        }
    }

    //it is generic class though so far it is used only for the MemberInfoPanel
    class PopupManager<T> where T : Form, IPopupForm, new()
    {
        public bool Enabled = false;

        public T popupForm;

        protected Action PopupRequest;
        Point lastScheduledPoint;

        public PopupManager(Action popupRequest)
        {
            PopupRequest = popupRequest;
        }

        bool reqiestIsPending = false;
        object popupRequestSynch = new object();

        public void hook_MouseMove()
        {
            if (Enabled && !Plugin.SuppressCodeTolltips())
            {
                if (lastScheduledPoint != Cursor.Position)
                {
                    if (popupForm == null)
                    {
                        lastScheduledPoint = Cursor.Position;
                        reqiestIsPending = true;
                        Dispatcher.Schedule(700, () =>
                            {
                                lock (popupRequestSynch)
                                {
                                    if (reqiestIsPending)
                                    {
                                        reqiestIsPending = false;
                                        PopupRequest();
                                    }
                                }
                            });
                    }
                    else
                    {
                        if (popupForm.Visible && popupForm.AutoClose)
                        {
                            Close();
                        }
                    }
                }
            }
            else
            {
                Close();
            }
        }

        public void Close()
        {
            try
            {
                if (popupForm != null && popupForm.Visible)
                {
                    popupForm.Close();
                    popupForm = null;
                }
            }
            catch { }
        }

        public void Popup(Action<T> init, Action<T> end)
        {
            if (popupForm != null)
            {
                Close();
                return;
            }

            popupForm = new T();
            popupForm.FormClosed += (sender, e) =>
                                    {
                                        end(popupForm);
                                        popupForm = null;
                                    };
            init(popupForm);

            //popupForm.Show(owner: Npp.NppHandle);//better but still crashes NPP;
            //popupForm.Show(owner: Plugin.GetCurrentScintilla()); //does not work well on Win7-x64; after 3-4 popups just hangs the whole NPP
            popupForm.ShowDialog();
        }

        public bool IsShowing
        {
            get
            {
                return popupForm != null;
            }
        }
    }

    static class FormExtensions
    {
        public static void Show(this Form form, IntPtr owner)
        {
            try
            {
                //has very bad side-effect on Win7-x64 (hangs the NPP)
                var nativeWindow = new NativeWindow();
                nativeWindow.AssignHandle(owner);
                form.Show(nativeWindow);
            }
            catch
            {
            }
        }
    }
}