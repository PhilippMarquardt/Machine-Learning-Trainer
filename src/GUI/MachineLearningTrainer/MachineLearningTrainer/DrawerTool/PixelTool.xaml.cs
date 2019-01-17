using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
    /// Interaktionslogik für PixelTool.xaml
    /// </summary>
    public partial class PixelTool : UserControl
    {
        public PixelTool()
        {
            InitializeComponent();
        }

        private System.Windows.Point currentPoint;
        private System.Windows.Point startPoint;
        private ResizableRectangle rectSelectArea;
        //private PointCollection points { get; set; } = new PointCollection();
        //private Polygon p = new Polygon();
        //private Mat drawMask { get; set; } = new Mat();



        private void WrapPanel_FileExplorer_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle_FileExplorer.Fill = System.Windows.Media.Brushes.DodgerBlue;
            Button_FileExplorer.Foreground = System.Windows.Media.Brushes.DodgerBlue;
        }

        private void WrapPanel_FileExplorer_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle_FileExplorer.Fill = System.Windows.Media.Brushes.Gray;
            Button_FileExplorer.Foreground = System.Windows.Media.Brushes.Black;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (FolderView_Panel.Visibility == Visibility.Visible)
            {
                FolderView_Panel.Visibility = Visibility.Collapsed;
            }

            else
            {
                FolderView_Panel.Visibility = Visibility.Visible;

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            this.Focus();
            (this.DataContext as DrawerViewModel).AnnoToolMode = "Pixel";
            (this.DataContext as DrawerViewModel).MyInkCanvas = cnvInk;
            (this.DataContext as DrawerViewModel).MyCanvas = cnvImage;
            (this.DataContext as DrawerViewModel).MyPreview = imgPreview;

            var gridHeight = gridY0.ActualHeight + gridY1.ActualHeight + gridY2.ActualHeight;
            (this.DataContext as DrawerViewModel).ZoomBorderHeight = gridHeight; ;
            (this.DataContext as DrawerViewModel).ZoomBorderWidth = gridX.ActualWidth;

            foreach (var drive in Directory.GetLogicalDrives())
            {
                // Create a new item for it
                var item = new TreeViewItem()
                {
                    // Set the header
                    Header = drive,
                    // And the full path
                    Tag = drive
                };

                // Add a dummy item
                item.Items.Add(null);

                // Listen out for item being expanded
                item.Expanded += Folder_Expanded;

                // Add it to the main tree-view
                FolderView.Items.Add(item);
            }
        }


        #region Folder Expanded

        /// <summary>
        /// When a folder is expanded, find the sub folders/files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            #region Initial Checks

            var item = (TreeViewItem)sender;

            // If the item only contains the dummy data
            if (item.Items.Count != 1 || item.Items[0] != null)
                return;

            // Clear dummy data
            item.Items.Clear();

            // Get full path
            var fullPath = (string)item.Tag;

            #endregion

            #region Get Folders

            // Create a blank list for directories
            var directories = new List<string>();

            // Try and get directories from the folder
            // ignoring any issues doing so
            try
            {
                var dirs = Directory.GetDirectories(fullPath);

                if (dirs.Length > 0)
                    directories.AddRange(dirs);
            }
            catch { }

            // For each directory...
            directories.ForEach(directoryPath =>
            {
                // Create directory item
                var subItem = new TreeViewItem()
                {
                    // Set header as folder name
                    Header = GetFileFolderName(directoryPath),
                    // And tag as full path
                    Tag = directoryPath
                };

                // Add dummy item so we can expand folder
                subItem.Items.Add(null);

                // Handle expanding
                subItem.Expanded += Folder_Expanded;

                // Add this item to the parent
                item.Items.Add(subItem);
            });

            #endregion

            #region Get Files

            // Create a blank list for files
            var files = new List<string>();

            // Try and get files from the folder
            // ignoring any issues doing so
            try
            {
                var fs = Directory.GetFiles(fullPath);
                if (fs.Length > 0)
                    files.AddRange(fs);
            }
            catch { }

            // For each file...
            files.ForEach(filePath =>
            {
                // Create file item
                var subItem = new TreeViewItem()
                {
                    // Set header as file name
                    Header = GetFileFolderName(filePath),
                    // And tag as full path
                    Tag = filePath
                };

                var checkExtension = subItem.ToString();

                // Add this item to the parent
                if (checkExtension.Contains(".png") || checkExtension.Contains(".jpg") || checkExtension.Contains(".tiff"))
                {
                    item.Items.Add(subItem);
                }

            });

            #endregion
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Find the file or folder name from a full path
        /// </summary>
        /// <param name="path">The full path</param>
        /// <returns></returns>
        public static string GetFileFolderName(string path)
        {
            // If we have no path, return empty
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            // Make all slashes back slashes
            var normalizedPath = path.Replace('/', '\\');

            // Find the last backslash in the path
            var lastIndex = normalizedPath.LastIndexOf('\\');

            // If we don't find a backslash, return the path itself
            if (lastIndex <= 0)
                return path;

            // Return the name after the last back slash
            return path.Substring(lastIndex + 1);
        }



        #endregion

        #region Zoom

        private void MenuItem_Reset(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).ImagePath != null)
                zoomBorder.Reset();
        }

        private void MenuItem_ZoomOut(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOut();
        }

        private void MenuItem_ZoomIn(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomIn();
        }

        #endregion
        
        private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                cnvImage.Cursor = Cursors.Cross;
                startPoint = e.GetPosition(cnvImage);
                rectSelectArea = new ResizableRectangle();
                rectSelectArea.RectangleBorderThickness = 3;
                rectSelectArea.ThumbColor = System.Windows.Media.Brushes.Blue;
                rectSelectArea.ThumbSize = 5;
                rectSelectArea.RectangleOpacity = 0;
                rectSelectArea.ResizeThumbColor = System.Windows.Media.Brushes.Blue;

                (this.DataContext as DrawerViewModel).PixelRectangles.Add(rectSelectArea);
                
                Canvas.SetLeft(rectSelectArea, startPoint.X);
                Canvas.SetTop(rectSelectArea, startPoint.Y);
            }
            
            currentPoint = e.GetPosition(cnvImage);
            startPoint = e.GetPosition(cnvImage);

        }

        private void ImgCamera_MouseMove(object sender, MouseEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)

            {
                cnvImage.Cursor = Cursors.Cross;
                if (e.LeftButton == MouseButtonState.Released || rectSelectArea == null)
                    return;

                var pos = e.GetPosition(cnvImage);
                
                var x = Math.Min(pos.X, startPoint.X);
                var y = Math.Min(pos.Y, startPoint.Y);
                
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

        private void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                foreach (var q in (this.DataContext as DrawerViewModel).PixelRectangles)
                    q.RectangleMovable = true;
                (this.DataContext as DrawerViewModel).Enabled = true;
                cnvImage.Cursor = Cursors.Arrow;
            }
        }
        

        private void CreateSaveBitmap(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)canvas.Width, (int)canvas.Height,
             96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            canvas.Measure(new System.Windows.Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new System.Windows.Rect(new System.Windows.Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            cnvInk = (this.DataContext as DrawerViewModel).MyInkCanvas;
            var viewModel = (this.DataContext as DrawerViewModel);

            if (e.Key == Key.D)
            {
                cnvInk.IsEnabled = viewModel.DrawEnabled;
                cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
                cnvInk.EditingMode = InkCanvasEditingMode.Ink;

                foreach (var q in viewModel.PixelRectangles)
                {
                    q.RectangleMovable = false;
                    q.Visibility = Visibility.Collapsed;
                }
            }

            if (e.Key == Key.F || e.Key == Key.B || e.Key == Key.E || e.Key == Key.OemPlus || e.Key == Key.OemMinus)
            {

                if (cnvInk.DefaultDrawingAttributes.Height < 3)
                {
                    cnvInk.UseCustomCursor = true;
                    cnvInk.Cursor = Cursors.Pen;
                }

                else
                {
                    cnvInk.UseCustomCursor = false;
                }

                switch (e.Key)
                {
                    case Key.F:
                        cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
                        cnvInk.EditingMode = InkCanvasEditingMode.Ink;
                        break;
                    case Key.B:
                        cnvInk.DefaultDrawingAttributes.Color = Colors.Red;
                        cnvInk.EditingMode = InkCanvasEditingMode.Ink;
                        break;
                    case Key.E:
                        cnvInk.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        cnvInk.DefaultDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
                        break;
                    case Key.OemPlus:
                        cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height + 1;
                        cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width + 1;
                        CreateSaveBitmap(cnvImage, @"C:\Users\hsa\Desktop\output.png");
                        break;
                    case Key.OemMinus:
                        if (cnvInk.DefaultDrawingAttributes.Height > 1 && cnvInk.DefaultDrawingAttributes.Width > 1)
                        {
                            cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height - 1;
                            cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width - 1;
                        }
                        break;
                }
            }
        }

        private void Button_Crop(object sender, RoutedEventArgs e)
        {
            BitmapImage bImage = new BitmapImage(new Uri(imgPreview.Source.ToString()));
            Bitmap src;

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                src = new Bitmap(bitmap);
            }

            var rec = (this.DataContext as DrawerViewModel).PixelRectangles[(this.DataContext as DrawerViewModel).PixelRectangles.Count - 1];

            double RECX = rec.X;
            double RECY = rec.Y;
            double RECH = rec.RectangleHeight;
            double RECW = rec.RectangleWidth;


            Mat mat = SupportCode.ConvertBmp2Mat(src);
            OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);
            Mat croppedImage = new Mat(mat, rectCrop);
            Cv2.ImWrite(@"C:\Users\hsa\Desktop\2345525.png", croppedImage);
            (this.DataContext as DrawerViewModel).image = croppedImage;
            
            (this.DataContext as DrawerViewModel).cropOrNot = 1;

            (this.DataContext as DrawerViewModel).GrabCut();

            var viewmodel = (this.DataContext as DrawerViewModel);
            //cnvInk.Children.Remove(viewmodel.pFull);
            //cnvInk.Children.Add(viewmodel.pFull);
        }

        private void Button_Finish(object sender, RoutedEventArgs e)
        {
            var viewmodel = (this.DataContext as DrawerViewModel);
            viewmodel.polygonsCollection.Add(viewmodel.pFull);
            //cnvInk.Children.Remove(viewmodel.pFull);
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            cnvInk.Children.Add((this.DataContext as DrawerViewModel).polygonsCollection[i]);
            i++;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //cnvInk.Children.RemoveAt(cnvInk.Children.Count);
            Console.WriteLine(cnvInk.Children.Count);
        }
    }
}
 




