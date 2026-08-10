using System.Windows.Forms;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace Project1.Win
{
    public class WinProject1Controller : ViewController
    {
        public WinProject1Controller()
        {
            SimpleAction winButton = new SimpleAction(this, "WinProject1Button", PredefinedCategory.View)
            {
                Caption = "Windows Özel Buton",
                ImageName = "State_Validation_Valid"
            };

            winButton.Execute += WinButton_Execute;
        }

        private void WinButton_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            MessageBox.Show("Bu pencere sadece Windows tarafında çalışır!", "Windows Pop-Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
