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
        // Initialization of ImageDrawer

        #region Initialization

        public ImageDrawer()
        {
            InitializeComponent();
            DriveInfo[] drives = DriveInfo.GetDrives();

            DispatcherTimer autosaveTimer = new DispatcherTimer(TimeSpan.FromSeconds(ConfigClass.autosaveRefreshRate), DispatcherPriority.Background, 
                new EventHandler(DoAutoSave), Application.Current.Dispatcher);

            //foreach (DriveInfo driveInfo in drives)
            //    treeView.Items.Add(CreateTreeItem(driveInfo));
        }

        //Gets called when UserControl is loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            this.KeyDown += new KeyEventHandler(Zoom_In);
            this.KeyDown += new KeyEventHandler(Zoom_Out);
            this.KeyDown += new KeyEventHandler(Zoom_Reset);

            (this.DataContext as DrawerViewModel).MyCanvas = cnvImage;
            (this.DataContext as DrawerViewModel).MyPreview = imgPreview;
            (this.DataContext as DrawerViewModel).vmMousePoint = MousePosition;

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

        #endregion



        //Autosave

        #region Autosave

        #region Variables

        private double timeCounter = 0;

        #endregion

        private async void DoAutoSave(object sender, EventArgs e)
        {
            timeCounter += ConfigClass.autosaveRefreshRate;

            if (((this.DataContext as DrawerViewModel).ImagePath != null && (this.DataContext as DrawerViewModel).ChangeDetected == true)
                || timeCounter > ConfigClass.autosaveIntervall)
            {
                (this.DataContext as DrawerViewModel).ChangeDetected = false;
                timeCounter = 0;

                (this.DataContext as DrawerViewModel).ExportToPascal(DrawerViewModel.CallMode.Autosave);

                DoubleAnimation increaseOpacity = new DoubleAnimation(ConfigClass.minOpacity, ConfigClass.maxOpacity, ConfigClass.durationSaveIconAnimated);
                DoubleAnimation decreaseOpacity = new DoubleAnimation(ConfigClass.maxOpacity, ConfigClass.minOpacity, ConfigClass.durationSaveIconAnimated);

                saveIcon.BeginAnimation(Viewbox.OpacityProperty, increaseOpacity);
                await Task.Delay(ConfigClass.durationSaveIconShown);
                saveIcon.BeginAnimation(Viewbox.OpacityProperty, decreaseOpacity);
            }
        }

        #endregion



        //Variables

        #region Variables

        public System.Windows.Point MousePosition { get; set; }

        #endregion



        //MouseEvents

        #region Define MouseEvents on Image/DrawCanvas

        /// <summary>
        /// Select Rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).SelectCustomShape();
        }


        private int tmpRVCount = 0;
        /// <summary>
        /// Handels all MouseMove-Events:
        /// - Add Rectangle
        /// - Resize "
        /// - Move "
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgCamera_MouseMove(object sender, MouseEventArgs e)
        {
            MousePosition = e.GetPosition(cnvImage);
            (this.DataContext as DrawerViewModel).vmMousePoint = MousePosition;
            txtBox.Content = "X: " + (int)MousePosition.X + "; Y: " + (int)MousePosition.Y;
            txtBox1.Content = (this.DataContext as DrawerViewModel).ImagePath;

            //Create Rectangle region
            if ((this.DataContext as DrawerViewModel).MouseHandlingState == DrawerViewModel.MouseState.CreateRectangle)
            {

                if (tmpRVCount != (this.DataContext as DrawerViewModel).RectanglesView.Count())
                {
                    tmpRVCount = (this.DataContext as DrawerViewModel).RectanglesView.Count();
                    int zIndex = tmpRVCount + 2;

                    Panel.SetZIndex(horizontalLine, zIndex);
                    Panel.SetZIndex(verticalLine, zIndex);
                }

                horizontalLine.Y1 = MousePosition.Y;
                horizontalLine.Y2 = MousePosition.Y;
                horizontalLine.StrokeThickness = 1;
                verticalLine.X1 = MousePosition.X;
                verticalLine.X2 = MousePosition.X;
                verticalLine.StrokeThickness = 1;

                Mouse.OverrideCursor = Cursors.Cross;
                (this.DataContext as DrawerViewModel).CreateRectangle(MousePosition);
            }

            //Detect Rectangle
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
                    (this.DataContext as DrawerViewModel).DetectCustomShape(MousePosition);
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

                //Resize and Move Rectangle
                (this.DataContext as DrawerViewModel).Resize(MousePosition);
                (this.DataContext as DrawerViewModel).Move(MousePosition);
            }
        }


        /// <summary>
        /// Checks if undo stack is empty and if not enables Undo functionallity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).undoCustomShapes.Count > 0)
            {
                (this.DataContext as DrawerViewModel).UndoEnabled = true;
            }
        }


        /// <summary>
        /// Detects when Mouse leaves Image for DuplicateMethod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cnvImage_MouseLeave(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 0;
            Mouse.OverrideCursor = Cursors.Arrow;
        }


        /// <summary>
        /// Detects when Mouse enters Image for DuplicateMethod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cnvImage_MouseEnter(object sender, MouseEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DuplicateVar = 1;
        }


        /// <summary>
        /// CapturesMouse to detect MouseUp events even when outside of DrawCanvas/Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).Enabled == false)
            {
                UIElement element = (UIElement)sender;
                element.CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Releases MouseCapture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Release the mouse capture, if this element held the capture.
            UIElement element = (UIElement)sender;
            element.ReleaseMouseCapture();
            e.Handled = true;
        }


        #endregion



        //MenuItems

        #region MenuItems

        /// <summary>
        /// Resets Zoom when new Image is Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OpenClick(object sender, RoutedEventArgs e)
        {
            if (imgPreview != null) 
                zoomBorder.Reset();
        }

        /// <summary>
        /// Reset Zoom and sets Zoomview to whole image Width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Reset(object sender, RoutedEventArgs e)
        {
            if ((this.DataContext as DrawerViewModel).ImagePath != null)
                zoomBorder.Reset();
        }

        /// <summary>
        /// Zoom out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ZoomOut(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOut();
        }

        /// <summary>
        /// Zoom in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ZoomIn(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomIn();
        }

        #endregion



        //Different Panels

        #region FileNavigation-TreeView Panel 

        #region Variables
        public string filenameTreeView { get; set; }

        #endregion


        #region Graphics of TreeView show/hide button

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

        #endregion


        /// <summary>
        /// Show/Hide FileNavigation TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// Loads file which was selected in FileNavigation-TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    (this.DataContext as DrawerViewModel).SortList();
                    (this.DataContext as DrawerViewModel).FilterName();
                    (this.DataContext as DrawerViewModel).ClearUndoRedoStack();
                    zoomBorder.Reset();

                }
            }
        }

        #endregion


        #region RectangleListView-Panel


        #region ContextMenu
        /// <summary>
        /// Refreshes ContextMenu Elements depending on selected Rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ListBox_RightButtonUp(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as DrawerViewModel).RefreshSubtypeList();
            (this.DataContext as DrawerViewModel).IsOpen = true;
        }

        /// <summary>
        /// Forces Subtypes to Update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Subtype_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ((MenuItem)sender).GetBindingExpression(MenuItem.ItemsSourceProperty).UpdateTarget();
        }

        /// <summary>
        /// Refreshes Rectangles List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenu_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            (this.DataContext as DrawerViewModel).FilterName();
        }
        #endregion


        /// <summary>
        /// Select Rectangle depening on selected ListElement and viceversa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Changes ListView between List and Gallary List Mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Selects element of RectangleList when clicking on it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblTextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            item.IsSelected = true;
        }


        #endregion


        #region ButtonPanel

        //for ZoomButtons look at #region MenuItems

        #endregion


        #region LabelList-Navigation-Panel

        #region Label Sets Control-Panel

        private void AddLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).AddLabelColorFormat();
        }

        #endregion

        #region LabelList

        #region TextBox

        /// <summary>
        /// Select correct TreeViewItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectTreeViewItem(sender);
        }

        /// <summary>
        /// Activate TextBox for renaming Label/Subtype
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Clears tmpLabel to prevent problems for next renaming
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Updates Rectangles Label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Show/Hide Rectangles with according Label/Subtype
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Adds Suptype to Label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Removes Label/Subtype
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            animatedRoatateTransform.Angle = 0;
            listBoxLabels.Visibility = Visibility.Hidden;
            listViewMode.Visibility = Visibility.Hidden;
            ColorPicker_Panel.Visibility = Visibility.Visible;
            (this.DataContext as DrawerViewModel).IsEnabled = false;
            gridLV.Width = new GridLength(265);
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
            listViewMode.Visibility = Visibility.Visible;
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

        /// <summary>
        /// Change selected Label to new SelectedItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        #endregion



        //Key-Bindings

        #region Key-Bindings for Zoom

        /// <summary>
        /// Zoom in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Zoom_In(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemPlus || e.Key == Key.Add) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.ZoomIn();
            }
        }

        /// <summary>
        /// Zoom out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Zoom_Out(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.ZoomOut();
            }
        }

        /// <summary>
        /// Reset Zoom and sets Zoomview to whole image Width
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Zoom_Reset(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                zoomBorder.Reset();
            }
        }

        #endregion



        //Property Changed Area

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


    }
}