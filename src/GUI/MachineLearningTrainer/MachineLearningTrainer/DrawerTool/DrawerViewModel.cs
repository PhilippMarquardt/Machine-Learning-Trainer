using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.IO;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MachineLearningTrainer.DrawerTool;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Threading;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Collections.Specialized;
using Brushes = System.Drawing.Brushes;


using System.Diagnostics;

namespace MachineLearningTrainer.DrawerTool 
{
    public class DrawerViewModel : INotifyPropertyChanged
    {
        #region PropertyChangedArea
        public event PropertyChangedEventHandler PropertyChanged;

        //Only for debugging
        int test = 0;
        //

        /// <summary>
        /// Raise Property changed event for INotifyPropertyChanged => dynamlically updating bindings for UI element)
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            //For debugging only
            //Console.WriteLine("OnPropertyChanged: #" + test);
            //test++;
            //

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(name));
                //if (name != "SelectedIndex")
                //{
                //    CollectionViewSource.GetDefaultView(RectanglesView).Refresh();
                //}
            }

        }
        #endregion


        #region RaisePropertChanged Area
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
        #endregion


        #region ShapeCollectionChangedHandler

        /// <summary>
        /// gets called when Element is added or deleted to ShapeCollection / RectanglesView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void ShapeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        {
        //            //foreach (object item in e.NewItems)
        //            //{
        //            //    if (item is CustomShape)
        //            //        ((CustomShape)item).PropertyChanged += Shape_PropertyChanged;
        //            //}
        //        }

        //        if (e.OldItems != null)
        //        {
        //            //foreach (object item in e.OldItems)
        //            //{
        //            //    if (item is CustomShape)
        //            //        ((CustomShape)item).PropertyChanged -= Shape_PropertyChanged;
        //            //}
        //        }
        //    }
        //}

        ///// <summary>
        ///// gets called every time a Property in an RectangleView-Object is changed
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        ////private void Shape_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //private void Shape_PropertyChanged(object sender)
        //{
        //    CollectionViewSource.GetDefaultView(RectanglesView).Refresh();
        //}



        //private void ManipulatableShape_PropertyChanged(object sender)
        //{
        //    CollectionViewSource.GetDefaultView(manipulatableShape).Refresh();
        //}
        #endregion


        private DrawerModel _drawerModel;
        private bool _canExecute = true;
        private MainModel _mainModel;
        private Grid _mainGrid;
        private MainViewModel _mainViewModel;

        public DrawerViewModel(DrawerModel drawerModel, MainModel model, Grid mainGrid, MainViewModel mainViewModel)
        {
            this._drawerModel = drawerModel;
            this._mainGrid = mainGrid;
            this._mainModel = model;
            this._mainViewModel = mainViewModel;
            //RenameCommand = new MyICommand(OnRename, CanRename);
            ComboBoxItems.Add("All Labels");
            SelectedComboBoxItem = "All Labels";

            RectanglesView = new ObservableCollection<CustomShape>();
            Rectangles = new ObservableCollection<CustomShape>();
            LabelColorFormat = new ObservableCollection<CustomShapeFormat>();
            Random _rand = new Random();
            LabelColorFormat.Add(new CustomShapeFormat("test 1", "Red", "Blue", 0.3));
            LabelColorFormat.Add(new CustomShapeFormat("test 2", "Blue", "Red", 0.3));
            //RectanglesView.CollectionChanged += ShapeCollectionChanged;
            //manipulatableShape.PropertyChanged += ManipulatableShape_PropertyChanged;
            undoCustomShapes.Push(new CustomShape(0, 0));
            undoInformation.Push("Dummy");
            redoCustomShapes.Push(new CustomShape(0, 0));
            redoInformation.Push("Dummy");


            //Only for debugging            

            //RectanglesView.Add(new CustomShape(100, 50, 200, 100, 0));
            //Rectangles.Add(new CustomShape(RectanglesView[indexRectanglesView]));
            //undoCustomShapes.Push(RectanglesView[0]);
            //undoInformation.Push("Add");
            //id++;
            //indexRectangles++;
            //indexRectanglesView++;
            //
        }


        public ObservableCollection<CustomShape> RectanglesView { get; set; }
        public ObservableCollection<CustomShape> Rectangles { get; set; }
        public Stack<CustomShape> undoCustomShapes { get; set; } = new Stack<CustomShape>();
        public Stack<string> undoInformation { get; set; } = new Stack<string>();
        public Stack<CustomShape> redoCustomShapes { get; set; } = new Stack<CustomShape>();
        public Stack<string> redoInformation { get; set; } = new Stack<string>();
        public CustomShape manipulatableShape { get; set; } = new CustomShape(100, 100, 200, 100, 0);
        public ObservableCollection<CustomShapeFormat> LabelColorFormat { get; set; }


        #region constant values from ConfigClass: (go to ConfigClass for description)

        private readonly int borderWidth = ConfigClass.borderWidth;
        private readonly double minShapeSize = ConfigClass.minShapeSize;
        private readonly double fieldWidth = ConfigClass.fieldHeight;
        private readonly double fieldHeight = ConfigClass.fieldHeight;
        private readonly double distanceToBorder = ConfigClass.distanceToBorder;

        private readonly bool _isDevModeEnabled = ConfigClass.IsDevModeEnabled;
        public bool IsDevModeEnabled
        {
            get => _isDevModeEnabled;
        } 


        #endregion



        /// <summary>
        /// declares a string that stores the IconPath to differentiate between active and not active
        /// </summary>
        private string _iconPath = "\\Icons\\new.png";
        public string IconPath
        {
            get
            {
                return this._iconPath;
            }
            set
            {
                this._iconPath = value;
                OnPropertyChanged("IconPath");
            }
        }

        public enum MouseState { Normal, CreateRectangle, CreateEllipse, Move, Resize }
        private MouseState mouseHandlingState;
        public MouseState MouseHandlingState
        {
            get => mouseHandlingState;
            set
            {
                if (mouseHandlingState != value)
                {
                    this.mouseHandlingState = value;
                    OnPropertyChanged("MouseHandlingState");
                }
            }
        }


        private System.Windows.Point _vmMousePoint;
        public System.Windows.Point vmMousePoint
        {
            get { return _vmMousePoint; }
            set { _vmMousePoint = value; }
        }





        #region Check If Mouse || Shape On Canvas
        //CheckCanvas:
        //makes sure that you only draw on canvas

        private double tmpX;
        private double tmpY;

        private void CheckOnCanvas(System.Windows.Point mousePosition)
        {
            if (mousePosition.X < distanceToBorder)
            {
                tmpX = distanceToBorder;
            }
            else if (mousePosition.X > MyCanvas.ActualWidth-distanceToBorder)
            {
                tmpX = MyCanvas.ActualWidth-distanceToBorder;
            }
            if (mousePosition.Y < distanceToBorder)
            {
                tmpY = distanceToBorder;
            }
            else if (mousePosition.Y > MyCanvas.ActualHeight-distanceToBorder)
            {
                tmpY = MyCanvas.ActualHeight-distanceToBorder;
            }
        }

        private void CheckOnCanvas(System.Windows.Point mousePosition, double deltaX, double deltaY)
        {
            if (mousePosition.X - deltaX - selectedCustomShape.Width / 2 < distanceToBorder)
            {
                tmpX = distanceToBorder;
                selectedCustomShape.X1 = distanceToBorder;
                selectedCustomShape.XLeft = distanceToBorder;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
            }
            else if (mousePosition.X - deltaX + selectedCustomShape.Width / 2 > MyCanvas.ActualWidth - distanceToBorder)
            {
                tmpX = MyCanvas.ActualWidth - distanceToBorder;
                selectedCustomShape.X2 = MyCanvas.ActualWidth - distanceToBorder;
            }
            if (mousePosition.Y - deltaY - selectedCustomShape.Height / 2 < distanceToBorder)
            {
                tmpY = distanceToBorder;
                selectedCustomShape.Y1 = distanceToBorder;
                selectedCustomShape.YTop = distanceToBorder;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
            }
            else if (mousePosition.Y - deltaY + selectedCustomShape.Height / 2 > MyCanvas.ActualHeight - distanceToBorder)
            {
                tmpY = MyCanvas.ActualHeight - distanceToBorder;
                selectedCustomShape.Y2 = MyCanvas.ActualHeight - distanceToBorder;
            }
        }

        private CustomShape CheckOnCanvas(CustomShape customShape)
        {
            if (customShape.X1 < distanceToBorder)
            {
                customShape.X1 = distanceToBorder;
                customShape.XLeft = distanceToBorder;
                customShape.XLeftBorder = customShape.XLeft - customShape.StrokeThickness;
            }
            if (customShape.X2 > MyCanvas.ActualWidth - distanceToBorder)
            {
                customShape.X2 = MyCanvas.ActualWidth - distanceToBorder;
            }
            if (customShape.Y1 < distanceToBorder)
            {
                customShape.Y1 = distanceToBorder;
                customShape.YTop = distanceToBorder;
                customShape.YTopBorder = customShape.YTop - customShape.StrokeThickness;
            }
            if (customShape.Y2 > MyCanvas.ActualHeight - distanceToBorder)
            {
                customShape.Y2 = MyCanvas.ActualHeight - distanceToBorder;
            }
            customShape.Width = customShape.X2 - customShape.X1;
            customShape.Height = customShape.Y2 - customShape.Y1;

            return customShape;
        }



        /// <summary>
        /// Checks if Object is on Canvas and if not, sets Borders right
        /// </summary>
        private void CheckIfObjectOnCanvas()
        {
            if (selectedCustomShape.X1 < distanceToBorder)
            {
                selectedCustomShape.X1 = distanceToBorder;
                selectedCustomShape.XLeft = distanceToBorder;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Width = Math.Abs(selectedCustomShape.X2 - selectedCustomShape.X1);
            }
            else if (selectedCustomShape.X2 > MyCanvas.ActualWidth - distanceToBorder)
            {
                selectedCustomShape.X2 = MyCanvas.ActualWidth - distanceToBorder;
                selectedCustomShape.Width = Math.Abs(selectedCustomShape.X2 - selectedCustomShape.X1);
            }


            if (selectedCustomShape.Y1 < distanceToBorder)
            {
                selectedCustomShape.Y1 = distanceToBorder;
                selectedCustomShape.YTop = distanceToBorder;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Height = Math.Abs(selectedCustomShape.Y2 - selectedCustomShape.Y1);
            }
            else if (selectedCustomShape.Y2 > MyCanvas.ActualHeight - distanceToBorder)
            {
                selectedCustomShape.Y2 = MyCanvas.ActualHeight - distanceToBorder;
                selectedCustomShape.Height = Math.Abs(selectedCustomShape.Y2 - selectedCustomShape.Y1);
            }
        }
        #endregion


        #region Create ShapeRectangles
        //Create Rectangles:
        //routine to create rectangles

        private int indexRectangles;
        private int indexRectanglesView;
        private int id;


        private ICommand _addRectangle;
        public ICommand AddRectangle
        {
            get
            {
                return _addRectangle ?? (_addRectangle = new CommandHandler(() => AddNewRectangle(), _canExecute));
            }
        }

        /// <summary>
        /// boolean, which tells if checkbox from default label is checked, which means that something was written into the box manually
        /// </summary>
        private bool _isChecked = true;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        /// <summary>
        /// this method switches between draw/not draw state for rectangle 
        /// </summary>
        public void AddNewRectangle()
        {
            if (MouseHandlingState == MouseState.Normal)
            {
                MouseHandlingState = MouseState.CreateRectangle;
                IconPath = "\\Icons\\new_activated.png";
                Enabled = false;
            }
            else if (MouseHandlingState != MouseState.Normal)
            {
                MouseHandlingState = MouseState.Normal;
                IconPath = "\\Icons\\new.png";
                Enabled = true;
            }
        }

        /// <summary>
        /// Create Rectangle depending from MouseState+Position
        /// </summary>
        /// <param name="mousePosition"></param>
        public void CreateRectangle(System.Windows.Point mousePosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                tmpX = mousePosition.X;
                tmpY = mousePosition.Y;
                CheckOnCanvas(mousePosition);

                if (RectanglesView.Count < indexRectanglesView + 1)
                {
                    RectanglesView.Add(new CustomShape(tmpX, tmpY, id));
                    CheckFormat(RectanglesView[indexRectanglesView]);
                }
                else
                {
                    if (RectanglesView[indexRectanglesView].X1 < distanceToBorder && RectanglesView[indexRectanglesView].Y1 < distanceToBorder)
                    {
                        RectanglesView[indexRectanglesView].X1 = tmpX;
                        RectanglesView[indexRectanglesView].Y1 = tmpY;
                    }
                    RectanglesView[indexRectanglesView].X2 = tmpX;
                    RectanglesView[indexRectanglesView].Y2 = tmpY;

                    if (RectanglesView[indexRectanglesView].X1 > RectanglesView[indexRectanglesView].X2)
                    {
                        RectanglesView[indexRectanglesView].XLeft = RectanglesView[indexRectanglesView].X2;
                    }
                    else
                    {
                        RectanglesView[indexRectanglesView].XLeft = RectanglesView[indexRectanglesView].X1;
                    }
                    RectanglesView[indexRectanglesView].XLeftBorder = RectanglesView[indexRectanglesView].XLeft - RectanglesView[indexRectanglesView].StrokeThickness;

                    if (RectanglesView[indexRectanglesView].Y1 > RectanglesView[indexRectanglesView].Y2)
                    {
                        RectanglesView[indexRectanglesView].YTop = RectanglesView[indexRectanglesView].Y2;
                    }
                    else
                    {
                        RectanglesView[indexRectanglesView].YTop = RectanglesView[indexRectanglesView].Y1;
                    }
                    RectanglesView[indexRectanglesView].YTopBorder = RectanglesView[indexRectanglesView].YTop - RectanglesView[indexRectanglesView].StrokeThickness;

                    RectanglesView[indexRectanglesView].Width = Math.Abs(RectanglesView[indexRectanglesView].X2 - RectanglesView[indexRectanglesView].X1);
                    RectanglesView[indexRectanglesView].Height = Math.Abs(RectanglesView[indexRectanglesView].Y2 - RectanglesView[indexRectanglesView].Y1);
                }
            }
            else if (Mouse.LeftButton == MouseButtonState.Released)
            {
                double tmp;
                if (RectanglesView.Count == indexRectanglesView + 1)
                {
                    if (Math.Abs(RectanglesView[indexRectanglesView].X1 - RectanglesView[indexRectanglesView].X2) < minShapeSize
                        || Math.Abs(RectanglesView[indexRectanglesView].Y1 - RectanglesView[indexRectanglesView].Y2) < minShapeSize)
                    {
                        RectanglesView.RemoveAt(indexRectanglesView);
                        indexRectanglesView = RectanglesView.Count();
                    }
                    else
                    {
                        CustomShape createdRectangle = new CustomShape(RectanglesView[indexRectanglesView]);
                        RectanglesView.RemoveAt(indexRectanglesView);
                        indexRectanglesView = RectanglesView.Count();

                        if (createdRectangle.X1 > createdRectangle.X2)
                        {
                            tmp = createdRectangle.X1;
                            createdRectangle.X1 = createdRectangle.X2;
                            createdRectangle.X2 = tmp;
                        }
                        if (createdRectangle.Y1 > createdRectangle.Y2)
                        {
                            tmp = createdRectangle.Y1;
                            createdRectangle.Y1 = createdRectangle.Y2;
                            createdRectangle.Y2 = tmp;
                        }

                        if (SelectedComboBoxItem != "All Labels")
                        {
                            Enabled = false;
                            createdRectangle.Label = SelectedComboBoxItem;
                        }

                        else
                        {
                            Enabled = false;
                            createdRectangle.Label = "default";
                        }

                        CheckFormat(createdRectangle);
                        Rectangles.Add(createdRectangle);
                        RectanglesView.Add(createdRectangle);
                        SaveShapeToField(createdRectangle);
                        createdRectangle = null;

                        undoCustomShapes.Push(RectanglesView[indexRectanglesView]);
                        undoInformation.Push("Add");

                        indexRectanglesView++;
                        indexRectangles++;
                        id++;



                        //Only for debugging
                        Console.WriteLine("RectanglesView count: " + RectanglesView.Count);
                        Console.WriteLine("Rectangles count: " + Rectangles.Count);
                        //

                        RectangleCount = "#" + RectanglesView.Count.ToString();
                        ComboBoxNames();


                        //OnPropertyChanged("Rectangles");
                        OnPropertyChanged("RectanglesView");
                    }
                }
            }
        }

        /// <summary>
        /// Checks Label and if allready exists, copys Color-Format
        /// </summary>
        private void CheckFormat(CustomShape Rectangle)
        {
            if (SelectedComboBoxItem != null || SelectedComboBoxItem != "All Labels")
            {

                string tmpStroke = "LawnGreen";
                string tmpFill = "White";
                double tmpOpacity = 0.5;

                foreach (var r in LabelColorFormat)
                {
                    if (r.Label == Rectangle.Label)
                    {
                        tmpStroke = r.Stroke;
                        tmpFill = r.Fill;
                        tmpOpacity = r.Opacity;
                        break;
                    }
                }

                Rectangle.Stroke = tmpStroke;
                Rectangle.TmpStroke = tmpStroke;
                Rectangle.Fill = tmpFill;
                Rectangle.TmpFill = tmpFill;
                Rectangle.Opacity = tmpOpacity;
                Rectangle.TmpOpacity = tmpOpacity;
            }
        }

        #endregion


        #region Duplicate-Routine
        private ICommand _duplicateCommand;
        public ICommand DuplicateCommand
        {
            get
            {
                return _duplicateCommand ?? (_duplicateCommand = new CommandHandler(() => OnDuplicate(), true));
            }
        }

        /// <summary>
        /// this method, let you duplicate the selected rectangle with its text, height, ... to current mouse position
        /// </summary>
        private void OnDuplicate()
        {
            if (selectedCustomShape != null)
            {
                double tmpHeight;
                double tmpWidth;
                double tmpX1;
                double tmpY1;

                if (DuplicateVar == 1)
                {
                    tmpHeight = selectedCustomShape.Height;
                    tmpWidth = selectedCustomShape.Width;
                    tmpX1 = vmMousePoint.X - selectedCustomShape.Width / 2;
                    tmpY1 = vmMousePoint.Y - selectedCustomShape.Height / 2;

                    DuplicateShape(tmpX1, tmpY1, tmpWidth, tmpHeight);
                }

                else if (DuplicateVar == 0)
                {
                    tmpHeight = selectedCustomShape.Height;
                    tmpWidth = selectedCustomShape.Width;
                    tmpX1 = selectedCustomShape.X1 + 30;
                    tmpY1 = selectedCustomShape.Y1 + 30;

                    DuplicateShape(tmpX1, tmpY1, tmpWidth, tmpHeight);
                }
            }
        }

        private void DuplicateShape(double tmpX1, double tmpY1, double tmpWidth, double tmpHeight)
        {
            CustomShape duplicatedCustomShape = new CustomShape(tmpX1, tmpY1, tmpWidth, tmpHeight, id);
            duplicatedCustomShape = CheckOnCanvas(duplicatedCustomShape);
            CheckFormat(duplicatedCustomShape);
            RectanglesView.Add(duplicatedCustomShape);
            Rectangles.Add(duplicatedCustomShape);
            SaveShapeToField(duplicatedCustomShape);
            indexRectanglesView++;
            indexRectangles++;
            id++;

            undoCustomShapes.Push(duplicatedCustomShape);
            undoInformation.Push("Add");

            RectangleCount = "#" + RectanglesView.Count.ToString();
        }


        private int _duplicateVar;

        public int DuplicateVar
        {
            get { return _duplicateVar; }
            set
            {
                _duplicateVar = value;
            }
        }
        #endregion


        #region Divide img into fields for better performance

        /// <summary>
        /// Saves Shape to according fields in image-grid
        /// </summary>
        /// <param name="customShape"></param>
        private void SaveShapeToField(CustomShape customShape)
        {
            int columnX1 = Convert.ToInt32(Math.Floor(customShape.X1 / fieldWidth));
            int columnX2 = Convert.ToInt32(Math.Floor(customShape.X2 / fieldWidth));
            int rowY1 = Convert.ToInt32(Math.Floor(customShape.Y1 / fieldHeight));
            int rowY2 = Convert.ToInt32(Math.Floor(customShape.Y2 / fieldHeight));

            int numberColumns = Math.Abs(columnX2 - columnX1) + 1;
            int numberRows = Math.Abs(rowY2 - rowY1) + 1;

            for (int i = 0; i < numberColumns; i++)
            {
                for (int j = 0; j < numberRows; j++)
                {
                    ShapeToField(Math.Min(columnX1, columnX2) + i, Math.Min(rowY1, rowY2) + j, customShape);
                }
            }
        }

        private void ShapeToField(int column, int row, CustomShape customShape)
        {
            int fieldNumber = column + row * meshColumnNumber;
            fields[fieldNumber].Add(new CustomShape(customShape));
            int indexRectField = fields[fieldNumber].Count() - 1;
            fields[fieldNumber][indexRectField] = customShape;
        }

        private void RemoveShapeFromField(CustomShape customShape)
        {
            int columnX1 = Convert.ToInt32(Math.Floor(customShape.X1 / fieldWidth));
            int columnX2 = Convert.ToInt32(Math.Floor(customShape.X2 / fieldWidth));
            int rowY1 = Convert.ToInt32(Math.Floor(customShape.Y1 / fieldHeight));
            int rowY2 = Convert.ToInt32(Math.Floor(customShape.Y2 / fieldHeight));

            int numberColumns = Math.Abs(columnX2 - columnX1) + 1;
            int numberRows = Math.Abs(rowY2 - rowY1) + 1;

            for (int i = 0; i < numberColumns; i++)
            {
                for (int j = 0; j < numberRows; j++)
                {
                    ShapeFromField(Math.Min(columnX1, columnX2) + i, Math.Min(rowY1, rowY2) + j, customShape);
                }
            }
        }

        private void ShapeFromField(int column, int row, CustomShape customShape)
        {
            int fieldNumber = column + row * meshColumnNumber;
            if (fieldNumber < 0)
            {
                return;
            }
            foreach (CustomShape r in fields[fieldNumber])
            {
                if (r.Id == customShape.Id)
                {
                    fields[fieldNumber].Remove(r);
                    break;
                }
            }
        }

        #endregion


        #region MoveShape
        //Move-Block:
        //Routine to move different shapes on canvas

        private double deltaX = 0;
        private double deltaY = 0;
        private System.Windows.Point tmpRelativPosition;



        internal void Move(System.Windows.Point mousePosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && selectedCustomShape != null)
            {
                if (selectedCustomShape.Move == false)
                {
                    ActivateMove(mousePosition);
                }
                else
                {
                    MoveCustomShape(mousePosition);
                }
            }
            else
            {
                DetectMove(mousePosition);
                if (selectedCustomShape != null)
                {
                    if (selectedCustomShape.Move == false)
                    {
                    }
                    else
                    {
                        DeactivateMove(mousePosition);
                    }
                } 
            }
        }

        private void DetectMove(System.Windows.Point mousePosition)
        {
            if (detectedCustomShape != null)
            {
                if ((detectedCustomShape.X1 + borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 - borderWidth) &&
                (detectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 - borderWidth))
                {
                    Mouse.OverrideCursor = Cursors.SizeAll;
                }
            }
        }

        private void ActivateMove(System.Windows.Point mousePosition)
        {
            if (selectedCustomShape.IsMouseOver == true && selectedCustomShape.Resize == false)
            {
                deltaX = mousePosition.X - selectedCustomShape.Center.X;
                deltaY = mousePosition.Y - selectedCustomShape.Center.Y;
                mouseHandlingState = MouseState.Move;

                selectedCustomShape.Move = true;
                CopyForUndo("Move");

                RemoveShapeFromField(selectedCustomShape);
            }
        }

        private void MoveCustomShape(System.Windows.Point mousePosition)
        {
            if (selectedCustomShape.Move == true)
            {
                selectedCustomShape.Resize = false;
                tmpRelativPosition.X = mousePosition.X - deltaX;
                tmpRelativPosition.Y = mousePosition.Y - deltaY;
                selectedCustomShape.Center = tmpRelativPosition;
                selectedCustomShape.YTop = selectedCustomShape.Y1;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                selectedCustomShape.XLeft = selectedCustomShape.X1;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                Mouse.OverrideCursor = Cursors.SizeAll;
            }
        }

        private void DeactivateMove(System.Windows.Point mousePosition)
        {
            CheckIfObjectOnCanvas();

            int tmpIndex = 0;
            foreach (CustomShape r in Rectangles)
            {
                if (r.Id == selectedCustomShape.Id)
                {
                    Rectangles.RemoveAt(tmpIndex);
                    Rectangles.Insert(tmpIndex, selectedCustomShape);

                    SaveShapeToField(selectedCustomShape);
                    break;
                }
                tmpIndex++;
            }

            selectedCustomShape.Move = false;
        }
        #endregion


        #region ResizeCustomShape
        //resize-Block:
        //routine to resize different shapes on canvas

        enum ResizeDirection { SizeN, SizeNE, SizeE, SizeSE, SizeS, SizeSW, SizeW, SizeNW }
        ResizeDirection resizeDirection;

        internal void Resize(System.Windows.Point mousePosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && selectedCustomShape != null)
            {
                if (selectedCustomShape.Resize == false)
                {
                    ActivateResize(mousePosition);
                }
                else
                {
                    ResizeCustomShape(mousePosition);
                }
            }
            else
            {
                DetectResize(mousePosition);
                if (selectedCustomShape != null)
                {
                    if (!selectedCustomShape.Resize == false)
                    {
                        DeactivateResize();
                    }
                }
            }
        }

        private void DetectResize(System.Windows.Point mousePosition)
        {
            if (detectedCustomShape != null)
            {
                if (detectedCustomShape.X1 - borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X1 + borderWidth)
                {
                    if (detectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y1 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    }
                    else if (detectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 - borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeWE;
                    }
                    else if (detectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                    }
                }
                else if (detectedCustomShape.X2 - borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 + borderWidth)
                {
                    if (detectedCustomShape.Y1 < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y1 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNESW;
                    }
                    else if (detectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 - borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeWE;
                    }
                    else if (detectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNWSE;
                    }
                }
                else if (detectedCustomShape.X1 + borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 - borderWidth)
                {
                    if (detectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y1 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNS;
                    }
                    else if (detectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 + borderWidth)
                    {
                        Mouse.OverrideCursor = Cursors.SizeNS;
                    }
                }
                else
                {
                    mouseHandlingState = MouseState.Normal;
                }
            }
        }

        private void ActivateResize(System.Windows.Point mousePosition)
        {
            if (selectedCustomShape.X1 - borderWidth < mousePosition.X && mousePosition.X < selectedCustomShape.X1 + borderWidth)
            {
                if (selectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y1 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeNW;
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 - borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeW;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeSW;
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
            }
            else if (selectedCustomShape.X2 - borderWidth < mousePosition.X && mousePosition.X < selectedCustomShape.X2 + borderWidth)
            {
                if (selectedCustomShape.Y1 < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y1 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeNE;
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 - borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeE;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeSE;
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
            }
            else if (selectedCustomShape.X1 + borderWidth < mousePosition.X && mousePosition.X < selectedCustomShape.X2 - borderWidth)
            {
                if (selectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y1 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeN;
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeS;
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    RemoveShapeFromField(selectedCustomShape);
                    CopyForUndo("Resize");
                }
            }
        }

        private void ResizeCustomShape(System.Windows.Point mousePosition)
        {
            tmpX = mousePosition.X;
            tmpY = mousePosition.Y;
            CheckOnCanvas(mousePosition);

            if (resizeDirection == ResizeDirection.SizeN)
            {
                if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                {
                    selectedCustomShape.Y1 = tmpY;
                    selectedCustomShape.YTop = tmpY;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeNE)
            {
                if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                {
                    selectedCustomShape.Y1 = tmpY;
                    selectedCustomShape.YTop = tmpY;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }

                if (minShapeSize < selectedCustomShape.Width + (tmpX - selectedCustomShape.X2))
                {
                    selectedCustomShape.X2 = tmpX;
                }
                else
                {
                    selectedCustomShape.X2 = selectedCustomShape.X1 + minShapeSize;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeE)
            {
                if (minShapeSize < selectedCustomShape.Width + (tmpX - selectedCustomShape.X2))
                {
                    selectedCustomShape.X2 = tmpX;
                }
                else
                {
                    selectedCustomShape.X2 = selectedCustomShape.X1 + minShapeSize;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeSE)
            {
                if (minShapeSize < selectedCustomShape.Width + (tmpX - selectedCustomShape.X2))
                {
                    selectedCustomShape.X2 = tmpX;
                }
                else
                {
                    selectedCustomShape.X2 = selectedCustomShape.X1 + minShapeSize;
                }

                if (minShapeSize < selectedCustomShape.Height + (tmpY - selectedCustomShape.Y2))
                {
                    selectedCustomShape.Y2 = tmpY;
                }
                else
                {
                    selectedCustomShape.Y2 = selectedCustomShape.Y1 + minShapeSize;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeS)
            {
                if (minShapeSize < selectedCustomShape.Height + (tmpY - selectedCustomShape.Y2))
                {
                    selectedCustomShape.Y2 = tmpY;
                }
                else
                {
                    selectedCustomShape.Y2 = selectedCustomShape.Y1 + minShapeSize;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeSW)
            {
                if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 - tmpX))
                {
                    selectedCustomShape.X1 = tmpX;
                    selectedCustomShape.XLeft = tmpX;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }

                if (minShapeSize < selectedCustomShape.Height + (tmpY - selectedCustomShape.Y2))
                {
                    selectedCustomShape.Y2 = tmpY;
                }
                else
                {
                    selectedCustomShape.Y2 = selectedCustomShape.Y1 + minShapeSize;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeW)
            {
                if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 - tmpX))
                {
                    selectedCustomShape.X1 = tmpX;
                    selectedCustomShape.XLeft = tmpX;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }
            }
            else if (resizeDirection == ResizeDirection.SizeNW)
            {
                if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 - tmpX))
                {
                    selectedCustomShape.X1 = tmpX;
                    selectedCustomShape.XLeft = tmpX;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                    selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                }

                if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                {
                    selectedCustomShape.Y1 = tmpY;
                    selectedCustomShape.YTop = tmpY;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }
                else
                {
                    selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                    selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                }
            }

            selectedCustomShape.Width = Math.Abs(selectedCustomShape.X2 - selectedCustomShape.X1);
            selectedCustomShape.Height = Math.Abs(selectedCustomShape.Y2 - selectedCustomShape.Y1);

            ////Only for debugging
            //var s = Stopwatch.StartNew();
            ////IfMethod();
            ////SwitchMethod();
            //s.Stop();
            //Console.WriteLine(((double)(s.Elapsed.TotalMilliseconds*1000)).ToString("0.00 ns"));
            ////
        }

        private void DeactivateResize()
        {
            int tmpIndex = 0;
            foreach (CustomShape r in Rectangles)
            {
                if (r.Id == selectedCustomShape.Id)
                {
                    Rectangles.RemoveAt(tmpIndex);
                    Rectangles.Insert(tmpIndex, selectedCustomShape);
                    SaveShapeToField(selectedCustomShape);
                    break;
                }
                tmpIndex++;
            }

            selectedCustomShape.Resize = false;
        }

        #endregion


        #region Detect & Select Shape
        //DetectShape:
        //routines to detect different Shapes on Canvas
        private bool shapeDetected = false;
        private CustomShape detectedCustomShape = null;

        /// <summary>
        /// raise OnPropertyChanged-Event when shape is detected
        /// </summary>
        public bool ShapeDetected
        {
            get => shapeDetected;
            set
            {
                if (shapeDetected != value)
                {
                    this.shapeDetected = value;
                    OnPropertyChanged("ShapeDetected");
                }
            }
        }

        private bool shapeSelected = false;
        private CustomShape selectedCustomShape = null;
        public CustomShape SelectedCustomShape
        {
            get
            {
                return selectedCustomShape;
            }

            set
            {
                if (value != null)
                {
                    selectedCustomShape = new CustomShape(value);
                }

            }
        }

        //Only for debugging
        private CustomShape _test = new CustomShape(10,10,200,300,0);
        public CustomShape Test
        {
            get
            {
                return _test;
            }
            set
            {
                if(value != null)
                {
                    _test = new CustomShape(value);
                }
            }
        }
        //


        /// <summary>
        /// raise OnPropertyChanged-Event when shape is selected
        /// </summary>
        public bool ShapeSelected
        {
            get => shapeDetected;
            set
            {
                if (shapeSelected != value)
                {
                    this.shapeSelected = value;
                    OnPropertyChanged("ShapeSelected");
                }
            }
        }

        private string tmpFill = "Transparent";
        private double tmpOpacity = 1;

        internal void DetectCustomShape(System.Windows.Point mousePosition)
        {
            if (detectedCustomShape == null)
            {
                int column = Convert.ToInt32(Math.Floor(mousePosition.X / fieldWidth));
                int row = Convert.ToInt32(Math.Floor(mousePosition.Y / fieldHeight));
                int fieldNumber = column + row * meshColumnNumber;

                //only for debugging

                //Console.WriteLine("CustomShapes in field " + fieldNumber + ": " + fields[fieldNumber].Count() + " \n new Loop");
                //

                if (fieldNumber < 0)
                {
                    return;
                }
                foreach (CustomShape r in fields[fieldNumber])
                {
                    if ((r.X1 < mousePosition.X && mousePosition.X < r.X2) && (r.Y1 < mousePosition.Y && mousePosition.Y < r.Y2))
                    {
                        detectedCustomShape = r;
                        break;
                    }
                }
            }
            else if (!((detectedCustomShape.X1 - borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 + borderWidth)
                && (detectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 + borderWidth)) && shapeSelected == false)
            {
                detectedCustomShape.Opacity = detectedCustomShape.TmpOpacity;
                detectedCustomShape.Fill = detectedCustomShape.TmpFill;
                detectedCustomShape.IsMouseOver = false;
                shapeDetected = false;
                detectedCustomShape = null;
            }
            else if (detectedCustomShape.Fill != "Gray")
            {
                detectedCustomShape.TmpOpacity = detectedCustomShape.Opacity;
                detectedCustomShape.TmpFill = detectedCustomShape.Fill;
                detectedCustomShape.Opacity = 0.5;
                detectedCustomShape.Fill = "Gray";
                detectedCustomShape.IsMouseOver = true;
                shapeDetected = true;

                //only for debugging

                //foreach (CustomShape r in Rectangles)
                //{
                //    if (r.Id == detectedCustomShape.Id)
                //    {
                //        Console.WriteLine("rectangleView Fill: " + r.Fill);
                //        break;
                //    }
                //}

                //foreach (CustomShape r in RectanglesView)
                //{
                //    if (r.Id == detectedCustomShape.Id)
                //    {
                //        Console.WriteLine("rectangle Fill: " + r.Fill);
                //        break;
                //    }
                //}


                //Console.WriteLine("detectedCustomShape Fill: " + detectedCustomShape.Fill);
                //
            }
        }

        internal void SelectCustomShape()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.Stroke = selectedCustomShape.TmpStroke;
            }
            if (detectedCustomShape != null)
            {
                selectedCustomShape = this.detectedCustomShape;
                selectedCustomShape.Stroke = "Red";
                SelectListItem();
            }
        }

        internal void SelectCustomShape(int indexView)
        {
            foreach (CustomShape r in RectanglesView)
            {
                r.Stroke = r.TmpStroke;
            }
            if (-1 < indexView && indexView < RectanglesView.Count())
            {
                selectedCustomShape = RectanglesView[indexView];
                selectedCustomShape.Stroke = "Red";
                //DefaultLabel = selectedCustomShape.Label;
            }
        }


        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        internal void SelectListItem()
        {
            int tmpIndex = 0;
            if (selectedCustomShape != null)
            {
                foreach (CustomShape r in RectanglesView)
                {
                    if (r.Id == selectedCustomShape.Id)
                    {
                        SelectedIndex = tmpIndex;
                        return;
                    }
                    tmpIndex++;
                }
            }
            else
            {
                SelectedIndex = -1;
            }
        }


        internal int GetSelectedItemIndex()
        {
            int itemIndex = 0;
            if (selectedCustomShape != null)
            {
                foreach (CustomShape r in RectanglesView)
                {
                    if (r.Id == selectedCustomShape.Id)
                    {
                        return itemIndex;
                    }
                    itemIndex++;
                }
            }

            return -1;
        }
        #endregion


        #region Crop-Mode
        private bool _cropModeChecked = false;

        public bool CropModeChecked
        {
            get
            {
                return _cropModeChecked;
            }
            set
            {
                _cropModeChecked = value;
                if (_cropModeChecked == true)
                {
                    ActivateCropMode();
                }
                else
                {
                    DeactivateCropMode();
                }
                OnPropertyChanged("CropModeChecked");
            }
        }

        public void ActivateCropMode()
        {
            if(selectedCustomShape != null)
            {
                foreach (CustomShape r in RectanglesView)
                {
                    r.Stroke = "Transparent";
                }

                ClearFields();
                selectedCustomShape.Stroke = "Red";
                SaveShapeToField(selectedCustomShape);
            }
            else
            {
                _cropModeChecked = false;
            }
        }

        public void DeactivateCropMode()
        {
            foreach (CustomShape r in RectanglesView)
            {
                r.Stroke = r.TmpStroke;
            }
            selectedCustomShape.Stroke = "Red";
            FilterName();
        }
        #endregion


        #region Delete Routine
        
        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new CommandHandler(() => OnDelete(), true));
            }
        }

        /// <summary>
        /// Delete the selected shape
        /// </summary>
        private void OnDelete()
        {
            if (selectedCustomShape != null)
            {
                foreach (CustomShape rv in RectanglesView)
                {
                    if (rv.Id == selectedCustomShape.Id)
                    {
                        undoCustomShapes.Push(rv);
                        undoInformation.Push("Delete");
                        RectanglesView.Remove(rv);
                        indexRectanglesView--;
                        RemoveShapeFromField(rv);

                        foreach (CustomShape r in Rectangles)
                        {
                            if (r.Id == selectedCustomShape.Id)
                            {
                                Rectangles.Remove(r);
                                indexRectangles--;

                                //Only for debugging
                                Console.WriteLine("RectanglesView count: " + RectanglesView.Count);
                                Console.WriteLine("Rectangles count: " + Rectangles.Count);
                                //

                                string tmpFilterName = SelectedComboBoxItem;
                                ComboBoxNames();
                                SelectedComboBoxItem = tmpFilterName;
                                if (RectanglesView.Count() == 0)
                                {
                                    SelectedComboBoxItem = "All Labels";
                                }
                                FilterName();
                                RectangleCount = "#" + RectanglesView.Count.ToString();

                                break;
                            }
                        }
                        break;
                    }
                }
            }

            //this.IsOpen = false;
            //temp = SelectedComboBoxItem;
            //ComboBoxNames();
            //AllRectanglesView = AllRectangles;
            //FilteredRectangles = AllRectangles;
            //OnPropertyChanged("AllRectanglesView");
            //OnPropertyChanged("FilteredRectangles");
            //OnPropertyChanged("ComboBoxNames");
            //SelectedComboBoxItem = temp;
            //FilterName();
        }

        
        #endregion


        #region Undo
        private ICommand _undoStackCommand;
        public ICommand UndoCommand
        {
            get
            {
                return _undoStackCommand ?? (_undoStackCommand = new CommandHandler(() => Undo(), _canExecute));
            }
        }

        private void CopyForUndo(string undoInformation)
        {
            //double tmpHeight = selectedCustomShape.Height;
            //double tmpWidth = selectedCustomShape.Width;
            //double tmpX1 = selectedCustomShape.X1;
            //double tmpY1 = selectedCustomShape.Y1;
            //int tmpId = selectedCustomShape.Id;

            //CustomShape undoShape = new CustomShape(selectedCustomShape);
            //undoCustomShapes.Push(undoShape);
            //this.undoInformation.Push(undoInformation);
        }

        /// <summary>
        /// this method undoes actions on canvas 
        /// </summary>
        private void Undo()
        {
            if (undoCustomShapes.Count() > 1 && undoInformation.Count() > 1)
            {
                switch (undoInformation.Peek())
                {
                    // when you have added a rectangle, the undo command will delete the rectangle.
                    case "Add":
                        {
                            CustomShape top = undoCustomShapes.Pop();
                            string info = undoInformation.Pop();

                            redoCustomShapes.Push(top);
                            redoInformation.Push(info);

                            RectanglesView.Remove(top);
                            Rectangles.Remove(top);
                            RemoveShapeFromField(top);

                            indexRectanglesView--;
                            indexRectangles--;
                            break;
                        }
                    // when you have deleted a rectangle, the undo command will add the rectangle.
                    case "Delete":
                        {
                            CustomShape top = undoCustomShapes.Pop();
                            string info = undoInformation.Pop();

                            redoCustomShapes.Push(top);
                            redoInformation.Push(info);

                            RectanglesView.Add(top);
                            Rectangles.Add(top);
                            SaveShapeToField(top);

                            indexRectanglesView++;
                            indexRectangles++;
                            break;
                        }
                    // when you have resized a rectangle, the undo command will restore the old size.
                    // when you have moved a rectangle, the undo command will restore the old position.
                    case "Resize":
                    case "Move":
                        {
                            CustomShape top = undoCustomShapes.Pop();
                            string info = undoInformation.Pop();

                            redoCustomShapes.Push(top);
                            redoInformation.Push(info);

                            foreach (CustomShape rv in RectanglesView)
                            {
                                if (rv.Id == top.Id)
                                {
                                    int tmpIndex = RectanglesView.IndexOf(rv);
                                    RectanglesView.RemoveAt(tmpIndex);
                                    RectanglesView.Insert(tmpIndex, top);
                                    tmpIndex = 0;
                                    foreach (CustomShape r in Rectangles)
                                    {
                                        if (r.Id == top.Id)
                                        {
                                            Rectangles.RemoveAt(tmpIndex);
                                            Rectangles.Insert(tmpIndex, top);
                                            break;
                                        }
                                        tmpIndex++;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                }

                RectangleCount = "#" + RectanglesView.Count.ToString();

            }
            OnPropertyChanged("");
        }
        #endregion


        #region Redo
        private ICommand _redoStackCommand;
        public ICommand RedoCommand
        {
            get
            {
                return _redoStackCommand ?? (_redoStackCommand = new CommandHandler(() => Redo(), _canExecute));
            }
        }

        private void Redo()
        {
            // Undo the undo command :D
            if (redoCustomShapes.Count() > 1 && redoInformation.Count() > 1)
            {
                switch (redoInformation.Peek())
                {
                    // see undo command documentation above.
                    case "Add":
                        {
                            var top = redoCustomShapes.Pop();
                            var info = redoInformation.Pop();

                            undoCustomShapes.Push(top);
                            undoInformation.Push(info);

                            RectanglesView.Add(top);
                            Rectangles.Add(top);
                            SaveShapeToField(top);

                            indexRectanglesView++;
                            indexRectangles++;
                            break;
                        }

                    // see undo documentation above.
                    case "Delete":
                        {
                            var top = redoCustomShapes.Pop();
                            var info = redoInformation.Pop();

                            undoCustomShapes.Push(top);
                            undoInformation.Push(info);

                            RectanglesView.Remove(top);
                            Rectangles.Remove(top);
                            RemoveShapeFromField(top);

                            indexRectanglesView--;
                            indexRectangles--;
                            break;
                        }

                    // see undo documentation above.
                    case "Resize":
                    case "Move":
                        {
                            CustomShape top = redoCustomShapes.Pop();
                            string info = redoInformation.Pop();

                            undoCustomShapes.Push(top);
                            undoInformation.Push(info);

                            foreach (CustomShape rv in RectanglesView)
                            {
                                if (rv.Id == top.Id)
                                {
                                    int tmpIndex = RectanglesView.IndexOf(rv);
                                    RectanglesView.RemoveAt(tmpIndex);
                                    RectanglesView.Insert(tmpIndex, top);
                                    tmpIndex = 0;
                                    foreach (CustomShape r in Rectangles)
                                    {
                                        if (r.Id == top.Id)
                                        {
                                            Rectangles.RemoveAt(tmpIndex);
                                            Rectangles.Insert(tmpIndex, top);
                                            break;
                                        }
                                        tmpIndex++;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                }

                if (SelectedComboBoxItem == "All Labels")
                {
                    RectangleCount = "#" + Rectangles.Count.ToString();
                }

                else if (SelectedComboBoxItem != "All Labels")
                {
                    RectangleCount = "#" + RectanglesView.Count.ToString();
                }
            }
            OnPropertyChanged("");
        }
        #endregion


        #region ComboBox with ItemLabels


        private bool _isOpen = false;

        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }

            set
            {
                this._isOpen = value;
                OnPropertyChanged("IsOpen");
            }
        }

        private bool _colorPickerEnabled;
        public bool ColorPickerEnabled
        {
            get
            {
                return _colorPickerEnabled;
            }
            set
            {
                if (value != _colorPickerEnabled)
                {
                    _colorPickerEnabled = value;
                    OnPropertyChanged("ColorPickerEnabled");
                }
            }
        }

        private string _colorPickerIconPath = "\\Icons\\colorpicker_grayedout.jpg";
        public string ColorPickerIconPath
        {
            get
            {
                return _colorPickerIconPath;
            }
            set
            {
                if(value != _colorPickerIconPath)
                {
                    _colorPickerIconPath = value;
                    OnPropertyChanged("ColorPickerIconPath");
                }
            }
        }

        private CustomShapeFormat _selectedLabel = new CustomShapeFormat("","","",1);
        public CustomShapeFormat SelectedLabel
        {
            get
            {
                return _selectedLabel;
            }
            set
            {
                if(value != _selectedLabel)
                {
                    _selectedLabel = value;
                    OnPropertyChanged("SelectedLabel");
                }
            }
        }
        


        /// <summary>
        /// this method filters the list, depending on which label is selected in the combobox
        /// </summary>
        public void FilterName()
        {
            //if (ListViewImage.Contains("grid"))
            //{
            //    FilterVisibilitySelectedGallery = false;
            //    FilterVisibilityAllLabels = false;
            //    FilterVisibilityAllLabelsGallery = false;
            //    FilterVisibilitySelected = true;
            //}
            //else if (ListViewImage.Contains("list"))
            //{
            //    FilterVisibilitySelectedGallery = true;
            //    FilterVisibilityAllLabels = false;
            //    FilterVisibilityAllLabelsGallery = false;
            //    FilterVisibilitySelected = false;
            //}

            RectanglesView.Clear();
            ClearFields();

            foreach (CustomShape r in Rectangles)
            {
                if (r.Label == SelectedComboBoxItem || "All Labels" == SelectedComboBoxItem)
                {
                    RectanglesView.Add(r);
                    SaveShapeToField(r);
                }
            }

            indexRectanglesView = RectanglesView.Count();

            RectangleCount = "#" + RectanglesView.Count.ToString();
        }

        /// <summary>
        /// this method adds all different labels to the combobox
        /// </summary>
        public void ComboBoxNames()
        {
            temp = SelectedComboBoxItem;
            ComboBoxItems.Clear();
            ComboBoxItems.Add("All Labels");

            foreach (var rec in Rectangles)
            {
                if (!ComboBoxItems.Contains(rec.Label))
                {
                    ComboBoxItems.Add(rec.Label);
                    //OnPropertyChanged("ComboBoxItems");
                }
            }
            if (SelectedComboBoxItem != null)
            {
                SelectedComboBoxItem = temp;
            }
            else
            {
                SelectedComboBoxItem = "All Labels";
            }
        }

        #region Variablen
        private string _selectedComboBoxItem;

        public string SelectedComboBoxItem
        {
            get
            {
                return _selectedComboBoxItem;
            }
            set
            {
                _selectedComboBoxItem = value;
                OnPropertyChanged("SelectedComboBoxItem");

            }
        }


        private bool _filterVisibilitySelected = false;

        public bool FilterVisibilitySelected
        {
            get
            {
                return _filterVisibilitySelected;
            }
            set
            {
                _filterVisibilitySelected = value;
                OnPropertyChanged("FilterVisibility1");
            }
        }

        private bool _filterVisibilityAllLabels = true;

        public bool FilterVisibilityAllLabels
        {
            get
            {
                return _filterVisibilityAllLabels;
            }
            set
            {
                _filterVisibilityAllLabels = value;
                OnPropertyChanged("FilterVisibility");
            }
        }

        private bool _filterVisibilitySelectedGallery = false;

        public bool FilterVisibilitySelectedGallery
        {
            get
            {
                return _filterVisibilitySelectedGallery;
            }
            set
            {
                _filterVisibilitySelectedGallery = value;
                OnPropertyChanged("FilterVisibilitySelectedGallery");
            }
        }

        private bool _filterVisibilityAllLabelsGallery = false;

        public bool FilterVisibilityAllLabelsGallery
        {
            get
            {
                return _filterVisibilityAllLabelsGallery;
            }
            set
            {
                _filterVisibilityAllLabelsGallery = value;
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");
            }
        }
        #endregion


        #endregion


        #region ListBox

        /// <summary>
        /// sorts the list of rectangles in ascending order of their name
        /// </summary>
        public void SortList()
        {
            //ObservableCollection<CustomShape> sortedRectangles = new ObservableCollection<CustomShape>
            //    (RectanglesView.OrderBy(resizable => resizable.Label));

            //RectanglesView.Clear();
            //foreach (CustomShape r in sortedRectangles)
            //{
            //    RectanglesView.Add(r);
            //}
        }


        /// <summary>
        /// string variabel, which contains the name of rectangle
        /// </summary>
        private string _label;
        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                _label = value;
                OnPropertyChanged("Label");
            }
        }


        #endregion


        #region Colorpicker

        #region public Variables with Getter/Setter-Method

        private string _selectedColor = "Red";
        public string SelectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                _selectedColor = value;
                OnPropertyChanged("SelectedColor");
            }
        }


        private double _selectedOpacity = 1;
        public double SelectedOpacity
        {
            get
            {
                return _selectedOpacity;
            }
            set
            {
                _selectedOpacity = value;
            }
        }

        public ObservableCollection<string> ModeItems { get; } = new ObservableCollection<string>() { "Fill", "Border", "Both" };

        private string _selectedModeItem = "Fill";
        public string SelectedModeItem
        {
            get
            {
                return _selectedModeItem;
            }
            set
            {
                _selectedModeItem = value;
                OnPropertyChanged("SelectedModeItem");
            }
        }

        private bool _deactivatedAddLabel = true;
        public bool DeactivatedAddLabel
        {
            get
            {
                return _deactivatedAddLabel;
            }
            set
            {
                _deactivatedAddLabel = value;
                OnPropertyChanged("DeactivatedAddLabel");
            }
        }

        private  CustomShapeFormat _tmpNewLabel = new CustomShapeFormat("","White","Black",0.3);
        public CustomShapeFormat TmpNewLabel
        {
            get
            {
                return _tmpNewLabel;
            }
            set
            {
                _tmpNewLabel = value;
                OnPropertyChanged("TmpNewLabel");
            }
        }

        public ObservableCollection<string> LabelList = new ObservableCollection<string>();

        private string _selectedLabelListItem;
        public string SelectedLabelListItem
        {
            get
            {
                return _selectedLabelListItem;
            }
            set
            {
                _selectedLabelListItem = value;
                OnPropertyChanged("SelectedLabelList");
            }
        }


        #endregion

        internal void TestLabelName()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.lblTextBox_Color = "Red";
                foreach (var lcf in LabelColorFormat)
                {
                    if (lcf.Label == selectedCustomShape.Label)
                    {
                        selectedCustomShape.lblTextBox_Color = "Black";
                    }
                }
            }

            foreach (var rv in RectanglesView)
            {
                foreach (var lcf in LabelColorFormat)
                {
                    if (rv.Label == lcf.Label)
                    {
                        rv.lblTextBox_Color = "Black";
                    }
                }
            }
        }

        //Determine which Shapes have to be changed
        public void ChangeColor()
        {
            if (SelectedComboBoxItem != null && SelectedLabel != null)
            {
                {
                    foreach (var r in Rectangles)
                    {
                        if (SelectedLabel.Label == r.Label)
                        {
                            ChangeColorRoutine(r);
                        }
                    }
                }
            }
            OnPropertyChanged("ChangeColor");
        }

        //Change color of selected Part (Fill, Border, Both)
        public void ChangeColorRoutine(CustomShape r)
        {
            switch (_selectedModeItem)
            {
                case "Fill":
                    r.Fill = SelectedLabel.Fill;
                    r.TmpFill = SelectedLabel.Fill;
                    break;
                case "Border":
                    r.Stroke = SelectedLabel.Stroke;
                    r.TmpStroke = SelectedLabel.Stroke;
                    break;
                case "Both":
                    SelectedLabel.Stroke = SelectedLabel.Fill;
                    r.Fill = SelectedLabel.Fill;
                    r.TmpFill = SelectedLabel.Fill;
                    r.Stroke = SelectedLabel.Stroke;
                    r.TmpStroke = SelectedLabel.Stroke;
                    break;
            }
        }

        //Realtime Opacity change
        internal void ChangeOpacity()
        {
            if (SelectedComboBoxItem != null && SelectedLabel != null)
            {
                {
                    foreach (var r in Rectangles)
                    {
                        if (SelectedLabel.Label == r.Label)
                        {
                            r.Opacity = SelectedLabel.Opacity;
                        }
                    }
                }
            }
            OnPropertyChanged("ChangeOpacity");
        }

        /// <summary>
        /// Sets SelectedColor according to selected Label when opening ColorPicker
        /// </summary>
        internal void SetSelectedColor()
        {
            if (SelectedComboBoxItem == "All Labels")
            {
                if (LabelColorFormat.Count() != 0)
                {
                    SelectedLabel = LabelColorFormat[0];
                    return;
                }
            }
            foreach (var r in LabelColorFormat)
            {
                if (r.Label == SelectedComboBoxItem)
                {
                    SelectedLabel = r;
                    return;
                }
            }
        }

        internal void DeleteSelectedLabel()
        {
            if (LabelColorFormat.Count() > 0)
            {
                foreach (var r in LabelColorFormat)
                {
                    if (r.Label == SelectedLabel.Label)
                    {
                        if(!LabelColorFormat.Any(x => x.Label == "null"))
                        {
                            LabelColorFormat.Add(new CustomShapeFormat("null", "White", "LawnGreen", 0));
                        }
                        RemoveLabel();
                        ComboBoxNames();
                        FilterName();
                        LabelColorFormat.Remove(r);
                        SelectedLabel = LabelColorFormat[0];
                        RefreshLabelList();
                        break;
                    }
                }
                SetSelectedColor();
            }
        }

        /// <summary>
        /// Removes Label from all Rectangles with deleted label and resets ColorFormat
        /// </summary>
        private void RemoveLabel()
        {
            foreach(var lcf in LabelColorFormat)
            {
                if (lcf.Label == "null")
                {
                    foreach (var r in Rectangles)
                    {
                        if (r.Label == SelectedLabel.Label)
                        {
                            r.Label = lcf.Label;
                            r.Fill = lcf.Fill;
                            r.Opacity = lcf.Opacity;
                            r.Stroke = lcf.Stroke;
                        }
                    }
                }
            }
        }

        internal void AddLabelColorFormat()
        {
            if (LabelColorFormat[LabelColorFormat.Count() - 1].Label != "")
            {
                LabelColorFormat.Add(new CustomShapeFormat("", "White", "White", 0));
            }
            SelectedLabel = LabelColorFormat[LabelColorFormat.Count() - 1];
            RefreshLabelList();
        }

        private void RefreshLabelList()
        {
            LabelList.Clear();
            foreach(var lcf in LabelColorFormat)
            {
                LabelList.Add(lcf.Label);
            }
        }

        internal void CbItemLabel_DropDownClosed()
        {
            SelectedCustomShape.Label = SelectedLabelListItem;
            foreach (var lcf in LabelColorFormat)
            {
                SelectedCustomShape.Fill = lcf.Fill;
                SelectedCustomShape.TmpFill = lcf.Fill;
                SelectedCustomShape.Stroke = lcf.Stroke;
                SelectedCustomShape.TmpStroke = lcf.Stroke;
                SelectedCustomShape.Opacity = lcf.Opacity;
                SelectedCustomShape.TmpOpacity = lcf.Opacity;
            }
            
        }

        #endregion


        #region Import/Export Rectangles <=> XML

        private ICommand _loadXMLCommand;
        public ICommand LoadXMLCommand
        {
            get
            {
                return _loadXMLCommand ?? (_loadXMLCommand = new CommandHandler(() => LoadXML(), _canExecute));
            }
        }

        private ICommand _exportPascalVoc;
        public ICommand ExportPascalVoc
        {
            get
            {
                return _exportPascalVoc ?? (_exportPascalVoc = new CommandHandler(() => ExportToPascal(), _canExecute));
            }
        }

        /// <summary>
        /// export rectangles to xml file
        /// </summary>
        private void ExportToPascal()
        {
            string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";
            XMLWriter.WritePascalVocToXML(Rectangles.ToList(), destFileName, 1337, 1337, 3);
        }


        /// <summary>
        /// this method loads all rectangles from an xml file and draws them on the canvas
        /// </summary>
        public void LoadRectangles()
        {
            string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";

            if (File.Exists(destFileName) == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(destFileName);

                string stroke = "LawnGreen";
                string fill = "Transparent";
                double opacity = 1;

                foreach (XmlNode node in doc.DocumentElement)
                {
                    if (node.Name == "object")
                    {
                        name = node["name"].InnerText;

                        foreach (XmlNode objectChild in node)
                        {
                            if (objectChild.Name == "Format")
                            {
                                
                                stroke = objectChild["stroke"].InnerText;
                                fill = objectChild["fill"].InnerText;
                                opacity = double.Parse(objectChild["opacity"].InnerText);

                                //RectangleText = name;
                            }

                            if (objectChild.Name == "bndbox")
                            {
                                int xmin = int.Parse(objectChild["xmin"].InnerText);
                                int ymin = int.Parse(objectChild["ymin"].InnerText);
                                int xmax = int.Parse(objectChild["xmax"].InnerText);
                                int ymax = int.Parse(objectChild["ymax"].InnerText);
                                CustomShape loadedRect = new CustomShape(xmin, ymin, xmax - xmin, ymax - ymin, id);
                                id++;
                                loadedRect.Label = name;
                                loadedRect.Stroke = stroke;
                                loadedRect.TmpStroke = stroke;
                                loadedRect.Fill = fill;
                                loadedRect.TmpFill = fill;
                                loadedRect.Opacity = opacity;
                                loadedRect.TmpOpacity = opacity;

                                Rectangles.Add(loadedRect);
                                RectanglesView.Add(loadedRect);

                                bool addNewFormat = true;
                                foreach (var r in LabelColorFormat)
                                {
                                    if (r.Label == name)
                                    {
                                        addNewFormat = false;
                                        break;
                                    }
                                }
                                if (addNewFormat == true)
                                {
                                    LabelColorFormat.Add(new CustomShapeFormat(name, fill, stroke, opacity));
                                }

                                //RectanglesView[indexRectanglesView] = Rectangles[indexRectangles];
                                SaveShapeToField(loadedRect);
                                indexRectangles++;
                                indexRectanglesView++;
                            }
                        }
                    }
                }
            }
            Console.WriteLine("NumberOfFormats: " + LabelColorFormat.Count());
        }


        /// <summary>
        /// with this method, you can open an xml file from a different location as the loaded image.
        /// </summary>
        private async void LoadXML()
        {
            IsEnabled = true;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files | *.xml";


            if (openFileDialog.ShowDialog() == true)
                dst = openFileDialog.FileName;

            if (dst != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(dst);

                string stroke = "Black";
                string fill = "Black";
                double opacity = 1;

                Rectangles.Clear();
                RectanglesView.Clear();
                ClearFields();

                foreach (XmlNode node in doc.DocumentElement)
                {

                    if (node.Name == "object")
                    {
                        foreach (XmlNode objectChild in node)
                        {

                            if (objectChild.Name == "Format")
                            {
                                name = objectChild["label"].InnerText;
                                stroke = objectChild["stroke"].InnerText;
                                fill = objectChild["fill"].InnerText;
                                opacity = double.Parse(objectChild["opacity"].InnerText);

                                //RectangleText = name;
                            }

                            if (objectChild.Name == "bndbox")
                            {
                                int xmin = int.Parse(objectChild["xmin"].InnerText);
                                int ymin = int.Parse(objectChild["ymin"].InnerText);
                                int xmax = int.Parse(objectChild["xmax"].InnerText);
                                int ymax = int.Parse(objectChild["ymax"].InnerText);

                                CustomShape loadedRect = new CustomShape(xmin, ymin, xmax - xmin, ymax - ymin, id);
                                id++;
                                loadedRect.Label = name;
                                loadedRect.Stroke = stroke;
                                loadedRect.TmpStroke = stroke;
                                loadedRect.Fill = fill;
                                loadedRect.TmpFill = fill;
                                loadedRect.Opacity = opacity;
                                loadedRect.TmpOpacity = opacity;

                                Rectangles.Add(loadedRect);
                                RectanglesView.Add(loadedRect);
                                SaveShapeToField(loadedRect);
                                indexRectangles++;
                                indexRectanglesView++;
                                OnPropertyChanged("");
                            }
                        }
                    }
                }
            }
            RefreshLabelList();
            ComboBoxNames();
            SortList();
            await cropImageLabelBegin();
        }

        #endregion


        #region LoadImage

        /// <summary>
        /// opens filedialog and let us browse any images which ends with .jpg, .jped, .png and .tiff
        /// </summary>

        private ICommand _loadImageCommand;
        public ICommand LoadImageCommand
        {
            get
            {
                return _loadImageCommand ?? (_loadImageCommand = new CommandHandler(() => LoadImage(), _canExecute));
            }
        }


        private int meshColumnNumber;
        private int meshRowNumber;
        private double imgWidth;
        private double imgHeight;
        private ObservableCollection<CustomShape>[] fields;

        /// <summary>
        /// Clears all fields of the ObservableCollection Array
        /// </summary>
        private void ClearFields()
        {
            if (fields != null)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i].Clear();
                }
            }
        }

        private void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files | *.jpg; *.jpeg; *.png; *.tif";

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePath = openFileDialog.FileName;
            }



            if (ImagePath != null)
            {
                this.IsEnabled = true;
                Rectangles.Clear();
                RectanglesView.Clear();
                LabelColorFormat.Clear();
                indexRectangles = Rectangles.Count();
                indexRectanglesView = RectanglesView.Count();
                id = 0;
                ClearUndoRedoStack();

                System.Drawing.Image img = System.Drawing.Image.FromFile(ImagePath);
                imgWidth = img.Width;
                imgHeight = img.Height;
                meshColumnNumber = Convert.ToInt32(Math.Ceiling(imgWidth / fieldWidth));
                meshRowNumber = Convert.ToInt32(Math.Ceiling(imgHeight / fieldHeight));
                fields = new ObservableCollection<CustomShape>[meshColumnNumber * meshRowNumber];
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = new ObservableCollection<CustomShape>();
                }
                LoadRectangles();
                RectangleCount = "#" + RectanglesView.Count.ToString();


                RefreshLabelList();
                ComboBoxNames();
                SortList();
                FilterName();



                //Only for debugging
                //for (int i = 0; i < 10; i++)
                //{
                //    Rectangles.Add(new CustomShape(i * fieldWidth + 100, 50, 100, 100, 0));
                //    RectanglesView.Add(new CustomShape(i * fieldWidth + 100, 50, 100, 100, 0));
                //    indexRectangles++;
                //    indexRectanglesView++;

                //    fields[i].Add(new CustomShape(i * fieldWidth + 100, 50, 00, 100, 0));

                //    Console.WriteLine(fields[i].Count());
                //}

                //fields[0].Add(new CustomShape(100, 50, 100, 100, 0));
                //fields[3].Add(new CustomShape(700, 50, 100, 100, 1));

                //Console.WriteLine(meshColumnNumber);
                //Console.WriteLine(meshRowNumber);
                //
            }
        }



        #endregion


        #region ButtonCommands

        #region ESC-Button => deselect Rectangle

        private ICommand _deleteSelectionRectangle;
        public ICommand DeleteSelectionRectangle
        {
            get
            {
                return _deleteSelectionRectangle ?? (_deleteSelectionRectangle = new CommandHandler(() => DeleteSelection(), _canExecute));
            }
        }

        /// <summary>
        /// this method unselect the selected rectangle
        /// </summary>
        public void DeleteSelection()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.Fill = selectedCustomShape.TmpFill;
                selectedCustomShape.Stroke = selectedCustomShape.TmpStroke;
                selectedCustomShape.Opacity = selectedCustomShape.TmpOpacity;
                selectedCustomShape = null;
            }

            detectedCustomShape = null;
            SelectedIndex = -1;
            Enabled = true;
            mouseHandlingState = MouseState.Normal;
            IconPath = "\\Icons\\new.png";
        }

        public void DeleteSelectionForRename()
        {
            tmpLabel = SelectedCustomShape.Label;
        }
        #endregion

        #region Return-Button => refresh FilterList

        private ICommand _enterCommand;
        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand ?? (_enterCommand = new CommandHandler(() => Enter(), _canExecute));
            }
        }

        private string tmpLabel;
        public void Enter()
        {
            if (ColorPickerEnabled == true)
            {
                OnRename();
            }
            ComboBoxNames();
            FilterName();
            if (selectedCustomShape != null)
            {
                CheckFormat(selectedCustomShape);
            }
            SortList();
            Keyboard.ClearFocus();
        }


        /// <summary>
        /// this method rename all files in the listox
        /// </summary>
        private void OnRename()
        {
            foreach (var r in Rectangles)
            {
                if (r.Label == TmpNewLabel.Label)
                {
                    r.Label = SelectedLabel.Label;
                }
            }
        }

        #endregion

        #region KeyArrowCommands

        private ICommand _rightButtonCommand_Move;
        public ICommand RightButtonCommand_Move
        {
            get
            {
                return _rightButtonCommand_Move ?? (_rightButtonCommand_Move = new CommandHandler(() => RightButton_Move(), _canExecute));
            }
        }

        public void RightButton_Move()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.X1 += 2;
                selectedCustomShape.XLeft += 2;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
            }
        }

        private ICommand _leftButtonCommand_Move;
        public ICommand LeftButtonCommand_Move
        {
            get
            {
                return _leftButtonCommand_Move ?? (_leftButtonCommand_Move = new CommandHandler(() => LeftButton_Move(), _canExecute));
            }
        }

        public void LeftButton_Move()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.X1 -= 2;
                selectedCustomShape.XLeft -= 2;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
            }
        }

        private ICommand _upButtonCommand_Move;
        public ICommand UpButtonCommand_Move
        {
            get
            {
                return _upButtonCommand_Move ?? (_upButtonCommand_Move = new CommandHandler(() => UpButton_Move(), _canExecute));
            }
        }

        public void UpButton_Move()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.Y2 -= 2;
                selectedCustomShape.YTop -= 2;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
            }
        }

        private ICommand _downButtonCommand_Move;
        public ICommand DownButtonCommand_Move
        {
            get
            {
                return _downButtonCommand_Move ?? (_downButtonCommand_Move = new CommandHandler(() => DownButton_Move(), _canExecute));
            }
        }

        public void DownButton_Move()
        {
            if (selectedCustomShape != null)
            {
                selectedCustomShape.Y2 += 2;
                selectedCustomShape.YTop += 2;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
            }
        }


        private ICommand _rightButtonCommand;
        public ICommand RightButtonCommand
        {
            get
            {
                return _rightButtonCommand ?? (_rightButtonCommand = new CommandHandler(() => RightButton(), _canExecute));
            }
        }

        public void RightButton()
        {
            if (selectedCustomShape != null && selectedCustomShape.Width > minShapeSize)
            {
                selectedCustomShape.X1 += 2;
                selectedCustomShape.XLeft += 2;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Width -= 2;
            }
        }

        private ICommand _leftButtonCommand;
        public ICommand LeftButtonCommand
        {
            get
            {
                return _leftButtonCommand ?? (_leftButtonCommand = new CommandHandler(() => LeftButton(), _canExecute));
            }
        }

        public void LeftButton()
        {
            if (selectedCustomShape != null && selectedCustomShape.Width > minShapeSize)
            {
                selectedCustomShape.X2 -= 2;
                selectedCustomShape.Width -= 2;
            }
        }

        private ICommand _upButtonCommand;
        public ICommand UpButtonCommand
        {
            get
            {
                return _upButtonCommand ?? (_upButtonCommand = new CommandHandler(() => UpButton(), _canExecute));
            }
        }
        public void UpButton()
        {
            if (selectedCustomShape != null && selectedCustomShape.Height > minShapeSize)
            {
                selectedCustomShape.Y2 -= 2;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Height -= 2;
            }
        }

        private ICommand _downButtonCommand;
        public ICommand DownButtonCommand
        {
            get
            {
                return _downButtonCommand ?? (_downButtonCommand = new CommandHandler(() => DownButton(), _canExecute));
            }
        }

        public void DownButton()
        {
            if (selectedCustomShape != null && selectedCustomShape.Height > minShapeSize)
            {
                selectedCustomShape.Y1 += 2;
                selectedCustomShape.YTop += 2;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Height -= 2;
            }
        }

        private ICommand _rightButtonCommand1;
        public ICommand RightButtonCommand1
        {
            get
            {
                return _rightButtonCommand1 ?? (_rightButtonCommand1 = new CommandHandler(() => RightButton1(), _canExecute));
            }
        }

        public void RightButton1()
        {
            if (selectedCustomShape != null && selectedCustomShape.Width > minShapeSize)
            {
                selectedCustomShape.X1 -= 2;
                selectedCustomShape.XLeft -= 2;
                selectedCustomShape.XLeftBorder = selectedCustomShape.XLeft - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Width += 2;
            }
        }

        private ICommand _leftButtonCommand1;
        public ICommand LeftButtonCommand1
        {
            get
            {
                return _leftButtonCommand1 ?? (_leftButtonCommand1 = new CommandHandler(() => LeftButton1(), _canExecute));
            }
        }

        public void LeftButton1()
        {
            if (selectedCustomShape != null && selectedCustomShape.Width > minShapeSize)
            {
                selectedCustomShape.X2 += 2;
                selectedCustomShape.Width += 2;
            }
        }

        private ICommand _upButtonCommand1;
        public ICommand UpButtonCommand1
        {
            get
            {
                return _upButtonCommand1 ?? (_upButtonCommand1 = new CommandHandler(() => UpButton1(), _canExecute));
            }
        }
        public void UpButton1()
        {
            if (selectedCustomShape != null && selectedCustomShape.Height > minShapeSize)
            {
                selectedCustomShape.Y2 += 2;
                selectedCustomShape.Height += 2;
            }
        }

        private ICommand _downButtonCommand1;
        public ICommand DownButtonCommand1
        {
            get
            {
                return _downButtonCommand1 ?? (_downButtonCommand1 = new CommandHandler(() => DownButton1(), _canExecute));
            }
        }

        public void DownButton1()
        {
            if (selectedCustomShape != null && selectedCustomShape.Height > minShapeSize)
            {
                selectedCustomShape.Y1 -= 2;
                selectedCustomShape.YTop -= 2;
                selectedCustomShape.YTopBorder = selectedCustomShape.YTop - selectedCustomShape.StrokeThickness;
                selectedCustomShape.Height += 2;
            }
        }

        #endregion

        #region Tab => next/previous Rectangle
        private ICommand _tabNextShape;
        public ICommand TabNextShape
        {
            get
            {
                return _tabNextShape ?? (_tabNextShape = new CommandHandler(() => NextShape(), _canExecute));
            }
        }

        private ICommand _tabPreviousShape;
        public ICommand TabPreviousShape
        {
            get
            {
                return _tabPreviousShape ?? (_tabPreviousShape = new CommandHandler(() => PreviousShape(), _canExecute));
            }
        }

        enum ChangeMode {next, previous }

        public void NextShape()
        {
            if (selectedCustomShape != null)
            {
                ChangeShape(ChangeMode.next);
                _duplicateVar = 0;
                SelectListItem();
                _duplicateVar = 1;
            }
        }

        private void PreviousShape()
        {
            if (selectedCustomShape != null)
            {
                ChangeShape(ChangeMode.previous);
                _duplicateVar = 0;
                SelectListItem();
                _duplicateVar = 1;
            }
        }

        private void ChangeShape(ChangeMode mode)
        {
            int modifier = 1;
            if (mode == ChangeMode.next)
            {
                modifier = 1;
            }
            else if (mode == ChangeMode.previous)
            {
                modifier = -1;
            }
            CustomShape tmpCustomShape = selectedCustomShape;
            int tmpIndex = 0;
            DeleteSelection();

            if (_cropModeChecked == true)
            {
                foreach (CustomShape r in RectanglesView)
                {
                    r.Stroke = "Transparent";
                }
                ClearFields();
            }

            foreach(CustomShape r in RectanglesView)
            {
                if (r.Id == tmpCustomShape.Id)
                {
                    tmpIndex = RectanglesView.IndexOf(r) + modifier;
                    if (tmpIndex >= RectanglesView.Count())
                    {
                        tmpIndex = 0;
                    }
                    else if (tmpIndex < 0)
                    {
                        tmpIndex = RectanglesView.Count() - 1;
                    }
                    selectedCustomShape = RectanglesView[tmpIndex];
                    break;
                }
            }

            if (_cropModeChecked == true)
            {
                SaveShapeToField(selectedCustomShape);
            }

            selectedCustomShape.Stroke = "Red";
        }
        #endregion

        #endregion


        /// <summary>
        /// Old stuff
        /// </summary>



        public MyICommand RenameCommand { get; set; }
        public bool Enabled { get; set; } = true;


        //public ObservableCollection<ResizableRectangle> AllRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();

        //public ObservableCollection<ResizableRectangle> AllRectanglesView { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<Polygon> polygonsCollection { get; set; } = new ObservableCollection<Polygon>();
        public ObservableCollection<ResizableRectangle> PixelRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<ResizableRectangle> FilteredRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<string> ComboBoxItems { get; set; } = new ObservableCollection<string>();





        public void ClearUndoRedoStack()
        {
            // Clear Undo and Redo Stack
            undoCustomShapes.Clear();
            undoInformation.Clear();
            redoCustomShapes.Clear();
            redoInformation.Clear();

            // Add one Dummy item to each Stack
            undoCustomShapes.Push(new CustomShape(0,0));
            undoInformation.Push("Dummy");
            redoCustomShapes.Push(new CustomShape(0,0));
            redoInformation.Push("Dummy");

        }

        /// <summary>
        /// declares a string that stores the file path
        /// </summary>
        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return this._imagePath;
            }
            set
            {
                this._imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        /// <summary>
        /// declares a boolean activate buttons on right,
        /// "Load XML" and "Export to XML"
        /// </summary>
        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                this._isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        private ICommand _previousPage;
        public ICommand PreviousPage
        {
            get
            {
                return _previousPage ?? (_previousPage = new CommandHandler(() => SetNextState(Command.Previous), _canExecute));
            }
        }

        /// <summary>
        /// browse to the previous page
        /// </summary>
        /// <param name="command"></param>
        public void SetNextState(Command command)
        {
            UserControl usc = this._mainModel.SetNextState(_mainGrid, command);
            this._mainGrid.Children.Clear();
            usc.DataContext = this._mainViewModel;
            this._mainGrid.Children.Add(usc);
        }



        //private ResizableRectangle _selectedResizableRectangle;
        //public ResizableRectangle SelectedResizableRectangle
        //{
        //    get
        //    {
        //        return _selectedResizableRectangle;
        //    }

        //    set
        //    {
        //        if(value != null)
        //        {
        //            _selectedResizableRectangle = value;
        //            if (value.X < 0)
        //            {
        //                _selectedResizableRectangle.X += value.X;
        //                _selectedResizableRectangle.X = 0;
        //            }
        //            if (value.Y < 0)
        //            {
        //                _selectedResizableRectangle.Y += value.Y;
        //                _selectedResizableRectangle.Y = 0;
        //            }
        //            SelectedRectangleFill();
        //            DeleteCommand.RaiseCanExecuteChanged();
        //            DuplicateCommand.RaiseCanExecuteChanged();
        //            RenameCommand.RaiseCanExecuteChanged();
        //        }

        //    }
        //}




        public string temp { get; set; }

        



        

        /// <summary>
        /// says if you can execute "Can Rename" method
        /// </summary>
        private bool CanRename()
        {
            return SelectedCustomShape != null;
        }

        private ICommand _deleteRectanglesCommand;
        public ICommand DeleteRectanglesCommand
        {
            get
            {
                return _deleteRectanglesCommand ?? (_deleteRectanglesCommand = new CommandHandler(() => DeleteAll(), _canExecute));
            }
        }

        /// <summary>
        /// this method deletes the complete list with all its rectangles
        /// </summary>
        private void DeleteAll()
        {
            Rectangles.Clear();
            FilterName();
        }

        /// <summary>
        /// string variabel, which contains the name of rectangle
        /// </summary>
        private string _rectangleText;
        public string RectangleText
        {
            get
            {
                return _rectangleText;
            }

            set
            {
                _rectangleText = value;
                OnPropertyChanged("RectangleText");
            }
        }

        /// <summary>
        /// this is the default label. if you add multiple rectangles and do not want to manually enter 
        /// the name each time, the name is stored in default label
        /// </summary>
        private string _defaultLabel = "";

        public string DefaultLabel
        {
            get
            {
                return _defaultLabel;
            }
            set
            {
                _defaultLabel = value;
                RaisePropertyChanged("DefaultLabel");
            }

        }

        /// <summary>
        /// variable, which includes the opacity of the rectangle
        /// </summary>
        private double _rectangleOpacity = 0.07;

        public double RectangleOpacity
        {
            get
            {
                return _rectangleOpacity;
            }
            set
            {
                _rectangleOpacity = value;
                OnPropertyChanged("RectangleOpacity");
            }
        }

        /// <summary>
        /// variable, which includes the thum size of the rectangle
        /// </summary>
        private int _thumbSize = 3;

        public int ThumbSize
        {
            get
            {
                return _thumbSize;
            }
            set
            {
                _thumbSize = value;
                OnPropertyChanged("ThumbSize");
            }
        }

        /// <summary>
        /// variable, which contains the color of unselected rectangles
        /// </summary>
        public System.Windows.Media.Brush _rectangleFill = System.Windows.Media.Brushes.Blue;

        public System.Windows.Media.Brush RectangleFill
        {
            get
            {
                return _rectangleFill;
            }
            set
            {
                _rectangleFill = value;
                OnPropertyChanged("RectangleFill");
            }
        }

        /// <summary>
        /// variable, which contains the color of the rectangle thumbs
        /// </summary>
        public System.Windows.Media.Brush _thumbColor = System.Windows.Media.Brushes.LawnGreen;

        public System.Windows.Media.Brush ThumbColor
        {
            get
            {
                return _thumbColor;
            }
            set
            {
                _thumbColor = value;
                OnPropertyChanged("ThumbColor");
            }
        }

        public System.Windows.Media.Brush _resizeThumbColor = System.Windows.Media.Brushes.Gray;

        public System.Windows.Media.Brush ResizeThumbColor
        {
            get
            {
                return _resizeThumbColor;
            }
            set
            {
                _resizeThumbColor = value;
                OnPropertyChanged("ResizeThumbColor");
            }
        }

        public System.Windows.Media.Brush _rectangleOverFill = System.Windows.Media.Brushes.Gray;

        public System.Windows.Media.Brush RectangleOverFill
        {
            get
            {
                return _rectangleOverFill;
            }
            set
            {
                _rectangleOverFill = value;
                OnPropertyChanged("RectangleOverFill");
            }
        }

        private int _rectangleBorderThickness = 2;
    
        public int RectangleBorderThickness
        {
            get
            {
                return
                    _rectangleBorderThickness;
            }
            set
            {
                _rectangleBorderThickness = value;
                OnPropertyChanged("RectangleBorderThickness");
            }
        }
        
        ///// <summary>
        ///// this method colors the selected rectangle and increases the opacity
        ///// </summary>
        //public void SelectedRectangleFill()
        //{
        //    if (SelectedResizableRectangle != null && AnnoToolMode == "Object")
        //    {
        //        if(CropModeChecked == false)
        //        {
        //            foreach (var rect in AllRectangles)
        //            {
        //                rect.RectangleFill = System.Windows.Media.Brushes.Blue;
        //                rect.RectangleOpacity = 0.07;
        //                rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
        //                rect.ThumbSize = 3;
        //                rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
        //                rect.Visibility = Visibility.Visible;
        //            }
        //            SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
        //            SelectedResizableRectangle.RectangleOpacity = 0.0;
        //            SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
        //            SelectedResizableRectangle.ThumbSize = 3;
        //            SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
        //            SelectedResizableRectangle.Visibility = Visibility.Visible;
        //        }

        //        if(CropModeChecked == true)
        //        {
        //            foreach (var rect in AllRectangles)
        //            {
        //                rect.RectangleFill = null;
        //                rect.RectangleOpacity = 0.0;
        //                rect.ThumbColor = System.Windows.Media.Brushes.Transparent;
        //                rect.ThumbSize = 3;
        //                rect.ResizeThumbColor = System.Windows.Media.Brushes.Transparent;
        //                rect.Visibility = Visibility.Collapsed;

        //            }

        //            SelectedResizableRectangle.Visibility = Visibility.Visible;
        //            SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
        //            SelectedResizableRectangle.RectangleOpacity = 0.0;
        //            SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
        //            SelectedResizableRectangle.ThumbSize = 3;
        //            SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
        //        }
        //    }
            
        //    if(SelectedResizableRectangle == null && CropModeChecked == false)
        //    {
        //        foreach (var rect in AllRectangles)
        //        {
        //            rect.RectangleFill = System.Windows.Media.Brushes.Blue;
        //            rect.RectangleOpacity = 0.07;
        //            rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
        //            rect.ThumbSize = 3;
        //            rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
        //            rect.Visibility = Visibility.Visible;
        //        }
        //    }
        //}

        private BitmapImage _croppedImage;
        public BitmapImage CroppedImage
        {
            get
            {
                return _croppedImage;
            }
            set
            {
                _croppedImage = value;
                OnPropertyChanged("CroppedImage");
            }
        }

        private double x;
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        private double y;

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _dst;
        public string dst
        {
            get { return _dst; }
            set { _dst = value; }
        }

        ///// <summary>
        ///// this method loads all rectangles from an xml file and draws them on the canvas
        ///// </summary>
        //public void LoadRectangles()
        //{
        //    string destFileName = ImagePath.Remove(ImagePath.LastIndexOf('.')) + ".xml";

        //    if (File.Exists(destFileName) == true)
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(destFileName);

        //        foreach (XmlNode node in doc.DocumentElement)
        //        {

        //            if (node.Name == "object")
        //            {
        //                foreach (XmlNode objectChild in node)
        //                {
        //                    if (objectChild.Name == "name")
        //                    {
        //                        name = objectChild.InnerText;
        //                        RectangleText = name;
        //                    }

        //                    if (objectChild.Name == "bndbox")
        //                    {
        //                        int xmin = int.Parse(objectChild["xmin"].InnerText);
        //                        int ymin = int.Parse(objectChild["ymin"].InnerText);
        //                        int xmax = int.Parse(objectChild["xmax"].InnerText);
        //                        int ymax = int.Parse(objectChild["ymax"].InnerText);
                                
        //                            ResizableRectangle loadedRect = new ResizableRectangle();

        //                            loadedRect.RectangleHeight = ymax - ymin;
        //                            loadedRect.RectangleWidth = xmax - xmin;
        //                            loadedRect.RectangleText = name;
        //                            loadedRect.X = xmin;
        //                            loadedRect.Y = ymin;

        //                            Canvas.SetLeft(loadedRect, xmin);
        //                            Canvas.SetTop(loadedRect, ymin);

        //                            AllRectangles.Add(loadedRect);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool _isOpen = false;

        //public bool IsOpen
        //{
        //    get
        //    {
        //        return this._isOpen;
        //    }

        //    set
        //    {
        //        this._isOpen = value;
        //        OnPropertyChanged("IsOpen");
        //    }
        //}

        ///// <summary>
        ///// sorts the list of rectangles in ascending order of their name
        ///// </summary>
        //public void SortList()
        //{
        //    ObservableCollection<ResizableRectangle> sortedRectangles = new ObservableCollection<ResizableRectangle>
        //        (AllRectangles.OrderBy(resizable => resizable.RectangleText));

        //    AllRectangles = sortedRectangles;
        //    OnPropertyChanged("AllRectangles");
        //}

        ///// <summary>
        ///// this method filters the list, depending on which label is selected in the combobox
        ///// </summary>
        //public void FilterName()
        //{
        //    if (SelectedComboBoxItem == "All Labels")
        //    {
        //        if (ListViewImage.Contains("grid"))
        //        {
        //            FilterVisibilitySelectedGallery = false;
        //            FilterVisibilityAllLabels = true;
        //            FilterVisibilityAllLabelsGallery = false;
        //            FilterVisibilitySelected = false;
        //        }

        //        else if (ListViewImage.Contains("list"))
        //        {
        //            FilterVisibilitySelectedGallery = false;
        //            FilterVisibilityAllLabels = false;
        //            FilterVisibilityAllLabelsGallery = true;
        //            FilterVisibilitySelected = false;
        //        }
                
        //        AllRectanglesView = AllRectangles;
        //        RectangleCount = "#" + AllRectangles.Count.ToString();
        //        OnPropertyChanged("AllRectangles");
        //        OnPropertyChanged("AllRectanglesView");
        //        OnPropertyChanged("FilteredRectangles");
        //        OnPropertyChanged("RectangleCount");
        //        OnPropertyChanged("FilterVisibilitySelected");
        //        OnPropertyChanged("FilterVisibilitySelectedGallery");
        //        OnPropertyChanged("FilterVisibilityAllLabels");
        //        OnPropertyChanged("FilterVisibilityAllLabelsGallery");
                
        //    }

        //    else if (SelectedComboBoxItem != "All Labels")
        //    {
        //        if (ListViewImage.Contains("grid"))
        //        {
        //            FilterVisibilitySelectedGallery = false;
        //            FilterVisibilityAllLabels = false;
        //            FilterVisibilityAllLabelsGallery = false;
        //            FilterVisibilitySelected = true;
        //        }

        //        else if (ListViewImage.Contains("list"))
        //        {
        //            FilterVisibilitySelectedGallery = true;
        //            FilterVisibilityAllLabels = false;
        //            FilterVisibilityAllLabelsGallery = false;
        //            FilterVisibilitySelected = false;
        //        }

        //        DefaultLabel = SelectedComboBoxItem;
        //        RectangleCount = "#" + AllRectanglesView.Count.ToString();

        //        ObservableCollection<ResizableRectangle> FilteredRectangles = new ObservableCollection<ResizableRectangle>
        //            (AllRectangles.Where(AllRectangles => AllRectangles.RectangleText == SelectedComboBoxItem));
        //        AllRectanglesView = FilteredRectangles;
        //        OnPropertyChanged("AllRectangles");
        //        OnPropertyChanged("AllRectanglesView");
        //        OnPropertyChanged("FilteredRectangles");
        //        OnPropertyChanged("DefaultLabel");
        //        OnPropertyChanged("RectangleCount");
        //        OnPropertyChanged("FilterVisibilitySelected");
        //        OnPropertyChanged("FilterVisibilitySelectedGallery");
        //        OnPropertyChanged("FilterVisibilityAllLabels");
        //        OnPropertyChanged("FilterVisibilityAllLabelsGallery");
        //    }
        //}

        ///// <summary>
        ///// this method adds all different labels to the combobox
        ///// </summary>
        //public void ComboBoxNames()
        //{
        //    temp = SelectedComboBoxItem;
        //    ComboBoxItems.Clear();
        //    ComboBoxItems.Add("All Labels");
        //    SelectedComboBoxItem = temp;

        //    foreach (var rec in AllRectangles)
        //    {
        //        if (!ComboBoxItems.Contains(rec.RectangleText))
        //        {
        //            ComboBoxItems.Add(rec.RectangleText);
        //            OnPropertyChanged("ComboBoxItems");
        //        }
        //    }
        //}

        //private string _selectedComboBoxItem;

        //public string SelectedComboBoxItem
        //{
        //    get
        //    {
        //        return _selectedComboBoxItem;
        //    }
        //    set
        //    {
        //        _selectedComboBoxItem = value;
        //        OnPropertyChanged("SelectedComboBoxItem");
        //    }
        //}

        //private bool _filterVisibilitySelected = false;

        //public bool FilterVisibilitySelected
        //{
        //    get
        //    {
        //        return _filterVisibilitySelected;
        //    }
        //    set
        //    {
        //        _filterVisibilitySelected = value;
        //        OnPropertyChanged("FilterVisibility1");
        //    }
        //}

        //private bool _filterVisibilityAllLabels = true;

        //public bool FilterVisibilityAllLabels
        //{
        //    get
        //    {
        //        return _filterVisibilityAllLabels;
        //    }
        //    set
        //    {
        //        _filterVisibilityAllLabels = value;
        //        OnPropertyChanged("FilterVisibility");
        //    }
        //}

        //private bool _filterVisibilitySelectedGallery = false;

        //public bool FilterVisibilitySelectedGallery
        //{
        //    get
        //    {
        //        return _filterVisibilitySelectedGallery;
        //    }
        //    set
        //    {
        //        _filterVisibilitySelectedGallery = value;
        //        OnPropertyChanged("FilterVisibilitySelectedGallery");
        //    }
        //}

        //private bool _filterVisibilityAllLabelsGallery = false;

        //public bool FilterVisibilityAllLabelsGallery
        //{
        //    get
        //    {
        //        return _filterVisibilityAllLabelsGallery;
        //    }
        //    set
        //    {
        //        _filterVisibilityAllLabelsGallery = value;
        //        OnPropertyChanged("FilterVisibilityAllLabelsGallery");
        //    }
        //}
        
        //private ICommand _loadXMLCommand;
        //public ICommand LoadXMLCommand
        //{
        //    get
        //    {
        //        return _loadXMLCommand ?? (_loadXMLCommand = new CommandHandler(() => LoadXML(), _canExecute));
        //    }
        //}

        ///// <summary>
        ///// with this method, you can open an xml file from a different location as the loaded image.
        ///// </summary>
        //private async void LoadXML()
        //{
        //        this.IsEnabled = true;
        //        OpenFileDialog openFileDialog = new OpenFileDialog();
        //        openFileDialog.Filter = "XML Files | *.xml";

        //        if (openFileDialog.ShowDialog() == true)
        //            dst = openFileDialog.FileName;

        //        if (dst != null)
        //        {
        //            XmlDocument doc = new XmlDocument();
        //            doc.Load(dst);

        //            foreach (XmlNode node in doc.DocumentElement)
        //            {

        //                if (node.Name == "object")
        //                {
        //                    foreach (XmlNode objectChild in node)
        //                    {
        //                        if (objectChild.Name == "name")
        //                        {
        //                            name = objectChild.InnerText;
        //                            RectangleText = name;
        //                        }

        //                        if (objectChild.Name == "bndbox")
        //                        {
        //                            int xmin = int.Parse(objectChild["xmin"].InnerText);
        //                            int ymin = int.Parse(objectChild["ymin"].InnerText);
        //                            int xmax = int.Parse(objectChild["xmax"].InnerText);
        //                            int ymax = int.Parse(objectChild["ymax"].InnerText);

        //                            ResizableRectangle loadedRect = new ResizableRectangle();

        //                            loadedRect.RectangleHeight = ymax - ymin;
        //                            loadedRect.RectangleWidth = xmax - xmin;
        //                            loadedRect.RectangleText = name;
        //                            loadedRect.X = xmin;
        //                            loadedRect.Y = ymin;

        //                            Canvas.SetLeft(loadedRect, xmin);
        //                            Canvas.SetTop(loadedRect, ymin);

        //                            AllRectangles.Add(loadedRect);
        //                            AllRectanglesView = AllRectangles;
        //                            OnPropertyChanged("");
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        ComboBoxNames();
        //        SortList();
        //        await cropImageLabelBegin();
          
            
        //}

        private string _rectangleCount;
        public string RectangleCount
        {
            get
            {
                return _rectangleCount;
            }
            set
            {
                _rectangleCount = value;
                OnPropertyChanged("RectangleCount");
            }
        }

        private Canvas _myCanvas;

        public Canvas MyCanvas
        {
            get { return _myCanvas; }
            set { _myCanvas = value; }
        }

        private InkCanvas _myInkCanvas;

        public InkCanvas MyInkCanvas
        {
            get { return _myInkCanvas; }
            set { _myInkCanvas = value; }
        }


        private System.Windows.Controls.Image _myPreview;

        public System.Windows.Controls.Image MyPreview
        {
            get { return _myPreview; }
            set { _myPreview = value; }
        }

        private double _zoomBorderWidth;

        public double ZoomBorderWidth
        {
            get { return _zoomBorderWidth; }
            set { _zoomBorderWidth = value; }
        }

        private double _zoomBorderHeight;

        public double ZoomBorderHeight
        {
            get { return _zoomBorderHeight; }
            set { _zoomBorderHeight = value; }
        }

        //public ResizableRectangle validateResizableRect(ResizableRectangle resizable)
        //{
        //    if (resizable.X < 0)
        //    {
        //        resizable.RectangleWidth += resizable.X;
        //        resizable.X = 0;
        //    }
        //    if (resizable.Y < 0)
        //    {
        //        resizable.RectangleHeight += resizable.Y;
        //        resizable.Y = 0;
        //    }
        //    if (resizable.X + resizable.RectangleWidth > MyCanvas.ActualWidth) resizable.RectangleWidth = MyCanvas.ActualWidth - resizable.X;
        //    if (resizable.Y + resizable.RectangleHeight > MyCanvas.ActualHeight) resizable.RectangleHeight = MyCanvas.ActualHeight - resizable.Y;
        //    return resizable;
        //}


        ///// <summary>
        ///// update only the cropped image of the selected rectangle
        ///// </summary>
        ///// <param name="resizable"></param>
        //public void UpdateCropedImage(ResizableRectangle resizable)
        //{
        //    resizable = validateResizableRect(resizable);
        //    if (resizable.RectangleHeight > 5 && resizable.RectangleWidth > 5)
        //    {
        //        BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
        //        Bitmap src;
        //        using (MemoryStream outStream = new MemoryStream())
        //        {
        //            BitmapEncoder enc = new BmpBitmapEncoder();
        //            enc.Frames.Add(BitmapFrame.Create(bImage));
        //            enc.Save(outStream);
        //            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

        //            src = new Bitmap(bitmap);
        //        }


        //        Mat mat = SupportCode.ConvertBmp2Mat(src);
        //        OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)resizable.X, (int)resizable.Y, (int)resizable.RectangleWidth, (int)resizable.RectangleHeight);

        //        Mat croppedImage = new Mat(mat, rectCrop);
        //        resizable.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
        //    }
                
        //}

        //public void SelectClickedRectangle(ResizableRectangle resizableRectangle)
        //{
        //    resizableRectangle = validateResizableRect(resizableRectangle);
        //    SelectedResizableRectangle = resizableRectangle;
        //    OnPropertyChanged("SelectedResizableRectangle");
        //}

        private BitmapImage _recI;

        public BitmapImage RECI
        {
            get { return _recI; }
            set { _recI = value;
                OnPropertyChanged("RECI");
            }
        }

        /// <summary>
        /// this method only cropes images when there is an item without a cropped image in the list
        /// </summary>
        public async Task cropImageLabelBegin()
        {
            //if (MyPreview.Source != null)
            //{
            //    BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
            //    Bitmap src;

            //    using (MemoryStream outStream = new MemoryStream())
            //    {
            //        BitmapEncoder enc = new BmpBitmapEncoder();
            //        enc.Frames.Add(BitmapFrame.Create(bImage));
            //        enc.Save(outStream);
            //        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

            //        src = new Bitmap(bitmap);
            //    }

            //    foreach (var rec in AllRectanglesView)
            //    {

            //        if (rec.X > 0 && rec.X + rec.RectangleWidth < MyCanvas.ActualWidth && rec.Y > 0 && rec.Y + rec.RectangleHeight < MyCanvas.ActualHeight && rec.CroppedImage == null)
            //        {
            //            double RECX = rec.X;
            //            double RECY = rec.Y;
            //            double RECH = rec.RectangleHeight;
            //            double RECW = rec.RectangleWidth;
            //            RECI = rec.CroppedImage;

            //            await Task.Run(() =>
            //            {
                            
            //                Mat mat = SupportCode.ConvertBmp2Mat(src);
            //                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);

            //                Mat croppedImage = new Mat(mat, rectCrop);
            //                RECI = SupportCode.ConvertMat2BmpImg(croppedImage);
            //            });

            //            rec.CroppedImage = RECI;
            //            OnPropertyChanged("CroppedImage");
            //        }
            //    }
            //}
        }

        //private ICommand _updatePreviewsCommand;
        //public ICommand UpdatePreviewsCommand
        //{
        //    get
        //    {
        //        return _updatePreviewsCommand ?? (_updatePreviewsCommand = new CommandHandler(() => UpdatePreviews(), _canExecute));
        //    }
        //}

        /// <summary>
        /// this method updates all cropped images in the list
        /// </summary>
        //public async Task UpdatePreviews()
        //{
            //BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
            //Bitmap src;

            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(bImage));
            //    enc.Save(outStream);
            //    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

            //    src = new Bitmap(bitmap);
            //}

            //foreach (var rec in AllRectanglesView)
            //{
            //    if (rec.X > 0 && rec.X + rec.RectangleWidth < MyCanvas.ActualWidth && rec.Y > 0 && rec.Y + rec.RectangleHeight < MyCanvas.ActualHeight && rec.CroppedImage == null)
            //    {
            //        rec.CroppedImage = null;
            //        double RECX = rec.X;
            //        double RECY = rec.Y;
            //        double RECH = rec.RectangleHeight;
            //        double RECW = rec.RectangleWidth;
            //        RECI = rec.CroppedImage;

            //        await Task.Run(() =>
            //        {

            //            Mat mat = SupportCode.ConvertBmp2Mat(src);
            //            OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);

            //            Mat croppedImage = new Mat(mat, rectCrop);
            //            RECI = SupportCode.ConvertMat2BmpImg(croppedImage);
            //        });

            //        rec.CroppedImage = RECI;
            //        OnPropertyChanged("CroppedImage");
            //    }
            //}
        //}



        private ICommand _listViewCommand;
        public ICommand ListViewCommand
        {
            get
            {
                return _listViewCommand ?? (_listViewCommand = new CommandHandler(() => ListView(), _canExecute));
            }
        }

        private string _listViewImage = @"/Icons/grid_view.png";

        public string ListViewImage
        {
            get
            {
                return _listViewImage;
            }
            set
            {
                _listViewImage = value;
                OnPropertyChanged("ListViewImage");
            }
        }

        private void ListView()
        {
            if (ListViewImage.Contains("grid"))
            {
                ListViewImage = @"/Icons/list_view.png";
                OnPropertyChanged("ListViewTextVisibility");

                if (SelectedComboBoxItem == "All Labels")
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = true;
                    FilterVisibilitySelected = false;

                }

                else
                {
                    FilterVisibilitySelectedGallery = true;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }
            }

            else if (ListViewImage.Contains("list"))
            {
                ListViewImage = @"/Icons/grid_view.png";
                OnPropertyChanged("ListViewTextVisibility");

                if (SelectedComboBoxItem == "All Labels")
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = true;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                else
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = true;
                }
            }

            OnPropertyChanged("FilterVisibilitySelected");
            OnPropertyChanged("FilterVisibilitySelectedGallery");
            OnPropertyChanged("FilterVisibilityAllLabels");
            OnPropertyChanged("FilterVisibilityAllLabelsGallery");

        }


        private bool _undoEnabled = false;

        public bool UndoEnabled
        {
            get
            {
                return _undoEnabled;
            }

            set
            {
                _undoEnabled = value;
                OnPropertyChanged("UndoEnabled");
            }
        }

        private string _annoToolMode = "Object";

        public string AnnoToolMode
        {
            get
            {
                return _annoToolMode;
            }
            set
            {
                _annoToolMode = value;
                OnPropertyChanged("AnnoToolMode");
            }
        }

        private ICommand _pixelDrawRectangleCommand;
        public ICommand PixelDrawRectangleCommand
        {
            get
            {
                return _pixelDrawRectangleCommand ?? (_pixelDrawRectangleCommand = new CommandHandler(() => PixelDrawRectangle(), _canExecute));
            }
        }

        public void PixelDrawRectangle()
        {
            if(Enabled == true) 
            {
                Enabled = false;
                drawEnabled = false;
                MyCanvas.Cursor = Cursors.Cross; ;
            }

            else
            {
                Enabled = true;
                drawEnabled = false;
                MyCanvas.Cursor = Cursors.Arrow;
            }

        }

        private ICommand _pixelDrawCommand;
        public ICommand PixelDrawCommand
        {
            get
            {
                return _pixelDrawCommand ?? (_pixelDrawCommand = new CommandHandler(() => PixelDraw(), _canExecute));
            }
        }

        public void PixelDraw()
        {
            if (drawEnabled == false)
            {
                Enabled = true;
                DrawEnabled = true;
            }

            else
            {
                Enabled = true;
                DrawEnabled = false;
            }
        }

        private bool drawEnabled = false;

        public bool DrawEnabled
        {
            get
            {
                return drawEnabled;
            }
            set
            {
                drawEnabled = value;
                OnPropertyChanged("DrawEnabled");
            }
        }
        
        private ICommand _strokesCommand;
        public ICommand StrokesCommand
        {
            get
            {
                return _strokesCommand ?? (_strokesCommand = new CommandHandler(() => Strokes(), _canExecute));
            }
        }

        private bool bool_strokes = true;

        public void Strokes()
        {
            
            if(bool_strokes == true)
            {
                using (FileStream fs = new FileStream("inkstrokes.isf", FileMode.Create))
                {
                    MyInkCanvas.Strokes.Save(fs);
                    fs.Close();
                    MyInkCanvas.Strokes.Clear();
                }
                bool_strokes = false;
            }
            else
            {
                FileStream fs = new FileStream("inkstrokes.isf", FileMode.Open, FileAccess.Read);
                StrokeCollection strokes = new StrokeCollection(fs);
                MyInkCanvas.Strokes = strokes;
                fs.Close();
                bool_strokes = true;
            }
        }
        
        public PointCollection pointsFull { get; set; } = new PointCollection();
        public Polygon pFull = new Polygon();
        public Mat drawMask { get; set; } = new Mat();
        
        private ICommand _grabCutCommand;
        public ICommand GrabCutCommand
        {
            get
            {
                return _grabCutCommand ?? (_grabCutCommand = new CommandHandler(() => GrabCutOrMask(), _canExecute));
            }
        }

        private int _rectOrMask = 0;

        public int RectOrMask
        {
            get
            {
                return _rectOrMask;
            }
            set
            {
                _rectOrMask = value;
                OnPropertyChanged("RectOrMask");
            }
        }


        public void GrabCutOrMask()
        {
            if(_rectOrMask == 0)
            {
                GrabCut();
            }

            else
            {
                GrabCutMask();
            }
        }

        public Mat image { get; set; } = new Mat();
        
        //if 0, no crop. if 1, crop.
        public int cropOrNot { get; set; } = 0;

        public void GrabCut()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
            Bitmap src;

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new JpegBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                src = new Bitmap(bitmap);
            }

            var rec = PixelRectangles[PixelRectangles.Count - 1];

            double RECX = rec.X;
            double RECY = rec.Y;
            double RECH = rec.RectangleHeight;
            double RECW = rec.RectangleWidth;

            Mat mat = SupportCode.ConvertBmp2Mat(src);
            OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)RECX, (int)RECY, (int)RECW, (int)RECH);
            Mat croppedImage = new Mat(mat, rectCrop);
            image = croppedImage;
            cropOrNot = 1;

            var rectSelectArea = PixelRectangles[PixelRectangles.Count - 1];
            //var lastRectangle = new ResizableRectangle();
            //if (PixelRectangles.Count >=2)
            //    lastRectangle = PixelRectangles[PixelRectangles.Count - 2];

            int x1 = 0;
            int x2 = (int)rectSelectArea.RectangleWidth;
            int y1 = 0;
            int y2 = (int)rectSelectArea.RectangleHeight;
            
            var rect = new OpenCvSharp.Rect(x1+2,y1+2,x2-5,y2-5);
            var bgdModel = new Mat();
            var fgdModel = new Mat();

            if(cropOrNot == 0)
            {
                BitmapImage bImage1 = new BitmapImage(new Uri(MyPreview.Source.ToString()));
                Bitmap src1;

                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bImage1));
                    enc.Save(outStream);
                    Bitmap bitmap = new Bitmap(outStream);

                    src1 = new Bitmap(bitmap);
                }


                image = SupportCode.ConvertBmp2Mat(src1);
            }

            Mat mask = new Mat();

            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);
            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2BGR);
            
            Cv2.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, GrabCutModes.InitWithRect);
            Cv2.Threshold(mask, mask, 2, 255, ThresholdTypes.Binary & ThresholdTypes.Otsu);
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            pointsFull.Clear();

            for (int l = 0; l < contours.Length; l++)
            {
                int m = 0;

                if (contours[l].Count() > m)
                    m = l;

                if (l == contours.Length - 1)
                {
                    for (int k = 0; k < contours[m].Length; k++)
                    {
                        pointsFull.Add(new System.Windows.Point(contours[m][k].X + rectSelectArea.X, contours[m][k].Y + rectSelectArea.Y));
                    }
                }
            }

            polygonsCollection.Add(new Polygon());

            foreach (var q in PixelRectangles)
            {
                q.RectangleMovable = false;
                q.Visibility = Visibility.Collapsed;
            }


            _rectOrMask++;
            OnPropertyChanged("polygonsCollection");
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs + " ms");

        }

        public async void GrabCutMask()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var rectSelectArea = PixelRectangles[PixelRectangles.Count - 1];

            int x1 = (int)rectSelectArea.X;
            int x2 = (int)rectSelectArea.RectangleWidth;
            int y1 = (int)rectSelectArea.Y;
            int y2 = (int)rectSelectArea.RectangleHeight;

            System.Windows.Rect rect1 = new System.Windows.Rect();
            rect1.X = x1;
            rect1.Y = y1;
            rect1.Width = x2;
            rect1.Height = y2;

            await CreateSaveBitmap(MyInkCanvas, rect1);
            //MyInkCanvas.Children.Remove(pFull);

            var rect = new OpenCvSharp.Rect(x1 + 2, y1 + 2, x2 - 5, y2 - 5);
            var bgdModel = new Mat();
            var fgdModel = new Mat();

            Mat mask = drawMask;
            Cv2.CvtColor(image, image, ColorConversionCodes.BGR2RGB);
            Cv2.CvtColor(image, image, ColorConversionCodes.RGB2BGR);


            Cv2.GrabCut(image, mask, rect, bgdModel, fgdModel, 1, GrabCutModes.InitWithMask);
            Mat mask1 = new Mat();
            Mat mask2 = new Mat();

            Cv2.Threshold(mask, mask1, 2, 255, ThresholdTypes.Binary & ThresholdTypes.Otsu);
            Cv2.InRange(mask, 1, 1, mask2);
            mask = mask1 + mask2;
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
            
            pointsFull.Clear();

            for (int l = 0; l < contours.Length; l++)
            {
                int m = 0;

                if (contours[l].Count() > m)
                    m = l;

                if (l == contours.Length - 1)
                {
                    for (int k = 0; k < contours[m].Length; k++)
                    {
                        pointsFull.Add(new System.Windows.Point(contours[m][k].X + rectSelectArea.X, contours[m][k].Y + rectSelectArea.Y));
                    }
                }
            }

            polygonsCollection.RemoveAt(polygonsCollection.Count - 1);
            polygonsCollection.Add(new Polygon());

            foreach (var q in PixelRectangles)
            {
                q.RectangleMovable = false;
                q.Visibility = Visibility.Collapsed;
            }

            OnPropertyChanged("polygonsCollection");
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine(elapsedMs + " ms");

        }
        
        private async Task CreateSaveBitmap(InkCanvas canvas, System.Windows.Rect rectangle)
        {
            
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height));

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));
            
            MemoryStream stream = new MemoryStream();
            pngEncoder.Save(stream);

            Bitmap img = new Bitmap(stream);
            int width = (int)MyPreview.ActualWidth;
            int height = (int)MyPreview.ActualHeight;
            Bitmap newBitmap = new Bitmap(100, 100);
            System.Drawing.Color actualColor;
            System.Drawing.Color white = System.Drawing.Color.White;
            System.Drawing.Color black = System.Drawing.Color.Black;
            var sure_bg = System.Drawing.Color.FromArgb(0, 0, 0);
            var sure_fg = System.Drawing.Color.FromArgb(1, 1, 1);
            var mask_rect = System.Drawing.Color.FromArgb(2, 2, 2);
            var mask_mask = System.Drawing.Color.FromArgb(3, 3, 3);

            var mat = new Mat(img.Height, img.Width, MatType.CV_8U, Scalar.White);
            var indexer = mat.GetGenericIndexer<Vec3b>();

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Vec3b color = mat.Get<Vec3b>(i, j);
                    color.Item0 = 0;
                    color.Item1 = 0;
                    color.Item2 = 0;
                    indexer[i, j] = color;
                }
            }

            var rectSelectArea = PixelRectangles[PixelRectangles.Count - 1];
            int x2 = (int)rectSelectArea.RectangleWidth;
            int y2 = (int)rectSelectArea.RectangleHeight;

            await Task.Run(() =>
            {
                for (int i = 2; i < y2 - 2; i++)
                {
                    for (int j = 2; j < x2 - 2; j++)
                    {
                        actualColor = img.GetPixel(j, i);
                        if (actualColor.A == 0)
                        {
                            Vec3b color = mat.Get<Vec3b>(i, j);
                            color.Item0 = 2;
                            color.Item1 = 2;
                            color.Item2 = 2;
                            indexer[i, j] = color;
                        }

                        else if (actualColor.R == 255 && actualColor.G == 0 && actualColor.B == 0)
                        {
                            Vec3b color = mat.Get<Vec3b>(i, j);
                            color.Item0 = 0;
                            color.Item1 = 0;
                            color.Item2 = 0;
                            indexer[i, j] = color;
                        }

                        else if (actualColor.R == 124 && actualColor.G == 252 && actualColor.B == 0)
                        {
                            Vec3b color = mat.Get<Vec3b>(i, j);
                            color.Item0 = 1;
                            color.Item1 = 1;
                            color.Item2 = 1;
                            indexer[i, j] = color;
                        }

                        else
                        {
                            Vec3b color = mat.Get<Vec3b>(i, j);
                            color.Item0 = 3;
                            color.Item1 = 3;
                            color.Item2 = 3;
                            indexer[i, j] = color;
                        }

                    }
                }
            drawMask = mat;
            });
        }

        private ICommand _resetMaskCommand;
        public ICommand ResetMaskCommand
        {
            get
            {
                return _resetMaskCommand ?? (_resetMaskCommand = new CommandHandler(() => ResetMask(), _canExecute));
            }
        }

        public void ResetMask()
        {
            pointsFull.Clear();
            drawMask = new Mat();
            PixelRectangles.Clear();
            //MyInkCanvas.Children.Remove(pFull);
            MyInkCanvas.Strokes.Clear();

            using (FileStream fs = new FileStream("inkstrokes.isf", FileMode.Create))
            {
                MyInkCanvas.Strokes.Save(fs);
                fs.Close();
                MyInkCanvas.Strokes.Clear();
            }
        }

        private InkCanvas _inkCanvas;

        public InkCanvas inkCanvas
        {
            get
            {
                return _inkCanvas;
            }
            set
            {
                _inkCanvas = value;
                OnPropertyChanged("inkCanvas");
            }
        }

        private ICommand _preprocessFolderCommand;
        public ICommand PreprocessFolderCommand
        {
            get
            {
                return _preprocessFolderCommand ?? (_preprocessFolderCommand = new CommandHandler(() => PreprocessFolder(), _canExecute));
            }
        }

        private bool showMessageBox;

        public void PreprocessFolder()
        {
            showMessageBox = false;
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string folderPath = fbd.SelectedPath;
                    string croppedImageFolder = System.IO.Path.Combine(folderPath, "Cropped Images");
                    
                    Directory.CreateDirectory(croppedImageFolder);
                    
                    string[] files = Directory.GetFiles(fbd.SelectedPath);
                    int found = 0;
                    
                    foreach (var file in files)
                    {
                        if (file.Contains(".png") || file.Contains(".tif"))
                        {
                            string s = file.ToString();

                            found = s.IndexOf(folderPath);

                            string filename = s.Substring(found + folderPath.Length);
                            string destFileName = filename.Remove(filename.LastIndexOf('.'));



                            BitmapImage src = new BitmapImage();
                            src.BeginInit();
                            src.UriSource = new Uri(file, UriKind.Relative);
                            src.CacheOption = BitmapCacheOption.OnLoad;
                            src.EndInit();

                            int width = (int)src.Width / 2;
                            int height = (int)src.Height / 2;


                            for (int i = 0; i < 2; i++)
                                for (int j = 0; j < 2; j++)
                                {
                                    CroppedBitmap croppedBitmap = new CroppedBitmap(src, new Int32Rect(i * width, j * height, width, height));

                                    if (file.ToString().Contains(".tif"))
                                        TIFFEncoder(destFileName, croppedBitmap, i, j, croppedImageFolder);
                                    else if (file.ToString().Contains(".png"))
                                        PNGEncoder(destFileName, croppedBitmap, i, j, croppedImageFolder);
                                }
                        }
                    }
                }
            }
            if(showMessageBox == true)
                MessageBox.Show("Successful!", "Preprocess Folder", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        private void PNGEncoder(string file, CroppedBitmap croppedBitmap, int i, int j, string folder)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            string filename = folder +  file + j.ToString() + i.ToString() + ".png";
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
            showMessageBox = true;
        }

        private void TIFFEncoder(string file, CroppedBitmap croppedBitmap, int i, int j, string folder)
        {
            BitmapEncoder encoder = new TiffBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            string filename = folder + file + j.ToString() + i.ToString() + ".tif";
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
            showMessageBox = true;
        }
    }

}
