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
            CopyCommand = new MyICommand(OnCopy, CanCopy);
        }


        //TODO: Mouse event handler
        //private ICommand _imageMouseDown;

        //public ICommand ImageMouseDown
        //{
        //    get
        //    {
        //        return _imageMouseDown ?? (_imageMouseDown = new CommandHandler(() => WriteDNNXML(), _canExecute));
        //    }
        //}

        public MyICommand DeleteCommand { get; set; }
        public MyICommand CopyCommand   { get; set; }
        public bool Enabled             { get; set; } = true;

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
            Enabled = false;
            if (IsChecked == true && DefaultLabel.Length > 0)
                _rectangleText = _defaultLabel;
            else
                _rectangleText = "";
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
            }

            LoadRectangles();

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
                CopyCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnDelete()
        {
            for(int i=0; i<AllRectangles.Count;i++)
            AllRectangles.Remove(SelectedResizableRectangle);
            this.IsOpen = false;
        }

        private bool CanDelete()
        {
            return SelectedResizableRectangle != null;
        }

        private void OnCopy()
        {
            AllRectangles.Add(new ResizableRectangle { X = SelectedResizableRectangle.X + 30, Y = SelectedResizableRectangle.Y + 30,
                RectangleHeight = SelectedResizableRectangle.RectangleHeight, RectangleWidth = SelectedResizableRectangle.RectangleWidth,
                RectangleText = SelectedResizableRectangle.RectangleText });
        }

        private bool CanCopy()
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
                foreach(var rect in AllRectangles)
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

        private void LoadRectangles()
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
                                double xmin = double.Parse(objectChild["xmin"].InnerText);
                                double ymin = double.Parse(objectChild["ymin"].InnerText);
                                double xmax = double.Parse(objectChild["xmax"].InnerText);
                                double ymax = double.Parse(objectChild["ymax"].InnerText);

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
            ObservableCollection<ResizableRectangle> sortedRectangles = new ObservableCollection<ResizableRectangle>(AllRectangles.OrderBy(resizable => resizable.RectangleText));

            AllRectangles = sortedRectangles;
            OnPropertyChanged("AllRectangles");
        }

        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                return _renameCommand ?? (_renameCommand = new CommandHandler(() => Rename(), _canExecute));
            }
        }

        public void Rename()
        {
            foreach(var rec in AllRectangles)
            {
                for(int i = 0; i < AllRectangles.Count; i++) 
                SelectedResizableRectangle.RectangleText = DefaultLabel;
            }
        }

        private ICommand _filterCommand;
        public ICommand FilterCommand
        {
            get
            {
                return _filterCommand ?? (_filterCommand = new CommandHandler(() => FilterName(), _canExecute));
            }
        }

        public void FilterName()
        {
            string beispiel = "Test1";
            ObservableCollection<ResizableRectangle> resizableRectangles = new ObservableCollection<ResizableRectangle>(AllRectangles.Where(AllRectangles => AllRectangles.RectangleText == beispiel).ToList());
            AllRectangles = resizableRectangles;
            OnPropertyChanged("AllRectangles");
            
        }
    }
}
