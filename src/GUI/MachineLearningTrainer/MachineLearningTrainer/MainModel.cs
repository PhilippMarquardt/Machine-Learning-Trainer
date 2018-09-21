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

        public void SetNextState(Grid mainGrid)
        {
            this._stateMachine.NextState(mainGrid);
        }
    }
}
