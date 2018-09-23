using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MachineLearningTrainer
{
    public class MainViewModel
    {
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

        public void SetNextState(Command command)
        {
            UserControl usc = this._mainModel.SetNextState(_mainGrid, command);
            this._mainGrid.Children.Clear();
            usc.DataContext = this;
            this._mainGrid.Children.Add(usc);
        }
        #endregion

        #region DeepNeuralNetwork
        
        public ObservableCollection<DeepNeuralNetworkLayer> DeepNeuralNetworkHiddenLayers { get; set; } = new ObservableCollection<DeepNeuralNetworkLayer>() { new DeepNeuralNetworkLayer(ActivationFunction.ReLu, 5, new Dimension(3, 0, 0),true), new DeepNeuralNetworkLayer(ActivationFunction.ReLu, 5, new Dimension(),true) };

        private ICommand _addDNNLayer;
        public ICommand AddDNNLayer
        {
            get
            {
                return _addDNNLayer ?? (_addDNNLayer = new CommandHandler(() => AddLayer(), _canExecute));
            }
        }

        private ICommand _saveChangedHiddenLayer;

        public ICommand SaveChangesHiddenLayer
        {
            get
            {
                return _saveChangedHiddenLayer ?? (_saveChangedHiddenLayer = new CommandHandler(() => EditCurrentSelectedHiddenLayer(), _canExecute));
            }
        }


        private void EditCurrentSelectedHiddenLayer()
        {
           _mainModel.EditCurrentDNNLayer(SelectedDeepNeuralNetworkLayer, NewLayerNumberOfNodes, NewLayerSelectedActivationFunction.Content.ToString(), NewLayerDimension);
        }



        public string NewLayerNumberOfNodes { get; set; }
        public string NewLayerDimension { get; set; }
        public ComboBoxItem NewLayerSelectedActivationFunction { get; set; }

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
  
        private void AddLayer()
        {
            var newLayer = _mainModel.AddNewDNNLayer(NewLayerNumberOfNodes, NewLayerSelectedActivationFunction.Content.ToString(), NewLayerDimension);
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
}
