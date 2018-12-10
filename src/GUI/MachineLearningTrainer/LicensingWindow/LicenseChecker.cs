using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LicensingWindow
{
    public class LicenseChecker
    {
        public const string ProductKey = "MachineLearningTrainer";  //muss für jedes Projekt geändert werden zum jeweiligen Projektnamen (Zeile 14,32,87)
        public const string dateZero = "111461072352141312312034921501411159123377";


        static string VolumeSerialNumberString
        {
            get
            {
                return "VolumeSerialNumber";
            }
        }

        /// <summary>
        /// the path of the license file that is created when the product is successfully activated
        /// </summary>
        static string licenseFilePath
        {
            get
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\HS-Analysis\\Licenses\\MachineLearningTrainer.lic";  //In jedem Projekt ändern
            }
        }

        public static bool Activate(String activationKey, String activationDate)
        {
            if (!isLicenseKeyValid(activationKey))
            {
                return false;
            }

            Deactivate();
            Directory.CreateDirectory(Path.GetDirectoryName(licenseFilePath));
            using (StreamWriter outputFile = new StreamWriter(licenseFilePath))
            {
                outputFile.WriteLine(activationKey);

            }
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dateFilePath));
            using (StreamWriter outputFile = new StreamWriter(dateFilePath))
            {
                outputFile.WriteLine(activationDate);

            }
            return true;
        }

        public static void Deactivate()
        {
            if (LicenseFileExists())
            {
                File.Delete(licenseFilePath);
            }
            if (dateFileExists())
            {
                File.Delete(dateFilePath);
            }
        }


        public static bool dateFileExists()
        {
            return File.Exists(dateFilePath);
        }

        public static bool LicenseFileExists()
        {
            return File.Exists(licenseFilePath);
        }

        static string dateFilePath  //get methode für die Date-Datei

        {
            get
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\HS-Analysis\\Licenses\\MachineLearningTrainerDate.lic";  //In jedem Projekt ändern
            }
        }

        public static bool IsLicenseActivated()   //DIESE METHODE AUFRUFEN UM ZU CHECKEN, OB DIE LIZENZ AKTIV IST
        {
            if (!LicenseFileExists() || !dateFileExists())
            {
                return false;
            }

            string[] keyFileLines = System.IO.File.ReadAllLines(licenseFilePath);
            string[] dateKeyFileLines = File.ReadAllLines(dateFilePath);

            if (keyFileLines.Length != 1)
            {
                return false;
            }

            return (isLicenseKeyValid(keyFileLines[0]) && isLicenseDateValid());
        }
        private static string removeEmptySpace(string date)
        {
            date = string.Join("", date.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return date;
        }

        static bool isLicenseDateValid()  //check if the date in datelicenseFile is not older than 20 days
        {
            DateTime check = DateTime.Today;
            var dateCheck = check.Date;

            for (int i = 0; i <= 360; i++)
            {

                if (removeEmptySpace(getDate) == createHashForDate(check.AddDays(-i).ToString("dd.MM.yyyy")) || removeEmptySpace(getDate) == "-" + dateZero)
                {

                    return true;

                }



            }

            return false;
        }
        public static string getDate //returns date from datelicenseFile


        {

            get
            {
                string date;
                var fileStream = new FileStream(dateFilePath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    date = streamReader.ReadToEnd();
                }

                return date;
            }
        }

        static bool isLicenseKeyValid(String licenseKey)
        {
            return RequestKeyToLicenseKey(GetRequestKey()).Equals(licenseKey);
        }

        public static string GetRequestKey()
        {
            string machineDescriptor = Environment.MachineName + getHDDId() + ProductKey;

            HashAlgorithm sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(new ASCIIEncoding().GetBytes(machineDescriptor));

            string result = "";
            foreach (byte b in hash.Take(8)) //only taking first 8 byte increases collision-chance greatly, but makes it easier to enter the result manually. 
            {
                result += b.ToString("X2"); // every byte turns into 2 hex-digits
            }

            return result;
        }
        public static string createHashForDate(string datum) //creates a MD5 Hash for Date and only return first X digits
        {
            MD5 md5 = MD5.Create();
            byte[] input = System.Text.Encoding.ASCII.GetBytes(datum);
            byte[] hash = md5.ComputeHash(input);

            //int j = Convert.ToInt32(hash);
            StringBuilder b = new StringBuilder();
            foreach (var q in hash)
            {



                b.Append(q);
            }

            string y = b.ToString();

            string firstFourCharOfDate = new string(y.Take(10).ToArray());  //if you only want to return some digits use this

            return "-" + y;

        }


        static string getHDDId()
        {
            ManagementObject dsk = new ManagementObject($"win32_logicaldisk.deviceid=\"{Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2)}\"");
            dsk.Get();

            return dsk[VolumeSerialNumberString].ToString();
        }

        public static string RequestKeyToLicenseKey(string requestKey)
        {
            long k0val = 1;
            long k1val = 1;
            long k2val = 999999;
            long k3val = requestKey.Length;
            for (int i = 0; i < requestKey.Length; i++)
            {
                k0val += requestKey.ElementAt(i) * 9933;
                k1val += requestKey.ElementAt(i) * 5295;
                k2val += requestKey.ElementAt(i) * 2816;
                k3val += requestKey.ElementAt(i) * 2822 * i;
            }

            k0val %= 9999;
            k1val %= 9999;
            k2val %= 9999;
            k3val %= 9999;

            string k0 = k0val.ToString("0000");
            string k1 = k1val.ToString("0000");
            string k2 = k2val.ToString("0000");
            string k3 = k3val.ToString("0000");

            return k0 + "-" + k1 + "-" + k2 + "-" + k3;
        }
    }
}
