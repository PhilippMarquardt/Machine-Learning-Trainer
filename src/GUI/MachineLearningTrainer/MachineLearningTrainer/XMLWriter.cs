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


        public static void WritePascalVocToXML(List<CustomShape> allRectangles, string name, int width, int height, int depth)
        {
            string filePath = name.Remove(name.LastIndexOf('\\'));
            string fileName = (name.Substring(name.LastIndexOf('\\')+1));
            string fileNameWriter = fileName.Remove(fileName.LastIndexOf('.'));
            string fileFolder = filePath.Substring(filePath.LastIndexOf('\\')+1);

            try
            {
                XmlWriter writer = XmlWriter.Create(filePath+"\\"+fileNameWriter+".xml");
                writer.WriteStartDocument();
                writer.WriteStartElement("annotation");

                writer.WriteStartElement("folder");
                writer.WriteString(fileFolder);
                writer.WriteEndElement();

                writer.WriteStartElement("filename");
                writer.WriteString(fileName);
                writer.WriteEndElement();

                writer.WriteStartElement("path");
                writer.WriteString(filePath);
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
                    writer.WriteString(rec.Label);
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
                    writer.WriteString(Convert.ToInt32(rec.X1).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("ymin");
                    writer.WriteString(Convert.ToInt32(rec.Y1).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("xmax");
                    writer.WriteString(Convert.ToInt32(rec.X2).ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("ymax");
                    writer.WriteString(Convert.ToInt32(rec.Y2).ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteStartElement("subtypes");

                    foreach (var sb in rec.Subtypes)
                    {
                        writer.WriteStartElement("def");

                        writer.WriteStartElement("label");
                        writer.WriteString(sb);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();
                writer.Close();
                //System.Windows.MessageBox.Show("Successful!", "Save to XML", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public static void WriteLabelsToCPF(List<CustomShapeFormat> LabelColorFormat, string path)
        {
            string filePath = path.Remove(path.LastIndexOf('\\'));
            string fileName = (path.Substring(path.LastIndexOf('\\') + 1));
            string fileNameWriter = fileName.Remove(fileName.LastIndexOf('.'));
            string fileFolder = filePath.Substring(filePath.LastIndexOf('\\') + 1);

            try
            {
                XmlWriter writer = XmlWriter.Create(filePath + "\\" + fileNameWriter + "_LabelData.cpf");
                writer.WriteStartDocument();
                writer.WriteStartElement("label");

                foreach (var lcf in LabelColorFormat)
                {
                    writer.WriteStartElement("object");

                    writer.WriteStartElement("name");
                    writer.WriteString(lcf.Label);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Format");

                    writer.WriteStartElement("stroke");
                    writer.WriteString(lcf.Stroke);
                    writer.WriteEndElement();

                    writer.WriteStartElement("fill");
                    writer.WriteString(lcf.Fill);
                    writer.WriteEndElement();

                    writer.WriteStartElement("opacity");
                    writer.WriteString(Convert.ToString(lcf.Opacity));
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    writer.WriteStartElement("subtypes");

                    foreach (var sb in lcf.Subtypes)
                    {
                        writer.WriteStartElement("def");

                        writer.WriteStartElement("label");
                        writer.WriteString(sb.Label);
                        writer.WriteEndElement();

                        writer.WriteStartElement("parent");
                        writer.WriteString(sb.Parent);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();
                writer.Close();
                //System.Windows.MessageBox.Show("Successful!", "Save Labels to XML", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
