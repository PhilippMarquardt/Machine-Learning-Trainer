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
            this._mainGrid.Children.Clear();
            if (usc is ImageDrawer)
            {
                usc.DataContext = new DrawerViewModel(new DrawerModel());
            }
            else 
            {
                usc.DataContext = this;
            }
            this._mainGrid.Children.Add(usc);
        }
        #endregion

        #region DeepNeuralNetwork

        public ObservableCollection<DeepNeuralNetworkLayer> DeepNeuralNetworkHiddenLayers { get; set; } = new ObservableCollection<DeepNeuralNetworkLayer>() { new Dense(ActivationFunction.ReLu, 5, new Dimension(3, 0, 0), true), new Dense(ActivationFunction.ReLu, 5, new Dimension(), true) };

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

        public ICommand _openinputtDirectory;

        public ICommand OpenInputDirectory
        {
            get
            {
                return _openinputtDirectory ?? (_openinputtDirectory = new CommandHandler(() => SpecifyInputDirectory(), _canExecute));
            }
        }

        private string _outputPath;

        public string OutputPath
        {
            get
            {
                return this._outputPath;
            }
            set
            {
                this._outputPath = value;
                OnPropertyChanged("OutputPath");
            }
        }

        private void SpecifyInputDirectory()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                OutputPath = openFileDialog.FileName;
        }
    


        private void ClosePopup(object param)
        {
            
            (param as MaterialDesignThemes.Wpf.DialogHost).IsOpen = false;
           
        }

        private void WriteDNNXML()
        {
           
            XMLWriter.WriteLayersToXML(DeepNeuralNetworkHiddenLayers.ToList(), Convert.ToDouble(LearningRate),Convert.ToInt32(Epochs), Optimizer.Content.ToString(), OutputPath);
            System.Windows.MessageBox.Show(PythonRunner.RunScript("prepro.py", true, new string[] { "" }));
        }

        private void EditCurrentSelectedHiddenLayer()
        {
           _mainModel.EditCurrentDNNLayer(SelectedDeepNeuralNetworkLayer, NewLayerNumberOfNodes, NewLayerSelectedActivationFunction.Content.ToString(), NewLayerDimension, NewLayerDropout);
        }


        public string NewLayerDropout { get; set; } = "";
        public string NewLayerNumberOfNodes { get; set; } = "";
        public string NewLayerDimension { get; set; } = "";
        public string LearningRate { get; set; }
        public string Epochs { get; set; }
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
            var isFirstOrLast = DeepNeuralNetworkHiddenLayers.Where(x => x == layer).First().IsFirstOrLastLayer;
            if(!isFirstOrLast)
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
        public RelayCommand(Action<object> action, bool canExecute)
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
            _action(parameter);
        }
    }
}
