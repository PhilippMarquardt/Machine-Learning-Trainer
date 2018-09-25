using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für ImageDrawer.xaml
    /// </summary>
    public partial class ImageDrawer : UserControl
    {
        public ImageDrawer()
        {
            InitializeComponent();
          
        }

        

        // This is the rectangle to be shown when mouse is dragged on camera image.
        private Point startPoint;
        private ResizableRectangle rectSelectArea;
        private Point endPoint;

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as MainViewModel).Enabled == false)
            {

                startPoint = e.GetPosition(cnvImage);


                txtLabel.Visibility = Visibility.Collapsed;
                foreach (var q in (this.DataContext as MainViewModel).AllRectangles)
                    q.RectangleMovable = false;

                rectSelectArea = new ResizableRectangle();
                (this.DataContext as MainViewModel).AllRectangles.Add(rectSelectArea);



                Canvas.SetLeft(rectSelectArea, startPoint.X);
                Canvas.SetTop(rectSelectArea, startPoint.Y);
                cnvImage.Children.Add(rectSelectArea);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgCamera_MouseMove(object sender, MouseEventArgs e)
        {
            if ((this.DataContext as MainViewModel).Enabled == false)
            {
                if (e.LeftButton == MouseButtonState.Released || rectSelectArea == null)
                    return;

                var pos = e.GetPosition(cnvImage);

                // Set the position of rectangle
                var x = Math.Min(pos.X, startPoint.X);
                var y = Math.Min(pos.Y, startPoint.Y);

                // Set the dimenssion of the rectangle
                var w = Math.Max(pos.X, startPoint.X) - x;
                var h = Math.Max(pos.Y, startPoint.Y) - y;

                rectSelectArea.RectangleWidth = w;
                rectSelectArea.RectangleHeight = h;

                Canvas.SetLeft(rectSelectArea, x);
                Canvas.SetTop(rectSelectArea, y);

                rectSelectArea.X = x;
                rectSelectArea.Y = y;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as MainViewModel).Enabled == false)
            {

                txtLabel.Visibility = Visibility.Visible;
                txtLabel.Text = "";
                txtLabel.Focus();
                Canvas.SetLeft(txtLabel, rectSelectArea.X + rectSelectArea.RectangleWidth + 5);
                Canvas.SetTop(txtLabel, rectSelectArea.Y - 35);
                foreach (var q in (this.DataContext as MainViewModel).AllRectangles)
                    q.RectangleMovable = true;
                (this.DataContext as MainViewModel).Enabled = true;
                // rectSelectArea = null;
            }
        }

        private void txtLabel_TextChanged(object sender, TextChangedEventArgs e)
        {
            rectSelectArea.Label = txtLabel.Text;
            
        }

        private void btnAddRectangle_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).Enabled = false;
        }
    }
}
