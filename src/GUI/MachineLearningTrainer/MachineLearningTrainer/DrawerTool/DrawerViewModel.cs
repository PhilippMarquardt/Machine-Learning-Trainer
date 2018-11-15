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
            RenameCommand = new MyICommand(OnRename, CanRename);
            ComboBoxItems.Add("All Labels");
            AllRectanglesView = AllRectangles;
            SelectedComboBoxItem = "All Labels";
        }

        //TODO: Mouse event handler
        //private ICommand _imageMouseDown;

        //public ICommand ImageMouseDown
        //{
        //    get
        //    {
        //        return _imageMouseDown ?? (_imageMouseDown = new CommandHandler(() => UpdateCropedImage(), _canExecute));
        //    }
        //}

        public MyICommand DeleteCommand { get; set; }
        public MyICommand DuplicateCommand { get; set; }
        public MyICommand RenameCommand { get; set; }
        public bool Enabled { get; set; } = true;

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

        private ICommand _exportPascalVoc;
        public ICommand ExportPascalVoc
        {
            get
            {
                return _exportPascalVoc ?? (_exportPascalVoc = new CommandHandler(() => ExportToPascal(), _canExecute));
            }
        }

        private void ExportToPascal()
        {
            string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";
            XMLWriter.WritePascalVocToXML(AllRectangles.ToList(), destFileName, 1337, 1337, 3);
        }


        private ICommand _addRectangle;
        public ICommand AddRectangle
        {
            get
            {
                return _addRectangle ?? (_addRectangle = new CommandHandler(() => AddNewRectangle(), _canExecute));
            }
        }

        public void AddNewRectangle()
        {
            //AllRectanglesView = AllRectangles;
            //OnPropertyChanged("AllRectanglesView");

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

                LoadRectangles();
                ComboBoxNames();
                SortList();
                FilterName();
            }
        }

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
                _selectedResizableRectangle = value;
                SelectedRectangleFill();
                DeleteCommand.RaiseCanExecuteChanged();
                DuplicateCommand.RaiseCanExecuteChanged();
                RenameCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnDelete()
        {
            for (int i = 0; i < AllRectangles.Count + 1; i++)
            {
                AllRectangles.Remove(SelectedResizableRectangle);
                AllRectanglesView.Remove(SelectedResizableRectangle);
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

        private bool CanDelete()
        {
            return SelectedResizableRectangle != null;
        }

        private void OnDuplicate()
        {
            ResizableRectangle DuplicateRect = new ResizableRectangle();

            DuplicateRect.RectangleHeight = SelectedResizableRectangle.RectangleHeight;
            DuplicateRect.RectangleWidth = SelectedResizableRectangle.RectangleWidth;
            DuplicateRect.RectangleText = SelectedResizableRectangle.RectangleText;
            DuplicateRect.X = SelectedResizableRectangle.X + 30;
            DuplicateRect.Y = SelectedResizableRectangle.Y + 30;

            Canvas.SetLeft(DuplicateRect, SelectedResizableRectangle.X + 30);
            Canvas.SetTop(DuplicateRect, SelectedResizableRectangle.Y + 30);

            AllRectanglesView.Add(DuplicateRect);
            AllRectangles.Add(DuplicateRect);
            OnPropertyChanged("AllRectanglesView");
            OnPropertyChanged("AllRectangles");
        }

        private bool CanDuplicate()
        {
            return SelectedResizableRectangle != null;
        }

        private void OnRename()
        {
            foreach (var rec in AllRectanglesView)
            {
                rec.RectangleText = DefaultLabel;
            }
        }

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

        private void DeleteAll()
        {
            AllRectangles.Clear();
            FilterName();
        }

        private bool _visibilityChanged = false;
        public bool VisibilityChanged
        {
            get
            {
                return _visibilityChanged;
            }
            set
            {
                _visibilityChanged = value;
                OnPropertyChanged("VisibilityChanged");
            }
        }

        private string _rectangleText = "Label";
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

        private string _defaultLabel = "DefaultLabel";

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

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

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
                OnPropertyChanged("RectangleFill");
            }
        }

        public void SelectedRectangleFill()
        {
            if (SelectedResizableRectangle != null)
            {
                foreach (var rect in AllRectangles)
                {
                    rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                    rect.RectangleOpacity = 0.07;
                    rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                    rect.ThumbSize = 3;
                    rect.VisibilityChanged = false;
                }

                SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
                SelectedResizableRectangle.RectangleOpacity = 0.5;
                SelectedResizableRectangle.VisibilityChanged = true;
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

        private ICommand _enterCommand;
        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand ?? (_enterCommand = new CommandHandler(() => FilterName(), _canExecute));
            }
        }

        public void DeleteSelection()
        {
            SelectedResizableRectangle = null;

            foreach (var rect in AllRectangles)
            {
                rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                rect.RectangleOpacity = 0.07;
                rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                rect.ThumbSize = 3;
                rect.VisibilityChanged = false;
            }
        }

        private ICommand _deleteLastRectangleCommand;
        public ICommand DeleteLastRectangleCommand
        {
            get
            {
                return _deleteLastRectangleCommand ?? (_deleteLastRectangleCommand = new CommandHandler(() => DeleteLastRectangle(), _canExecute));
            }
        }

        public void DeleteLastRectangle()
        {
            if (AllRectangles.Count > 0)
                AllRectangles.RemoveAt(AllRectangles.Count - 1);
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

        public void SortList()
        {
            ObservableCollection<ResizableRectangle> sortedRectangles = new ObservableCollection<ResizableRectangle>
                (AllRectangles.OrderBy(resizable => resizable.RectangleText));

            AllRectangles = sortedRectangles;
            OnPropertyChanged("AllRectangles");
        }

        public void FilterName()
        {
            if (SelectedComboBoxItem == "All Labels")
            {
                FilterVisibility1 = false;
                FilterVisibility = true;
                AllRectanglesView = AllRectangles;
                RectangleCount = "#" + AllRectangles.Count.ToString();
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("FilterVisibility");
                OnPropertyChanged("FilterVisibility1");
                OnPropertyChanged("RectangleCount");
            }

            else if (SelectedComboBoxItem != "All Labels")
            {
                DefaultLabel = SelectedComboBoxItem;
                FilterVisibility1 = true;
                FilterVisibility = false;
                RectangleCount = "#" + AllRectanglesView.Count.ToString();

                ObservableCollection<ResizableRectangle> FilteredRectangles = new ObservableCollection<ResizableRectangle>
                    (AllRectangles.Where(AllRectangles => AllRectangles.RectangleText == SelectedComboBoxItem));
                AllRectanglesView = FilteredRectangles;
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("FilterVisibility");
                OnPropertyChanged("FilterVisibility1");
                OnPropertyChanged("DefaultLabel");
                OnPropertyChanged("RectangleCount");
            }
        }

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

        private bool _filterVisibility1 = false;

        public bool FilterVisibility1
        {
            get
            {
                return _filterVisibility1;
            }
            set
            {
                _filterVisibility1 = value;
                OnPropertyChanged("FilterVisibility1");
            }
        }

        private bool _filterVisibility = true;

        public bool FilterVisibility
        {
            get
            {
                return _filterVisibility;
            }
            set
            {
                _filterVisibility = value;
                OnPropertyChanged("FilterVisibility");
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

        public void LoadXML()
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
            cropImageLabelBegin();
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


        public void UpdateCropedImage(ResizableRectangle resizable)
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

        public void SelectClickedRectangle(ResizableRectangle resizableRectangle)
        {
            SelectedResizableRectangle = resizableRectangle;
            OnPropertyChanged("SelectedResizableRectangle");
        }
        

        public void cropImageLabelBegin()
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
                    Mat mat = SupportCode.ConvertBmp2Mat(src);
                    OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rec.X, (int)rec.Y, (int)rec.RectangleWidth, (int)rec.RectangleHeight);

                    Mat croppedImage = new Mat(mat, rectCrop);
                    rec.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
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

        public void UpdatePreviews()
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

                if (rec.X > 0 && rec.X + rec.RectangleWidth < MyCanvas.ActualWidth && rec.Y > 0 && rec.Y + rec.RectangleHeight < MyCanvas.ActualHeight)
                {
                    Mat mat = SupportCode.ConvertBmp2Mat(src);
                    OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)rec.X, (int)rec.Y, (int)rec.RectangleWidth, (int)rec.RectangleHeight);

                    Mat croppedImage = new Mat(mat, rectCrop);
                    rec.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
                }
            }
        }

        //TO DO
        //private ICommand _imageMouseUpCommand;

        //public ICommand ImageMouseUpCommand
        //{
        //    get
        //    {
        //        return _imageMouseUpCommand ?? (_imageMouseUpCommand = new CommandHandler(() => ImageMouseUp((MouseButtonEventArgs)_imageMouseUpCommand), _canExecute));
        //    }
        //}

        //public void ImageMouseUp(MouseButtonEventArgs e)
        //{
            
        //}

    }
}
