using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
    public partial class ImageDrawer : UserControl, INotifyPropertyChanged
    {
        public ImageDrawer()
        {
            InitializeComponent();
        }

        #region Property changed area
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(name));

            }

        }
        #endregion
        
        public ResizableRectangle SelectedResizableRectangle { get; }
        private System.Windows.Point startPoint;
        private ResizableRectangle rectSelectArea;

        
        private void imgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                startPoint = e.GetPosition(cnvImage);

                //txtLabel.Visibility = Visibility.Collapsed;

                rectSelectArea = new ResizableRectangle();
                (this.DataContext as DrawerViewModel).AllRectangles.Add(rectSelectArea);

                Canvas.SetLeft(rectSelectArea, startPoint.X);
                Canvas.SetTop(rectSelectArea, startPoint.Y);
                //cnvImage.Children.Add(rectSelectArea);
            }
            
        }
        
        private void imgCamera_MouseMove(object sender, MouseEventArgs e)
        {

            if ((this.DataContext as DrawerViewModel).Enabled == false)

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

                int recStartX = (Convert.ToInt16(x));
                int recStartY = (Convert.ToInt16(y));
                int recWidth = (Convert.ToInt16(w));
                int recHeight = (Convert.ToInt16(h));

            }

        }

        private string _lastLabel = "Label";
        public string LastLabel
        {
            get
            {
                return _lastLabel;
            }

            set
            {
                _lastLabel = value;
                OnPropertyChanged("LastLabel");
            }
        }

        private void imgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                //txtLabel.Visibility = Visibility.Visible;
                //txtLabel.Text = "";
                //txtLabel.Focus();
                //Canvas.SetLeft(txtLabel, rectSelectArea.X + rectSelectArea.RectangleWidth + 5);
                //Canvas.SetTop(txtLabel, rectSelectArea.Y - 35);
                foreach (var q in (this.DataContext as DrawerViewModel).AllRectangles)
                    q.RectangleMovable = true;
                //imgPreview.Source = rectSelectArea.cropedImage;
                (this.DataContext as DrawerViewModel).Enabled = true;
                // rectSelectArea = null;

                //Bitmap src = new Bitmap(imgPreview.Source);
                BitmapImage bImage = new BitmapImage(new Uri(imgPreview.Source.ToString()));
                Bitmap src;

                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    src = new Bitmap(bitmap);
                }

                Mat mat = SupportCode.convertBmp2Mat(src);
                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rectSelectArea.X, (int)rectSelectArea.Y, (int)rectSelectArea.RectangleWidth, (int)rectSelectArea.RectangleHeight);
                Mat croppedImage = new Mat(mat, rectCrop);

                rectSelectArea.CroppedImage = SupportCode.convertMat2BmpImg(croppedImage);
                //txtLabel.Text = LastLabel;
                //txtLabel.SelectAll();
                //CroppedImagePreview.Source = SupportCode.convertMat2BmpImg(croppedImage);
            }
        }
        

        private void btnAddRectangle_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).Enabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, EventArgs e)
        {
            deleteRectangles();
        }
        
        public void deleteRectangles()
        {
            if (cnvImage.Children.Count>1)
            {
                cnvImage.Children.RemoveAt(cnvImage.Children.Count - 1);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            while (cnvImage.Children.Count > 1)
            cnvImage.Children.RemoveAt(cnvImage.Children.Count-1);
        }

    }
}
