using System;
using System.Collections.Generic;
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
        public DeepNeuralNetworkLayer InputLayer { get; set; } = new DeepNeuralNetworkLayer(ActivationFunction.ReLu, 5, new Dimension(3, 4, 5));
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
