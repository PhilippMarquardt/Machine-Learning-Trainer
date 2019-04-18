using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.ComponentModel;

namespace MachineLearningTrainer.DrawerTool
{
    class ShapeModel
    {
    }

    public class CustomShape : INotifyPropertyChanged
    {
        private string type;
        private double width;
        private double height;
        private double x1;
        private double x2;
        private double xLeft;
        private double y1;
        private double y2;
        private double yTop;
        private int id;
        private double opacity;
        private string fill;
        private string stroke;
        private Boolean isMouseOver;
        private Boolean move;
        private Boolean resize;
        private Point center;


        public CustomShape(double x1, double x2, double y1, double y2, int id)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.y1 = y1;
            this.y2 = y2;
            this.id = id;

            this.xLeft = x1;
            this.yTop = x2;
            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "LawnGreen";

        }
        public CustomShape(double x1, double x2, double y1, double y2)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.y1 = y1;
            this.y2 = y2;

            this.xLeft = x1;
            this.yTop = x2;
            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "Transparent";

        }

        public string Type
        {
            get { return this.type; }
            set
            {
                if (type != value)
                {
                    this.type = value;
                    this.NotifyPropertyChanged("Type");
                }
            }
        }


        public double Width
        {
            get
            {
                width = Math.Abs(this.X1 - this.X2);
                return width;
            }
        }

        public double Height
        {
            get
            {
                height = Math.Abs(this.Y1 - this.Y2);
                return height;
            }
        }

        public double X1
        {
            get { return this.x1; }
            set
            {
                if (x1 != value)
                {
                    this.x1 = value;
                    this.NotifyPropertyChanged("X1");
                    this.NotifyPropertyChanged("Center");
                }
            }
        }

        public double X2
        {
            get { return this.x2; }
            set
            {
                if (x2 != value)
                {
                    this.x2 = value;
                    this.NotifyPropertyChanged("X2");
                    this.NotifyPropertyChanged("Center");
                }
            }
        }

        //Muss immer manuell zugewiesen werden
        public double XLeft
        {
            get { return this.xLeft; }
            set
            {
                if (xLeft != value)
                {
                    this.xLeft = value;
                    this.NotifyPropertyChanged("xLeft");
                }
            }
        }

        public double Y1
        {
            get { return this.y1; }
            set
            {
                if (y1 != value)
                {
                    this.y1 = value;
                    this.NotifyPropertyChanged("Y1");
                    this.NotifyPropertyChanged("Center");
                }
            }
        }

        public double Y2
        {
            get { return this.y2; }
            set
            {
                if (y2 != value)
                {
                    this.y2 = value;
                    this.NotifyPropertyChanged("Y2");
                    this.NotifyPropertyChanged("Center");
                }
            }
        }

        //Muss immer manuell zugewiesen werden
        public double YTop
        {
            get { return this.yTop; }
            set
            {
                if (yTop != value)
                {
                    this.yTop = value;
                    this.NotifyPropertyChanged("YTop");
                }
            }
        }

        public int Id
        {
            get { return this.id; }
        }

        public double Opacity
        {
            get { return this.opacity; }
            set
            {
                if (opacity != value)
                {
                    this.opacity = value;
                    this.NotifyPropertyChanged("Opacity");
                }
            }
        }

        public string Fill
        {
            get { return this.fill; }
            set
            {
                if (fill != value)
                {
                    this.fill = value;
                    this.NotifyPropertyChanged("Fill");
                }
            }
        }

        public string Stroke
        {
            get => stroke;
            set
            {
                if (stroke != value)
                {
                    this.stroke = value;
                    this.NotifyPropertyChanged("Stroke");
                }
            }
        }

        public bool IsMouseOver
        {
            get => isMouseOver;
            set
            {
                if (isMouseOver != value)
                {
                    this.isMouseOver = value;
                    this.NotifyPropertyChanged("IsMouseOver");
                }
            }
        }

        public bool Move
        {
            get => move;
            set
            {
                if (move != value)
                {
                    this.move = value;
                    this.NotifyPropertyChanged("Move");
                }
            }
        }

        public bool Resize
        {
            get => resize;
            set
            {
                if (resize != value)
                {
                    this.resize = value;
                    this.NotifyPropertyChanged("Resize");
                }
            }
        }

        public Point Center
        {
            get
            {
                center.X = x1 + Math.Abs(x1 - x2) / 2;
                center.Y = y1 + Math.Abs(y1 - y2) / 2;
                return center;
            }
            set
            {
                if (center != value)
                {
                    this.center = value;
                    this.x1 = (this.center.X - this.width / 2);
                    this.x2 = (this.center.X + this.width / 2);
                    this.y1 = (this.center.Y - this.height / 2);
                    this.y2 = (this.center.Y + this.height / 2);
                    this.NotifyPropertyChanged("X1");
                    this.NotifyPropertyChanged("X2");
                    this.NotifyPropertyChanged("Y1");
                    this.NotifyPropertyChanged("Y2");
                    this.NotifyPropertyChanged("Center");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
