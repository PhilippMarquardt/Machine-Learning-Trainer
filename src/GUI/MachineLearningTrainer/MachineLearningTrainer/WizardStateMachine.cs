using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MachineLearningTrainer
{
    public enum ProcessState
    {
        WelcomePage,
        DataDecision,
       
    }

    public enum Command
    {
        Previous,
        Next
    }

    public class WizardStateMachine
    {
        private ProcessState _currentState = ProcessState.WelcomePage;
        
        public void NextState(Grid GridToChangeControl)
        {
            this._currentState = GetNextState();
            UserControl usc = GetUserControlToCurrentState();
            SetNextState(GridToChangeControl, usc);
        }

        private UserControl GetUserControlToCurrentState()
        {
            switch (_currentState)
            {
                case ProcessState.WelcomePage:
                    return new WelcomePage();
                default:
                    return new DataDecision();
            }
        }

        private void SetNextState(Grid GridToChangeControl, UserControl ControlToSet)
        {
            GridToChangeControl.Children.Clear();
            GridToChangeControl.Children.Add(ControlToSet);
        }

        private ProcessState GetNextState()
        {
            switch (_currentState)
            {
                case ProcessState.WelcomePage:
                    return ProcessState.DataDecision;
                default:
                    return ProcessState.WelcomePage;
            }
        }
    }
}
