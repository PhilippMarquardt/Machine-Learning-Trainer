using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineLearningTrainer.DrawerTool
{
    /// <summary>
    /// this class contains all methods for converting images in different formats.
    /// </summary>
    public class SupportCode
    {
        /// <summary>
        /// this method converts bitmap to mat 
        /// </summary>
        public static Mat ConvertBmp2Mat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }
        
        /// <summary>
        /// this method converts mat to bitmap
        /// </summary>
        public static BitmapImage ConvertMat2BmpImg(Mat mat)
        {

            Bitmap convertedImg = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);

            using (var memory = new MemoryStream())
            {
                convertedImg.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }

        }

        public static Bitmap MatToBitmap(Mat image)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
        }

    }
}
