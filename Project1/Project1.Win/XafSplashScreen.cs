using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.Skins;
using DevExpress.Utils.Drawing;
using DevExpress.Utils.Svg;
using DevExpress.XtraSplashScreen;

namespace Project1.Win
{
    public partial class XafSplashScreen : SplashScreen
    {
        void LoadBlankLogo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string blankLogoResourceName = assembly.GetName().Name + ".Images.Logo.svg";
            Stream svgStream = assembly.GetManifestResourceStream(blankLogoResourceName);
            if (svgStream != null)
            {
                svgStream.Position = 0;
                peLogo.SvgImage = SvgImage.FromStream(svgStream);
            }
        }
        protected override void DrawContent(GraphicsCache graphicsCache, Skin skin)
        {
            Rectangle bounds = ClientRectangle;
            bounds.Width--; bounds.Height--;
            graphicsCache.Graphics.DrawRectangle(graphicsCache.GetPen(Color.FromArgb(255, 49, 46, 129), 1), bounds);
        }
        protected void UpdateLabelsPosition()
        {
            labelApplicationName.CalcBestSize();
            int newLeft = (Width - labelApplicationName.Width) / 2;
            labelApplicationName.Location = new Point(newLeft, labelApplicationName.Top);
            labelSubtitle.CalcBestSize();
            newLeft = (Width - labelSubtitle.Width) / 2;
            labelSubtitle.Location = new Point(newLeft, labelSubtitle.Top);
        }
        public XafSplashScreen()
        {
            InitializeComponent();
            LoadBlankLogo();
            BackColor = Color.FromArgb(248, 250, 252);
            pcApplicationName.Appearance.BackColor = Color.FromArgb(11, 18, 32);
            labelApplicationName.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            labelApplicationName.Appearance.ForeColor = Color.White;
            labelSubtitle.Appearance.ForeColor = Color.FromArgb(148, 163, 184);
            labelStatus.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
            labelCopyright.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            progressBarControl.Properties.StartColor = Color.FromArgb(20, 184, 166);
            progressBarControl.Properties.EndColor = Color.FromArgb(56, 189, 248);
            progressBarControl.Properties.Appearance.BorderColor = Color.FromArgb(203, 213, 225);

            this.labelApplicationName.Text = "Project1 CRM";
            this.labelSubtitle.Text = "Müşteri, Kişi & Not Yönetim Sistemi";
            this.labelStatus.Text = "Çalışma alanınız hazırlanıyor...";
            this.labelCopyright.Text = "© " + DateTime.Now.Year.ToString() + " Project1 CRM";
            UpdateLabelsPosition();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
            if ((UpdateSplashCommand)cmd == UpdateSplashCommand.Description)
            {
                labelStatus.Text = (string)arg;
            }
        }

        #endregion

    }
}
