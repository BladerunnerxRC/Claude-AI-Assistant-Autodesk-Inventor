using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;

namespace ClaudeForInventor
{
    [Guid("7B3F9A21-C4E8-4D12-B5A6-F0E2D8C3A9B7")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ClaudeForInventor.StandardAddInServer")]
    [ComVisible(true)]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {
        // Shared so ChatForm can call Inventor API
        internal static Inventor.Application? InventorApp { get; private set; }

        private ButtonDefinition?   _chatButton;
        private ChatForm?           _chatForm;
        private DockableWindow?     _dockWin;
        private DockWinNativeHook?  _dockWinHook;
        private ApplicationEvents?  _appEvents;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        // ──────────────────────────────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────────────────────────────

        public void Activate(ApplicationAddInSite addInSite, bool firstTime)
        {
            InventorApp = addInSite.Application;
            CreateButton();
            CreateDockableWindow();
            if (firstTime) AddToRibbons();

            _appEvents = InventorApp.ApplicationEvents;
            _appEvents.OnActivateDocument += OnActivateDocument;
        }

        public void Deactivate()
        {
            _chatForm?.Close();

            _dockWinHook?.ReleaseHandle();
            _dockWinHook = null;
            if (_dockWin != null) { Marshal.ReleaseComObject(_dockWin); _dockWin = null; }

            if (_chatButton != null) _chatButton.OnExecute -= OnOpenChat;

            if (_appEvents != null)
            {
                _appEvents.OnActivateDocument -= OnActivateDocument;
                _appEvents = null;
            }

            if (InventorApp != null)
            {
                Marshal.ReleaseComObject(InventorApp);
                InventorApp = null;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID) { }
        public object Automation => null!;

        // ──────────────────────────────────────────────────────────────────────
        // Ribbon UI
        // ──────────────────────────────────────────────────────────────────────

        private void CreateButton()
        {
            var defs = InventorApp!.CommandManager.ControlDefinitions;
            var icon = MakeIconPicture();

            _chatButton = defs.AddButtonDefinition(
                DisplayName:    "Claude AI",
                InternalName:   "ClaudeAI_OpenChat",
                Classification: CommandTypesEnum.kShapeEditCmdType,
                ClientId:       "{7B3F9A21-C4E8-4D12-B5A6-F0E2D8C3A9B7}",
                DescriptionText: "Open Claude AI design assistant",
                ToolTipText:    "AI-powered Inventor assistant (Claude by Anthropic)",
                StandardIcon:   icon,
                LargeIcon:      icon);

            _chatButton.OnExecute += OnOpenChat;
        }

        private void AddToRibbons()
        {
            // Add a panel under Tools tab in Part, Assembly, and Drawing environments
            foreach (var ribbonId in new[] { "Part", "Assembly", "Drawing" })
            {
                try
                {
                    var ribbon = InventorApp!.UserInterfaceManager.Ribbons[ribbonId];
                    var tab = ribbon.RibbonTabs["id_TabTools"];
                    var panelId = $"ClaudeAI_Panel_{ribbonId}";
                    var panel = tab.RibbonPanels.Add("Claude AI", panelId,
                        "{7B3F9A21-C4E8-4D12-B5A6-F0E2D8C3A9B7}");
                    panel.CommandControls.AddButton(_chatButton, true);
                }
                catch
                {
                    // Panel already exists from a previous session; Inventor
                    // persists ribbon customizations across reloads.
                }
            }
        }

        private void OnOpenChat(NameValueMap context)
        {
            if (_chatForm == null || _chatForm.IsDisposed)
            {
                _chatForm = new ChatForm();

                var dockHwnd = _dockWin != null ? (IntPtr)_dockWin.HWND : IntPtr.Zero;
                if (dockHwnd != IntPtr.Zero)
                {
                    _chatForm.TopLevel = false;          // must be set before handle is created
                    SetParent(_chatForm.Handle, dockHwnd);
                    _chatForm.SetBounds(0, 0, _dockWin!.Width, _dockWin.Height);

                    _dockWinHook?.ReleaseHandle();
                    _dockWinHook = new DockWinNativeHook(dockHwnd, _chatForm);
                }

                _chatForm.Show();
            }

            if (_dockWin != null)
                _dockWin.Visible = true;
            else
                _chatForm.BringToFront();   // fallback: float if DW unavailable

            _chatForm.RefreshContext();
        }

        private void OnActivateDocument(Document documentObject, EventTimingEnum beforeOrAfter,
            NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            handlingCode = HandlingCodeEnum.kEventNotHandled;
            if (beforeOrAfter == EventTimingEnum.kAfter &&
                _chatForm != null && !_chatForm.IsDisposed)
            {
                _chatForm.BeginInvoke((Action)_chatForm.RefreshContext);
            }
        }

        private void CreateDockableWindow()
        {
            try
            {
                _dockWin = InventorApp!.UserInterfaceManager.DockableWindows.Add(
                    "{7B3F9A21-C4E8-4D12-B5A6-F0E2D8C3A9B7}",
                    "ClaudeAI_Panel",
                    "Claude AI");
            }
            catch
            {
                // DockableWindow persists across re-activations in the same session;
                // find the existing instance by InternalName.
                foreach (DockableWindow dw in InventorApp!.UserInterfaceManager.DockableWindows)
                {
                    if (dw.InternalName == "ClaudeAI_Panel") { _dockWin = dw; break; }
                }
                if (_dockWin == null) return;
            }

            _dockWin.ShowTitleBar          = true;
            _dockWin.ShowVisibilityCheckBox = false;
            _dockWin.DisableCloseButton    = false;
            _dockWin.DockingState          = DockingStateEnum.kDockRight;
            _dockWin.Width                 = 540;
            _dockWin.Visible               = false;  // always start hidden; user opens via toolbar
        }

        // ──────────────────────────────────────────────────────────────────────
        // Icon helper – AxHost converts Bitmap → IPictureDisp without P/Invoke
        // ──────────────────────────────────────────────────────────────────────

        private stdole.IPictureDisp MakeIconPicture()
        {
            using var bmp = AppIcon.Make(32);
            return AxHostConverter.GetIPictureDisp(bmp);
        }

        // Subclasses the DockableWindow's Win32 HWND to intercept WM_SIZE so the
        // embedded ChatForm can be kept flush with the panel's client area.
        private sealed class DockWinNativeHook : NativeWindow
        {
            private const int WM_SIZE = 0x0005;
            private readonly ChatForm _form;

            public DockWinNativeHook(IntPtr hwnd, ChatForm form)
            {
                _form = form;
                AssignHandle(hwnd);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);    // let Inventor process first
                if (m.Msg == WM_SIZE && !_form.IsDisposed)
                {
                    int w = (int)(long)m.LParam        & 0xFFFF;
                    int h = ((int)(long)m.LParam >> 16) & 0xFFFF;
                    if (w > 0 && h > 0)
                        _form.SetBounds(0, 0, w, h);
                }
            }
        }

        // Private AxHost subclass – the only legal way to call
        // AxHost.GetIPictureDispFromPicture from user code.
        private sealed class AxHostConverter : AxHost
        {
            private AxHostConverter() : base(string.Empty) { }

            public static stdole.IPictureDisp GetIPictureDisp(System.Drawing.Image img)
                => (stdole.IPictureDisp)GetIPictureDispFromPicture(img);
        }
    }
}
