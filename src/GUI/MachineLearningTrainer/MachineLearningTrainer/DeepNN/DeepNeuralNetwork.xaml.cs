using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MachineLearningTrainer
{
    /// <summary>
    /// Interaktionslogik für DeepNeuralNetwork.xaml
    /// </summary>
    public partial class DeepNeuralNetwork : UserControl
    {
        public DeepNeuralNetwork()
        {
            InitializeComponent();

        }

        private void btnDeleteHiddenLayer_Click(object sender, RoutedEventArgs e)
        {
            object dataContext = ((sender as Button).DataContext);
            DeepNeuralNetworkLayer layerToDelete = dataContext as DeepNeuralNetworkLayer;
            (this.DataContext as MainViewModel).DeleteHiddenLayer(layerToDelete);
        }

        private void listboxHiddenLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditHiddenLayer.IsOpen = true;
        }
    }
       
}
