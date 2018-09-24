using MachineLearningTrainer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MachineLearningTrainer
{
    public static class XMLWriter
    {
        public static void WriteLayersToXML(List<DeepNeuralNetworkLayer> allLayers)
        {
            try
            {
                System.Windows.MessageBox.Show("Start writing xml file");
                XmlWriter writer = XmlWriter.Create("layers.xml");
                writer.WriteStartDocument();
                writer.WriteStartElement("layers");

                foreach (var l in allLayers)
                {
                   
                    if (l.Type == LayerType.Dense)
                    {
                        Dense lay = l as Dense;
                        writer.WriteStartElement("Dense");

                        writer.WriteStartElement("input_shape");
                        writer.WriteString(lay.Dimension.ToString());
                        writer.WriteEndElement();

                        writer.WriteStartElement("activation_function");
                        writer.WriteString(lay.ActivationFunction.ToString());
                        writer.WriteEndElement();

                        writer.WriteStartElement("nodes");
                        writer.WriteString(lay.NumberOfNodes.ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    else if(l.Type == LayerType.Dropout)
                    {
                        Dropout lay = l as Dropout;
                        writer.WriteStartElement("Dropout");

                        writer.WriteStartElement("dropout_value");
                        writer.WriteString(lay.DropoutValue.ToString());
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                }

                writer.WriteEndDocument();
                writer.Close();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
                   
        }
    }
}
