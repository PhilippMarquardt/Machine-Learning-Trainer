using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MachineLearningTrainer
{
    public class MainViewModel
    {
        private MainModel _mainModel;
        private Grid _mainGrid;
        public MainViewModel(MainModel model, System.Windows.Controls.Grid mainGrid)
        {
            this._mainModel = model;
            this._mainGrid = mainGrid;
        }

        public void SetNextState()
        {
            this._mainModel.SetNextState(_mainGrid);
        }
    }
}
