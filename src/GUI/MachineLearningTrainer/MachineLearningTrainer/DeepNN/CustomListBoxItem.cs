using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer.DeepNN
{
    public class CustomListBoxItem : INotifyPropertyChanged
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


        public CustomListBoxItem() { }

        public CustomListBoxItem(string content)
        {
            this.Content = content;
        }

        private string _content;

        public string Content
        {
            get
            {
                return this._content.Replace("'","");
            }
            set
            {
                this._content = value;
                OnPropertyChanged("Content");
            }
        }

        private bool _isChecked = false;

        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }
            set
            {
                this._isChecked = value;
                OnPropertyChanged("IsChecked");
              
            }
        }
    }
}
