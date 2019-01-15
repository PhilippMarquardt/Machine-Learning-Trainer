using MachineLearningTrainer.Layer;
using System;
using MachineLearningTrainer.DrawerTool;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MachineLearningTrainer.DeepNN;

namespace MachineLearningTrainer
{
    public class MainViewModel : INotifyPropertyChanged
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
        #region NoContentPages

        private MainModel _mainModel;
        private Grid _mainGrid;
        private bool _canExecute = true;
        public MainViewModel(MainModel model, System.Windows.Controls.Grid mainGrid)
        {
            this._mainModel = model;
            this._mainGrid = mainGrid;
        }
        private ICommand _leftTransition;
        public ICommand LeftTransition
        {
            get
            {
                return _leftTransition ?? (_leftTransition = new CommandHandler(() => SetNextState(Command.Left), _canExecute));
            }
        }

        private ICommand _rightTransition;
        public ICommand RightTransition
        {
            get
            {
                return _rightTransition ?? (_rightTransition = new CommandHandler(() => SetNextState(Command.Right), _canExecute));
            }
        }

        private ICommand _nextPage;
        public ICommand NextPage
        {
            get
            {
                return _nextPage ?? (_nextPage = new CommandHandler(() => SetNextState(Command.Next), _canExecute));
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

        #region Drawer
        //public bool Enabled { get; set; } = true;

        //public ObservableCollection<ResizableRectangle> AllRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();

        //private ICommand _exportPascalVoc;
        //public ICommand ExportPascalVoc
        //{
        //    get
        //    {
        //        return _exportPascalVoc ?? (_exportPascalVoc = new CommandHandler(() => ExportToPascal(), _canExecute));
        //    }
        //}
        //private void ExportToPascal()
        //{
        //    XMLWriter.WritePascalVocToXML(AllRectangles.ToList(), "file.xml", 1337, 1337, 3);
        //}

        //private ICommand _loadImageCommand;
        //public ICommand LoadImageCommand
        //{
        //    get
        //    {
        //        return _loadImageCommand ?? (_loadImageCommand = new CommandHandler(() => LoadImage(), _canExecute));
        //    }
        //}
        //private void LoadImage()
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    if (openFileDialog.ShowDialog() == true)
        //        ImagePath = openFileDialog.FileName;
        //}
        //private string _imagePath;
        //public string ImagePath
        //{
        //    get
        //    {
        //        return this._imagePath;
        //    }
        //    set
        //    {
        //        this._imagePath = value;
        //        OnPropertyChanged("ImagePath");
        //    }
        //}

        #endregion
        public void SetNextState(Command command)
        {
            UserControl usc = this._mainModel.SetNextState(_mainGrid, command);
            //set variable if we are on specific page..
            this._mainGrid.Children.Clear();
            if (usc is ImageDrawer)
            {
                usc.DataContext = new DrawerViewModel(new DrawerModel(), this._mainModel, this._mainGrid, this);
            }
            else 
            {
                usc.DataContext = this;
            }
            this._mainGrid.Children.Add(usc);
        }

        #endregion

        #region DeepNeuralNetwork

        public ObservableCollection<DeepNeuralNetworkLayer> DeepNeuralNetworkHiddenLayers { get; set; } = new ObservableCollection<DeepNeuralNetworkLayer>() { new Dense(ActivationFunction.ReLu, 5, new Dimension(3, 0, 0), true, false), new Dense(ActivationFunction.ReLu, 5, new Dimension(), false, true) };

        //private ICommand _addDNNLayer;
        //public ICommand AddDNNLayer
        //{
        //    get
        //    {
        //        return _addDNNLayer ?? (_addDNNLayer = new CommandHandler(() => AddLayer(), _canExecute));
        //    }
        //}

        private ICommand _saveChangedHiddenLayer;

        public ICommand SaveChangesHiddenLayer
        {
            get
            {
                return _saveChangedHiddenLayer ?? (_saveChangedHiddenLayer = new CommandHandler(() => EditCurrentSelectedHiddenLayer(), _canExecute));
            }
        }

        private ICommand _deepNNWriteXML;

        public ICommand DeepNNWriteXML
        {
            get
            {
                return _deepNNWriteXML ?? (_deepNNWriteXML = new CommandHandler(() => WriteDNNXML(), _canExecute));
            }
        }

        private ICommand _closePopup;

        public ICommand ClosePopupCommand
        {
            get
            {
                return _closePopup ?? (_closePopup = new RelayCommand((param) => ClosePopup(param), _canExecute));
            }
        }

        private ICommand _openinputtDirectory;

        public ICommand OpenInputDirectory
        {
            get
            {
                return _openinputtDirectory ?? (_openinputtDirectory = new CommandHandler(() => SpecifyInputDirectory(), _canExecute));
            }
        }

        private ICommand _radioButtonCheckedCommand;

        public ICommand RadioButtonCheckedCommand
        {
            get
            {
                return _radioButtonCheckedCommand ?? (_radioButtonCheckedCommand = new CommandHandler(() => System.Windows.MessageBox.Show("sad"), _canExecute));
            }
        }



        private string _inputPath;

        public string InputPath
        {
            get
            {
                return this._inputPath;
            }
            set
            {
                this._inputPath = value;
                ReadCsvHeaders();
                OnPropertyChanged("InputPath");
            }
        }

        private void setInputPathDNN()
        {
            InputPath = SpecifyInputDirectory();
        }

        

        private string SpecifyInputDirectory()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            return "";
        }
    


        private void ClosePopup(object param)
        {
            
            (param as MaterialDesignThemes.Wpf.DialogHost).IsOpen = false;
           
        }

        private void WriteDNNXML()
        {
            try
            {
                if (CSVHeaders.Where(x => x.IsChecked).ToList().Count == 0)
                    throw new Exception("Please select feautures");
                if (CSVTarget.Where(x => x.IsChecked).ToList().Count == 0)
                    throw new Exception("Please select a target");

                XMLWriter.WriteLayersToXML(CSVHeaders.Where(x => x.IsChecked).ToList(), CSVTarget.Where(x => x.IsChecked).FirstOrDefault(), DeepNeuralNetworkHiddenLayers.ToList(), Convert.ToDouble(LearningRate), Convert.ToInt32(Epochs), Optimizer.Content.ToString(), InputPath, ModelName);
                System.Windows.MessageBox.Show(PythonRunner.RunScriptAsynchronous("prepro.py", true, new string[] { "" }, false));
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void EditCurrentSelectedHiddenLayer()
        {
           _mainModel.EditCurrentDNNLayer(SelectedDeepNeuralNetworkLayer, NewLayerNumberOfNodes, NewLayerSelectedActivationFunction.Content.ToString(), NewLayerDimension, NewLayerDropout);
        }

        public ObservableCollection<CustomListBoxItem> CSVHeaders { get; set; } = new ObservableCollection<CustomListBoxItem>();
        public ObservableCollection<CustomListBoxItem> CSVTarget { get; set; } = new ObservableCollection<CustomListBoxItem>();

        private void ReadCsvHeaders()
        {
            var headers = PythonRunner.RunScriptSynchronous("csv_header_reader.py", false, new string[] { InputPath });
            headers = headers.Replace("[", "");
            headers = headers.Replace("]", "");
            var headersList = headers.Split(',');

            foreach (var header in headersList)
            {
                CSVHeaders.Add(new CustomListBoxItem(header));
                CSVTarget.Add(new CustomListBoxItem(header));
            }
        }


        public string NewLayerDropout { get; set; } = "";
        public string NewLayerNumberOfNodes { get; set; } = "";
        public string NewLayerDimension { get; set; } = "";
        public string LearningRate { get; set; }
        public string Epochs { get; set; }
        public string ModelName { get; set; }
        public ComboBoxItem Optimizer { get; set; }
        public ComboBoxItem NewLayerSelectedActivationFunction { get; set; }
        private bool _autoFindParameter = false;
        public bool AutoFindParameter
        {
            get
            {
                return this._autoFindParameter;
            }
            set
            {
                this._autoFindParameter = value;
                OnPropertyChanged("AutoFindParameter");
            }
        }

        public void DeleteHiddenLayer(DeepNeuralNetworkLayer layer)
        {
            var isFirstOrLast = DeepNeuralNetworkHiddenLayers.Where(x => x == layer).First().IsFirstLayer;
            var isLastLayer = DeepNeuralNetworkHiddenLayers.Where(x => x == layer).First().IsLastLayer;
            if (!(isFirstOrLast||isLastLayer))
                DeepNeuralNetworkHiddenLayers.Remove(layer);
        }

        private DeepNeuralNetworkLayer _selectedDeepNeuralNetworkLayer;
        public DeepNeuralNetworkLayer SelectedDeepNeuralNetworkLayer
        {
            get
            {
                return _selectedDeepNeuralNetworkLayer;
            }
            set
            {
                if(value != null)
                    this._selectedDeepNeuralNetworkLayer = value;
            }
        }
  
        public void AddLayer(LayerType type)
        {

            var newLayer = _mainModel.AddNewDNNLayer(NewLayerNumberOfNodes, NewLayerSelectedActivationFunction == null ? "" : NewLayerSelectedActivationFunction.Content.ToString(), NewLayerDimension, type, NewLayerDropout);
            if(newLayer != null)
                DeepNeuralNetworkHiddenLayers.Insert(DeepNeuralNetworkHiddenLayers.Count-1,newLayer);
        }
        #endregion


       

        #region ObjectDetector

        private string SpecifyInputFolder()
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }
            return "";
        }

        private ICommand _onSelectImageFolderClick;

        public ICommand OnSelectImageFolderClick
        {
            get
            {
                return _onSelectImageFolderClick ?? (_onSelectImageFolderClick = new CommandHandler(() => SelectImageFolderPath(), _canExecute));
            }
        }

        private ICommand _onSelectAnnoFolderClick;

        public ICommand OnSelectAnnoFolderClick
        {
            get
            {
                return _onSelectAnnoFolderClick ?? (_onSelectAnnoFolderClick = new CommandHandler(() => SelectAnnoFolderPath(), _canExecute));
            }
        }


        private ICommand _onRunRetinanetClick;

        public ICommand OnRunRetinanetClick
        {
            get
            {
                return _onRunRetinanetClick ?? (_onRunRetinanetClick = new CommandHandler(() => RunRetinanetThreaded(), _canExecute));
            }
        }

        private ICommand _onRunDebugClick;

        public ICommand OnRunDebugClick
        {
            get
            {
                return _onRunDebugClick ?? (_onRunDebugClick = new CommandHandler(() => RunDebugThreaded(), _canExecute));
            }
        }

        private void RunDebugThreaded()
        {
            System.Threading.ThreadStart childref = new System.Threading.ThreadStart(RunDebug);
            new System.Threading.Thread(childref).Start();
        }

        private void RunDebug()
        {
            PythonRunner.RunScriptSynchronous("keras-retinanet/keras_retinanet/bin/debug.py", true, new string[] {"--annotations", "pascal", "keras-retinanet/newdata" }, false);
        }

        private void RunRetinanetThreaded()
        {
            System.Threading.ThreadStart childref = new System.Threading.ThreadStart(RunRetinanet);
            new System.Threading.Thread(childref).Start();
        }

        private void RunRetinanet()
        {
            PythonRunner.RunScriptSynchronous("keras-retinanet/keras_retinanet/bin/train.py", true, new string[] { "--batch-size=1", "--steps=150", "--image-min-side=1332", "--image-max-side=1640", "--backbone=resnet50", "pascal", "keras-retinanet/newdata" }, false);
        }


        private void SelectImageFolderPath()
        {
            ImageFolderPath = SpecifyInputFolder();
        }

        private void SelectAnnoFolderPath()
        {
            AnnoFolderPath = SpecifyInputFolder();
        }


        private string _imageFolderPath;

        public string ImageFolderPath
        {
            get
            {
                return this._imageFolderPath;
            }
            set
            {
                this._imageFolderPath = value;
                OnPropertyChanged("ImageFolderPath");
            }
        }


        private string _annoFolderPath;

        public string AnnoFolderPath
        {
            get
            {
                return this._annoFolderPath;
            }
            set
            {
                this._annoFolderPath = value;
                OnPropertyChanged("AnnoFolderPath");
            }
        }

        #endregion

        #region ObjectClassificator
        private bool _useImageNet;

        public List<string> NetworkArchitectures { get; set; } = new List<string> { "Resnet" };

        public bool UseImageNet
        {
            get
            {
                return this._useImageNet;
            }
            set
            {
                this._useImageNet = value;
                OnPropertyChanged("UseImageNet");
            }
        }


        private ICommand _onRunResnetClick;

        public ICommand OnRunResnetClick
        {
            get
            {
                return _onRunResnetClick ?? (_onRunResnetClick = new CommandHandler(() => RunResnetThreaded(), _canExecute));
            }
        }

        private void RunResnetThreaded()
        {
            System.Threading.ThreadStart childref = new System.Threading.ThreadStart(RunResnet);
            new System.Threading.Thread(childref).Start();
        }

        private void RunResnet()
        {
            PythonRunner.RunScriptSynchronous("resnet.py", true, new string[] { "" }, false);
        }


        #endregion
    }


    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> _action;
        private bool _canExecute;
        private Action<object> execute;
        private Predicate<object> canExecute;

        public RelayCommand(Action<object> action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
