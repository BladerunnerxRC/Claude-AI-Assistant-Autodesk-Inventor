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

        private ButtonDefinition? _chatButton;
        private ChatForm? _chatForm;

        // ──────────────────────────────────────────────────────────────────────
        // Lifecycle
        // ──────────────────────────────────────────────────────────────────────

        public void Activate(ApplicationAddInSite addInSite, bool firstTime)
        {
            InventorApp = addInSite.Application;
            CreateButton();
            if (firstTime) AddToRibbons();
        }

        public void Deactivate()
        {
            _chatForm?.Close();
            if (_chatButton != null) _chatButton.OnExecute -= OnOpenChat;

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
                    var ribbon = InventorApp!.UserInterface.Ribbons[ribbonId];
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
                _chatForm = new ChatForm();

            _chatForm.Show();
            _chatForm.BringToFront();
            _chatForm.RefreshContext();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Icon helper – AxHost converts Bitmap → IPictureDisp without P/Invoke
        // ──────────────────────────────────────────────────────────────────────

        private stdole.IPictureDisp MakeIconPicture()
        {
            using var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(204, 75, 55));           // Anthropic coral
                g.FillEllipse(Brushes.White,
                    new RectangleF(4, 4, 24, 24));
                using var f = new Font("Segoe UI", 11f, FontStyle.Bold);
                g.DrawString("C", f, new SolidBrush(Color.FromArgb(204, 75, 55)),
                    new PointF(9f, 7f));
            }
            return AxHostConverter.GetIPictureDisp(bmp);
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
