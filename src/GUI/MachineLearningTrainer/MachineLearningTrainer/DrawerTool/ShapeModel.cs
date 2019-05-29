using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MachineLearningTrainer.DrawerTool
{
    class ShapeModel
    {
    }

    public class CustomShape : INotifyPropertyChanged
    {
        private string label;
        private ObservableCollection<string> subtypes = new ObservableCollection<string>();
        private double width;
        private double height;
        private double x1;
        private double x2;
        private double xLeft;
        private double xLeftBorder;
        private double y1;
        private double y2;
        private double yTop;
        private double yTopBorder;
        private int id;

        private BitmapImage croppedImage;
        private bool changed = true;

        private double opacity;
        private string fill;
        private string stroke;
        private string _lblTextBox_Color = "Black";
        private bool visible = true;

        private string viewport = ConfigClass.viewportUnSelected;
        private System.Windows.Media.TileMode viewportTileMode = System.Windows.Media.TileMode.None;


        private readonly int strokeThickness = ConfigClass.strokeThickness;
        private Boolean isMouseOver;
        private Boolean move;
        private Boolean resize;
        private Point center;

        /// <summary>
        /// Needed for CustomShape creation init
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="id"></param>
        public CustomShape(double x, double y, int id)
        {
            this.x1 = x;
            this.x2 = x;
            this.y1 = y;
            this.y2 = y;
            this.id = id;
            this.label = "";

            this.xLeft = x1;
            this.xLeftBorder = x1 - strokeThickness;
            this.yTop = y1;
            this.yTopBorder = y1 - strokeThickness;
            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "LawnGreen";
            this.width = 0;
            this.height = 0;

        }
        public CustomShape(double x, double y)
        {
            this.x1 = x;
            this.x2 = x;
            this.y1 = y;
            this.y2 = y;
            this.label = "";

            this.xLeft = x1;
            this.xLeftBorder = x1 - strokeThickness;
            this.yTop = y1;
            this.yTopBorder = y1 - strokeThickness;
            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "LawnGreen";
            this.width = 0;
            this.height = 0;
        }

        public CustomShape(double x1, double y1, double width, double height, int id, ObservableCollection<string> subtypes)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.width = width;
            this.height = height;
            this.id = id;
            this.label = "";

            foreach (var sb in subtypes)
            {
                this.subtypes.Add(sb);
            }

            this.xLeft = x1;
            this.xLeftBorder = x1 - strokeThickness;
            this.yTop = y1;
            this.yTopBorder = y1 - strokeThickness;
            this.x2 = x1 + width;
            this.y2 = y1 + height;

            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "LawnGreen";
        }

        public CustomShape(CustomShape tmp)
        {
            this.x1 = tmp.X1;
            this.x2 = tmp.X2;
            this.xLeft = tmp.XLeft;
            this.xLeftBorder = tmp.XLeftBorder;

            this.y1 = tmp.Y1;
            this.y2 = tmp.Y2;
            this.yTop = tmp.YTop;
            this.yTopBorder = tmp.YTopBorder;

            this.id = tmp.Id;
            this.label = tmp.Label;

            this.width = tmp.width;
            this.height = tmp.height;

            foreach (var sb in tmp.Subtypes)
            {
                this.Subtypes.Add(sb);
            }

            this.opacity = 1;
            this.fill = "Transparent";
            this.isMouseOver = false;
            this.stroke = "LawnGreen";
        }

        public string Label
        {
            get { return this.label; }
            set
            {
                if (label != value)
                {
                    this.label = value;
                    this.NotifyPropertyChanged("Label");
                }
            }
        }

        public ObservableCollection<string> Subtypes
        {
            get => subtypes;
            set
            {
                if (subtypes != value)
                {
                    this.subtypes = value;
                    this.NotifyPropertyChanged("Subtypes");
                }
            }
        }

        #region Informations for Geometrie (Drawing in XAML)

        public double Width
        {
            get => width;
            set
            {
                width = value;
                this.NotifyPropertyChanged("Width");
            }
        }

        public double Height
        {
            get => height;
            set
            {
                height = value;
                this.NotifyPropertyChanged("Height");
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

        public double XLeftBorder
        {
            get { return this.xLeftBorder; }
            set
            {
                if (xLeftBorder != value)
                {
                    this.xLeftBorder = value;
                    this.NotifyPropertyChanged("XLeftBorder");
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

        public double YTopBorder
        {
            get { return yTopBorder; }
            set
            {
                if (yTopBorder != value)
                {
                    this.yTopBorder = value;
                    this.NotifyPropertyChanged("YTopBorder");
                }
            }
        }

        public int Id
        {
            get { return this.id; }
        }

        #endregion

        #region Colorinformations for Drawing

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

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (visible != value)
                {
                    this.visible = value;
                    this.NotifyPropertyChanged("Visible");
                }
            }
        }

        #endregion


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

        public int StrokeThickness
        {
            get => strokeThickness;
        }

        public string lblTextBox_Color
        {
            get
            {
                return _lblTextBox_Color;
            }
            set
            {
                _lblTextBox_Color = value;
                NotifyPropertyChanged("lblTextBox_Color");
            }
        }

        public string Viewport
        {
            get
            {
                return viewport;
            }
            set
            {
                viewport = value;
                NotifyPropertyChanged("Viewport");
            }
        }

        public System.Windows.Media.TileMode ViewportTileMode
        {
            get
            {
                return viewportTileMode;
            }
            set
            {
                viewportTileMode = value;
                NotifyPropertyChanged("ViewportTileMode");
            }
        }

        public BitmapImage CroppedImage
        {
            get => croppedImage;
            set
            {
                if (value != croppedImage)
                {
                    croppedImage = value;
                    NotifyPropertyChanged("CroppedImage");
                }
            }
        }

        public bool Changed
        {
            get => changed;
            set
            {
                if (value != changed)
                {
                    changed = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (changed != true)
                {
                    changed = true;
                }
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
