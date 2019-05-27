using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer.DrawerTool
{
    public class CustomShapeFormat : INotifyPropertyChanged
    {
        private string label;
        private string fill;
        private string stroke;
        private double opacity;
        private int count = 100;
        private bool visible;
        private bool isSelected;
        private bool isExpanded;
        private ObservableCollection<Subtypes> subtypes;


        public CustomShapeFormat(string label, string fill, string stroke, double opacity)
        {
            this.label = label;
            this.fill = fill;
            this.stroke = stroke;
            this.opacity = opacity;

            this.visible = true;

            this.subtypes = new ObservableCollection<Subtypes>();
        }

        public CustomShapeFormat(string label, string fill, string stroke, double opacity, ObservableCollection<Subtypes> subtypes)
        {
            this.label = label;
            this.fill = fill;
            this.stroke = stroke;
            this.opacity = opacity;

            this.visible = true;

            this.subtypes = new ObservableCollection<Subtypes>();

            foreach (Subtypes sb in subtypes)
            {
                this.subtypes.Add(sb);
            }
        }

        public ObservableCollection<Subtypes> Subtypes
        {
            get { return subtypes; }
            set
            {
                if (value != subtypes)
                {
                    subtypes = value;
                    NotifyPropertyChanged("Subtypes");
                }
            }

        }


        #region Getter/Setter
        public object Me { get { return this; } }

        public string Label
        {
            get { return this.label; }
            set
            {
                if (label != value)
                {
                    this.label = value;
                    NotifyPropertyChanged("Label");
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
                    NotifyPropertyChanged("Opacity");
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
                    NotifyPropertyChanged("Fill");
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
                    NotifyPropertyChanged("Stroke");
                }
            }
        }

        public int Count
        {
            get { return this.count; }
            set
            {
                if (count != value)
                {
                    this.count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (isSelected != value)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (visible != value)
                {
                    this.visible = value;
                    NotifyPropertyChanged("Visible");
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class Subtypes : INotifyPropertyChanged
    {
        private string label;
        private bool visible;
        private bool isSelected;
        private string parent;
        private int count = 100;

        public Subtypes(string label, string parent)
        {
            this.label = label;
            this.parent = parent;

            this.visible = true;
        }


        #region Getter/Setter
        public object Me { get { return this; } }

        public string Label
        {
            get { return this.label; }
            set
            {
                if (label != value)
                {
                    this.label = value;
                    NotifyPropertyChanged("Label");
                }
            }
        }

        public string Parent
        {
            get { return this.parent; }
            set
            {
                if (parent != value)
                {
                    this.parent = value;
                    NotifyPropertyChanged("Parent");
                }
            }
        }

        public int Count
        {
            get { return this.count; }
            set
            {
                if (count != value)
                {
                    this.count = value;
                    NotifyPropertyChanged("Count");
                }
            }
        }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (visible != value)
                {
                    this.visible = value;
                    NotifyPropertyChanged("Visible");
                }
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (isSelected != value)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
