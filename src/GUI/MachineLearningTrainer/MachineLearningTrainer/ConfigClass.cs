using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MachineLearningTrainer
{
    public static class ConfigClass
    {
        //Set to True, if you want to use the whole program
        public static bool IsDevModeEnabled = false;

        #region ConfigData for ImageDrawer.xaml.cs
        //ConfigData for ImageDrawer.xaml.cs

        #region AutoSave
        //Sets the autosave Intervall in seconds (always saves)
        public static double autosaveIntervall = 60;
        //Sets the autosave refreshrate that saves always when a change has occured
        public static double autosaveRefreshRate = 1;

        //Sets minWidth of SaveIcon animation
        public static double smallWidth = 24;
        //Sets maxWidht of SaveIcon animation
        public static double bigWidth = 48;

        //Sets minOpacity of SaveIcon animation
        public static double minOpacity = 0.2;
        //Sets maxOpacity of SaveIcon animation
        public static double maxOpacity = 0.7;

        //Sets Animation TimeSpan
        public static TimeSpan durationSaveIconShown = new TimeSpan(0, 0, 0, 0, 150);

        //Sets Animation duration
        public static Duration durationSaveIconAnimated = new Duration(durationSaveIconShown);

        #endregion

        #endregion


        #region ConfigData for DrawerViewModel
        //ConfigData for DrawerViewModel:


        //BorderWidth for Resize-Detection (e.g. bW=10, up to 10 pixels away 
        //from actual border detecting the same shape and resize-Mode
        public static int borderWidth = 4;                   
        
        //min Heigth and Width of Shape
        public static double minShapeSize = 10;

        //Stepsize for Keybindings
        public static int stepSize = 2;

        //Dimensions of rectangles in which Canvas is splited for faster 
        //Shape detection -> "#region Divide img into fields for better performance"
        public static double fieldWidth = 200;
        public static double fieldHeight = 200;
        
        //sets min distance between imgBorder and nearest Shape
        //needed to see Shape-Border even in the corner
        public static double distanceToBorder = 1;

        //sets format of dotted line for selected Rectangle
        public static string viewportUnSelected = "0,0,2000,2000";
        public static string viewportSelected = "0,0,10,10";
        public static System.Windows.Media.TileMode viewportTileModeUnSelected = System.Windows.Media.TileMode.None;
        public static System.Windows.Media.TileMode viewportTileModeSelected = System.Windows.Media.TileMode.Tile;
        #endregion


        #region ConfigData for ShapeModel
        //ConfigData for ShapeModel:


        //Defines BorderThickness of drawn Rectangles
        public static int strokeThickness = 2;
        //public static int strokeThicknessWhenSelected = 4;
        #endregion


    }
}
