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
        }

        //-----------------------------------------------------------------------------------------------------------------
        //TODO: Mouse event handler
        //private ICommand _imageMouseDown;

        //public ICommand ImageMouseDown
        //{
        //    get
        //    {
        //        return _imageMouseDown ?? (_imageMouseDown = new CommandHandler(() => WriteDNNXML(), _canExecute));
        //    }
        //}
        //-----------------------------------------------------------------------------------------------------------------

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

        private ICommand _btnCroppedImage;
        public ICommand BtnCroppedImage
        {
            get
            {
                return _btnCroppedImage ?? (_btnCroppedImage = new CommandHandler(() => TestImage(), _canExecute));
            }
        }

        public BitmapImage convertedImg;
        private void TestImage()
        {
            foreach (var rec in AllRectangles)
            {
                int recStartX = (Convert.ToInt16(rec.X));
                int recStartY = (Convert.ToInt16(rec.Y-23));
                int recWidth  = (Convert.ToInt16(rec.RectangleWidth));
                int recHeight = (Convert.ToInt16(rec.RectangleHeight));
                
                
                Bitmap src = new Bitmap(ImagePath);
                Mat mat = SupportCode.convertBmp2Mat(src);

                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect(recStartX, recStartY, recWidth, recHeight);
                Mat croppedImage = new Mat(mat, rectCrop);

                convertedImg = SupportCode.convertMat2BmpImg(croppedImage);
                CroppedImage = convertedImg;
                croppedImage.SaveImage("output.png");
                MessageBox.Show("Höhe: " + recHeight + "  Breite: " + recWidth + "  X: "+recStartX + "  Y: "+recStartY);
            }
        }
        

        private BitmapImage _croppedImage;
        public BitmapImage CroppedImage
        {
            get
            {
                return this._croppedImage;
            }

            set
            {
                this._croppedImage = value;
                OnPropertyChanged("CroppedImage");
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





    }
}
