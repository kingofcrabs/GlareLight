using EngineDll;
using GlareCalculator.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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

        //List<ShapeBase> shapes = new List<ShapeBase>();
        Brightness brightness = new Brightness();
        HistogramModel viewModel;
        Point scrollViewOffset;
        EngineDll.IEngine engine = null;
        readonly Point invalidPt = new Point(-1, -1);
        double ratio = 1.0;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            myCanvas.onRoadPlayGroundFinished += myCanvas_onRoadOrPlaygroundFinished;
            InitToggleOperationDict();
            viewModel = new ViewModels.HistogramModel();
            DataContext = viewModel;
            grpShape.IsEnabled = false;
            engine = new EngineDll.IEngine();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            License license = new License();
            string key = Utility.GetKeyString();
            bool bValid = license.CheckRegistCode(key);
            
            GlobalVars.Instance.Registed = bValid;
            if (!bValid)
            {
                this.Title = "软件未注册！";
            }
            tabs.IsEnabled = false;
            myCanvas.IsHitTestVisible = false;
            scrollViewer.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;
            
            
        }
      
        #region scrollview
     

    
        bool IsInvalidPt(Point pt)
        {

            return pt.X - scrollViewer.HorizontalOffset > scrollViewer.ViewportWidth
                || pt.Y - scrollViewer.VerticalOffset > scrollViewer.ViewportHeight;
        }

        void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            if (IsInvalidPt(pt))
                return;
            if (GetCurrentOperation() == Operation.select) //save the original point
                scrollViewOffset = pt;
            myCanvas.LeftMouseDown(pt);
        }

        private void ScrollViewer_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            if (IsInvalidPt(pt))
                return;

            if (GetCurrentOperation() == Operation.select && scrollViewOffset.X != -1) //scroll the viewer
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - (pt.X - scrollViewOffset.X));
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (pt.Y - scrollViewOffset.Y));
            }
            else
                myCanvas.LeftMouseMove(pt, GetCurrentOperation());
        }

        void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(myCanvas);
            if (IsInvalidPt(pt))
                return;
            scrollViewOffset = invalidPt;
            myCanvas.LeftMouseUp(pt);
        }
        #endregion

        #region misc
        void myCanvas_onRoadOrPlaygroundFinished()
        {
            OperationToggleButtonPressed(Operation.none);
        }
        private void SetInfo(string str, bool error)
        {
            Brush brush = error ? Brushes.Red : Brushes.Black;
            txtInfo.Text = str;
            txtInfo.Foreground = brush;
        }
        #endregion

        #region button events

        private void btnSearchRegions_Click(object sender, RoutedEventArgs e)
        {
            List<byte> thresholdData = new List<byte>();
            List<List<MPoint>> contours = new List<List<MPoint>>();
            int val = engine.SearchLights(brightness.grayValsInArray, brightness.Width, brightness.Height,ref contours);
            OperationToggleButtonPressed(Operation.none);
            SetInfo(string.Format("阙值：{0}", val), false);
            myCanvas.SetContours(contours);
        }

        private void lstViewTISelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstviewTIResult.SelectedIndex == -1)
                return;
            int polygonIndex = lstviewTIResult.SelectedIndex;
            myCanvas.Select(polygonIndex);
        }


        private void lstViewUGRSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstviewResult.SelectedIndex == -1)
                return;
            int polygonIndex = lstviewResult.SelectedIndex;
            myCanvas.Select(polygonIndex);
        }
        private void btnCalculateGR_Click(object sender, RoutedEventArgs e)
        {
            if (myCanvas.Shapes.Count == 0)
            {
                SetInfo("未设置任何发光区域！", true);
                return;
            }

            if(!myCanvas.PlayGround.Finished)
            {
                SetInfo("未设置广场区域！", true);
                return;
            }
            SetInfo("正在计算，请稍候！", false);
            myCanvas.SortPolygons();
            this.Refresh();
            OperationToggleButtonPressed(Operation.none); //reset
            GlareLight glareLight = new GlareLight();
            double GR = glareLight.CalculateGR(brightness.orgVals, myCanvas.Shapes, myCanvas.PlayGround);
            txtGR.Text = GR.ToString("0.00");
            SetInfo("", false);
        }

        private void btnCalculateTI_Click(object sender, RoutedEventArgs e)
        {
            if (myCanvas.Shapes.Count == 0)
            {
                SetInfo("未设置任何发光区域！", true);
                return;
            }
            if(!myCanvas.RoadPolygon.Finished)
            {
                SetInfo("未设置任何路面！", true);
                return;
            }
            SetInfo("正在计算，请稍候！", false);
            this.Refresh();
            myCanvas.SortPolygons();
            OperationToggleButtonPressed(Operation.none); //reset
            GlareLight glareLight = new GlareLight();
            double U0 = 0;
            double Lave = 0;
            TIResult tiResult = null;
            double TI = glareLight.CalculateTI(brightness.orgVals, myCanvas.Shapes, myCanvas.RoadPolygon,
                ref U0, ref Lave, ref tiResult);
            txtTI.Text = TI.ToString("0.00");
            txtU0.Text = U0.ToString("0.00");
            
            DataTable tbl = new DataTable("result");
            tbl.Columns.Add("ID", typeof(string));
            tbl.Columns.Add("Ul", typeof(string));
            int ID = 1;
            foreach (var Ul in tiResult.eachLane_UlList)
            {
                string sID = ID.ToString();
                ID++;
                tbl.Rows.Add(sID, Ul.ToString("0.00"));
            }
            lstviewTIResult.ItemsSource = tbl.DefaultView;
            SetInfo("", false);
        }

        private void btnCalculateUGR_Click(object sender, RoutedEventArgs e)
        {
            if(myCanvas.Shapes.Count == 0)
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
            myCanvas.SortPolygons();
            var ugr = glareLight.CalculateUGR(brightness.orgVals, myCanvas.Shapes, ref results, ref LA);

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
            InitUI();
           
        }

        private void InitUI()
        {
            OperationToggleButtonPressed(Operation.none);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            Save2File(bmpImage);
            myCanvas.Width = (int)bmpImage.Width;
            myCanvas.Height = (int)bmpImage.Height;
            colorBar.Height = scrollViewer.Height;
            myCanvas.SetBkGroundImage(bmpImage);
            myCanvas.Shapes.Clear();
            lstviewResult.ItemsSource = null;
            lstviewTIResult.ItemsSource = null;
            txtGR.Text = txtTI.Text = txtUGR.Text = txtU0.Text = "";
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

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ratio *= 1.2;
            Zoom(ratio);
        }

        private void Zoom(double ratio)
        {
            ScaleTransform scaler = new ScaleTransform(ratio, ratio);
            grid.LayoutTransform = scaler;
        }

        private void ZoomIn_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ratio /= 1.2;
            Zoom(ratio);
        }

        private void ZoomOut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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

        private void CreateBoundPolygon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CreateBoundPolygon_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            myCanvas.CreateBoundingPolygon();
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

        bool IsPolygon(Operation op)
        {
            return op == Operation.polygon ||
                 op == Operation.road ||
                 op == Operation.playground;
        }
        private void OnComplete()
        {
            var curOp = GetCurrentOperation();
            if (!IsPolygon(curOp))
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
            operation_ButtonControl.Add(Operation.road, btnRoadDef);
            operation_ButtonControl.Add(Operation.search, btnAutoFind);
            operation_ButtonControl.Add(Operation.playground, btnSetPlayGround);
            return operation_ButtonControl;
        }
        private void OperationToggleButtonPressed(Operation op)
        {
            List<Operation> operations = new List<Operation>();
            foreach(var pair in operation_ButtonControl)
            {
                operations.Add(pair.Key);
            }

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

            //hide or show fake color
            bool isFakeColor = GetCurrentOperation() == Operation.fakeColor;
            int columnSpan = isFakeColor ? 1 : 2;
            Grid.SetColumnSpan(scrollViewer, columnSpan);
            colorBar.SetMinMax(brightness.Min, brightness.Max);
            colorBar.Visibility = isFakeColor ? Visibility.Visible : Visibility.Collapsed;
           
            int laneCnt = 1;
            int ptsPerLane = 5;
            if(op == Operation.road)
            {
                 laneCnt = int.Parse(txtLanes.Text);
                 ptsPerLane = int.Parse(txtPts.Text);
            }
            myCanvas.CreateNewShape(op,laneCnt,ptsPerLane);
        }

      
        private void PseudoOrGrayColor()
        {
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

        private void btnSetPlayGround_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.playground);

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

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.select);
        }

        private void btnRoad_Click(object sender, RoutedEventArgs e)
        {
            OperationToggleButtonPressed(Operation.road);
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
            PseudoOrGrayColor();//process pseudoColor
            SwitchView();
            
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
