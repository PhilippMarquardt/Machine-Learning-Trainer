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
        private PointCollection points { get; set; } = new PointCollection();
        private Polygon p = new Polygon();
        private Mat drawMask { get; set; } = new Mat();



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


        public void GrabCut()
        { 
            foreach(var rec in (this.DataContext as DrawerViewModel).PixelRectangles)
            {
                if (rec != null)
                {
                    var rect = new OpenCvSharp.Rect((int)rec.X, (int)rec.Y, (int)rec.RectangleWidth, (int)rec.RectangleHeight);
                    var bgdModel = new Mat();
                    var fgdModel = new Mat();


                    BitmapImage bImage = new BitmapImage(new Uri(imgPreview.Source.ToString()));
                    Bitmap src;

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(bImage));
                        enc.Save(outStream);
                        Bitmap bitmap = new Bitmap(outStream);

                        src = new Bitmap(bitmap);
                    }

                    Mat image = SupportCode.ConvertBmp2Mat(src);
                    Mat mask = new Mat();

                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);
                    Cv2.CvtColor(image, image, ColorConversionCodes.RGB2BGR);

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    Cv2.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, GrabCutModes.InitWithRect);
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine(elapsedMs + " ms");
                    Cv2.Threshold(mask, mask, 2, 255, ThresholdTypes.Binary & ThresholdTypes.Otsu);
                    OpenCvSharp.Point[][] contours;
                    HierarchyIndex[] hierarchy;
                    
                    Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    
                    p.Stroke = System.Windows.Media.Brushes.Blue;
                    p.StrokeThickness = 2;
                    p.Fill = System.Windows.Media.Brushes.LightBlue;
                    p.Opacity = 0.5;

                    for (int l = 0; l < contours.Length; l++)
                    {
                        int m = 0;

                        if (contours[l].Count() > m)
                            m = l;

                        if(l == contours.Length - 1)
                        {
                            for (int k = 0; k < contours[m].Length; k++)
                            {
                                points.Add(new System.Windows.Point(contours[m][k].X, contours[m][k].Y));
                            }
                        }
                    }
                    
                    p.Points = points;
                    p.IsHitTestVisible = false;
                    cnvInk.Children.Add(p);
                    
                    foreach (var q in (this.DataContext as DrawerViewModel).PixelRectangles)
                    {
                        q.RectangleMovable = false;
                        q.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void GrabCutMask()
        {
            foreach (var rec in (this.DataContext as DrawerViewModel).PixelRectangles)
            {
                if (rec != null)
                {
                    var rect = new OpenCvSharp.Rect((int)rec.X, (int)rec.Y, (int)rec.RectangleWidth, (int)rec.RectangleHeight);
                    var bgdModel = new Mat();
                    var fgdModel = new Mat();
                    
                    BitmapImage bImage = new BitmapImage(new Uri(imgPreview.Source.ToString()));
                    Bitmap src;

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(bImage));
                        enc.Save(outStream);
                        Bitmap bitmap = new Bitmap(outStream);

                        src = new Bitmap(bitmap);
                    }

                    Mat image = SupportCode.ConvertBmp2Mat(src);
                    
                    Mat mask = drawMask;
                    Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);
                    Cv2.CvtColor(image, image, ColorConversionCodes.RGB2BGR);

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    Cv2.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, GrabCutModes.InitWithMask);
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine(elapsedMs + " ms");

                    Cv2.Threshold(mask, mask, 2, 255, ThresholdTypes.Binary & ThresholdTypes.Otsu);
                    OpenCvSharp.Point[][] contours;
                    HierarchyIndex[] hierarchy;

                    Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                    p.Stroke = System.Windows.Media.Brushes.Blue;
                    p.Fill = System.Windows.Media.Brushes.LightBlue;
                    p.Opacity = 0.4;
                    p.StrokeThickness = 2;
                    points.Clear();

                    for (int l = 0; l < contours.Length; l++)
                    {
                        int m = 0;

                        if (contours[l].Count() > m)
                            m = l;

                        if (l == contours.Length - 1)
                        {
                            for (int k = 0; k < contours[m].Length; k++)
                            {
                                points.Add(new System.Windows.Point(contours[m][k].X, contours[m][k].Y));
                            }
                        }

                    }

                    p.Points = points;
                    p.IsHitTestVisible = false;
                    cnvInk.Children.Add(p);

                    foreach (var q in (this.DataContext as DrawerViewModel).PixelRectangles)
                    {
                        q.RectangleMovable = false;
                        q.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }


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

        private void DrawPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F || e.Key == Key.B || e.Key == Key.E || e.Key == Key.OemPlus || e.Key == Key.OemMinus) 
            {

                if(cnvInk.DefaultDrawingAttributes.Height < 3)
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
                        Draw_Button.Background = System.Windows.Media.Brushes.LawnGreen;
                        break;
                    case Key.B:
                        cnvInk.DefaultDrawingAttributes.Color = Colors.Red;
                        Draw_Button.Background = System.Windows.Media.Brushes.Red;
                        cnvInk.EditingMode = InkCanvasEditingMode.Ink;
                        break;
                    case Key.E:
                        cnvInk.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        cnvInk.DefaultDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
                        Draw_Button.Background = System.Windows.Media.Brushes.SlateGray;
                        break;
                    case Key.OemPlus:
                        cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height + 1;
                        cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width + 1;
                        break;
                    case Key.OemMinus:
                        if(cnvInk.DefaultDrawingAttributes.Height > 1 && cnvInk.DefaultDrawingAttributes.Width > 1)
                        {
                            cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height - 1;
                            cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width - 1;
                        }
                        break;
                }
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
                GrabCut();

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as DrawerViewModel);
            if ((this.DataContext as DrawerViewModel).DrawEnabled == false)
            {
                viewModel.DrawEnabled = true;
                viewModel.Enabled = true;
                cnvInk.IsEnabled = true;
                cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
                cnvInk.EditingMode = InkCanvasEditingMode.Ink;
                Draw_Button.Background = System.Windows.Media.Brushes.LawnGreen;
                foreach (var q in viewModel.PixelRectangles)
                {
                    q.RectangleMovable = false;
                    q.Visibility = Visibility.Collapsed;
                }
                
            }

            else
            {
                Draw_Button.Background = System.Windows.Media.Brushes.White;
                viewModel.DrawEnabled = false;
                cnvInk.IsEnabled = false;
            }

        }



        private void CreateSaveBitmap(InkCanvas canvas)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)canvas.Width, (int)canvas.Height,
             96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new System.Windows.Size((int)canvas.Width, (int)canvas.Height));
            canvas.Arrange(new System.Windows.Rect(new System.Windows.Size((int)canvas.Width, (int)canvas.Height)));

            renderBitmap.Render(canvas);
            

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(stream);

            Bitmap img = new Bitmap(stream);
            int width = (int)imgPreview.ActualWidth;
            int height = (int)imgPreview.ActualHeight;
            Bitmap newBitmap = new Bitmap(100,100);
            System.Drawing.Color actualColor;
            System.Drawing.Color white = System.Drawing.Color.White;
            System.Drawing.Color black = System.Drawing.Color.Black;
            var sure_bg = System.Drawing.Color.FromArgb(0, 0, 0);
            var sure_fg = System.Drawing.Color.FromArgb(1, 1, 1);
            var mask_rect = System.Drawing.Color.FromArgb(2, 2, 2);
            var mask_mask = System.Drawing.Color.FromArgb(3, 3, 3);

            var mat = new Mat(img.Height, img.Width, MatType.CV_8U, Scalar.White);
            var indexer = mat.GetGenericIndexer<Vec3b>();
            Console.WriteLine(indexer[0, 0]);

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Vec3b color = mat.Get<Vec3b>(i, j);
                    color.Item0 = 0;
                    color.Item1 = 0;
                    color.Item2 = 0;
                    indexer[i, j] = color;
                }
            }

            int x1 = (int)rectSelectArea.X;
            int x2 = (int)rectSelectArea.X + (int)rectSelectArea.RectangleWidth;
            int y1 = (int)rectSelectArea.Y;
            int y2 = (int)rectSelectArea.Y + (int)rectSelectArea.RectangleHeight;
            
            for (int i = y1; i <= y2; i++)
            {
                for (int j = x1; j <= x2; j++)
                {
                    actualColor = img.GetPixel(j, i);
                    if (actualColor.A == 0)
                    {
                        Vec3b color = mat.Get<Vec3b>(i, j);
                        color.Item0 = 2;
                        color.Item1 = 2;
                        color.Item2 = 2;
                        indexer[i, j] = color;
                    }
                        
                    else if(actualColor.R == 255 && actualColor.G == 0 && actualColor.B == 0)
                    {
                        Vec3b color = mat.Get<Vec3b>(i, j);
                        color.Item0 = 0;
                        color.Item1 = 0;
                        color.Item2 = 0;
                        indexer[i, j] = color;
                    }

                    else if (actualColor.R == 124 && actualColor.G == 252 && actualColor.B == 0)
                    {
                        Vec3b color = mat.Get<Vec3b>(i, j);
                        color.Item0 = 1;
                        color.Item1 = 1;
                        color.Item2 = 1;
                        indexer[i, j] = color;
                    }

                    else
                    {
                        Vec3b color = mat.Get<Vec3b>(i, j);
                        color.Item0 = 3;
                        color.Item1 = 3;
                        color.Item2 = 3;
                        indexer[i, j] = color;
                    }


                }
            }

            drawMask = mat;

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CreateSaveBitmap(cnvInk);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cnvInk.Children.Remove(p);
            cnvInk.Strokes.Clear();
            GrabCutMask();
        }
        
    }
}
 




