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
        RoadPolygon roadPolygon = new RoadPolygon(1,1);
        Polygon playGround = new Polygon();
        Point invalidPt = new Point(-1,-1);
        bool shouldBlow = false;
        public delegate void RoadFinished();
        public event RoadFinished onRoadPlayGroundFinished;
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

        public Polygon PlayGround
        {
            get
            {
                return playGround;
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
            NotifyRoadPlayGroundFinished();
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
            if(roadPolygon.Finished)
                roadPolygon.Render(drawingContext, shouldBlow);
            if (playGround.Finished)
                playGround.Render(drawingContext, shouldBlow);

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
                roadPolygon.Normalize();
                NotifyRoadPlayGroundFinished();
            }
            else if(CurrentOperation == Operation.playground)
            {
                playGround = (PlayGround)newShape;
                NotifyRoadPlayGroundFinished();
            }
            else
            {
                shapes.Add(newShape);
                CreateNewShape(Operation.polygon);
            }
            InvalidateVisual();
                
        }

        private void CheckIsValidRoad()
        {
            if (((Polygon)newShape).pts.Count != 4)
                throw new Exception("道路必须用梯形画出！");
        }


        bool NoNeed2Draw()
        {
            return CurrentOperation != Operation.circle &&
                CurrentOperation != Operation.polygon &&
                CurrentOperation != Operation.road &&
                CurrentOperation != Operation.playground;
        }
        public void CreateNewShape(Operation operation, int lanes = 1, int ptsPerLane = 1)
        {
            CurrentOperation = operation;
            shapes.ForEach(x => x.Selected = false);
            InvalidateVisual();
            newShape = null;
            if (NoNeed2Draw())
                return;
            if (operation == Operation.polygon)
                newShape = new Polygon();
            else if (operation == Operation.playground)
                newShape = new PlayGround();
            else if (operation == Operation.road)
            {
                roadPolygon = null; //only allow one road
                newShape = new RoadPolygon(lanes, ptsPerLane);
            }
            else
                newShape = new Circle();
        }

        private void NotifyRoadPlayGroundFinished()
        {
            if (onRoadPlayGroundFinished != null)
                onRoadPlayGroundFinished();
        }
        
        internal void LeftMouseDown(Point pt)
        {
            if (NoNeed2Draw())
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
                NotifyRoadPlayGroundFinished();
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

        internal void SortPolygons()
        {
            int ID = 1;
            foreach(var shape in shapes)
            {
                shape.ID = ID.ToString();
                ID++;
            }
        }

        internal void Select(int polygonIndex)
        {
            foreach(var shape in shapes)
            {
                int ID = int.Parse(shape.ID);
                shape.Selected = ID == polygonIndex + 1;
            }
            InvalidateVisual();
        }
    }

   

   
}
