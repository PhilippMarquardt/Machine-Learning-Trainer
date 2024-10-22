﻿using System;
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

namespace MachineLearningTrainer.DecisionPages
{
    /// <summary>
    /// Interaktionslogik für AnnotationSelection.xaml
    /// </summary>
    public partial class AnnotationSelection : UserControl
    {
        public AnnotationSelection()
        {
            InitializeComponent();
            Application.Current.MainWindow.Title = "Annotation Tool";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(ConfigClass.IsDevModeEnabled == false)
            {
                MainViewModel viewModel = (this.DataContext as MainViewModel);
                if (viewModel.NextPage.CanExecute(null))
                {
                    viewModel.NextPage.Execute(null);
                }
            }
        }
    }
}
