using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer.DrawerTool
{
    public class CustomShapeFormat
    {
        private string label;
        private string fill;
        private string stroke;
        private double opacity;


        public CustomShapeFormat(string label, string fill, string stroke, double opacity)
        {
            this.label = label;
            this.fill = fill;
            this.stroke = stroke;
            this.opacity = opacity;
        }



        public string Label
        {
            get { return this.label; }
            set
            {
                if (label != value)
                {
                    this.label = value;
                }
            }
        }

        public double Opacity
        {
            get { return this.opacity; }
            set
            {
                if (opacity != value)
                {
                    this.opacity = value;
                }
            }
        }

        public string Fill
        {
            get { return this.fill; }
            set
            {
                if (fill != value)
                {
                    this.fill = value;
                }
            }
        }

        public string Stroke
        {
            get { return this.stroke; }
            set
            {
                if (stroke != value)
                {
                    this.stroke = value;
                }
            }
        }
    }
}
