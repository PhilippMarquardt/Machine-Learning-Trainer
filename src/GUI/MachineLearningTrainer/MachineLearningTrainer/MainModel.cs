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
            System.Windows.MessageBox.Show(this._stateMachine.CurrentState.ToString());
            switch (this._stateMachine.CurrentState)
            {
                case ProcessState.WelcomePage:
                    return new WelcomePage();
                    
                case ProcessState.DataDecision:
                    return new DataDecision();
                default:
                    return new WelcomePage();
            }
        }

        
    }
}
