using System.Windows;

namespace GlareCalculator
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        public Configuration()
        {
            InitializeComponent();
            this.Loaded += Configuration_Loaded;
        }

        void Configuration_Loaded(object sender, RoutedEventArgs e)
        {
            cmbLen.SelectedIndex = GlobalVars.Instance.UserSettings.Focus == 6 ? 0 : 1;
            cmbPixel.SelectedIndex = GlobalVars.Instance.UserSettings.PixelLength == 2.2 ? 0 : 1;
            txtAge.Text = GlobalVars.Instance.UserSettings.Age.ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int len = cmbLen.SelectedIndex == 0 ? 6 : 8;
            double pixel = cmbPixel.SelectedIndex == 0? 2.2 : 5.5;
            int age = int.Parse(txtAge.Text);

            GlobalVars.Instance.UserSettings = new UserSettings(len,pixel,age);
            string settingFile = Utility.GetExeFolder() + "settingInfo.xml";
            Utility.SaveSettings(GlobalVars.Instance.UserSettings, settingFile);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
