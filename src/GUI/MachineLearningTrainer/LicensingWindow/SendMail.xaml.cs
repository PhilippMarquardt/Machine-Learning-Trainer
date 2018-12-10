using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

namespace LicensingWindow
{
    /// <summary>
    /// Interaktionslogik für SendMail.xaml
    /// </summary>
    public partial class SendMail : Window
    {
        string license;
        public SendMail(string lic)
        {
            InitializeComponent();
            license = lic;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBox.Text != "" && textBox.Text != null)
                {
                    MailMessage newMail = new MailMessage();
                    SmtpClient newSmtpServer = new SmtpClient("smtp.1und1.de");
                    newMail.Body = "Hallo Sergey,\n\nder zu aktivierende Code lautet: " + license + "\n\nGesendet von:" + textBox.Text + "/" + textBoxCompany.Text + "\n\nGesendet am:" + DateTime.Now;
                    newMail.From = new MailAddress("licensing@hs-analysis.com");
                    newMail.To.Add("sergey.biniaminov@hs-analysis.com");
                    newMail.Subject = "Lizenz Aktivierung";
                    newSmtpServer.Port = 587;
                    newSmtpServer.Credentials = new System.Net.NetworkCredential("licensing@hs-analysis.com", "Software13");
                    newSmtpServer.EnableSsl = true;
                    newSmtpServer.Send(newMail);
                    MessageBox.Show("Erfolgreich versendet, Sie werden in Kürze den Lizenzschlüssel erhalten");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Bitte einen Namen eintragen");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Fehler beim Senden der Email. Eventuell Internetverbindung überprüfen.");
            }
        }
    }
}

