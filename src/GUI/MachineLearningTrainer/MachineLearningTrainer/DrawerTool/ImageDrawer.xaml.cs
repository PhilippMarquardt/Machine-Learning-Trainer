using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml;

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
            cnvImage.Cursor = Cursors.Cross;
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

        public void ListBox_RightButtonUp(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).IsOpen = true;
        }
        private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                startPoint = e.GetPosition(cnvImage);

                //txtLabel.Visibility = Visibility.Collapsed;

                rectSelectArea = new ResizableRectangle();

                if((this.DataContext as DrawerViewModel).SelectedComboBoxItem == "All Labels")
                {
                    (this.DataContext as DrawerViewModel).AllRectanglesView.Add(rectSelectArea);
                }

                else
                {
                    (this.DataContext as DrawerViewModel).AllRectanglesView.Add(rectSelectArea);
                    (this.DataContext as DrawerViewModel).AllRectangles.Add(rectSelectArea);
                }
                
                
                Canvas.SetLeft(rectSelectArea, startPoint.X);
                Canvas.SetTop(rectSelectArea, startPoint.Y);
                //cnvImage.Children.Add(rectSelectArea);
            }
            
        }

        private void ImgCamera_MouseMove(object sender, MouseEventArgs e)
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
            var position = e.GetPosition(cnvImage);
            txtBox.Content = "X: " + (int)position.X + "; Y: " + (int)position.Y;
        }

        private void cropImageLabel()
        {
            Cursor = Cursors.Wait;
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

            foreach (var rec in (this.DataContext as DrawerViewModel).AllRectanglesView)
            {
                Mat mat = SupportCode.ConvertBmp2Mat(src);
                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rec.X, (int)rec.Y, (int)rec.RectangleWidth, (int)rec.RectangleHeight);

                Mat croppedImage = new Mat(mat, rectCrop);
                rec.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
            }
            Cursor = Cursors.Arrow;
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            (this.DataContext as DrawerViewModel).FilterName();
        }

        private void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).SortList();
            (this.DataContext as DrawerViewModel).ComboBoxNames();

            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                foreach (var q in (this.DataContext as DrawerViewModel).AllRectanglesView)
                    q.RectangleMovable = true;
                (this.DataContext as DrawerViewModel).Enabled = true;
                
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

                Mat mat = SupportCode.ConvertBmp2Mat(src);
                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rectSelectArea.X, (int)rectSelectArea.Y, (int)rectSelectArea.RectangleWidth, (int)rectSelectArea.RectangleHeight);
                Mat croppedImage = new Mat(mat, rectCrop);

                rectSelectArea.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
            }

            //foreach(var rec in (this.DataContext as DrawerViewModel).AllRectangles)
            //{
            //    if(rec.CroppedImage == null)
            //    {
            //        cropImageLabel();
            //    }
            //}
            
            
        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
        public void ListBoxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).ComboBoxNames();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cropImageLabel();

            string destFileName = (this.DataContext as DrawerViewModel).ImagePath.Remove((this.DataContext as DrawerViewModel).ImagePath.LastIndexOf('.'));

            foreach (var rec in (this.DataContext as DrawerViewModel).AllRectangles)
            {
                string path1 = destFileName + @"_Cropped_Images\" + rec.RectangleText + @"\";

                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }

                if (rec.CroppedImage != null)
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rec.CroppedImage));
                    string filename = path1 + (this.DataContext as DrawerViewModel).AllRectangles.IndexOf(rec) + ".png";
                    using (var fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
            
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            cropImageLabel();
        }
    }
}

