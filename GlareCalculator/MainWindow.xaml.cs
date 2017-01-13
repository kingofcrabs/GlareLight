using GlareCalculator.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            myCanvas.onShapeChanged += myCanvas_onShapeChanged;
            InitToggleOperationDict();
            viewModel = new ViewModels.HistogramModel();
            DataContext = viewModel;
        }

        void myCanvas_onShapeChanged(List<ShapeBase> shapes)
        {
            this.shapes = shapes;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //myCanvas.SetBkGroundImage();
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
            OperationToggleButtonPressed(Operation.none); //reset
            GlareLight glareLight = new GlareLight();
            List<GlareResult> results = new List<GlareResult>();
            double LA = 0;
            var ugr = glareLight.Calculate(brightness.orgVals, shapes, ref results, ref LA);

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

            brightness.Read(fileDialog.FileName);
            SetInfo("Load brightness file successfully.", false);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            myCanvas.SetBkGroundImage(bmpImage);
        }

        private void OnOpenFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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
            return operation_ButtonControl;
        }
        private void OperationToggleButtonPressed(Operation op)
        {
            List<Operation> operations = new List<Operation>(){
                Operation.polygon,Operation.circle,Operation.select,Operation.fakeColor
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
            bool isChecked = (bool)btnHistogram.IsChecked;

            oxyplot.Visibility = isChecked ?System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
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
            BitmapImage bmpImage;
            if ((bool)btnFakeColor.IsChecked)
            {
                bmpImage = ImageHelper.CreateImage(brightness.colorVals);
            }
            else
            {
                bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            }
            myCanvas.SetBkGroundImage(bmpImage);
            OperationToggleButtonPressed(Operation.fakeColor);
        }
        #endregion
    }
}
