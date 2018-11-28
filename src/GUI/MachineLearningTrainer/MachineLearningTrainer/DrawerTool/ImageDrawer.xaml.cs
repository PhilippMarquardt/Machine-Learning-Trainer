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
using System.Windows.Controls.Primitives;
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
            cnvImage.Cursor = Cursors.Arrow;
            DriveInfo[] drives = DriveInfo.GetDrives();
            //foreach (DriveInfo driveInfo in drives)
            //    treeView.Items.Add(CreateTreeItem(driveInfo));
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
        public string filenameTreeView { get; set; }
        public System.Windows.Point mousePosition { get; set; }

        public void ListBox_RightButtonUp(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).IsOpen = true;
        }
        private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                startPoint = e.GetPosition(cnvImage);
                rectSelectArea = new ResizableRectangle();

                if ((this.DataContext as DrawerViewModel).SelectedComboBoxItem == "All Labels")
                {
                    (this.DataContext as DrawerViewModel).AllRectanglesView.Insert(0, rectSelectArea);
                }

                else
                {
                    (this.DataContext as DrawerViewModel).AllRectanglesView.Insert(0, rectSelectArea);
                    (this.DataContext as DrawerViewModel).AllRectangles.Insert(0, rectSelectArea);
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
                cnvImage.Cursor = Cursors.Cross;
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

            else
            {
                cnvImage.Cursor = Cursors.Arrow;
            }

            mousePosition = e.GetPosition(cnvImage);
            (this.DataContext as DrawerViewModel).vmMousePoint = mousePosition;
            txtBox.Content = "X: " + (int)mousePosition.X + "; Y: " + (int)mousePosition.Y;
            txtBox1.Content = (this.DataContext as DrawerViewModel).ImagePath;
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            (this.DataContext as DrawerViewModel).FilterName();

            if ((this.DataContext as DrawerViewModel).SelectedComboBoxItem != "All Labels")
            {
                (this.DataContext as DrawerViewModel).RectangleCount = "#" + (this.DataContext as DrawerViewModel).AllRectanglesView.Count.ToString();
            }
            else
            {
                (this.DataContext as DrawerViewModel).RectangleCount = "#" + (this.DataContext as DrawerViewModel).AllRectangles.Count.ToString();
            }
        }

        private async void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //(this.DataContext as DrawerViewModel).SortList();
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

            else
            {
                await (this.DataContext as DrawerViewModel).cropImageLabelBegin();
            }

            if ((this.DataContext as DrawerViewModel).SelectedComboBoxItem != "All Labels")
            {
                (this.DataContext as DrawerViewModel).RectangleCount = "#" + (this.DataContext as DrawerViewModel).AllRectanglesView.Count.ToString();
            }
            else
            {
                (this.DataContext as DrawerViewModel).RectangleCount = "#" + (this.DataContext as DrawerViewModel).AllRectangles.Count.ToString();
            }
        }

        //public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
        //    binding.UpdateSource();
        //}

        public void ListBoxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).ComboBoxNames();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            (this.DataContext as DrawerViewModel).MyCanvas = cnvImage;
            (this.DataContext as DrawerViewModel).MyPreview = imgPreview;
            (this.DataContext as DrawerViewModel).vmMousePoint = mousePosition;

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
                filenameTreeView = fullPath;
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (FolderView_Panel.Visibility == Visibility.Visible)
            {
                FolderView_Panel.Visibility = Visibility.Collapsed;
            }

            else
            {
                //Folder_Panel.Visibility = Visibility.Collapsed;
                FolderView_Panel.Visibility = Visibility.Visible;

            }
        }

        //private void Button_Folder_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Folder_Panel.Visibility == Visibility.Visible)
        //    {
        //        Folder_Panel.Visibility = Visibility.Collapsed;
        //    }

        //    else
        //    {
        //        FolderView_Panel.Visibility = Visibility.Collapsed;
        //        Folder_Panel.Visibility = Visibility.Visible;

        //    }
        //}

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FolderView.SelectedItem != null)
            {
                int found = 0;
                string s = FolderView.SelectedItem.ToString();
                found = s.IndexOf("Header:");
                string filename = s.Substring(found + 7);
                int index = filename.IndexOf("Items");
                if (index > 0)
                    filename = filename.Substring(0, index - 1);
                string fullFileName = filenameTreeView + @"\" + filename;

                if (fullFileName.Contains(".jpg") || fullFileName.Contains(".png") || fullFileName.Contains(".tiff"))
                {
                    FolderView_Panel.Visibility = Visibility.Collapsed;
                    (this.DataContext as DrawerViewModel).ImagePath = fullFileName;

                    (this.DataContext as DrawerViewModel).IsEnabled = true;
                    (this.DataContext as DrawerViewModel).AllRectangles.Clear();

                    (this.DataContext as DrawerViewModel).LoadRectangles();
                    (this.DataContext as DrawerViewModel).ComboBoxNames();
                    (this.DataContext as DrawerViewModel).SortList();
                    (this.DataContext as DrawerViewModel).FilterName();

                }

            }
        }

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

        private void listBoxLabels_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).SelectedResizableRectangle != null)
            {
                zoomBorder.ZoomToRectangle();
            }
        }

        #endregion

        private void WrapPanel_FileExplorer_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle_FileExplorer.Fill = System.Windows.Media.Brushes.DodgerBlue;
            Button_FileExplorer.Foreground = System.Windows.Media.Brushes.DodgerBlue;
        }

        //private void WrapPanel_Folder_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    Rectangle_Folder.Fill = System.Windows.Media.Brushes.DodgerBlue;
        //    Button_Folder.Foreground = System.Windows.Media.Brushes.DodgerBlue;
        //}

        private void WrapPanel_FileExplorer_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle_FileExplorer.Fill = System.Windows.Media.Brushes.Gray;
            Button_FileExplorer.Foreground = System.Windows.Media.Brushes.Black;
        }

        //private void WrapPanel_Folder_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    Rectangle_Folder.Fill = System.Windows.Media.Brushes.Gray;
        //    Button_Folder.Foreground = System.Windows.Media.Brushes.Black;
        //}

        private void listBoxLabels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newSelectedItem = (this.DataContext as DrawerViewModel).SelectedResizableRectangle;
            if (newSelectedItem != null)
            {
                (sender as ListBox).ScrollIntoView(newSelectedItem);
            }
        }

        private void cnvImage_MouseEnter(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 1;
        }

        private void cnvImage_MouseLeave(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 0;
        }
    }
}