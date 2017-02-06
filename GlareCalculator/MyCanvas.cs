using EngineDll;
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
        RoadPolygon roadPolygon = new RoadPolygon();
        Point invalidPt = new Point(-1,-1);
        bool shouldBlow = false;
        public delegate void RoadFinished();
        public event RoadFinished onRoadPolygonFinished;
        private Timer timer = new Timer(500);

        public MyCanvas()
        {
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        public List<ShapeBase> Shapes
        {
            get
            {
                return shapes;
            }
        }

        public RoadPolygon RoadPolygon
        {
            get
            {
                return roadPolygon;
            }
        }

        public void SetContours(List<List<MPoint>> contours)
        {
            shapes.Clear();
            foreach(var pts in contours)
            {
                Polygon polygon = new Polygon(pts);
                shapes.Add(polygon);
            }
            NotifyRoadFinished();
            InvalidateVisual();
        }

        public void SetBkGroundImage(BitmapImage bmpImage)
        {
            shapes.Clear();
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
            if(roadPolygon != null)
                roadPolygon.Render(drawingContext, shouldBlow);
        }



        internal void OnDelete()
        {
            if(newShape != null) //operate on newShape
            {
                OnDelete(newShape);
                return;
            }
            var selectedShapes = shapes.Where(x => x.Selected);
            if (selectedShapes.Count() == 0)
                return;
            OnDelete(selectedShapes.First());
        }

        private void OnDelete(ShapeBase shapeBase)
        {
            if (shapeBase.Finished || shapeBase is Circle)
            {
                if (shapeBase == newShape)
                    newShape = null;
                else
                {
                    shapes.Remove(shapeBase);
                }
                
            }
            else
            {
                Polygon polygon = (Polygon)shapeBase;
                if (polygon.pts.Count > 0)
                {
                    polygon.pts.Remove(polygon.pts.Last());
                }
            }
            InvalidateVisual();



        }

        internal void CompletePolygon()
        {
            if (!(newShape is Polygon))
                throw new Exception("当前形状不是多边形！");
            
            ((Polygon)newShape).Enclose();
            if (CurrentOperation == Operation.road)
            {
                CheckIsValidRoad();
                roadPolygon = (RoadPolygon)newShape;
                NotifyRoadFinished();
            }
            else
            {
                shapes.Add(newShape);
                InvalidateVisual();
                CreateNewShape(Operation.polygon);
            }
            
                
        }

        private void CheckIsValidRoad()
        {
            if (((Polygon)newShape).pts.Count != 4)
                throw new Exception("道路必须用梯形画出！");
        }

        public void CreateNewShape(Operation operation)
        {
            CurrentOperation = operation;
            shapes.ForEach(x => x.Selected = false);
            InvalidateVisual();
            newShape = null;
            if (operation != Operation.circle && operation != Operation.polygon && operation != Operation.road)
                return;
            if(operation == Operation.polygon)
                newShape = new Polygon();
            else if(operation == Operation.road)
            {
                roadPolygon = null; //only allow one road
                newShape = new RoadPolygon();
            }
            else
                newShape = new Circle();
        }

        private void NotifyRoadFinished()
        {
            if (onRoadPolygonFinished != null)
                onRoadPolygonFinished();
        }
        
        internal void LeftMouseDown(Point pt)
        {
            if (CurrentOperation != Operation.circle && CurrentOperation != Operation.polygon && CurrentOperation != Operation.road)
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
                if(shape.PtIsInside(pt))
                {
                    newShape = null;
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
                NotifyRoadFinished();
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
