using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace GlareCalculator
{
    public class MyCanvas : Canvas
    {

        List<ShapeBase> shapes = new List<ShapeBase>();
        ShapeBase newShape = null;

        Point invalidPt = new Point(-1,-1);
        bool shouldBlow = false;
        public delegate void ShapeChanged(List<ShapeBase> shapes);
        public event ShapeChanged onShapeChanged;
        private Timer timer = new Timer(500);

        public MyCanvas()
        {
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        

        public void SetBkGroundImage(BitmapImage bmpImage)
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bmpImage;
            this.Background = imageBrush;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            shouldBlow = !shouldBlow;
            this.Dispatcher.Invoke(new System.Action(() =>
            {
                InvalidateVisual();
            }));
        }
       

        internal static bool IsInDesignMode()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio");
        }
      

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (IsInDesignMode())
                return;
            if(newShape != null)
                newShape.Render(drawingContext, shouldBlow);
            var grayBrush = Brushes.Gray;
            shapes.ForEach(x => x.Render(drawingContext, shouldBlow));
        }


        internal void CompletePolygon()
        {
            if (!(newShape is Polygon))
                throw new Exception("当前形状不是多边形！");
            ((Polygon)newShape).Enclose();
            shapes.Add(newShape);
            InvalidateVisual();
            CreateNewShape(Operation.polygon);
            NotifyShapeChanged();
        }

        public void CreateNewShape(Operation operation)
        {
            CurrentOperation = operation;
            shapes.ForEach(x => x.Selected = false);
            InvalidateVisual();
            newShape = null;
            if (operation != Operation.circle && operation != Operation.polygon)
                return;
            if(operation == Operation.polygon)
                newShape = new Polygon();
            else
                newShape = new Circle();
        }

        private void NotifyShapeChanged()
        {
            if (onShapeChanged != null)
                onShapeChanged(shapes);
        }
        
        internal void LeftMouseDown(Point pt)
        {
            if (CurrentOperation == Operation.none || CurrentOperation == Operation.select)
                return;

           
            if (newShape.Finished)
                return;
            newShape.OnLeftMouseDown(pt);
            InvalidateVisual();
        }

        private void SelectShape(Point pt)
        {
            shapes.ForEach(x => x.Selected = false);
            foreach(var shape in shapes)
            {
                if(shape.IsInside(pt))
                {
                    shape.Selected = true;
                    break;
                }
            }
            InvalidateVisual();
        }

        internal void LeftMouseUp(Point pt) //add pt to polygon or circle
        {
            if (CurrentOperation == Operation.select)
            {
                SelectShape(pt);
                return;
            }

            if (newShape == null)
                return;
            if (newShape.Finished)
                return;
          
            newShape.OnLeftMouseUp(pt);
            if (newShape is Circle) //create a new circle
            {
                shapes.Add(newShape);
                CreateNewShape(Operation.circle);
                NotifyShapeChanged();
            }
            InvalidateVisual();
        }

        internal void LeftMouseMove(Point pt, Operation operation)
        {
            if (newShape == null)
                return;
        
            newShape.OnMouseMove(pt);
            InvalidateVisual();
        }

        Operation currentOp;
        internal Operation CurrentOperation
        { 
            get
            {
                return currentOp;
            }
            set
            {
                currentOp = value;
               
            }
        }

       
    }

   

   
}
