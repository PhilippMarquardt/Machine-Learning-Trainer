using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer
{
    public class Dimension
    {
       
        public Dimension(int row, int column, int channel)
        {
            this.Rows = row;
            this.Columns = column;
            this.Channel = channel;
        }

        public Dimension()
        {
        }

        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Channel { get; set; }

        //(x,y,z) shape = for images
        // input_dim = 3 //this results in tensor with 3 elements and batch size of None
        public override string ToString()
        {
            if (!(Rows == 0 && Columns == 0 && Channel == 0) && Rows == 0)
                return String.Format("({0},{1},{2})", Rows, Columns, Channel);
            else if (Columns == 0 && Channel == 0 && !(Rows == 0))
                return Rows.ToString();
            else
                return "";
            
      
        }
    }
}
