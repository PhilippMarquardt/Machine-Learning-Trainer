using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer.Layer
{
    public class Dropout : DeepNeuralNetworkLayer
    {
      
        public Dropout(double dropout, bool isFirstOrLast = false, LayerType type = LayerType.Dropout)
        {
            this.IsFirstOrLastLayer = isFirstOrLast;
            this.DropoutValue = dropout;
            this.Type = type;
        }

        private double _dropout;
        public double DropoutValue
        {
            get
            {
                return this._dropout;
            }
            set
            {
                this._dropout = value;
                OnPropertyChanged("DropoutValue");
            }
        }
    }
}
