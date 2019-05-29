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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
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
            DriveInfo[] drives = DriveInfo.GetDrives();

            DispatcherTimer autosaveTimer = new DispatcherTimer(TimeSpan.FromSeconds(ConfigClass.autosaveRefreshRate), DispatcherPriority.Background, 
                new EventHandler(DoAutoSave), Application.Current.Dispatcher);

            //foreach (DriveInfo driveInfo in drives)
            //    treeView.Items.Add(CreateTreeItem(driveInfo));
        }

        private double timeCounter = 0;

        private async void DoAutoSave(object sender, EventArgs e)
        {
            timeCounter += ConfigClass.autosaveRefreshRate;

            if (((this.DataContext as DrawerViewModel).ImagePath != null && (this.DataContext as DrawerViewModel).ChangeDetected == true)
                || timeCounter > ConfigClass.autosaveIntervall)
            {
                (this.DataContext as DrawerViewModel).ChangeDetected = false;
                timeCounter = 0;

                (this.DataContext as DrawerViewModel).ExportToPascal(DrawerViewModel.CallMode.Autosave);

                //saveIcon.Visibility = Visibility.Visible;

                //DoubleAnimation animationZoomIn = new DoubleAnimation(ConfigClass.smallWidth, ConfigClass.bigWidth, ConfigClass.durationSaveIconAnimated);
                //saveIcon.BeginAnimation(Viewbox.WidthProperty, animationZoomIn);

                //await Task.Delay(ConfigClass.durationSaveIconShown);
                //saveIcon.Visibility = Visibility.Hidden;


                DoubleAnimation increaseOpacity = new DoubleAnimation(ConfigClass.minOpacity, ConfigClass.maxOpacity, ConfigClass.durationSaveIconAnimated);
                DoubleAnimation decreaseOpacity = new DoubleAnimation(ConfigClass.maxOpacity, ConfigClass.minOpacity, ConfigClass.durationSaveIconAnimated);

                saveIcon.BeginAnimation(Viewbox.OpacityProperty, increaseOpacity);
                await Task.Delay(ConfigClass.durationSaveIconShown);
                saveIcon.BeginAnimation(Viewbox.OpacityProperty, decreaseOpacity);
            }
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
            (this.DataContext as DrawerViewModel).RefreshSubtypeList();
            (this.DataContext as DrawerViewModel).IsOpen = true;
        }

        private void Subtype_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ((MenuItem)sender).GetBindingExpression(MenuItem.ItemsSourceProperty).UpdateTarget();
        }

        private void ContextMenu_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            (this.DataContext as DrawerViewModel).FilterName();
            //listBoxLabels.LayoutUpdated();
        }


        #region Define MouseEvents

        private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).SelectCustomShape();
        }

        private int tmpRVCount = 0;

        private void ImgCamera_MouseMove(object sender, MouseEventArgs e)
        {
            mousePosition = e.GetPosition(cnvImage);
            (this.DataContext as DrawerViewModel).vmMousePoint = mousePosition;
            txtBox.Content = "X: " + (int)mousePosition.X + "; Y: " + (int)mousePosition.Y;
            txtBox1.Content = (this.DataContext as DrawerViewModel).ImagePath;

            if ((this.DataContext as DrawerViewModel).MouseHandlingState == DrawerViewModel.MouseState.CreateRectangle)
            {

                if (tmpRVCount != (this.DataContext as DrawerViewModel).RectanglesView.Count())
                {
                    tmpRVCount = (this.DataContext as DrawerViewModel).RectanglesView.Count();
                    int zIndex = tmpRVCount + 2;

                    Panel.SetZIndex(horizontalLine, zIndex);
                    Panel.SetZIndex(verticalLine, zIndex);
                }

                horizontalLine.Y1 = mousePosition.Y;
                horizontalLine.Y2 = mousePosition.Y;
                horizontalLine.StrokeThickness = 1;
                verticalLine.X1 = mousePosition.X;
                verticalLine.X2 = mousePosition.X;
                verticalLine.StrokeThickness = 1;

                Mouse.OverrideCursor = Cursors.Cross;
                (this.DataContext as DrawerViewModel).CreateRectangle(mousePosition);
            }
            else
            {
                if(horizontalLine.Y1 != -1)
                { 
                    horizontalLine.Y1 = -1;
                    horizontalLine.Y2 = -1;
                    horizontalLine.StrokeThickness = 0;
                    verticalLine.X1 = -1;
                    verticalLine.X2 = -1;
                    verticalLine.StrokeThickness = 0;
                }

                if (e.LeftButton == MouseButtonState.Released)
                {
                    (this.DataContext as DrawerViewModel).DetectCustomShape(mousePosition);
                    if ((this.DataContext as DrawerViewModel).ShapeDetected == true)
                    {
                        (this.DataContext as DrawerViewModel).Enabled = false;
                    }
                    else
                    {
                        (this.DataContext as DrawerViewModel).Enabled = true;
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                }
                //Uncommented, because it overrides Resize-MouseCursor
                //else
                //{
                //    Mouse.OverrideCursor = Cursors.Hand;
                //}

                (this.DataContext as DrawerViewModel).Resize(mousePosition);
                (this.DataContext as DrawerViewModel).Move(mousePosition);
            }
        }

        private async void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //(this.DataContext as DrawerViewModel).SortList();
            //(this.DataContext as DrawerViewModel).ComboBoxNames();

            //if ((this.DataContext as DrawerViewModel).Enabled == false && rectSelectArea != null)
            //{
            //    //foreach (var q in (this.DataContext as DrawerViewModel).RectanglesView)
            //    //    q.RectangleMovable = true;
            //    (this.DataContext as DrawerViewModel).Enabled = true;
            //    var w = (int)rectSelectArea.RectangleWidth;
            //    var h = (int)rectSelectArea.RectangleHeight;
            //    if (w > 1 && h > 1)
            //    {
            //        BitmapImage bImage = new BitmapImage(new Uri(imgPreview.Source.ToString()));
            //        Bitmap src;

            //        using (MemoryStream outStream = new MemoryStream())
            //        {
            //            BitmapEncoder enc = new BmpBitmapEncoder();
            //            enc.Frames.Add(BitmapFrame.Create(bImage));
            //            enc.Save(outStream);
            //            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

            //            src = new Bitmap(bitmap);
            //        }

            //        Mat mat = SupportCode.ConvertBmp2Mat(src);
            //        OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rectSelectArea.X, (int)rectSelectArea.Y, (int)rectSelectArea.RectangleWidth, (int)rectSelectArea.RectangleHeight);
            //        Mat croppedImage = new Mat(mat, rectCrop);

            //        rectSelectArea.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
            //    }

            //}

            ////else
            ////{
            ////    await (this.DataContext as DrawerViewModel).cropImageLabelBegin();
            ////}
            if ((this.DataContext as DrawerViewModel).undoCustomShapes.Count > 0)
            {
                (this.DataContext as DrawerViewModel).UndoEnabled = true;
            }
        }

        private void cnvImage_MouseLeave(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 0;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void cnvImage_MouseEnter(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 1;
        }

        #endregion


        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            (this.DataContext as DrawerViewModel).FilterName();
            (this.DataContext as DrawerViewModel).RectangleCount = "#" + (this.DataContext as DrawerViewModel).RectanglesView.Count.ToString();
        }


        //public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
        //    binding.UpdateSource();
        //}

        void Zoom_In(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemPlus || e.Key == Key.Add) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.ZoomIn();
            }
        }

        void Zoom_Out(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.ZoomOut();
            }
        }

        void Zoom_Reset(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.Reset();
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            this.KeyDown += new KeyEventHandler(Zoom_In);
            this.KeyDown += new KeyEventHandler(Zoom_Out);
            this.KeyDown += new KeyEventHandler(Zoom_Reset);

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

        private void Button_Click_FolderView_Panel(object sender, RoutedEventArgs e)
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
                    (this.DataContext as DrawerViewModel).Rectangles.Clear();

                    (this.DataContext as DrawerViewModel).LoadLabelData((this.DataContext as DrawerViewModel).ImagePath);
                    (this.DataContext as DrawerViewModel).LoadRectangles();
                    (this.DataContext as DrawerViewModel).ComboBoxNames();
                    (this.DataContext as DrawerViewModel).SortList();
                    (this.DataContext as DrawerViewModel).FilterName();
                    (this.DataContext as DrawerViewModel).ClearUndoRedoStack();
                    zoomBorder.Reset();

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
            if (listBoxLabels.SelectedItem != null)
            {
                int indexView = listBoxLabels.SelectedIndex;
                (this.DataContext as DrawerViewModel).SelectCustomShape(indexView);
            }

            var newSelectedItem = (this.DataContext as DrawerViewModel).SelectedCustomShape;


            if (newSelectedItem != null)
            {
                (sender as ListBox).ScrollIntoView(newSelectedItem);
            }
            if ((this.DataContext as DrawerViewModel).DuplicateVar == 0 || (this.DataContext as DrawerViewModel).CropModeChecked == true)
            {
                zoomBorder.ZoomToRectangle();
            }

        }

        #region LabelListBox

        private void LblTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }
        
        

        private void LabelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (labelListBox.SelectedIndex > -1)
            //{
            //    animatedRoatateTransform.Angle = 180;
            //    gridLV.Width = new GridLength(150);
            //    listBoxLabels.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    animatedRoatateTransform.Angle = 0;
            //    gridLV.Width = new GridLength(0);
            //    listBoxLabels.Visibility = Visibility.Hidden;
            //}
        }


        private void LabelTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Console.WriteLine(labelTreeView.SelectedItem);

            if (labelTreeView.SelectedItem != null)
            {
                if (labelTreeView.SelectedItem.GetType() == typeof(CustomShapeFormat))
                {
                    (this.DataContext as DrawerViewModel).SelectedLabel = (CustomShapeFormat)labelTreeView.SelectedItem;
                    (this.DataContext as DrawerViewModel).SelectedSubLabel = null;
                }
                else if (labelTreeView.SelectedItem.GetType() == typeof(Subtypes))
                {
                    (this.DataContext as DrawerViewModel).SelectedLabel = null;
                    (this.DataContext as DrawerViewModel).SelectedSubLabel = (Subtypes)labelTreeView.SelectedItem;
                }
            }
        }

        #endregion

        private void MenuItem_OpenClick(object sender, RoutedEventArgs e)
        {
            if (imgPreview != null) 
                zoomBorder.Reset();
        }

        #region ColorPicker

        private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                UIElement element = (UIElement)sender;
                element.CaptureMouse();
                e.Handled = true;
            }
        }

        private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Release the mouse capture, if this element held the capture.
            UIElement element = (UIElement)sender;
            element.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void EditLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).SetSelectedColor();
            (this.DataContext as DrawerViewModel).ColorPickerEnabled = true;
            ColorPicker newColorPicker = new ColorPicker();
            newColorPicker.DataContext = this.DataContext;
            newColorPicker.ShowDialog();
        }

        #endregion


        private void CbItemLabel_DropDownClosed(object sender, EventArgs e)
        {
            (this.DataContext as DrawerViewModel).CbItemLabel_DropDownClosed();
        }

        private void CbItemLabel_DropDownOpened(object sender, EventArgs e)
        {
            //(this.DataContext as DrawerViewModel).FilterName();
        }


        /// <summary>
        /// show/hides SideBar with rectangles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (animatedRoatateTransform.Angle != 180)
            {
                animatedRoatateTransform.Angle = 180;
                listBoxLabels.Visibility = Visibility.Hidden;
                ColorPicker_Panel.Visibility = Visibility.Hidden;
                gridLV.Width = new GridLength(0);
            }
            else
            {
                animatedRoatateTransform.Angle = 0;
                listBoxLabels.Visibility = Visibility.Visible;
                gridLV.Width = new GridLength(240);
            }
        }



        #region Labelset Panel

        private void AddLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            //(this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
            (this.DataContext as DrawerViewModel).AddLabelColorFormat();
            //if (RenameTxtBox.IsFocused == true)
            //{
            //    (this.DataContext as DrawerViewModel).Enter();
            //}
        }

        #endregion


        #region LabelControl


        #region TextBox

        private void LabelTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectTreeViewItem(sender);
        }

        private void LabelTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {    
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = false;
            textBox.SelectAll();
            if (textBox.Tag.GetType() == typeof(Subtypes))
            {
                Subtypes tmpSubtype = (Subtypes)textBox.Tag;
                (this.DataContext as DrawerViewModel).TmpNewLabel.Parent = tmpSubtype.Parent;
            }
            (this.DataContext as DrawerViewModel).TmpNewLabel.Label = textBox.Text;
        }

        private void LblTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.IsReadOnly = true;

                (this.DataContext as DrawerViewModel).TmpNewLabel.Parent = "";
                (this.DataContext as DrawerViewModel).TmpNewLabel.Label = "";

            }
        }

        private void LabelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            (this.DataContext as DrawerViewModel).OnRename();

            if (textBox.Tag.GetType() == typeof(Subtypes))
            {
                Subtypes tmpSubtype = (Subtypes)textBox.Tag;
                (this.DataContext as DrawerViewModel).TmpNewLabel.Parent = tmpSubtype.Parent;
            }
            (this.DataContext as DrawerViewModel).TmpNewLabel.Label = textBox.Text;
        }

        #endregion


        private void ShowHideData_Click(object sender, RoutedEventArgs e)
        {

            SelectTreeViewItem(sender);

            (this.DataContext as DrawerViewModel).ShowHideData();

            if (labelTreeView.SelectedItem.GetType() == typeof(Subtypes))
            {
                Subtypes tmpSubtype = (Subtypes)labelTreeView.SelectedItem;
                if (tmpSubtype.Visible == false)
                {
                    foreach (CustomShapeFormat csf in labelTreeView.Items)
                    {
                        if (csf.Label == tmpSubtype.Parent)
                        {
                            csf.Visible = false;
                        }
                    }
                }
                else if (tmpSubtype.Visible == true)
                {
                    foreach (CustomShapeFormat csf in labelTreeView.Items)
                    {
                        if (csf.Label == tmpSubtype.Parent)
                        {
                            csf.Visible = true;
                            foreach (Subtypes sb in csf.Subtypes)
                            {
                                if (sb.Visible == false)
                                {
                                    csf.Visible = false;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            else if (labelTreeView.SelectedItem.GetType() == typeof(CustomShapeFormat))
            {
                CustomShapeFormat tmpCSF = (CustomShapeFormat)labelTreeView.SelectedItem;

                foreach (CustomShapeFormat csf in labelTreeView.Items)
                {
                    if (csf.Label == tmpCSF.Label)
                    {
                        foreach (Subtypes sb in csf.Subtypes)
                        {
                            sb.Visible = tmpCSF.Visible;
                        }
                    }
                }
            }
        }

        private void AddSubtype_Click(object sender, RoutedEventArgs e)
        {
            SelectTreeViewItem(sender);

            CustomShapeFormat tmpCSF = (CustomShapeFormat)labelTreeView.SelectedItem;
            foreach (CustomShapeFormat csf in labelTreeView.Items)
            {
                if (csf.Label == tmpCSF.Label)
                {
                    csf.IsExpanded = true;
                    break;
                }
            }

            (this.DataContext as DrawerViewModel).AddSubtype();
        }

        private void RmvLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            SelectTreeViewItem(sender);

            (this.DataContext as DrawerViewModel).DeleteSelectedLabel();
        }


        /// <summary>
        /// Select TreeViewItem depending on clicked TextBox/Icon
        /// </summary>
        /// <param name="sender"></param>
        private void SelectTreeViewItem(object sender)
        {

            foreach (CustomShapeFormat lcf in labelTreeView.Items)
            {
                lcf.IsSelected = false;

                foreach (Subtypes sub in lcf.Subtypes)
                //foreach(CustomShapeFormat sub in lcf.Subtypes)
                {
                    sub.IsSelected = false;
                }
            }

            if (sender.GetType() == typeof(Button))
            {
                if (((Button)sender).Tag != null)
                {
                    if (((Button)sender).Tag.GetType() == typeof(CustomShapeFormat))
                    {
                        CustomShapeFormat selectedItem = ((Button)sender).Tag as CustomShapeFormat;

                        foreach (CustomShapeFormat lcf in labelTreeView.Items)
                        {
                            if (lcf.Label == selectedItem.Label)
                            {
                                lcf.IsSelected = true;
                                (this.DataContext as DrawerViewModel).SelectedLabel = lcf;
                                return;
                            }
                        }
                    }
                    else if (((Button)sender).Tag.GetType() == typeof(Subtypes))
                    {
                        Subtypes selectedItem = ((Button)sender).Tag as Subtypes;

                        foreach (CustomShapeFormat lcf in labelTreeView.Items)
                        {
                            if (lcf.Label == selectedItem.Parent)
                            {
                                foreach (Subtypes sub in lcf.Subtypes)
                                //foreach (CustomShapeFormat sub in lcf.Subtypes)
                                {
                                    if (sub.Label == selectedItem.Label)
                                    {
                                        sub.IsSelected = true;
                                        (this.DataContext as DrawerViewModel).SelectedSubLabel = sub;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            else if (sender.GetType() == typeof(TextBox))
            {
                if (((TextBox)sender).Tag != null)
                {
                    if (((TextBox)sender).Tag.GetType() == typeof(CustomShapeFormat))
                    {
                        CustomShapeFormat selectedItem = ((TextBox)sender).Tag as CustomShapeFormat;

                        foreach (CustomShapeFormat lcf in labelTreeView.Items)
                        {
                            if (lcf.Label == selectedItem.Label)
                            {
                                lcf.IsSelected = true;
                                return;
                            }
                        }
                    }
                    else if (((TextBox)sender).Tag.GetType() == typeof(Subtypes))
                    {
                        Subtypes selectedItem = ((TextBox)sender).Tag as Subtypes;

                        foreach (CustomShapeFormat lcf in labelTreeView.Items)
                        {
                            if (lcf.Label == selectedItem.Parent)
                            {
                                foreach (Subtypes sub in lcf.Subtypes)
                                //foreach (CustomShapeFormat sub in lcf.Subtypes)
                                {
                                    if (sub.Label == selectedItem.Label)
                                    {
                                        sub.IsSelected = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
           
        }


        #region ColorPicker

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            SelectTreeViewItem(sender);

            if (animatedRoatateTransform.Angle != 180)
            {
                animatedRoatateTransform.Angle = 180;
                listBoxLabels.Visibility = Visibility.Hidden;
                ColorPicker_Panel.Visibility = Visibility.Hidden;
                gridLV.Width = new GridLength(0);
            }
            else
            {
                animatedRoatateTransform.Angle = 0;
                ColorPicker_Panel.Visibility = Visibility.Visible;
                (this.DataContext as DrawerViewModel).IsEnabled = false;
                gridLV.Width = new GridLength(265);

                Console.WriteLine((this.DataContext as DrawerViewModel).SelectedLabel.Label);
            }
        }

        /// <summary>
        /// Shows ColorCanvas according to selected coloring mode in ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColoringMode_DropDownClosed(object sender, EventArgs e)
        {
            switch (coloringMode.Text)
            {
                case "Fill":
                case "Both":
                    fillColorCanvas.Visibility = Visibility.Visible;
                    strokeColorCanvas.Visibility = Visibility.Hidden;
                    _colorPicker.SelectedColor = fillColorCanvas.SelectedColor;
                    break;
                case "Border":
                    fillColorCanvas.Visibility = Visibility.Hidden;
                    strokeColorCanvas.Visibility = Visibility.Visible;
                    _colorPicker.SelectedColor = strokeColorCanvas.SelectedColor;
                    break;
            }
        }

        /// <summary>
        /// Changes and refreshes Colorformat of all Rectangles of selected Label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _colorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            switch (coloringMode.Text)
            {
                case "Fill":
                case "Both":
                    _colorPicker.SelectedColor = fillColorCanvas.SelectedColor;
                    break;
                case "Border":
                    _colorPicker.SelectedColor = strokeColorCanvas.SelectedColor;
                    break;
            }
            

            (this.DataContext as DrawerViewModel).ChangeColor();
            //(this.DataContext as DrawerViewModel).ChangeOpacity();
        }

        /// <summary>
        /// Change Color by colorPicker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            switch (coloringMode.Text)
            {
                case "Fill":
                case "Both":
                    fillColorCanvas.SelectedColor = _colorPicker.SelectedColor;
                    break;
                case "Border":
                    strokeColorCanvas.SelectedColor = _colorPicker.SelectedColor;
                    break;
            }
        }

        /// <summary>
        /// Changes and refreshes Opacity of all Rectangles of selected Label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _sliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _opacityTextBox.Text = Convert.ToString(_sliderOpacity.Value);
            (this.DataContext as DrawerViewModel).ChangeOpacity();
        }

        /// <summary>
        /// Closes ColorPicker and reactivates TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _closeColorChange_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).ChangeDetected = true;

            (this.DataContext as DrawerViewModel).IsEnabled = true;
            animatedRoatateTransform.Angle = 180;
            listBoxLabels.Visibility = Visibility.Hidden;
            ColorPicker_Panel.Visibility = Visibility.Hidden;
            gridLV.Width = new GridLength(0);

            (this.DataContext as DrawerViewModel).SelectedModeItem = "Fill";
            fillColorCanvas.Visibility = Visibility.Visible;
            strokeColorCanvas.Visibility = Visibility.Hidden;

            (this.DataContext as DrawerViewModel).ColorPickerEnabled = false;

            if ((this.DataContext as DrawerViewModel).DeactivatedAddLabel == false)
            {
                (this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
            }
        }

        #endregion


        /// <summary>
        /// Key-Bindings on Selected TreeView Element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelTreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                (this.DataContext as DrawerViewModel).LeftButton_Move();
            }
            else if (e.Key == Key.Right)
            {
                (this.DataContext as DrawerViewModel).RightButton_Move();
            }
            else if (e.Key == Key.Up)
            {
                (this.DataContext as DrawerViewModel).UpButton_Move();
            }
            else if (e.Key == Key.Down)
            {
                (this.DataContext as DrawerViewModel).DownButton_Move();
            }
            else if (e.Key == Key.Left && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                (this.DataContext as DrawerViewModel).LeftButton1();
            }
            else if (e.Key == Key.Right && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                (this.DataContext as DrawerViewModel).RightButton1();
            }
            else if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                (this.DataContext as DrawerViewModel).UpButton1();
            }
            else if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                (this.DataContext as DrawerViewModel).DownButton1();
            }
        }


        #endregion

        private void ChangeListViewMode_Click(object sender, RoutedEventArgs e)
        {
            if (!((this.DataContext as DrawerViewModel).selectedListViewMode == DrawerViewModel.ListViewModes.List))
            {
                gridLV.Width = new GridLength(150);
            }
            else
            {
                gridLV.Width = new GridLength(240);
            }
        }
    }
}