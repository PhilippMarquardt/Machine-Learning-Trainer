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
    public class SupportCode
    {
        public static Mat convertBmp2Mat(Bitmap bitmap)
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
        }

        public static BitmapImage convertMat2BmpImg(Mat mat)
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
    }
}
