using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlareCalculator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Polygon> polygons = new List<Polygon>();
        Brightness brightness = new Brightness();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            myCanvas.onPolygonChanged += myCanvas_onPolygonChanged;
            this.MouseDoubleClick += MainWindow_MouseDoubleClick;
        }

        void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        void myCanvas_onPolygonChanged(List<Polygon> newPolygons)
        {
            List<string> strs = new List<string>();
            this.polygons = newPolygons;
            polygons.ForEach(x => strs.Add(Format(x)));
            
        }


        //void myCanvas_onRectChanged(List<Rect> rects)
        //{
        //    List<string> strs = new List<string>();
        //    rects.ForEach(x=>strs.Add(Format(x)));
        //    lstRegions.ItemsSource = strs;
        //    lstRegions.SelectedIndex = strs.Count - 1;
        //}

        private string Format(Polygon polygon)
        {
            int ptsCnt = polygon.pts.Count;
            Point firstPt = polygon.pts[0];
            bool finished = polygon.Finished;
            return string.Format("Pts:{0}, first Pt {1}-{2}, finished: {3}", ptsCnt, firstPt.X,firstPt.Y,finished);
        }
     
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //myCanvas.SetBkGroundImage();
            myCanvas.IsHitTestVisible = false;
            scrollViewer.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
            //scrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;
  
        }

        //private void ScrollViewer_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    Point pt = e.GetPosition(myCanvas);
        //    if(Mouse.LeftButton == MouseButtonState.Pressed)
        //        myCanvas.LeftMouseMove(pt);
        //}

        void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                myCanvas.LeftMouseUp(pt);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "亮度文件(*.txt)|*.txt|所有文件(*.*)|(*.*)";
            if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
           
            brightness.Read(fileDialog.FileName);
            SetInfo("Load brightness file successfully.", false);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.vals);
            
            myCanvas.SetBkGroundImage(bmpImage);
            //System.Windows.Forms.MessageBox.Show("Read finished!");
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                myCanvas.Add();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, true);
            }
            
        }

        private void SetInfo(string str, bool error)
        {
            Brush brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Text = str;
            txtInfo.Foreground = brush;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            GlareLight glareLight = new GlareLight();
            //glareLight.Test();
            if(brightness.vals.Count == 0)
            {
                SetInfo("No brightness file has been selected!", true);
                return;
            }

            if(polygons.Count == 0)
            {
                SetInfo("No polygons has been set!", true);
                return;
            }
            double ugr = glareLight.Calculate(brightness.vals, polygons);
            SetInfo(string.Format("UGR is: {0}", ugr),false);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            //if (lstRegions.SelectedIndex == -1)
            //    return;
            
            //myCanvas.Delete(lstRegions.SelectedIndex);
        }


        #region commands
       

        private void DeleteLastPoint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DeleteLastPoint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myCanvas.DeleteLastPoint();
        }

        private void CompletePolygon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void CompletePolygon_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myCanvas.CompletePolygon();
        }
        #endregion


        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
