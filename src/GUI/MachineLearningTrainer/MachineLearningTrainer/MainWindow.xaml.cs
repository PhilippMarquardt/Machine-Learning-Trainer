using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace MachineLearningTrainer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!LicensingWindow.LicenseChecker.IsLicenseActivated())
            {
                LicensingWindow.MainWindow licensingWindow = new LicensingWindow.MainWindow();
                licensingWindow.ShowDialog();
                Application.Current.Shutdown();
            }
            else
            {

                InitializeComponent();
                WelcomePage page = new WelcomePage();
                MainViewModel viewModel = new MainViewModel(new MainModel(), MainGrid);
                page.DataContext = viewModel;
                MainGrid.Children.Add(page);

                //Set IsDevModeEnabled == true, if you want to use the whole program
                if (ConfigClass.IsDevModeEnabled == false)
                {
                    if (viewModel.RightTransition.CanExecute(null))
                    {
                        viewModel.RightTransition.Execute(null);
                    }
                }

            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}
