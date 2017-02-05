using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GlareCalculator
{
    /// <summary>
    /// RegistInfo.xaml 的交互逻辑
    /// </summary>
    public partial class RegistInfo : Window
    {
        public RegistInfo()
        {
            InitializeComponent();
            License license = new License();
            txtPCInfo.Text = license.GetPCID();
            string sRegCode = license.GetRegistCode();
            //Console.WriteLine(sRegCode);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
