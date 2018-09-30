using MachineLearningTrainer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MachineLearningTrainer
{
    public class MainModel
    {
        private WizardStateMachine _stateMachine;

        public MainModel()
        {
            this._stateMachine = new WizardStateMachine();
        }

        public UserControl SetNextState(Grid mainGrid, Command command)
        {
            this._stateMachine.MoveNext(command);
            return SetNewWizardPage(mainGrid);
        }


        private UserControl SetNewWizardPage(Grid mainGrid)
        {
            UserControl usc = new UserControl();
            switch (this._stateMachine.CurrentState)
            {
                case ProcessState.WelcomePage:
                    return new WelcomePage();                 
                case ProcessState.DataDecision:
                    return new DataDecision();
                case ProcessState.TabularDataDecision:
                    return new TabularDataDecision();
                case ProcessState.DNN:
                    return new DeepNeuralNetwork();
                case ProcessState.Drawer:
                    return new DrawerTool.ImageDrawer();                   
                default:
                    return new WelcomePage();
            }
        }

        public DeepNeuralNetworkLayer AddNewDNNLayer(string numberOfNodes = "", string activationFunction = "relu", string dim="", LayerType type = LayerType.Dense, string dropout = "")
        {   
            if(type == LayerType.Dense)
            {
                return AddNewDenseLayer(numberOfNodes, activationFunction, dim);
            }
            else if(type == LayerType.Dropout)
            {
                return (AddNewDropoutLayer(dropout));
            }
            return null;
        }

        private Dropout AddNewDropoutLayer(string dropoutValue)
        {
            try
            {
                return new Dropout(Convert.ToDouble(dropoutValue));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Dense AddNewDenseLayer(string numberOfNodes, string activationFunction, string dim)
        {
            try
            {
                if (dim == null || dim == "")
                {
                    return new Dense(ConvertStringToActFunction(activationFunction), Convert.ToInt32(numberOfNodes), new Dimension(), false, false);
                }
                else
                {
                    List<string> tmp = dim.Split(',').ToList();
                    if (tmp.Count > 3)
                        throw new Exception("Dimension muss im richtigen Format vorliegen, siehe hint");
                    int rows = Convert.ToInt32(tmp[0]);
                    int columns = 0;
                    int channel = 0;
                    if (tmp.Count > 1)
                    {
                        columns = Convert.ToInt32(tmp[1]);
                        channel = Convert.ToInt32(tmp[2]);
                    }
                    return new Dense(ConvertStringToActFunction(activationFunction), Convert.ToInt32(numberOfNodes), new Dimension(rows, columns, channel), false, false);
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void EditCurrentDNNLayer(DeepNeuralNetworkLayer layer, string numberOfNodes = "0", string activationFunction = "relu", string dim = "", string dropoutValue = "")
        {
            if(layer.Type == LayerType.Dense)
            {
                EditCurrentDenseLayer(layer as Dense, numberOfNodes, activationFunction, dim);
            }
            else if(layer.Type == LayerType.Dropout)
            {
                EditCurrentDropoutLayer(layer as Dropout, dropoutValue);
            }
        }
        private void EditCurrentDropoutLayer(Dropout layer, string dropoutValue)
        {
            try
            {
                System.Windows.MessageBox.Show(dropoutValue);
                layer.DropoutValue = Convert.ToDouble(dropoutValue);
            }
            catch (Exception)
            {

            }
        }

        private void EditCurrentDenseLayer(Dense layer, string numberOfNodes, string activationFunction, string dim)
        {
            try
            {
                
                if (dim == null || dim == "")
                {
                    layer.ActivationFunction = ConvertStringToActFunction(activationFunction);
                    layer.NumberOfNodes = Convert.ToInt32(numberOfNodes);
                    return;
                }

                List<string> tmp = dim.Split(',').ToList();
                if (tmp.Count > 3)
                    throw new Exception("Dimension muss im richtigen Format vorliegen, siehe hint");
                int rows = 0;
                int columns = 0;
                int channel = 0;
                if (tmp.Count == 3)
                {
                    rows = Convert.ToInt32(tmp[0]);
                    columns = Convert.ToInt32(tmp[1]);
                    channel = Convert.ToInt32(tmp[2]);
                }
                else if (tmp.Count == 1)
                {
                    rows = Convert.ToInt32(tmp[0]);
                }
                layer.ActivationFunction = ConvertStringToActFunction(activationFunction);
                layer.NumberOfNodes = Convert.ToInt32(numberOfNodes);
                if (layer.IsFirstLayer)
                    layer.Dimension = new Dimension(rows, columns, channel);
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private ActivationFunction ConvertStringToActFunction(string actFun)
        {
            switch (actFun)
            {
                case "ReLu":
                    return ActivationFunction.ReLu;
                case "Softmax":
                    return ActivationFunction.Softmax;
                case "Sigmoid":
                    return ActivationFunction.Sigmoid;
                default:
                    return ActivationFunction.ReLu;
            }
        }


    }
}
