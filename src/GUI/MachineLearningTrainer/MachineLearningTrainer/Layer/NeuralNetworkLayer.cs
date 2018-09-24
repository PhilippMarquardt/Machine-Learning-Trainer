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

        public bool IsFirstOrLastLayer { get; set; } = false;


      
        public DeepNeuralNetworkLayer(bool isFirstOrLast)
        {

            this.IsFirstOrLastLayer = isFirstOrLast;
        }

        public DeepNeuralNetworkLayer()
        {

        }

        


    }
}
