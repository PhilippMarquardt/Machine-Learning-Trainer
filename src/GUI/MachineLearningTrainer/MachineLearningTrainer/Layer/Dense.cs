﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer.Layer
{
    public class Dense : DeepNeuralNetworkLayer, INotifyPropertyChanged
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

        private ActivationFunction _activationFunction;
        private int _numberOfNode;
        private Dimension _dimension;
        public ActivationFunction ActivationFunction
        {
            get
            {
                return this._activationFunction;
            }
            set
            {
                this._activationFunction = value;
                OnPropertyChanged("ActivationFunction");
            }
        }
        public int NumberOfNodes
        {
            get
            {
                return this._numberOfNode;
            }
            set
            {
                this._numberOfNode = value;
                OnPropertyChanged("NumberOfNodes");
            }
        }

        public Dimension Dimension
        {
            get
            {
                return this._dimension;
            }
            set
            {
                if (value != null)
                    this._dimension = value;
                OnPropertyChanged("Dimension");
            }
        }

        public Dense(ActivationFunction actFun, int numberOfNodes, Dimension dim, bool isFirstOrLast)
        {
            this.IsFirstOrLastLayer = isFirstOrLast;
            this.ActivationFunction = actFun;
            this.NumberOfNodes = numberOfNodes;
            this.Dimension = dim;
            this.Type = LayerType.Dense;
        }
    }
}