using EngineDll;
using GlareCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GlareCalculator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly  Dictionary<Operation, ToggleButton> operation_ButtonControl = new Dictionary<Operation, ToggleButton>();

        List<ShapeBase> shapes = new List<ShapeBase>();
        Brightness brightness = new Brightness();
        HistogramModel viewModel;
        EngineDll.IEngine engine = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            myCanvas.onShapeChanged += myCanvas_onShapeChanged;
            InitToggleOperationDict();
            viewModel = new ViewModels.HistogramModel();
            DataContext = viewModel;
            
            grpShape.IsEnabled = false;
            engine = new EngineDll.IEngine();
        }

        void myCanvas_onShapeChanged(List<ShapeBase> shapes)
        {
            this.shapes = shapes;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            License license = new License();
            string key = Utility.GetKeyString();
            bool bValid = license.CheckRegistCode(key);
            GlobalVars.Instance.Registed = bValid;
            if(!bValid)
            {
                this.Title = "软件未注册！";
            }
            tabs.IsEnabled = false;
            myCanvas.IsHitTestVisible = false;
            scrollViewer.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;
        }

        void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMouseDown(pt);
        }

        private void ScrollViewer_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMouseMove(pt,GetCurrentOperation());
        }

        void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            myCanvas.LeftMouseUp(pt);
        }
      
        private void SetInfo(string str, bool error)
        {
            Brush brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Text = str;
            txtInfo.Foreground = brush;
        }

        
        #region button events

        private void btnSearchRegions_Click(object sender, RoutedEventArgs e)
        {
            List<byte> thresholdData = new List<byte>();
            List<List<MPoint>> contours = new List<List<MPoint>>();
            //int val = engine.AdaptiveThreshold(brightness.grayValsInArray, brightness.Width, brightness.Height, ref thresholdData);
            int val = engine.SearchLights(brightness.grayValsInArray, brightness.Width, brightness.Height,ref contours);
            txtThreshold.Text = val.ToString();
            myCanvas.SetContours(contours);
        }

       
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            //if (lstRegions.SelectedIndex == -1)
            //    return;
            
            //myCanvas.Delete(lstRegions.SelectedIndex);
        }
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.select);
        }

        private void btnCalculateUGR_Click(object sender, RoutedEventArgs e)
        {
            if(shapes.Count == 0)
            {
                SetInfo("未设置任何发光区域！", true);
                return;
            }
            SetInfo("正在计算，请稍候！", false);
            this.Refresh();
            OperationToggleButtonPressed(Operation.none); //reset
            GlareLight glareLight = new GlareLight();
            List<GlareResult> results = new List<GlareResult>();
            double LA = 0;
            var ugr = glareLight.CalculateUGR(brightness.orgVals, shapes, ref results, ref LA);

            DataTable  tbl = new DataTable("result");
            tbl.Columns.Add("ID", typeof(string));
            tbl.Columns.Add("LA", typeof(string));
            tbl.Columns.Add("Omega", typeof(string));
            tbl.Columns.Add("P", typeof(string));

            int ID = 1;
            foreach(var result in results)
            {
                string sID = ID.ToString();
                ID++;
                tbl.Rows.Add(sID,
                    result.La.ToString("0.00"),
                    result.ω.ToString("0.000"),
                    result.P.ToString("0.00"));
            }
            tbl.Rows.Add("Avg",
                   LA.ToString("0.00"),
                   "--",
                   "--");
            lstviewResult.ItemsSource = tbl.DefaultView;
            txtUGR.Text = ugr.ToString("0.00");
            SetInfo("", false);
        }
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OnOpenFile();
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            Configuration configWindow = new Configuration();
            configWindow.Show();
        }
        #endregion

        #region commands

        private void OnOpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnOpenFile();
        }

        private void OnOpenFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "亮度文件(*.txt)|*.txt|所有文件(*.*)|(*.*)";
            if (fileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            
            SetInfo("正在读入亮度文件...", false);
            this.Refresh();
            brightness.Read(fileDialog.FileName);
            SetInfo("成功读入亮度文件。", false);
            tabs.IsEnabled = true;
            grpShape.IsEnabled = true;
            viewModel.Histogram = new List<GrayInfo>();
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            Save2File(bmpImage);
            myCanvas.Width = (int)bmpImage.Width;
            myCanvas.Height = (int)bmpImage.Height;
            myCanvas.SetBkGroundImage(bmpImage);
        }

        private void Save2File(BitmapImage bmpImage)
        {
            string pngFile = brightness.pngFile;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmpImage));
            using (var fileStream = new System.IO.FileStream(pngFile, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private void OnOpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalVars.Instance.Registed;
        }

        private void OnDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var currentOperation = GetCurrentOperation();
            List<Operation> validOperations = new List<Operation>();
            validOperations.Add(Operation.polygon);
            validOperations.Add(Operation.circle);
            validOperations.Add(Operation.select);
            if (!validOperations.Contains(currentOperation))
                return;
            myCanvas.OnDelete();
        }

        private void CompletePolygon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void CompletePolygon_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OnComplete();
            
        }
        private void btnReist_Click(object sender, RoutedEventArgs e)
        {
            RegistInfo registInfo = new RegistInfo();
            registInfo.ShowDialog();
        }


        private void OnComplete()
        {
            if (GetCurrentOperation() != Operation.polygon)
            {
                SetInfo("当前操作对象不是多边形，无法闭合！", true);
                return;
            }
            myCanvas.CompletePolygon();
        }
        #endregion

        #region operations

        private Operation GetCurrentOperation()
        {
            foreach (var pair in operation_ButtonControl)
            {
                if((bool)pair.Value.IsChecked)
                {
                    return pair.Key;
                }
            }
            return Operation.none;
        }

        Dictionary<Operation, ToggleButton> InitToggleOperationDict()
        {
            operation_ButtonControl.Add(Operation.polygon, btnPolygon);
            operation_ButtonControl.Add(Operation.circle, btnCircle);
            operation_ButtonControl.Add(Operation.fakeColor, btnFakeColor);
            operation_ButtonControl.Add(Operation.select, btnSelect);
            operation_ButtonControl.Add(Operation.histogram, btnHistogram);
            return operation_ButtonControl;
        }
        private void OperationToggleButtonPressed(Operation op)
        {
            List<Operation> operations = new List<Operation>(){
                Operation.polygon,Operation.circle,Operation.select,Operation.fakeColor,Operation.histogram
            };
            foreach(Operation tmpOp in operations)
            {
                if (tmpOp == op)
                {
                    if ((bool)!operation_ButtonControl[tmpOp].IsChecked)
                    {
                        op = Operation.none;
                    }
                    continue;
                }
                    
                operation_ButtonControl[tmpOp].IsChecked = false;
            }
            myCanvas.CreateNewShape(op);
        }

        private void btnHistogram_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.histogram);
            viewModel.Histogram = brightness.GetHistogram();
            //viewModel.UpdateHint(plot1);
            SwitchView();
          
        }

        private void SwitchView()
        {
            bool isChecked = (bool)btnHistogram.IsChecked;
            plot1.Visibility = isChecked ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            scrollViewer.Visibility = isChecked ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        private void btnPolygon_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.polygon);
        }

        private void btnCircle_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.circle);
        }

        private void btnFakeColor_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.fakeColor);
            SwitchView();
            BitmapImage bmpImage;
            if ((bool)btnFakeColor.IsChecked)
            {
                bmpImage = ImageHelper.CreateImage(brightness.pngFile);
            }
            else
            {
                bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            }
            myCanvas.SetBkGroundImage(bmpImage);
            
        }
        #endregion

       

       

     

       

        
       

     

       
    }


    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {

            uiElement.Dispatcher.Invoke(DispatcherPriority.ContextIdle, EmptyDelegate);

        }

    }   
    
}
