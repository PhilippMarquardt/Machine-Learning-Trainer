using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MachineLearningTrainer
{
    public enum ActivationFunction
    {
        ReLu,
        Sigmoid,
        Softmax
    }

    public enum LayerType
    {
        Dense,
        Conv2D,
        Dropout,
        BatchNormalization
    }

    public class DeepNeuralNetworkLayer : INotifyPropertyChanged
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

        public LayerType Type { get; set; }    

        public bool IsFirstLayer { get; set; } = false;

        public bool IsLastLayer { get; set; } = false;
      
        public DeepNeuralNetworkLayer(bool isFirst, bool isLast)
        {

            this.IsFirstLayer = isFirst;
            this.IsLastLayer = isLast;
        }

        public DeepNeuralNetworkLayer()
        {

        }

        


    }
}
