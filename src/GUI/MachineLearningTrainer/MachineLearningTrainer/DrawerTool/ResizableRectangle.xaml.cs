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

        public ResizableRectangle(string label)
        {
            this.Label = label;
        }
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


        public static readonly DependencyProperty LabelProperty =
          DependencyProperty.Register("Label", typeof(string), typeof(ResizableRectangle));
        public string Label
        {
            get
            {
                return Convert.ToString(this.GetValue(LabelProperty));
            }
            set
            {
                this.SetValue(LabelProperty, value);
            }
        }
    }
}
