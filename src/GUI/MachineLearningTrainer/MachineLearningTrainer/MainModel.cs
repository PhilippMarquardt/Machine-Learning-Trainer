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
                default:
                    return new WelcomePage();
            }
        }

        public DeepNeuralNetworkLayer AddNewDNNLayer(string numberOfNodes, string activationFunction, string dim)
        {   try
            {
                List<string> tmp = dim.Split(',').ToList();
                if (tmp.Count > 3)
                    throw new Exception("Dimension muss im richtigen Format vorliegen, siehe hint");
                int rows = Convert.ToInt32(tmp[0]);
                int columns = Convert.ToInt32(tmp[1]);
                int channel = Convert.ToInt32(tmp[2]);
                return new DeepNeuralNetworkLayer(ConvertStringToActFunction(activationFunction), Convert.ToInt32(numberOfNodes), new Dimension(rows, columns, channel));
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void EditCurrentDNNLayer(DeepNeuralNetworkLayer layer,string numberOfNodes, string activationFunction, string dim)
        {
            try
            {
                List<string> tmp = dim.Split(',').ToList();
                if (tmp.Count > 3)
                    throw new Exception("Dimension muss im richtigen Format vorliegen, siehe hint");
                int rows = Convert.ToInt32(tmp[0]);
                int columns = Convert.ToInt32(tmp[1]);
                int channel = Convert.ToInt32(tmp[2]);
                layer.ActivationFunction = ConvertStringToActFunction(activationFunction);
                layer.NumberOfNodes = Convert.ToInt32(numberOfNodes);
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
