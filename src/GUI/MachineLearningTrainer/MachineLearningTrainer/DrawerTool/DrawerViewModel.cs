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
            Bitmap bitmap = new Bitmap(ImagePath);  //convert from bitmap to mat
            Mat mat = SupportCode.convertBmp2Mat(bitmap);
            Mat ClearEdge = new Mat();

            // try to reduce image noises.
            Cv2.GaussianBlur(mat, mat, new OpenCvSharp.Size(3, 3), 9);
            Mat editedImg = new Mat();  //edit image     
            Cv2.Canny(mat, editedImg, 64, 98);
            Cv2.Canny(editedImg, editedImg, 32, 198);
            int Height = editedImg.Height;
            int Width = editedImg.Width;
            convertedImg = SupportCode.convertMat2BmpImg(editedImg);
            CroppedImage = convertedImg;
        }


        //private BitmapImage _icon;
        //public BitmapImage Icon
        //{
        //    get
        //    {
        //        return this._icon;
        //    }

        //    set
        //    {
        //        this._icon = value;
        //        OnPropertyChanged("Icon");
        //    }
        //}

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
