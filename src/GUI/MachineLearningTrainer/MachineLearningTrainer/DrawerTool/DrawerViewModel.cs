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
        //        return _imageMouseDown ?? (_imageMouseDown = new CommandHandler(() => WriteDNNXML(), _canExecute));
        //    }
        //}

        public MyICommand DeleteCommand { get; set; }
        public MyICommand CopyCommand   { get; set; }
        public MyICommand RenameCommand { get; set; }
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
                CopyCommand.RaiseCanExecuteChanged();
                RenameCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnDelete()
        {
            for (int i = 0; i < AllRectangles.Count+1; i++)
            {
                AllRectangles.Remove(SelectedResizableRectangle);
                AllRectanglesView.Remove(SelectedResizableRectangle);
            }
                
            this.IsOpen = false;
            ComboBoxNames();
            AllRectanglesView = AllRectangles;
            FilteredRectangles = AllRectangles;
            OnPropertyChanged("AllRectanglesView");
            OnPropertyChanged("FilteredRectangles");
            OnPropertyChanged("ComboBoxNames");


        }

        private bool CanDelete()
        {
            return SelectedResizableRectangle != null;
        }

        private void OnCopy()
        {
            ResizableRectangle copyRect = new ResizableRectangle();

            copyRect.RectangleHeight = SelectedResizableRectangle.RectangleHeight;
            copyRect.RectangleWidth = SelectedResizableRectangle.RectangleWidth;
            copyRect.RectangleText = SelectedResizableRectangle.RectangleText;
            copyRect.X = SelectedResizableRectangle.X + 30;
            copyRect.Y = SelectedResizableRectangle.Y + 30;

            Canvas.SetLeft(copyRect, SelectedResizableRectangle.X + 30);
            Canvas.SetTop(copyRect, SelectedResizableRectangle.Y + 30);
            
            AllRectanglesView.Add(copyRect);
            AllRectangles.Add(copyRect);
            OnPropertyChanged("AllRectanglesView");
            OnPropertyChanged("AllRectangles");
        }

        private bool CanCopy()
        {
            return SelectedResizableRectangle != null;
        }

        private void OnRename()
        {
            foreach(var rec in AllRectanglesView)
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

        private string _dst;
        public string dst
        {
            get { return _dst; }
            set { _dst = value; }
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

            else
            {
                MessageBoxResult result = MessageBox.Show("No XML File found!" + "\n" + "Do you want to browse a XML File?", "Information", MessageBoxButton.YesNo,MessageBoxImage.Information,MessageBoxResult.OK);

                if (result == MessageBoxResult.Yes)
                {
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
                                    }
                                }
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
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("FilterVisibility");
                OnPropertyChanged("FilterVisibility1");
            }

            else if(SelectedComboBoxItem != "All Labels")
            {
                DefaultLabel = SelectedComboBoxItem;
                FilterVisibility1 = true;
                FilterVisibility = false;

                ObservableCollection<ResizableRectangle> FilteredRectangles = new ObservableCollection<ResizableRectangle>
                    (AllRectangles.Where(AllRectangles => AllRectangles.RectangleText == SelectedComboBoxItem));
                AllRectanglesView = FilteredRectangles;
                OnPropertyChanged("AllRectangles");
                OnPropertyChanged("AllRectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("FilterVisibility");
                OnPropertyChanged("FilterVisibility1");
                OnPropertyChanged("DefaultLabel");
            }
        }
        
        public void ComboBoxNames()
        {
            string temp = SelectedComboBoxItem;
            ComboBoxItems.Clear();
            ComboBoxItems.Add("All Labels");
            SelectedComboBoxItem = temp;
            if (!ComboBoxItems.Contains(temp))
            {
                SelectedComboBoxItem = "All Labels";
            }

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

    }
}
