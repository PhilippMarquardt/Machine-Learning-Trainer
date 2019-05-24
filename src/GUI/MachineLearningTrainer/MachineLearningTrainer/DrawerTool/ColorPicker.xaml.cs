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
using System.Windows.Shapes;

namespace MachineLearningTrainer.DrawerTool
{
    /// <summary>
    /// Interaktionslogik für ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public ColorPicker()
        {
            InitializeComponent();
            //foreach(var r in (this.DataContext as DrawerViewModel).LabelColorFormat)
            //{
            //    if (r.Label != null)
            //    {
            //        (this.DataContext as DrawerViewModel).SelectedLabel = r;
            //        break;
            //    }
            //}
        }

        private void _colorCanvas_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _colorPicker.SelectedColor = _colorCanvas.SelectedColor;
            if (RenameTxtBox.IsFocused == true)
            {
                (this.DataContext as DrawerViewModel).Enter();
            }
            (this.DataContext as DrawerViewModel).ChangeColor();
            (this.DataContext as DrawerViewModel).ChangeOpacity();
        }

        private void _applyColorChange_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
        }

        private void _colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _colorCanvas.SelectedColor = _colorPicker.SelectedColor;
        }

        private void _sliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _opacityTextBox.Text = Convert.ToString(_sliderOpacity.Value);
            (this.DataContext as DrawerViewModel).ChangeOpacity();
        }

        private void _closeColorChange_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            (this.DataContext as DrawerViewModel).TestLabelName();
            (this.DataContext as DrawerViewModel).SelectedModeItem = "Fill";
            (this.DataContext as DrawerViewModel).ColorPickerEnabled = false;

            if ((this.DataContext as DrawerViewModel).DeactivatedAddLabel == false)
            {
                (this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = false;
            textBox.SelectAll();
            if (textBox.Tag.GetType() == typeof(Subtypes))
            {
                Subtypes tmp = (Subtypes)textBox.Tag;
                (this.DataContext as DrawerViewModel).TmpNewLabel.Label = tmp.Label;
                (this.DataContext as DrawerViewModel).TmpNewLabel.Parent = tmp.Parent;
            }
            else
            {
                (this.DataContext as DrawerViewModel).TmpNewLabel.Label = textBox.Text;
            }
            //(this.DataContext as DrawerViewModel).DeleteSelectionForRename();
        }

        private void RmvLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DrawerViewModel).DeleteSelectedLabel();
        }

        private void AddLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {
            //(this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
            (this.DataContext as DrawerViewModel).AddLabelColorFormat();
            if (RenameTxtBox.IsFocused == true)
            {
                (this.DataContext as DrawerViewModel).Enter();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (this.DataContext as DrawerViewModel).TestLabelName();
            (this.DataContext as DrawerViewModel).SelectedModeItem = "Fill";
            (this.DataContext as DrawerViewModel).ColorPickerEnabled = false;

            if ((this.DataContext as DrawerViewModel).DeactivatedAddLabel == false)
            {
                (this.DataContext as DrawerViewModel).DeactivatedAddLabel = !(this.DataContext as DrawerViewModel).DeactivatedAddLabel;
                
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.IsReadOnly = true;
        }

        private void ColorPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(RenameTxtBox.IsFocused == true)
            {
                (this.DataContext as DrawerViewModel).Enter();
            }
        }
    }
}
