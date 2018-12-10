using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Mail;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicensingWindow
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string license = LicenseChecker.GetRequestKey();

        public MainWindow()
        {
            InitializeComponent();
            textBox1.Text = LicenseChecker.GetRequestKey();
            textBoxLicenseKey.Opacity = 0.5;
            textBoxLicenseKey.FontSize = 12;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] stringSplitter = textBoxLicenseKey.Text.Split('-');
                string licenseKey = stringSplitter[0] + "-" + stringSplitter[1] + "-" + stringSplitter[2] + "-" + stringSplitter[3];
                string licenseDate = "-" + stringSplitter[4];
                LicenseChecker.Activate(licenseKey, licenseDate);
                Thread.Sleep(900);
                if (LicenseChecker.IsLicenseActivated())
                {
                    MessageBox.Show("License activated. Please restart the program");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("License Key not valid.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Bitte gültigen LicenseKey eingeben");
            }
        }

        private void textBoxLicenseKey_GotFocus(object sender, RoutedEventArgs e) //Text aus Textbox entfernen
        {
            textBoxLicenseKey.Opacity = 1.0;
            if (textBoxLicenseKey.Text == "Key eingeben")
            {
                textBoxLicenseKey.Text = "";
            }
            textBoxLicenseKey.FontSize = 20;
        }

        private void textBoxLicenseKey_LostFocus(object sender, RoutedEventArgs e)//Bei Eingabe nichts tun, ansonsten StandardText wieder in Textbox einfügen
        {
            if (textBoxLicenseKey.Text.Length == 0)
            {
                textBoxLicenseKey.Text = "Key eingeben";
                textBoxLicenseKey.Opacity = 0.5;
                textBoxLicenseKey.FontSize = 12;
            }
        }

        private void buttonSendMail_Click(object sender, RoutedEventArgs e)  //Mail wird von nicht genutzer Email gesendet
        {
            SendMail mail = new SendMail(textBox1.Text);
            mail.ShowDialog();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Drücken Sie auf den Button 'Code per Mail an HS-Analysis schicken' und geben Sie den Lizenschlüssel, den Sie dann in Kürze erhalten werden, in das Feld für den Lizenzschlüssel ein.\n Alternativ können Sie den Code auch kopieren und an 'sergey.biniaminov@hs-analysis.com' schicken.");
        }
    }
}
