using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MachineLearningTrainer.DrawerTool
{
    /// <summary>
    /// Interaktionslogik für ResizableRectangle.xaml
    /// </summary>
    public partial class ResizableRectangle : UserControl
    {
        public ResizableRectangle()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        
        public BitmapImage croppedImage = null;

        public static readonly DependencyProperty RectangleHeightProperty =
        DependencyProperty.Register("RectangleHeight", typeof(double), typeof(ResizableRectangle));
        public double RectangleHeight
        {
            get
            {
                return Convert.ToDouble(this.GetValue(RectangleHeightProperty));
            }
            set
            {
                this.SetValue(RectangleHeightProperty, value);
            }
        }

        public static readonly DependencyProperty RectangleMovableProperty =
        DependencyProperty.Register("RectangleMovable", typeof(bool), typeof(ResizableRectangle));
        public bool RectangleMovable
        {
            get
            {
                return (bool)(this.GetValue(RectangleMovableProperty));
            }
            set
            {
                this.SetValue(RectangleMovableProperty, value);
            }
        }

        public static readonly DependencyProperty RectangleWidthProperty =
        DependencyProperty.Register("RectangleWidth", typeof(double), typeof(ResizableRectangle));
        public double RectangleWidth
        {
            get
            {
                return Convert.ToDouble(this.GetValue(RectangleWidthProperty));
            }
            set
            {
                this.SetValue(RectangleWidthProperty, value);
            }
        }


       public static readonly DependencyProperty XProperty =
       DependencyProperty.Register("X", typeof(double), typeof(ResizableRectangle));
        public double X
        {
            get
            {
                return Convert.ToDouble(this.GetValue(XProperty));
            }
            set
            {
                this.SetValue(XProperty, value);
            }
        }

        public static readonly DependencyProperty YProperty =
        DependencyProperty.Register("Y", typeof(double), typeof(ResizableRectangle));
        public double Y
        {
            get
            {
                return Convert.ToDouble(this.GetValue(YProperty));
            }
            set
            {
                this.SetValue(YProperty, value);
            }
        }


        public static readonly DependencyProperty ViktorProperty =
          DependencyProperty.Register("Viktor", typeof(string), typeof(ResizableRectangle));
        public string Viktor
        {
            get
            {
                return Convert.ToString(this.GetValue(ViktorProperty));
            }
            set
            {
                this.SetValue(ViktorProperty, value);
            }
        }

        private static DependencyProperty CroppedImageProperty =
            DependencyProperty.Register("CroppedImage", typeof(BitmapImage), typeof(ResizableRectangle));
        public BitmapImage CroppedImage
        {
            get
            {
                return (BitmapImage)GetValue(CroppedImageProperty);
            }
            set
            {
                SetValue(CroppedImageProperty, value);
            }
        }

        private static DependencyProperty VisibilityChangedProperty =
            DependencyProperty.Register("VisibilityChanged", typeof(bool), typeof(ResizableRectangle));
        public bool VisibilityChanged
        {
            get
            {
                return (bool)GetValue(VisibilityChangedProperty);
            }
            set
            {
                SetValue(VisibilityChangedProperty, value);
            }
        }

        private static DependencyProperty RectangleOpacityProperty =
            DependencyProperty.Register("RectangleOpacity", typeof(double), typeof(ResizableRectangle));
        public double RectangleOpacity
        {
            get
            {
                return (double)GetValue(RectangleOpacityProperty);
            }
            set
            {
                SetValue(RectangleOpacityProperty, value);
            }
        }

        private static DependencyProperty RectangleFillProperty =
            DependencyProperty.Register("RectangleFill", typeof(Brush), typeof(ResizableRectangle));
        public Brush RectangleFill
        {
            get
            {
                return (Brush)GetValue(RectangleFillProperty);
            }
            set
            {
                SetValue(RectangleFillProperty, value);
            }
        }

        private static DependencyProperty ThumbColorProperty =
            DependencyProperty.Register("ThumbColor", typeof(Brush), typeof(ResizableRectangle));
        public Brush ThumbColor
        {
            get
            {
                return (Brush)GetValue(ThumbColorProperty);
            }
            set
            {
                SetValue(ThumbColorProperty, value);
            }
        }

        private static DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register("ThumbSize", typeof(int), typeof(ResizableRectangle));
        public int ThumbSize
        {
            get
            {
                return (int)GetValue(ThumbSizeProperty);
            }
            set
            {
                SetValue(ThumbSizeProperty, value);
            }
        }
    }
}
