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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MachineLearningTrainer.DrawerTool
{
    /// <summary>
    /// Interaktionslogik für DTLabelColorFormatControl.xaml
    /// </summary>
    public partial class DTLabelColorFormatControl : UserControl
    {
        public DTLabelColorFormatControl()
        {
            InitializeComponent();
        }

        private DrawerViewModel DrawerViewModel
        {
            get
            {
                MainWindow mw = (MainWindow)(Application.Current.MainWindow);

                return (mw.DataContext as DrawerViewModel);
            }
        }

        //private void LabelTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 2)
        //    {
        //        LabelTextBox.Visibility = Visibility.Visible;
        //        labelTextBlock.Visibility = Visibility.Collapsed;
        //        //LabelTextBox.Focus();
        //        e.Handled = true;
        //        LabelTextBox.SelectAll();

        //        Console.WriteLine(DrawerViewModel.SelectedLabel.Label);
        //    }
        //}

        //private void LblTextBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    labelTextBlock.Visibility = Visibility.Visible;
        //    LabelTextBox.Visibility = Visibility.Collapsed;

        //    Console.WriteLine(DrawerViewModel.SelectedLabel.Label);
        //}

        private void ShowHideData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSubtype_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RmvLabelColorFormat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtBlockName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                renamingTextBox.Visibility = Visibility.Visible;
                LabelTextBlock.Visibility = Visibility.Collapsed;
                renamingTextBox.Focus();
                e.Handled = true;
                renamingTextBox.SelectAll();



                DrawerViewModel.TmpNewLabel.Label = LabelTextBlock.Text;
            }
        }

        private void txtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            renamingTextBox.Visibility = Visibility.Hidden;
            LabelTextBlock.Visibility = Visibility.Visible;
            
        }

        private void OnTxtBoxKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                renamingTextBox.Visibility = Visibility.Hidden;
                LabelTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
