using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningTrainer
{
    public static class ConfigClass
    {
        //Set to True, if you want to use the whole program
        public static bool IsDevModeEnabled = false;

        //ConfigData for DrawerViewModel:

        public static int borderWidth = 10;                   //used when detecting, moving & resizing shapes
        public static double minShapeSize = 20;
        public static double fieldWidth = 200;
        public static double fieldHeight = 200;
        public static double distanceToBorder = 1;

    }
}
