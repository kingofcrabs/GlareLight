using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        static readonly  Dictionary<Operation, ToggleButton> operation_ButtonControl = new Dictionary<Operation, ToggleButton>();

        List<ShapeBase> shapes = new List<ShapeBase>();
        Brightness brightness = new Brightness();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            myCanvas.onShapeChanged += myCanvas_onShapeChanged;
            InitToggleOperationDict();
        }

        void myCanvas_onShapeChanged(List<ShapeBase> shapes)
        {
            this.shapes = shapes;
            //OperationToggleButtonPressed(Operation.none); //reset
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

        //private void btnTest_Click(object sender, RoutedEventArgs e)
        //{
        //    GlareLight glareLight = new GlareLight();
        //    //glareLight.Test();
        //    if(brightness.vals.Count == 0)
        //    {
        //        SetInfo("No brightness file has been selected!", true);
        //        return;
        //    }

        //    if(polygons.Count == 0)
        //    {
        //        SetInfo("No polygons has been set!", true);
        //        return;
        //    }
        //    double ugr = glareLight.Calculate(brightness.vals, polygons);
        //    SetInfo(string.Format("UGR is: {0}", ugr),false);
        //}
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
            GlareLight glareLight = new GlareLight();
            List<GlareResult> results = new List<GlareResult>();
            var ugr = glareLight.Calculate(brightness.vals, shapes, ref results);

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
            lstviewResult.ItemsSource = tbl.DefaultView;
            txtUGR.Text = ugr.ToString("0.00");
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
            //PseudoColor pesudoColor = new PseudoColor();
            //pesudoColor.Convert(brightness.vals);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.vals);

            myCanvas.SetBkGroundImage(bmpImage);
            //System.Windows.Forms.MessageBox.Show("Read finished!");
        }
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {

        }
        //private void btnAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        myCanvas.Add();
        //    }
        //    catch (Exception ex)
        //    {
        //        SetInfo(ex.Message, true);
        //    }

        //}

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        //Operation GetCurrentOperation()
        //{

        //}

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
            //myCanvas.DeleteLastPoint();
        }

        private void CompletePolygon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void CompletePolygon_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(GetCurrentOperation() != Operation.polygon)
            {
                SetInfo("当前操作对象不是多边形，无法闭合！",true);
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
            //operation_ButtonControl.Add(Operation.move, btnMove);
            operation_ButtonControl.Add(Operation.select, btnSelect);
            return operation_ButtonControl;
        }
        private void OperationToggleButtonPressed(Operation op)
        {
            List<Operation> operations = new List<Operation>(){
                Operation.polygon,Operation.circle,Operation.select
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

        }

        //private void btnMove_Click(object sender, RoutedEventArgs e)
        //{
        //    OperationToggleButtonPressed(Operation.move);
        //}

       
        #endregion

       
    }
}
