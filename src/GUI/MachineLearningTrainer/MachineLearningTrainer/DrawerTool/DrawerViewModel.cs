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

namespace MachineLearningTrainer.DrawerTool 
{
    public class DrawerViewModel : INotifyPropertyChanged
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
        public bool Enabled { get; set; } = true;

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
            XMLWriter.WritePascalVocToXML(AllRectangles.ToList(), "file.xml", 1337, 1337, 3);
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
            if (openFileDialog.ShowDialog() == true)
                ImagePath = openFileDialog.FileName;
            if (ImagePath != null)
            {
                this.IsEnabled = true;
                AllRectangles.Clear();
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
            }
        }

        private void OnDelete()
        {
            AllRectangles.Remove(SelectedResizableRectangle);
        }

        private bool CanDelete()
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

        private string _lastLabel = "Label";
        public string LastLabel
        {
            get
            {
                return _lastLabel;
            }

            set
            {
                if (_lastLabel != value)
                {
                    _lastLabel = value;
                    OnPropertyChanged("LastLabel");
                }
            }
        }

        private bool _isChecked = false;

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

        private string _defaultLabel = "Test";

        public string DefaultLabel
        {
            get
            {
                return _defaultLabel;
            }
            set
            {
                _defaultLabel = value;
                OnPropertyChanged("DefaultLabel");
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

                SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.MediumVioletRed;
                SelectedResizableRectangle.RectangleOpacity = 0.8;
                SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Yellow;
                SelectedResizableRectangle.ThumbSize = 5;
                SelectedResizableRectangle.VisibilityChanged = true;
                OnPropertyChanged("RectangleFill");
                OnPropertyChanged("RectangleOpacity");
                OnPropertyChanged("ThumbColor");
                OnPropertyChanged("ThumbSize");
                OnPropertyChanged("VisibilityChanged");
            }
        }
    }
}
