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

        public void TestRectangles()
        {
            ObservableCollection<ResizableRectangle> allRectangles = new ObservableCollection<ResizableRectangle>();

            allRectangles.Add(new ResizableRectangle { X = 0, Y = 0, RectangleHeight = 100, RectangleWidth = 100, Viktor = "Test" });
            allRectangles.Add(new ResizableRectangle { X = 400, Y = 300, RectangleHeight = 100, RectangleWidth = 100, Viktor = "Test" });

            AllRectangles = allRectangles;
        }
        


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
                DeleteCommand.RaiseCanExecuteChanged(); 
            }
        }  

        private ICommand _deleteRectanglesCommand;
        public ICommand DeleteRectanglesCommand
        {
            get
            {
                return _deleteRectanglesCommand ?? (_deleteRectanglesCommand = new CommandHandler(() => DeleteAll(), _canExecute));
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

        private void DeleteAll()
        {
            AllRectangles.Clear();
        }

        private ICommand _changeFillCommand;
        public ICommand ChangeFillCommand
        {
            get
            {
                return _changeFillCommand ?? (_changeFillCommand = new CommandHandler(() => ChangeOpacity(), _canExecute));
            }
        }

        public void ChangeOpacity()
        {
            _rectangleOpacity = 1.0;
            OnPropertyChanged("RectangleOpacity");
            MessageBox.Show("Test");
        }

        private double _rectangleOpacity = 0.1;

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

    }
}
