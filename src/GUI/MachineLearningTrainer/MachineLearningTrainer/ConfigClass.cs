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

        #region ConfigData for DrawerViewModel
        //ConfigData for DrawerViewModel:


        //BorderWidth for Resize-Detection (e.g. bW=10, up to 10 pixels away 
        //from actual border detecting the same shape and resize-Mode
        public static int borderWidth = 4;                   
        
        //min Heigth and Width of Shape
        public static double minShapeSize = 10;

        //Dimensions of rectangles in which Canvas is splited for faster 
        //Shape detection -> "#region Divide img into fields for better performance"
        public static double fieldWidth = 200;
        public static double fieldHeight = 200;
        
        //sets min distance between imgBorder and nearest Shape
        //needed to see Shape-Border even in the corner
        public static double distanceToBorder = 1;
        #endregion


        #region ConfigData for ShapeModel
        //ConfigData for ShapeModel:


        //Defines BorderThickness of drawn Rectangles
        public static int strokeThickness = 2;
        #endregion


    }
}
