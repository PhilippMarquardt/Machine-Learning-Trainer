using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MachineLearningTrainer.DrawerTool
{
    public class DrawerModel : INotifyPropertyChanged
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

        //public System.Windows.Media.Brush _rectangleFill = System.Windows.Media.Brushes.Green;

        //public System.Windows.Media.Brush RectangleFill
        //{
        //    get
        //    {
        //        return _rectangleFill;
        //    }
        //    set
        //    {
        //        _rectangleFill = value;
        //        OnPropertyChanged("RectangleFill");
        //    }
        //}

        //private double _rectangleOpacity = 0.2;

        //public double RectangleOpacity
        //{
        //    get
        //    {
        //        return _rectangleOpacity;
        //    }
        //    set
        //    {
        //        _rectangleOpacity = value;
        //        OnPropertyChanged("RectColor");
        //    }
        //}
    }


    
}
