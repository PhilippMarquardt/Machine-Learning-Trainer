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

        System.Windows.Point currentPoint = new System.Windows.Point();
        
        

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Shapes.Rectangle rect;
            rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Blue);
            rect.StrokeThickness = 3;
            rect.Width = 525;
            rect.Height = 521;
            Canvas.SetLeft(rect, 109);
            Canvas.SetTop(rect, 6);
            cnvImage.Children.Add(rect);
            

        }

        public void GrabCut()
        { 
            var rect = new OpenCvSharp.Rect(109, 6, 525, 521);
            var result = new Mat();
            var bgdModel = new Mat();
            var fgdModel = new Mat();
            var mask = new Mat();
            Mat image = Cv2.ImRead(@"C:\Users\hsa\Desktop\17.jpg");
            Mat original_img = Cv2.ImRead(@"C:\Users\hsa\Desktop\17.jpg");
            Cv2.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, GrabCutModes.InitWithRect);
            Cv2.ImWrite(@"C:\Users\hsa\Desktop\testaml.png", mask);

            Bitmap img = new Bitmap(@"C:\Users\hsa\Desktop\testaml.png");
            Bitmap newBitmap = new Bitmap(img.Width, img.Height);
            System.Drawing.Color actualColor;
            System.Drawing.Color white = System.Drawing.Color.White;
            System.Drawing.Color black = System.Drawing.Color.Black;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    actualColor = img.GetPixel(i, j);
                    if (actualColor.R > 2)
                        newBitmap.SetPixel(i, j, white);
                    else
                        newBitmap.SetPixel(i, j, black);

                }
            }

            newBitmap.Save(@"C:\Users\hsa\Desktop\mask_binary.png", ImageFormat.Png);
            Mat newMask = Cv2.ImRead(@"C:\Users\hsa\Desktop\mask_binary.png", ImreadModes.GrayScale);
            Cv2.Threshold(newMask, newMask, 0, 255, ThresholdTypes.Binary);


            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Scalar lightblue = Scalar.Blue;
            Scalar blue = Scalar.Blue;

            Cv2.FindContours(newMask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            Cv2.FillPoly(image, contours, lightblue);
            double alpha = 0.3;
            double beta = 1 - alpha;
            Mat output = new Mat();
            Cv2.AddWeighted(image, alpha, original_img, beta, 0.0, output, -1);
            Cv2.DrawContours(output, contours, 0, blue, 2);


            Cv2.ImWrite(@"C:\Users\hsa\Desktop\output.png", output);



            imgPreview.Source = new BitmapImage(new Uri(@"C:\Users\hsa\Desktop\output.png"));



        }

        private void cnvImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            
            if (e.ClickCount == 2)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                GrabCut();
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine(elapsedMs + " ms");
            }

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currentPoint = e.GetPosition(imgPreview);
            }


        }

        private void cnvImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = System.Windows.Media.Brushes.LawnGreen;
                line.StrokeThickness = 5;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(imgPreview).X;
                line.Y2 = e.GetPosition(imgPreview).Y;

                currentPoint = e.GetPosition(imgPreview);

                cnvImage.Children.Add(line);
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = System.Windows.Media.Brushes.Black;
                line.StrokeThickness = 5;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(imgPreview).X;
                line.Y2 = e.GetPosition(imgPreview).Y;

                currentPoint = e.GetPosition(imgPreview);

                cnvImage.Children.Add(line);
            }
        }
    }
}
 




