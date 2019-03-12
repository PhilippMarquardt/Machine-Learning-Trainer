using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.IO;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MachineLearningTrainer.DrawerTool;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Threading;

namespace MachineLearningTrainer.DrawerTool 
{
    public class DrawerViewModel : INotifyPropertyChanged, IComparable
    {
        #region PropertyChangedArea
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

        #region RaisePropertChanged Area
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        #endregion

        private DrawerModel _drawerModel;
        private bool _canExecute = true;
        private MainModel _mainModel;
        private Grid _mainGrid;
        private MainViewModel _mainViewModel;

        public DrawerViewModel(DrawerModel drawerModel, MainModel model, Grid mainGrid, MainViewModel mainViewModel)
        {
            this._drawerModel = drawerModel;
            this._mainGrid = mainGrid;
            this._mainModel = model;
            this._mainViewModel = mainViewModel;
            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            DuplicateCommand = new MyICommand(OnDuplicate, CanDuplicate);
            DuplicateMenuCommand = new MyICommand(OnDuplicateMenu, CanDuplicateMenu);
            RenameCommand = new MyICommand(OnRename, CanRename);
            ComboBoxItems.Add("All Labels");
            AllRectanglesView = AllRectangles;
            SelectedComboBoxItem = "All Labels";
            undoRectangles.Push(new ResizableRectangle());
            undoInformation.Push("Dummy");
            redoRectangles.Push(new ResizableRectangle());
            redoInformation.Push("Dummy");
        }



        public MyICommand DeleteCommand { get; set; }
        public MyICommand DuplicateCommand { get; set; }
        public MyICommand RenameCommand { get; set; }
        public MyICommand DuplicateMenuCommand { get; set; }
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// compare rectangle text to each other
        /// </summary>
        public int CompareTo(object obj)
        {
            ResizableRectangle resizable = obj as ResizableRectangle;
            if (resizable == null)
            {
                throw new ArgumentException("Object is not Rectangle");
            }
            return this.RectangleText.CompareTo(resizable.RectangleText);
        }

        public ObservableCollection<ResizableRectangle> AllRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();

