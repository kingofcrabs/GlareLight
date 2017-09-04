using System.Windows;
using System.Windows.Controls;

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
            cmbLen.SelectedIndex =  1;
            string wantedContent = string.Format("{0}mm",GlobalVars.Instance.UserSettings.Focus);
            foreach(ComboBoxItem item in cmbLen.Items)
            {
                string curItem = item.Content.ToString();
                if (curItem == wantedContent)
                {
                    cmbLen.SelectedItem = item;
                    break;
                }

            }
            
            cmbPixel.SelectedIndex = GlobalVars.Instance.UserSettings.PixelLength == 2.2 ? 0 : 1;
            txtAge.Text = GlobalVars.Instance.UserSettings.Age.ToString();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int len = int.Parse(cmbLen.Text.Replace("mm", ""));
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
