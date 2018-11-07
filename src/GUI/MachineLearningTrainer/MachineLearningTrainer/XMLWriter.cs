using MachineLearningTrainer.DeepNN;
using MachineLearningTrainer.DrawerTool;
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
        public static void WriteLayersToXML(List<CustomListBoxItem> feautures, CustomListBoxItem target, List<DeepNeuralNetworkLayer> allLayers, double learningRate = 0.1, int epochs = 500, string optimizer = "adam", string input_path = "", string modelName = "" )
        {
            try
            {
                System.Windows.MessageBox.Show("Start writing xml file");
                XmlWriter writer = XmlWriter.Create("layers.xml");
                writer.WriteStartDocument();
                


                writer.WriteStartElement("layers");

                writer.WriteStartElement("modelname");
                writer.WriteString(modelName);
                writer.WriteEndElement();

                writer.WriteStartElement("inputpath");
                writer.WriteString(input_path);
                writer.WriteEndElement();

                writer.WriteStartElement("learningrate");
                writer.WriteString(learningRate.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("epochs");
                writer.WriteString(epochs.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("optimizer");
                writer.WriteString(optimizer);
                writer.WriteEndElement();

                writer.WriteStartElement("features");        
                writer.WriteString(String.Join(",", feautures));
                writer.WriteEndElement();

                writer.WriteStartElement("number_of_features");
                writer.WriteString(feautures.Count.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("target");
                writer.WriteString(target.ToString().Replace("'", "").Replace(" ",""));
                writer.WriteEndElement();

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

                        writer.WriteStartElement("first");
                        writer.WriteString(lay.IsFirstLayer.ToString());
                        writer.WriteEndElement();

                        writer.WriteStartElement("last");
                        writer.WriteString(lay.IsLastLayer.ToString());
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

                writer.WriteEndElement();

            

                writer.WriteEndDocument();
                writer.Close();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
                   
        }

        public static void WritePascalVocToXML(List<ResizableRectangle> allRectangles, string name, int width, int height, int depth)
        {
            try
            {
                XmlWriter writer = XmlWriter.Create(name);
                writer.WriteStartDocument();
                writer.WriteStartElement("annotation");

                writer.WriteStartElement("folder");
                writer.WriteString("");
                writer.WriteEndElement();

                writer.WriteStartElement("source");
                writer.WriteStartElement("database");
                writer.WriteString("Unknown");
                writer.WriteEndElement();
                writer.WriteEndElement();


                writer.WriteStartElement("size");

                writer.WriteStartElement("width");
                writer.WriteString(width.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("height");
                writer.WriteString(height.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("depth");
                writer.WriteString(depth.ToString());
                writer.WriteEndElement();

                writer.WriteEndElement();


                writer.WriteStartElement("segmented");
                writer.WriteString("0");
                writer.WriteEndElement();

                foreach (var rec in allRectangles)
                {
                    writer.WriteStartElement("object");

                    writer.WriteStartElement("name");
                    writer.WriteString(rec.RectangleText);
                    writer.WriteEndElement();

                    writer.WriteStartElement("pose");
                    writer.WriteString("Unspecified");
                    writer.WriteEndElement();

                    writer.WriteStartElement("truncated");
                    writer.WriteString("0");
                    writer.WriteEndElement();

                    writer.WriteStartElement("difficult");
                    writer.WriteString("0");
                    writer.WriteEndElement();

                    writer.WriteStartElement("bndbox");

                    writer.WriteStartElement("xmin");
                    writer.WriteString((rec.X).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("ymin");
                    writer.WriteString((rec.Y).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("xmax");
                    writer.WriteString((rec.X + rec.RectangleWidth).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("ymax");
                    writer.WriteString((rec.Y + rec.RectangleHeight).ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteEndElement();



                }

                writer.WriteEndDocument();
                writer.Close();
                System.Windows.MessageBox.Show("Erfolgreich");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
