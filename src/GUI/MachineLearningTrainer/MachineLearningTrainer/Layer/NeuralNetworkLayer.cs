using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer
{
    public enum ActivationFunction
    {
        ReLu,
        Sigmoid,
        Softmax
    }
    public class DeepNeuralNetworkLayer
    {
        public ActivationFunction ActivationFunction { get; set; }
        public int NumberOfNodes { get; set; } = 5;

        public Dimension Dimension { get; set; }

        public DeepNeuralNetworkLayer(ActivationFunction actFun, int numberOfNodes, Dimension dim)
        {
            this.ActivationFunction = actFun;
            this.NumberOfNodes = numberOfNodes;
            this.Dimension = dim;
        }

        public DeepNeuralNetworkLayer()
        {

        }

        


    }
}
