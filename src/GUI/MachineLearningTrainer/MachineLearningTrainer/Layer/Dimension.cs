using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer
{
    public class Dimension
    {
        private int v1;
        private int v2;
        private int v3;

        public Dimension(int row, int column, int channel)
        {
            this.Rows = row;
            this.Columns = column;
            this.Channel = channel;
        }

        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Channel { get; set; }

        public override string ToString()
        {
            return String.Format("({0},{1},{2})", Rows, Columns, Channel);
        }
    }
}