        public ObservableCollection<ResizableRectangle> AllRectanglesView { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<ResizableRectangle> FilteredRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<string> ComboBoxItems { get; set; } = new ObservableCollection<string>();

        public Stack<ResizableRectangle> undoRectangles { get; set; } = new Stack<ResizableRectangle>();
        public Stack<string> undoInformation { get; set; } = new Stack<string>();
        public Stack<ResizableRectangle> redoRectangles { get; set; } = new Stack<ResizableRectangle>();
        public Stack<string> redoInformation { get; set; } = new Stack<string>();

        private ICommand _undoStackCommand;
        public ICommand UndoCommand
        {
            get
            {
                return _undoStackCommand ?? (_undoStackCommand = new CommandHandler(() => Undo(), _canExecute));
            }
        }

        /// <summary>
        /// this method undo a rectangle on canvas. 
        /// </summary>
        private void Undo()
        {
            if (undoRectangles.Count() > 1 && undoInformation.Count() > 1)
            {
                // when you have added a rectangle, the undo command will delete the rectangle.
                if (undoInformation.Peek() == "Add")
                {
                    var top = undoRectangles.Pop();
                    var info = undoInformation.Pop();

                    redoRectangles.Push(top);
                    redoInformation.Push(info);

                    top.RectangleFill = System.Windows.Media.Brushes.Blue;
                    top.RectangleOpacity = 0.07;
                    AllRectangles.Remove(top);
                    AllRectanglesView = AllRectangles;
                    UpdateCropedImage(top);
                }

                // when you have deleted a rectangle, the undo command will add the rectangle.
                if (undoInformation.Peek() == "Delete")
                {
                    var top = undoRectangles.Pop();
                    var info = undoInformation.Pop();

                    redoRectangles.Push(top);
                    redoInformation.Push(info);

                    top.RectangleFill = System.Windows.Media.Brushes.Blue;
                    top.RectangleOpacity = 0.07;
                    AllRectangles.Add(top);
                    AllRectanglesView = AllRectangles;
                    UpdateCropedImage(top);
                }
            }
            OnPropertyChanged("");
        }

        private ICommand _redoStackCommand;
        public ICommand RedoCommand
        {
            get
            {
                return _redoStackCommand ?? (_redoStackCommand = new CommandHandler(() => Redo(), _canExecute));
            }
        }

        private void Redo()
        {
            // Undo the undo command :D
            if (redoRectangles.Count() > 1 && redoInformation.Count() > 1)
            {
                // see undo command documentation above.
                if (redoInformation.Peek() == "Add")
                {
                    var top = redoRectangles.Pop();
                    var info = redoInformation.Pop();

                    undoRectangles.Push(top);
                    undoInformation.Push(info);

                    top.RectangleFill = System.Windows.Media.Brushes.Blue;
                    top.RectangleOpacity = 0.07;

                    AllRectangles.Add(top);
                    AllRectanglesView = AllRectangles;
                    UpdateCropedImage(top);
                }

                // see undo documentation above.
                if (redoInformation.Peek() == "Delete")
                {
                    var top = redoRectangles.Pop();
                    var info = redoInformation.Pop();

                    undoRectangles.Push(top);
                    undoInformation.Push(info);

                    top.RectangleFill = System.Windows.Media.Brushes.Blue;
                    top.RectangleOpacity = 0.07;
                    AllRectangles.Remove(top);
                    AllRectanglesView = AllRectangles;
                    UpdateCropedImage(top);
                }
            }
            OnPropertyChanged("");
        }


        private ICommand _exportPascalVoc;
        public ICommand ExportPascalVoc
        {
            get
            {
                return _exportPascalVoc ?? (_exportPascalVoc = new CommandHandler(() => ExportToPascal(), _canExecute));
            }
        }

        /// <summary>
        /// export rectangles to xml file
        /// </summary>
        private void ExportToPascal()
        {
            string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";
            XMLWriter.WritePascalVocToXML(AllRectangles.ToList(), destFileName, 1337, 1337, 3);

            //UpdatePreviews();

            //string destFileName1 = ImagePath.Remove(ImagePath.LastIndexOf('.'));

            //foreach (var rec in AllRectangles)
            //{
            //    string path1 = destFileName1 + @"_Cropped_Images\" + rec.RectangleText + @"\";

            //    if (!Directory.Exists(path1))
            //    {
            //        Directory.CreateDirectory(path1);
            //    }

            //    if (rec.CroppedImage != null)
            //    {
            //        BitmapEncoder encoder = new PngBitmapEncoder();
            //        encoder.Frames.Add(BitmapFrame.Create(rec.CroppedImage));
            //        string filename = path1 + AllRectangles.IndexOf(rec) + ".png";
            //        using (var fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            //        {
            //            encoder.Save(fileStream);
            //        }
            //    }
            //}
        }


        private ICommand _addRectangle;
        public ICommand AddRectangle
        {
            get
            {
                return _addRectangle ?? (_addRectangle = new CommandHandler(() => AddNewRectangle(), _canExecute));
            }
        }


        /// <summary>
        /// this method let us draw a rectangle 
        /// </summary>
        public void AddNewRectangle()
        {
            SortList();
            MyCanvas.Cursor = Cursors.Cross;

            if (SelectedComboBoxItem != "All Labels")
            {
                OnPropertyChanged("AllRectanglesView");
                Enabled = false;
                _rectangleText = SelectedComboBoxItem;
            }

            else
            {
                AllRectanglesView = AllRectangles;
                OnPropertyChanged("AllRectanglesView");
                Enabled = false;
                if (IsChecked == true && DefaultLabel.Length > 0)
                    _rectangleText = _defaultLabel;
                else
                    _rectangleText = "";
            }
        }

        private ICommand _loadImageCommand;
        public ICommand LoadImageCommand
        {
            get
            {
                return _loadImageCommand ?? (_loadImageCommand = new CommandHandler(() => LoadImage(), _canExecute));
            }
        }

        /// <summary>
        /// opens filedialog and let us browse any images which ends with .jpg, .jped, .png and .tiff
        /// </summary>
        private void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files | *.jpg; *.jpeg; *.png; *.tif";

            if (openFileDialog.ShowDialog() == true)
                ImagePath = openFileDialog.FileName;

            if (ImagePath != null)
            {
                this.IsEnabled = true;
                AllRectangles.Clear();
                clearUndoRedoStack();
                LoadRectangles();
                ComboBoxNames();
                SortList();
                FilterName();
            }
        }

        public void clearUndoRedoStack()
        {
            // Clear Undo and Redo Stack
            undoRectangles.Clear();
            undoInformation.Clear();
            redoRectangles.Clear();
            redoInformation.Clear();

            // Add one Dummy item to each Stack
            undoRectangles.Push(new ResizableRectangle());
            undoInformation.Push("Dummy");
            redoRectangles.Push(new ResizableRectangle());
            redoInformation.Push("Dummy");

        }

        /// <summary>
        /// declares a string that stores the file path
        /// </summary>
        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return this._imagePath;
            }
            set
            {
                this._imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        /// <summary>
        /// declares a boolean variable that says if you can execute the command "Add Rectangle", "Update Preview",
        /// "Load XML" and "Export to XML"
        /// </summary>
        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                this._isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        private ICommand _previousPage;
        public ICommand PreviousPage
        {
            get
            {
                return _previousPage ?? (_previousPage = new CommandHandler(() => SetNextState(Command.Previous), _canExecute));
            }
        }

        /// <summary>
        /// browse to the previous page
        /// </summary>
        /// <param name="command"></param>
        public void SetNextState(Command command)
        {
            UserControl usc = this._mainModel.SetNextState(_mainGrid, command);
            this._mainGrid.Children.Clear();
            usc.DataContext = this._mainViewModel;
            this._mainGrid.Children.Add(usc);
        }

        private ResizableRectangle _selectedResizableRectangle;
        public ResizableRectangle SelectedResizableRectangle
        {
            get
            {
                return _selectedResizableRectangle;
            }

            set
            {
                if(value != null)
                {
                    _selectedResizableRectangle = value;
                    if (value.X < 0) _selectedResizableRectangle.X = 0;
                    if (value.Y < 0) _selectedResizableRectangle.Y = 0;
                    SelectedRectangleFill();
                    DeleteCommand.RaiseCanExecuteChanged();
                    DuplicateCommand.RaiseCanExecuteChanged();
                    DuplicateMenuCommand.RaiseCanExecuteChanged();
                    RenameCommand.RaiseCanExecuteChanged();
                }
                
            }
        }


        /// <summary>
        /// this method deletes the selection of rectangles
        /// </summary>
        private void OnDelete() 
        {
            for(int i = 0; i < AllRectangles.Count + 1; i++)
            {
                while(SelectedResizableRectangle != null)
                {
                    if(undoRectangles != null)
                    {
                        undoRectangles.Push(SelectedResizableRectangle);
                        undoInformation.Push("Delete");
                        AllRectangles.Remove(SelectedResizableRectangle);
                        AllRectanglesView.Remove(SelectedResizableRectangle);
                    }
                }
            }

            this.IsOpen = false;
            temp = SelectedComboBoxItem;
            ComboBoxNames();
            AllRectanglesView = AllRectangles;
            FilteredRectangles = AllRectangles;
            OnPropertyChanged("AllRectanglesView");
            OnPropertyChanged("FilteredRectangles");
            OnPropertyChanged("ComboBoxNames");
            SelectedComboBoxItem = temp;
            FilterName();
        }

        public string temp { get; set; }

        /// <summary>
        /// says if you can execute the "On Delete" method
        /// </summary>
        /// <returns></returns>
        private bool CanDelete()
        {
            return SelectedResizableRectangle != null;
        }

        private System.Windows.Point _vmMousePoint;
        public System.Windows.Point vmMousePoint
        {
            get { return _vmMousePoint; }
            set { _vmMousePoint = value; }
        }

        /// <summary>
        /// this method, let you duplicate the selected rectangle with its text, height, ... to current mouse position
        /// </summary>
        private async void OnDuplicate()
        {
            if (DuplicateVar == 1) 
            {
                SortList();
                ResizableRectangle DuplicateRect = new ResizableRectangle();

                DuplicateRect.RectangleHeight = SelectedResizableRectangle.RectangleHeight;
                DuplicateRect.RectangleWidth = SelectedResizableRectangle.RectangleWidth;
                DuplicateRect.RectangleText = SelectedResizableRectangle.RectangleText;
                DuplicateRect.X = vmMousePoint.X - SelectedResizableRectangle.RectangleWidth / 2;
                DuplicateRect.Y = vmMousePoint.Y - SelectedResizableRectangle.RectangleHeight / 2;

                Canvas.SetLeft(DuplicateRect, vmMousePoint.X - SelectedResizableRectangle.RectangleWidth / 2);
                Canvas.SetTop(DuplicateRect, vmMousePoint.Y - SelectedResizableRectangle.RectangleHeight / 2);

                undoRectangles.Push(DuplicateRect);
                undoInformation.Push("Add");

                AllRectanglesView.Insert(0, DuplicateRect);
                AllRectangles.Insert(0, DuplicateRect);
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("AllRectangles");
                await cropImageLabelBegin();

                if (SelectedComboBoxItem == "All Labels")
                {
                    RectangleCount = "#" + AllRectangles.Count.ToString();
                }

                else if (SelectedComboBoxItem != "All Labels")
                {
                    RectangleCount = "#" + AllRectanglesView.Count.ToString();
                }
            }

            else if (DuplicateVar == 0)
            {
                OnDuplicateMenu();
            }
        }


        private int _duplicateVar;

        public int DuplicateVar
        {
            get { return _duplicateVar; }
            set { _duplicateVar = value;
                OnPropertyChanged("DuplicateVar");
            }
        }


        /// <summary>
        /// this method, let you duplicate the selected rectangle with its text, height, ...
        /// </summary>
        public async void OnDuplicateMenu()
        {
            SortList();
            ResizableRectangle DuplicateRect = new ResizableRectangle();

            DuplicateRect.RectangleHeight = SelectedResizableRectangle.RectangleHeight;
            DuplicateRect.RectangleWidth = SelectedResizableRectangle.RectangleWidth;
            DuplicateRect.RectangleText = SelectedResizableRectangle.RectangleText;
            DuplicateRect.X = SelectedResizableRectangle.X + 30;
            DuplicateRect.Y = SelectedResizableRectangle.Y + 30;

            Canvas.SetLeft(DuplicateRect, SelectedResizableRectangle.X + 30);
            Canvas.SetTop(DuplicateRect, SelectedResizableRectangle.Y + 30);

            undoRectangles.Push(DuplicateRect);
            undoInformation.Push("Add");

            AllRectanglesView.Insert(0, DuplicateRect);
            AllRectangles.Insert(0, DuplicateRect);
            OnPropertyChanged("AllRectanglesView");
            OnPropertyChanged("AllRectangles");
            await cropImageLabelBegin();

            if (SelectedComboBoxItem == "All Labels")
            {
                RectangleCount = "#" + AllRectangles.Count.ToString();
            }

            else if (SelectedComboBoxItem != "All Labels")
            {
                RectangleCount = "#" + AllRectanglesView.Count.ToString();
            }
        }

        /// <summary>
        /// says if you can execute the "On Duplicate" method
        /// </summary>
        private bool CanDuplicate()
        {
            return SelectedResizableRectangle != null;
        }

        private bool CanDuplicateMenu()
        {
            return SelectedResizableRectangle != null;
        }

        /// <summary>
        /// this method rename all files in the listox
        /// </summary>
        private void OnRename()
        {
            foreach(var rec in AllRectangles)
            {
                rec.RectangleText = DefaultLabel;
            }
        }

        /// <summary>
        /// says if you can execute "Can Rename" method
        /// </summary>
        private bool CanRename()
        {
            return SelectedResizableRectangle != null;
        }

        private ICommand _deleteRectanglesCommand;
        public ICommand DeleteRectanglesCommand
        {
            get
            {
                return _deleteRectanglesCommand ?? (_deleteRectanglesCommand = new CommandHandler(() => DeleteAll(), _canExecute));
            }
        }

        /// <summary>
        /// this method deletes the complete list with all its rectangles
        /// </summary>
        private void DeleteAll()
        {
            AllRectangles.Clear();
            FilterName();
        }

        /// <summary>
        /// string variabel, which contains the name of rectangle
        /// </summary>
        private string _rectangleText;
        public string RectangleText
        {
            get
            {
                return _rectangleText;
            }

            set
            {
                _rectangleText = value;
                OnPropertyChanged("RectangleText");
            }
        }

        /// <summary>
        /// boolean, which tells if checkbox from default label is checked
        /// </summary>
        private bool _isChecked = true;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        private bool _cropModeChecked = false;

        public bool CropModeChecked
        {
            get
            {
                return _cropModeChecked;
            }
            set
            {
                _cropModeChecked = value;
                SelectedRectangleFill();
                OnPropertyChanged("CropModeChecked");
            }
        }

        /// <summary>
        /// this is the default label. if you add multiple rectangles and do not want to manually enter 
        /// the name each time, the name is stored in default label
        /// </summary>
        private string _defaultLabel = "";

        public string DefaultLabel
        {
            get
            {
                return _defaultLabel;
            }
            set
            {
                _defaultLabel = value;
                RaisePropertyChanged("DefaultLabel");
            }

        }

        /// <summary>
        /// variable, which includes the opacity of the rectangle
        /// </summary>
        private double _rectangleOpacity = 0.07;

        public double RectangleOpacity
        {
            get
            {
                return _rectangleOpacity;
            }
            set
            {
                _rectangleOpacity = value;
                OnPropertyChanged("RectangleOpacity");
            }
        }

        /// <summary>
        /// variable, which includes the thum size of the rectangle
        /// </summary>
        private int _thumbSize = 3;

        public int ThumbSize
        {
            get
            {
                return _thumbSize;
            }
            set
            {
                _thumbSize = value;
                OnPropertyChanged("ThumbSize");
            }
        }

        /// <summary>
        /// variable, which contains the color of unselected rectangles
        /// </summary>
        public System.Windows.Media.Brush _rectangleFill = System.Windows.Media.Brushes.Blue;

        public System.Windows.Media.Brush RectangleFill
        {
            get
            {
                return _rectangleFill;
            }
            set
            {
                _rectangleFill = value;
                OnPropertyChanged("RectangleFill");
            }
        }

        /// <summary>
        /// variable, which contains the color of the rectangle thumbs
        /// </summary>
        public System.Windows.Media.Brush _thumbColor = System.Windows.Media.Brushes.LawnGreen;

        public System.Windows.Media.Brush ThumbColor
        {
            get
            {
                return _thumbColor;
            }
            set
            {
                _thumbColor = value;
                OnPropertyChanged("ThumbColor");
            }
        }

        public System.Windows.Media.Brush _resizeThumbColor = System.Windows.Media.Brushes.Gray;

        public System.Windows.Media.Brush ResizeThumbColor
        {
            get
            {
                return _resizeThumbColor;
            }
            set
            {
                _resizeThumbColor = value;
                OnPropertyChanged("ResizeThumbColor");
            }
        }

        public System.Windows.Media.Brush _rectangleOverFill = System.Windows.Media.Brushes.Gray;

        public System.Windows.Media.Brush RectangleOverFill
        {
            get
            {
                return _rectangleOverFill;
            }
            set
            {
                _rectangleOverFill = value;
                OnPropertyChanged("RectangleOverFill");
            }
        }


        /// <summary>
        /// this method colors the selected rectangle and increases the opacity
        /// </summary>
        public void SelectedRectangleFill()
        {
            if (SelectedResizableRectangle != null)
            {
                if(CropModeChecked == false)
                {
                    foreach (var rect in AllRectangles)
                    {
                        rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                        rect.RectangleOpacity = 0.07;
                        rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                        rect.ThumbSize = 3;
                        rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
                        rect.Visibility = Visibility.Visible;
                    }
                    SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
                    SelectedResizableRectangle.RectangleOpacity = 0.0;
                    SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.ThumbSize = 3;
                    SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.Visibility = Visibility.Visible;
                }

                if(CropModeChecked == true)
                {
                    foreach (var rect in AllRectangles)
                    {
                        rect.RectangleFill = null;
                        rect.RectangleOpacity = 0.0;
                        rect.ThumbColor = System.Windows.Media.Brushes.Transparent;
                        rect.ThumbSize = 3;
                        rect.ResizeThumbColor = System.Windows.Media.Brushes.Transparent;
                        rect.Visibility = Visibility.Collapsed;

                    }

                    SelectedResizableRectangle.Visibility = Visibility.Visible;
                    SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
                    SelectedResizableRectangle.RectangleOpacity = 0.0;
                    SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.ThumbSize = 3;
                    SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
                }
            }
            
            if(SelectedResizableRectangle == null && CropModeChecked == false)
            {
                foreach (var rect in AllRectangles)
                {
                    rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                    rect.RectangleOpacity = 0.07;
                    rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                    rect.ThumbSize = 3;
                    rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
                    rect.Visibility = Visibility.Visible;
                }
            }
        }

        

        private ICommand _deleteSelectionRectangle;
        public ICommand DeleteSelectionRectangle
        {
            get
            {
                return _deleteSelectionRectangle ?? (_deleteSelectionRectangle = new CommandHandler(() => DeleteSelection(), _canExecute));
            }
        }

        /// <summary>
        /// this method unselect the selected rectangle
        /// </summary>
        public void DeleteSelection()
        {
            SelectedResizableRectangle = null;

            foreach (var rect in AllRectangles)
            {
                rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                rect.RectangleOpacity = 0.07;
                rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                rect.ThumbSize = 3;
                rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
            }
            Enabled = true;
            MyCanvas.Cursor = Cursors.Arrow;
        }

        private ICommand _enterCommand;
        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand ?? (_enterCommand = new CommandHandler(() => Enter(), _canExecute));
            }
        }

        public void Enter()
        {
            FilterName();
            SortList();
        }
        
        private BitmapImage _croppedImage;
        public BitmapImage CroppedImage
        {
            get
            {
                return _croppedImage;
            }
            set
            {
                _croppedImage = value;
                OnPropertyChanged("CroppedImage");
            }
        }

        private double x;
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        private double y;

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _dst;
        public string dst
        {
            get { return _dst; }
            set { _dst = value; }
        }

        /// <summary>
        /// this method loads all rectangles from an xml file and draws them on the canvas
        /// </summary>
        public void LoadRectangles()
        {
            string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";

            if (File.Exists(destFileName) == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(destFileName);

                foreach (XmlNode node in doc.DocumentElement)
                {

                    if (node.Name == "object")
                    {
                        foreach (XmlNode objectChild in node)
                        {
                            if (objectChild.Name == "name")
                            {
                                name = objectChild.InnerText;
                                RectangleText = name;
                            }

                            if (objectChild.Name == "bndbox")
                            {
                                int xmin = int.Parse(objectChild["xmin"].InnerText);
                                int ymin = int.Parse(objectChild["ymin"].InnerText);
                                int xmax = int.Parse(objectChild["xmax"].InnerText);
                                int ymax = int.Parse(objectChild["ymax"].InnerText);
                                
                                    ResizableRectangle loadedRect = new ResizableRectangle();

                                    loadedRect.RectangleHeight = ymax - ymin;
                                    loadedRect.RectangleWidth = xmax - xmin;
                                    loadedRect.RectangleText = name;
                                    loadedRect.X = xmin;
                                    loadedRect.Y = ymin;

                                    Canvas.SetLeft(loadedRect, xmin);
                                    Canvas.SetTop(loadedRect, ymin);

                                    AllRectangles.Add(loadedRect);
                            }
                        }
                    }
                }
            }
        }

        private bool _isOpen = false;

        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }

            set
            {
                this._isOpen = value;
                OnPropertyChanged("IsOpen");
            }
        }

        /// <summary>
        /// sorts the list of rectangles in ascending order of their name
        /// </summary>
        public void SortList()
        {
            ObservableCollection<ResizableRectangle> sortedRectangles = new ObservableCollection<ResizableRectangle>
                (AllRectangles.OrderBy(resizable => resizable.RectangleText));

            AllRectangles = sortedRectangles;
            OnPropertyChanged("AllRectangles");
        }

        /// <summary>
        /// this method filters the list, depending on which label is selected in the combobox
        /// </summary>
        public void FilterName()
        {
            if (SelectedComboBoxItem == "All Labels")
            {
                if (ListViewImage.Contains("grid"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = true;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                else if (ListViewImage.Contains("list"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = true;
                    FilterVisibilitySelected = false;
                }
                
                AllRectanglesView = AllRectangles;
                RectangleCount = "#" + AllRectangles.Count.ToString();
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("RectangleCount");
                OnPropertyChanged("FilterVisibilitySelected");
                OnPropertyChanged("FilterVisibilitySelectedGallery");
                OnPropertyChanged("FilterVisibilityAllLabels");
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");
                
            }

            else if (SelectedComboBoxItem != "All Labels")
            {
                if (ListViewImage.Contains("grid"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = true;
                }

                else if (ListViewImage.Contains("list"))
                {
                    FilterVisibilitySelectedGallery = true;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                DefaultLabel = SelectedComboBoxItem;
                RectangleCount = "#" + AllRectanglesView.Count.ToString();

                ObservableCollection<ResizableRectangle> FilteredRectangles = new ObservableCollection<ResizableRectangle>
                    (AllRectangles.Where(AllRectangles => AllRectangles.RectangleText == SelectedComboBoxItem));
                AllRectanglesView = FilteredRectangles;
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("DefaultLabel");
                OnPropertyChanged("RectangleCount");
                OnPropertyChanged("FilterVisibilitySelected");
                OnPropertyChanged("FilterVisibilitySelectedGallery");
                OnPropertyChanged("FilterVisibilityAllLabels");
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");
            }
        }

        /// <summary>
        /// this method adds all different labels to the combobox
        /// </summary>
        public void ComboBoxNames()
        {
            temp = SelectedComboBoxItem;
            ComboBoxItems.Clear();
            ComboBoxItems.Add("All Labels");
            SelectedComboBoxItem = temp;

            foreach (var rec in AllRectangles)
            {
                if (!ComboBoxItems.Contains(rec.RectangleText))
                {
                    ComboBoxItems.Add(rec.RectangleText);
                    OnPropertyChanged("ComboBoxItems");
                }
            }
        }

        private string _selectedComboBoxItem;

        public string SelectedComboBoxItem
        {
            get
            {
                return _selectedComboBoxItem;
            }
            set
            {
                _selectedComboBoxItem = value;
                OnPropertyChanged("SelectedComboBoxItem");
            }
        }

        private bool _filterVisibilitySelected = false;

        public bool FilterVisibilitySelected
        {
            get
            {
                return _filterVisibilitySelected;
            }
            set
            {
                _filterVisibilitySelected = value;
                OnPropertyChanged("FilterVisibility1");
            }
        }

        private bool _filterVisibilityAllLabels = true;

        public bool FilterVisibilityAllLabels
        {
            get
            {
                return _filterVisibilityAllLabels;
            }
            set
            {
                _filterVisibilityAllLabels = value;
                OnPropertyChanged("FilterVisibility");
            }
        }

        private bool _filterVisibilitySelectedGallery = false;

        public bool FilterVisibilitySelectedGallery
        {
            get
            {
                return _filterVisibilitySelectedGallery;
            }
            set
            {
                _filterVisibilitySelectedGallery = value;
                OnPropertyChanged("FilterVisibilitySelectedGallery");
            }
        }

        private bool _filterVisibilityAllLabelsGallery = false;

        public bool FilterVisibilityAllLabelsGallery
        {
            get
            {
                return _filterVisibilityAllLabelsGallery;
            }
            set
            {
                _filterVisibilityAllLabelsGallery = value;
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");
            }
        }
        
        private ICommand _loadXMLCommand;
        public ICommand LoadXMLCommand
        {
            get
            {
                return _loadXMLCommand ?? (_loadXMLCommand = new CommandHandler(() => LoadXML(), _canExecute));
            }
        }

        /// <summary>
        /// with this method, you can open an xml file from a different location as the loaded image.
        /// </summary>
        private async void LoadXML()
        {
                this.IsEnabled = true;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML Files | *.xml";

                if (openFileDialog.ShowDialog() == true)
                    dst = openFileDialog.FileName;

                if (dst != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(dst);

                    foreach (XmlNode node in doc.DocumentElement)
                    {

                        if (node.Name == "object")
                        {
                            foreach (XmlNode objectChild in node)
                            {
                                if (objectChild.Name == "name")
                                {
                                    name = objectChild.InnerText;
                                    RectangleText = name;
                                }

                                if (objectChild.Name == "bndbox")
                                {
                                    int xmin = int.Parse(objectChild["xmin"].InnerText);
                                    int ymin = int.Parse(objectChild["ymin"].InnerText);
                                    int xmax = int.Parse(objectChild["xmax"].InnerText);
                                    int ymax = int.Parse(objectChild["ymax"].InnerText);

                                    ResizableRectangle loadedRect = new ResizableRectangle();

                                    loadedRect.RectangleHeight = ymax - ymin;
                                    loadedRect.RectangleWidth = xmax - xmin;
                                    loadedRect.RectangleText = name;
                                    loadedRect.X = xmin;
                                    loadedRect.Y = ymin;

                                    Canvas.SetLeft(loadedRect, xmin);
                                    Canvas.SetTop(loadedRect, ymin);

                                    AllRectangles.Add(loadedRect);
                                    AllRectanglesView = AllRectangles;
                                    OnPropertyChanged("");
                                }
                            }
                        }
                    }
                }
                ComboBoxNames();
                SortList();
                await cropImageLabelBegin();
          
            
        }

        private string _rectangleCount;
        public string RectangleCount
        {
            get
            {
                return _rectangleCount;
            }
            set
            {
                _rectangleCount = value;
                OnPropertyChanged("RectangleCount");
            }
        }

        private Canvas _myCanvas;

        public Canvas MyCanvas
        {
            get { return _myCanvas; }
            set { _myCanvas = value; }
        }

        private System.Windows.Controls.Image _myPreview;

        public System.Windows.Controls.Image MyPreview
        {
            get { return _myPreview; }
            set { _myPreview = value; }
        }

        private double _zoomBorderWidth;

        public double ZoomBorderWidth
        {
            get { return _zoomBorderWidth; }
            set { _zoomBorderWidth = value; }
        }

        private double _zoomBorderHeight;

        public double ZoomBorderHeight
        {
            get { return _zoomBorderWidth; }
            set { _zoomBorderWidth = value; }
        }


        /// <summary>
        /// update only the cropped image of the selected rectangle
        /// </summary>
        /// <param name="resizable"></param>
        public void UpdateCropedImage(ResizableRectangle resizable)
        {
            if (resizable.RectangleHeight > 5 && resizable.RectangleWidth > 5)
            {
                BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
                Bitmap src;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    src = new Bitmap(bitmap);
                }

                if (resizable.X > 0 && resizable.X + resizable.RectangleWidth < MyCanvas.ActualWidth && resizable.Y > 0 && resizable.Y + resizable.RectangleHeight < MyCanvas.ActualHeight)
                {
                    Mat mat = SupportCode.ConvertBmp2Mat(src);
                    OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)resizable.X, (int)resizable.Y, (int)resizable.RectangleWidth, (int)resizable.RectangleHeight);

                    Mat croppedImage = new Mat(mat, rectCrop);
                    resizable.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
                }
            }
                
        }

        public void SelectClickedRectangle(ResizableRectangle resizableRectangle)
        {
            SelectedResizableRectangle = resizableRectangle;
            OnPropertyChanged("SelectedResizableRectangle");
        }

        private BitmapImage _recI;

        public BitmapImage RECI
        {
            get { return _recI; }
            set { _recI = value;
                OnPropertyChanged("RECI");
            }
        }

        /// <summary>
        /// this method only cropes images when there is an item without a cropped image in the list
        /// </summary>
        public async Task cropImageLabelBegin()
        {
            if (MyPreview.Source != null)
            {
                BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
                Bitmap src;

                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    src = new Bitmap(bitmap);
                }

                foreach (var rec in AllRectanglesView)
                {

                    if (rec.X > 0 && rec.X + rec.RectangleWidth < MyCanvas.ActualWidth && rec.Y > 0 && rec.Y + rec.RectangleHeight < MyCanvas.ActualHeight && rec.CroppedImage == null)
                    {
                        double RECX = rec.X;
                        double RECY = rec.Y;
                        double RECH = rec.RectangleHeight;
                        double RECW = rec.RectangleWidth;
                        RECI = rec.CroppedImage;

                        await Task.Run(() =>
                        {
                            
                            Mat mat = SupportCode.ConvertBmp2Mat(src);
                            OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);

                            Mat croppedImage = new Mat(mat, rectCrop);
                            RECI = SupportCode.ConvertMat2BmpImg(croppedImage);
                        });

                        rec.CroppedImage = RECI;
                        OnPropertyChanged("CroppedImage");
                    }
                }
            }
        }

        private ICommand _updatePreviewsCommand;
        public ICommand UpdatePreviewsCommand
        {
            get
            {
                return _updatePreviewsCommand ?? (_updatePreviewsCommand = new CommandHandler(() => UpdatePreviews(), _canExecute));
            }
        }

        /// <summary>
        /// this method updates all cropped images in the list
        /// </summary>
        public async Task UpdatePreviews()
        {
            BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
            Bitmap src;

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                src = new Bitmap(bitmap);
            }

            foreach (var rec in AllRectanglesView)
            {
                if (rec.X > 0 && rec.X + rec.RectangleWidth < MyCanvas.ActualWidth && rec.Y > 0 && rec.Y + rec.RectangleHeight < MyCanvas.ActualHeight && rec.CroppedImage == null)
                {
                    rec.CroppedImage = null;
                    double RECX = rec.X;
                    double RECY = rec.Y;
                    double RECH = rec.RectangleHeight;
                    double RECW = rec.RectangleWidth;
                    RECI = rec.CroppedImage;

                    await Task.Run(() =>
                    {

                        Mat mat = SupportCode.ConvertBmp2Mat(src);
                        OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);

                        Mat croppedImage = new Mat(mat, rectCrop);
                        RECI = SupportCode.ConvertMat2BmpImg(croppedImage);
                    });

                    rec.CroppedImage = RECI;
                    OnPropertyChanged("CroppedImage");
                }
            }
        }

        #region KeyArrowCommands

        private ICommand _rightButtonCommand;
        public ICommand RightButtonCommand
        {
            get
            {
                return _rightButtonCommand ?? (_rightButtonCommand = new CommandHandler(() => RightButton(), _canExecute));
            }
        }

        public void RightButton()
        {
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleWidth > 5)
            {
                SelectedResizableRectangle.X = SelectedResizableRectangle.X + 2;
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth - 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        private ICommand _leftButtonCommand;
        public ICommand LeftButtonCommand
        {
            get
            {
                return _leftButtonCommand ?? (_leftButtonCommand = new CommandHandler(() => LeftButton(), _canExecute));
            }
        }

        public void LeftButton()
        {
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleWidth > 5)
            {
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth - 2;
                UpdateCropedImage(SelectedResizableRectangle);
            }
                
        }

        private ICommand _upButtonCommand;
        public ICommand UpButtonCommand
        {
            get
            {
                return _upButtonCommand ?? (_upButtonCommand = new CommandHandler(() => UpButton(), _canExecute));
            }
        }
        public void UpButton()
        {
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleHeight > 5)
            {
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight - 2;
                UpdateCropedImage(SelectedResizableRectangle);
            }
                
        }

        private ICommand _downButtonCommand;
        public ICommand DownButtonCommand
        {
            get
            {
                return _downButtonCommand ?? (_downButtonCommand = new CommandHandler(() => DownButton(), _canExecute));
            }
        }

        public void DownButton()
        {
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleHeight > 5)
            {
                SelectedResizableRectangle.Y = SelectedResizableRectangle.Y + 2;
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight - 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
            }
                
        }

        private ICommand _rightButtonCommand1;
        public ICommand RightButtonCommand1
        {
            get
            {
                return _rightButtonCommand1 ?? (_rightButtonCommand1 = new CommandHandler(() => RightButton1(), _canExecute));
            }
        }

        public void RightButton1()
        {
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.X > 1)
            {
                SelectedResizableRectangle.X = SelectedResizableRectangle.X - 2;
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth + 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        private ICommand _leftButtonCommand1;
        public ICommand LeftButtonCommand1
        {
            get
            {
                return _leftButtonCommand1 ?? (_leftButtonCommand1 = new CommandHandler(() => LeftButton1(), _canExecute));
            }
        }

        public void LeftButton1()
        {
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.X + SelectedResizableRectangle.RectangleWidth + 2 < MyCanvas.ActualWidth)
            {
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth + 2;
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        private ICommand _upButtonCommand1;
        public ICommand UpButtonCommand1
        {
            get
            {
                return _upButtonCommand1 ?? (_upButtonCommand1 = new CommandHandler(() => UpButton1(), _canExecute));
            }
        }
        public void UpButton1()
        {
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.Y + SelectedResizableRectangle.RectangleHeight + 2 < MyCanvas.ActualHeight)
            {
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight + 2;
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        private ICommand _downButtonCommand1;
        public ICommand DownButtonCommand1
        {
            get
            {
                return _downButtonCommand1 ?? (_downButtonCommand1 = new CommandHandler(() => DownButton1(), _canExecute));
            }
        }

        public void DownButton1()
        {
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.Y > 1)
            {
                SelectedResizableRectangle.Y = SelectedResizableRectangle.Y - 2;
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight + 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        #endregion

        private ICommand _listViewCommand;
        public ICommand ListViewCommand
        {
            get
            {
                return _listViewCommand ?? (_listViewCommand = new CommandHandler(() => ListView(), _canExecute));
            }
        }

        private string _listViewImage = @"/Icons/grid_view.png";

        public string ListViewImage
        {
            get
            {
                return _listViewImage;
            }
            set
            {
                _listViewImage = value;
                OnPropertyChanged("ListViewImage");
            }
        }

        private void ListView()
        {
            if (ListViewImage.Contains("grid"))
            {
                ListViewImage = @"/Icons/list_view.png";
                OnPropertyChanged("ListViewTextVisibility");

                if (SelectedComboBoxItem == "All Labels")
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = true;
                    FilterVisibilitySelected = false;

                }

                else
                {
                    FilterVisibilitySelectedGallery = true;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }
            }

            else if (ListViewImage.Contains("list"))
            {
                ListViewImage = @"/Icons/grid_view.png";
                OnPropertyChanged("ListViewTextVisibility");

                if (SelectedComboBoxItem == "All Labels")
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = true;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                else
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = true;
                }
            }

            OnPropertyChanged("FilterVisibilitySelected");
            OnPropertyChanged("FilterVisibilitySelectedGallery");
            OnPropertyChanged("FilterVisibilityAllLabels");
            OnPropertyChanged("FilterVisibilityAllLabelsGallery");

        }


        private bool _undoEnabled = false;

        public bool UndoEnabled
        {
            get
            {
                return _undoEnabled;
            }

            set
            {
                _undoEnabled = value;
                OnPropertyChanged("UndoEnabled");
            }
        }

    }

}
