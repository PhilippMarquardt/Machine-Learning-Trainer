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

namespace MachineLearningTrainer.DrawerTool 
{
    public class DrawerViewModel : INotifyPropertyChanged, IComparable
    {
        #region PropertyChangedArea
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {

                handler(this, new PropertyChangedEventArgs(name));

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
            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            DuplicateCommand = new MyICommand(OnDuplicate, CanDuplicate);
            RenameCommand = new MyICommand(OnRename, CanRename);
            ComboBoxItems.Add("All Labels");
            AllRectanglesView = AllRectangles;
            SelectedComboBoxItem = "All Labels";

            RectanglesView = new ObservableCollection<CustomShape>();
            Rectangles = new ObservableCollection<CustomShape>();
            RectanglesView.CollectionChanged += ShapeCollectionChanged;
            undoCustomShapes.Push(new CustomShape(0, 0));
            undoInformation.Push("Dummy");
            redoCustomShapes.Push(new CustomShape(0, 0));
            redoInformation.Push("Dummy");



            RectanglesView.Add(new CustomShape(100, 50, 200, 100, 0));
            Rectangles.Add(new CustomShape(RectanglesView[indexRectanglesView]));
            undoCustomShapes.Push(RectanglesView[0]);
            undoInformation.Push("Add");
            id++;
            indexRectangles++;
            indexRectanglesView++;
        }

        public ObservableCollection<CustomShape> RectanglesView { get; set; }
        public ObservableCollection<CustomShape> Rectangles { get; set; }
        public Stack<CustomShape> undoCustomShapes { get; set; } = new Stack<CustomShape>();
        public Stack<string> undoInformation { get; set; } = new Stack<string>();
        public Stack<CustomShape> redoCustomShapes { get; set; } = new Stack<CustomShape>();
        public Stack<string> redoInformation { get; set; } = new Stack<string>();

        private readonly int borderWidth = 10;                   //used when detecting, moving & resizing shapes
        private readonly double minShapeSize = 20;

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


        private bool shapeSelected = false;
        private CustomShape selectedCustomShape = new CustomShape(-1, -1);
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

        private void CheckCanvas(System.Windows.Point mousePosition)
        {
            if (mousePosition.X < 0)
            {
                tmpX = 0;
                selectedCustomShape.X1 = 0;
                selectedCustomShape.XLeft = 0;
            }
            else if (mousePosition.X > MyCanvas.ActualWidth)
            {
                tmpX = MyCanvas.ActualWidth;
                selectedCustomShape.X2 = MyCanvas.ActualWidth;
            }
            if (mousePosition.Y < 0)
            {
                tmpY = 0;
                selectedCustomShape.Y1 = 0;
                selectedCustomShape.YTop = 0;
            }
            else if (mousePosition.Y > MyCanvas.ActualHeight)
            {
                tmpY = MyCanvas.ActualHeight;
                selectedCustomShape.Y2 = MyCanvas.ActualHeight;
            }
        }
        private void CheckCanvas(System.Windows.Point mousePosition, double deltaX, double deltaY)
        {
            if (mousePosition.X - deltaX - selectedCustomShape.Width / 2 < 0)
            {
                tmpX = 0;
                selectedCustomShape.X1 = 0;
                selectedCustomShape.XLeft = 0;
            }
            else if (mousePosition.X - deltaX + selectedCustomShape.Width / 2 > MyCanvas.ActualWidth)
            {
                tmpX = MyCanvas.ActualWidth;
                selectedCustomShape.X2 = MyCanvas.ActualWidth;
            }
            if (mousePosition.Y - deltaY - selectedCustomShape.Height / 2 < 0)
            {
                tmpY = 0;
                selectedCustomShape.Y1 = 0;
                selectedCustomShape.YTop = 0;
            }
            else if (mousePosition.Y - deltaY + selectedCustomShape.Height / 2 > MyCanvas.ActualHeight)
            {
                tmpY = MyCanvas.ActualHeight;
                selectedCustomShape.Y2 = MyCanvas.ActualHeight;
            }
        }
        #endregion


        #region Create ShapeRectangles
        //Create Rectangles:
        //routine to create rectangles

        private int indexRectangles;
        private int indexRectanglesView;
        private int id;

        public void CreateRectangle(System.Windows.Point mousePosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                tmpX = mousePosition.X;
                tmpY = mousePosition.Y;
                CheckCanvas(mousePosition);

                if (RectanglesView.Count < indexRectanglesView + 1)
                {
                    RectanglesView.Add(new CustomShape(tmpX, tmpY, id));
                }
                else
                {
                    if (RectanglesView[indexRectanglesView].X1 == -1 && RectanglesView[indexRectanglesView].Y1 == -1)
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
                    if (RectanglesView[indexRectanglesView].Y1 > RectanglesView[indexRectanglesView].Y2)
                    {
                        RectanglesView[indexRectanglesView].YTop = RectanglesView[indexRectanglesView].Y2;
                    }
                    else
                    {
                        RectanglesView[indexRectanglesView].YTop = RectanglesView[indexRectanglesView].Y1;
                    }
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
                        RectanglesView[indexRectanglesView].X1 = -1;
                        RectanglesView[indexRectanglesView].X2 = -1;
                        RectanglesView[indexRectanglesView].Y1 = -1;
                        RectanglesView[indexRectanglesView].Y2 = -1;
                    }
                    else
                    {
                        if (RectanglesView[indexRectanglesView].X1 > RectanglesView[indexRectanglesView].X2)
                        {
                            tmp = RectanglesView[indexRectanglesView].X1;
                            RectanglesView[indexRectanglesView].X1 = RectanglesView[indexRectanglesView].X2;
                            RectanglesView[indexRectanglesView].X2 = tmp;
                        }
                        if (RectanglesView[indexRectanglesView].Y1 > RectanglesView[indexRectanglesView].Y2)
                        {
                            tmp = RectanglesView[indexRectanglesView].Y1;
                            RectanglesView[indexRectanglesView].Y1 = RectanglesView[indexRectanglesView].Y2;
                            RectanglesView[indexRectanglesView].Y2 = tmp;
                        }

                        if (SelectedComboBoxItem != "All Labels")
                        {
                            Enabled = false;
                            RectanglesView[indexRectanglesView].Label = SelectedComboBoxItem;
                        }

                        else
                        {
                            Enabled = false;
                            if (IsChecked == true && DefaultLabel.Length > 0)
                                RectanglesView[indexRectanglesView].Label = _defaultLabel;
                            else
                                RectanglesView[indexRectanglesView].Label = "";
                        }

                        Rectangles.Add(new CustomShape(RectanglesView[indexRectanglesView]));


                        undoCustomShapes.Push(RectanglesView[indexRectanglesView]);
                        undoInformation.Push("Add");

                        Console.WriteLine(RectanglesView[indexRectanglesView].Id);
                        id++;
                        indexRectangles++;
                        indexRectanglesView++;

                        Console.WriteLine("RectanglesView count: " + RectanglesView.Count);
                        Console.WriteLine("Rectangles count: " + Rectangles.Count);

                        if (SelectedComboBoxItem == "All Labels")
                        {
                            RectangleCount = "#" + Rectangles.Count.ToString();
                        }

                        else if (SelectedComboBoxItem != "All Labels")
                        {
                            RectangleCount = "#" + RectanglesView.Count.ToString();
                        }


                        OnPropertyChanged("Rectangles");
                        OnPropertyChanged("RectanglesView");
                    }
                }
            }
        }

        #endregion


        #region ShapeCollectionChangedHandler
        private void ShapeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("ShapeCollectionChanged()");
            if (e.NewItems != null)
            {
                {
                    foreach (object item in e.NewItems)
                    {
                        if (item is CustomShape)
                            ((CustomShape)item).PropertyChanged += Shape_PropertyChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        if (item is CustomShape)
                            ((CustomShape)item).PropertyChanged -= Shape_PropertyChanged;
                    }
                }
            }
        }

        private void Shape_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(RectanglesView).Refresh();
        }
        #endregion


        #region ResizeCustomShape
        //resize-Block:
        //routine to resize different shapes on canvas

        enum ResizeDirection { SizeN, SizeNE, SizeE, SizeSE, SizeS, SizeSW, SizeW, SizeNW }
        ResizeDirection resizeDirection;

        internal void Resize(System.Windows.Point mousePosition)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                if (selectedCustomShape.Resize == false)
                {
                    DetectResize(mousePosition);
                }
                else if (selectedCustomShape.Resize == true)
                {
                    DeactivateResize();
                }
            }
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (selectedCustomShape.Resize == false)
                {
                    ActivateResize(mousePosition);
                }
                else if (selectedCustomShape.Resize == true)
                {
                    ResizeCustomShape(mousePosition);
                }
            }
        }

        private void DetectResize(System.Windows.Point mousePosition)
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
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 - borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeW;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeSW;
                    Mouse.OverrideCursor = Cursors.SizeNESW;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
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
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 - borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeE;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeSE;
                    Mouse.OverrideCursor = Cursors.SizeNWSE;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
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
                    CopyForUndo("Resize");
                }
                else if (selectedCustomShape.Y2 - borderWidth < mousePosition.Y && mousePosition.Y < selectedCustomShape.Y2 + borderWidth)
                {
                    resizeDirection = ResizeDirection.SizeS;
                    Mouse.OverrideCursor = Cursors.SizeNS;
                    mouseHandlingState = MouseState.Resize;
                    selectedCustomShape.Resize = true;
                    CopyForUndo("Resize");
                }
            }
        }

        private void ResizeCustomShape(System.Windows.Point mousePosition)
        {
            tmpX = mousePosition.X;
            tmpY = mousePosition.Y;
            CheckCanvas(mousePosition);

            switch (resizeDirection)
            {
                case ResizeDirection.SizeN:
                    {
                        if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                        {
                            selectedCustomShape.Y1 = tmpY;
                            selectedCustomShape.YTop = tmpY;
                        }
                        else
                        {
                            selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                            selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeNE:
                    {
                        if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                        {
                            selectedCustomShape.Y1 = tmpY;
                            selectedCustomShape.YTop = tmpY;
                        }
                        else
                        {
                            selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                            selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                        }

                        if (minShapeSize < selectedCustomShape.Width + (tmpX - selectedCustomShape.X2))
                        {
                            selectedCustomShape.X2 = tmpX;
                        }
                        else
                        {
                            selectedCustomShape.X2 = selectedCustomShape.X1 + minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeE:
                    {
                        if (minShapeSize < selectedCustomShape.Width + (tmpX - selectedCustomShape.X2))
                        {
                            selectedCustomShape.X2 = tmpX;
                        }
                        else
                        {
                            selectedCustomShape.X2 = selectedCustomShape.X1 + minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeSE:
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
                        break;
                    }
                case ResizeDirection.SizeS:
                    {
                        if (minShapeSize < selectedCustomShape.Height + (tmpY - selectedCustomShape.Y2))
                        {
                            selectedCustomShape.Y2 = tmpY;
                        }
                        else
                        {
                            selectedCustomShape.Y2 = selectedCustomShape.Y1 + minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeSW:
                    {
                        if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 -tmpX))
                        {
                            selectedCustomShape.X1 = tmpX;
                            selectedCustomShape.XLeft = tmpX;
                        }
                        else
                        {
                            selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                            selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                        }

                        if (minShapeSize < selectedCustomShape.Height + (tmpY - selectedCustomShape.Y2))
                        {
                            selectedCustomShape.Y2 = tmpY;
                        }
                        else
                        {
                            selectedCustomShape.Y2 = selectedCustomShape.Y1 + minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeW:
                    {
                        if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 - tmpX))
                        {
                            selectedCustomShape.X1 = tmpX;
                            selectedCustomShape.XLeft = tmpX;
                        }
                        else
                        {
                            selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                            selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                        }
                        break;
                    }
                case ResizeDirection.SizeNW:
                    {
                        if (minShapeSize < selectedCustomShape.Width + (selectedCustomShape.X1 - tmpX))
                        {
                            selectedCustomShape.X1 = tmpX;
                            selectedCustomShape.XLeft = tmpX;
                        }
                        else
                        {
                            selectedCustomShape.X1 = selectedCustomShape.X2 - minShapeSize;
                            selectedCustomShape.XLeft = selectedCustomShape.X2 - minShapeSize;
                        }

                        if (minShapeSize < selectedCustomShape.Height + (selectedCustomShape.Y1 - tmpY))
                        {
                            selectedCustomShape.Y1 = tmpY;
                            selectedCustomShape.YTop = tmpY;
                        }
                        else
                        {
                            selectedCustomShape.Y1 = selectedCustomShape.Y2 - minShapeSize;
                            selectedCustomShape.YTop = selectedCustomShape.Y2 - minShapeSize;
                        }
                        break;
                    }
            }
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
                    break;
                }
                tmpIndex++;
            }

            selectedCustomShape.Resize = false;
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
            if (Mouse.LeftButton == MouseButtonState.Pressed)
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
                if (selectedCustomShape.Move == false)
                {
                    DetectMove(mousePosition);
                }
                else
                {
                    DeactivateMove(mousePosition);
                }
            }
        }

        private void DetectMove(System.Windows.Point mousePosition)
        {
            if ((detectedCustomShape.X1 + borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 - borderWidth) &&
                (detectedCustomShape.Y1 + borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 - borderWidth))
            {
                Mouse.OverrideCursor = Cursors.SizeAll;
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
                selectedCustomShape.XLeft = selectedCustomShape.X1;
                Mouse.OverrideCursor = Cursors.SizeAll;
            }
        }

        private void DeactivateMove(System.Windows.Point mousePosition)
        {
            CheckCanvas(mousePosition, deltaX, deltaY);

            int tmpIndex = 0;
            foreach (CustomShape r in Rectangles)
            {
                if (r.Id == selectedCustomShape.Id)
                {
                    Rectangles.RemoveAt(tmpIndex);
                    Rectangles.Insert(tmpIndex, selectedCustomShape);
                    break;
                }
                tmpIndex++;
            }

            selectedCustomShape.Move = false;
        }
        #endregion


        #region Detect & Select Shape
        //DetectShape:
        //routines to detect different Shapes on Canvas
        private bool shapeDetected = false;
        private CustomShape detectedCustomShape = new CustomShape(-1, -1);

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


        

        internal void DetectCustomShape(System.Windows.Point mousePosition)
        {

            if (!((detectedCustomShape.X1 - borderWidth < mousePosition.X && mousePosition.X < detectedCustomShape.X2 + borderWidth)
                && (detectedCustomShape.Y1 - borderWidth < mousePosition.Y && mousePosition.Y < detectedCustomShape.Y2 + borderWidth)) && shapeSelected == false)
            {
                detectedCustomShape.Opacity = 1;
                detectedCustomShape.Fill = "Transparent";
                detectedCustomShape.IsMouseOver = false;
                shapeDetected = false;

                foreach (CustomShape r in RectanglesView)
                {
                    if ((r.X1 < mousePosition.X && mousePosition.X < r.X2) && (r.Y1 < mousePosition.Y && mousePosition.Y < r.Y2))
                    {
                        detectedCustomShape = r;
                        break;
                    }
                }
            }
            else
            {
                detectedCustomShape.Opacity = 0.3;
                detectedCustomShape.IsMouseOver = true;
                shapeDetected = true;
                detectedCustomShape.Fill = "Gray";
            }
        }

        internal void SelectCustomShape()
        {
            selectedCustomShape.Stroke = "LawnGreen";

            selectedCustomShape = this.detectedCustomShape;
            selectedCustomShape.Stroke = "Red";
        }
        #endregion


        #region Duplicate-Routine
        /// <summary>
        /// this method, let you duplicate the selected rectangle with its text, height, ... to current mouse position
        /// </summary>
        private async void OnDuplicate()
        {
            if (DuplicateVar == 1)
            {
                double tmpHeight = selectedCustomShape.Height;
                double tmpWidth = selectedCustomShape.Width;
                double tmpX1 = vmMousePoint.X - selectedCustomShape.Width / 2;
                double tmpY1 = vmMousePoint.Y - selectedCustomShape.Height / 2;

                if (vmMousePoint.X - selectedCustomShape.Width / 2 < 0)
                {
                    tmpX1 = 0;
                }
                else if (vmMousePoint.X + selectedCustomShape.Width / 2 > MyCanvas.ActualWidth)
                {
                    tmpWidth = MyCanvas.ActualWidth - tmpX1;
                }
                if (vmMousePoint.Y - selectedCustomShape.Height / 2 < 0)
                {
                    tmpY1 = 0;
                }
                else if (vmMousePoint.Y + selectedCustomShape.Height / 2 > MyCanvas.ActualHeight)
                {
                    tmpHeight = MyCanvas.ActualHeight - tmpY1;
                }

                CustomShape duplicatedCustomShape = new CustomShape(tmpX1, tmpY1, tmpWidth, tmpHeight, id);
                duplicatedCustomShape.Label = selectedCustomShape.Label;
                RectanglesView.Add(duplicatedCustomShape);
                Rectangles.Add(duplicatedCustomShape);
                indexRectanglesView++;
                indexRectangles++;
                id++;

                undoCustomShapes.Push(duplicatedCustomShape);
                undoInformation.Push("Add");

                Console.WriteLine("RectanglesView Count: " + RectanglesView.Count());

                //await cropImageLabelBegin();

                if (SelectedComboBoxItem == "All Labels")
                {
                    RectangleCount = "#" + Rectangles.Count.ToString();
                }

                else if (SelectedComboBoxItem != "All Labels")
                {
                    RectangleCount = "#" + RectanglesView.Count.ToString();
                }
            }

            else if (DuplicateVar == 0)
            {
                double tmpHeight = selectedCustomShape.Height;
                double tmpWidth = selectedCustomShape.Width;
                double tmpX1 = selectedCustomShape.X1 + 30;
                double tmpY1 = selectedCustomShape.Y1 + 30;

                CustomShape duplicatedCustomShape = new CustomShape(tmpX1, tmpY1, tmpWidth, tmpHeight, id);
                RectanglesView.Add(duplicatedCustomShape);
                Rectangles.Add(duplicatedCustomShape);
                indexRectanglesView++;
                indexRectangles++;
                id++;

                undoCustomShapes.Push(duplicatedCustomShape);
                undoInformation.Push("Add");

                Console.WriteLine("RectanglesView Count: " + RectanglesView.Count());
            }
        }


        private int _duplicateVar;

        public int DuplicateVar
        {
            get { return _duplicateVar; }
            set
            {
                _duplicateVar = value;
                OnPropertyChanged("DuplicateVar");
            }
        }

        /// <summary>
        /// says if you can execute the "On Duplicate" method
        /// </summary>
        private bool CanDuplicate()
        {
            return selectedCustomShape != null;
        }
        #endregion


        #region Delete Routine
        /// <summary>
        /// this method deletes the selection of rectangles
        /// </summary>
        private void OnDelete()
        {
            foreach (CustomShape rv in RectanglesView)
            {
                if (rv == selectedCustomShape)
                {
                    undoCustomShapes.Push(rv);
                    undoInformation.Push("Delete");
                    RectanglesView.Remove(rv);
                    int tmpIndex = 0;
                    indexRectanglesView--;
                    indexRectangles--;
                    foreach (CustomShape r in Rectangles) 
                    {
                        if (r.Id == selectedCustomShape.Id)
                        {
                            Rectangles.RemoveAt(tmpIndex);

                            Console.WriteLine("RectanglesView count: " + RectanglesView.Count);
                            Console.WriteLine("Rectangles count: " + Rectangles.Count);

                            if (SelectedComboBoxItem == "All Labels")
                            {
                                RectangleCount = "#" + Rectangles.Count.ToString();
                            }

                            else if (SelectedComboBoxItem != "All Labels")
                            {
                                RectangleCount = "#" + RectanglesView.Count.ToString();
                            }
                            break;
                        }
                        tmpIndex++;
                    }
                    break;
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

        /// <summary>
        /// says if you can execute the "On Delete" method
        /// </summary>
        /// <returns></returns>
        private bool CanDelete()
        {
            return selectedCustomShape != null;
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


        #region ComboBox for ItemLabels

        private ICommand _enterCommand;
        public ICommand EnterCommand
        {
            get
            {
                return _enterCommand ?? (_enterCommand = new CommandHandler(() => Enter(), _canExecute));
            }
        }

        public void Enter()
        {
            FilterName();
            SortList();
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

                foreach (XmlNode node in doc.DocumentElement)
                {

                    if (node.Name == "object")
                    {
                        foreach (XmlNode objectChild in node)
                        {
                            if (objectChild.Name == "name")
                            {
                                name = objectChild.InnerText;
                                RectangleText = name;
                            }

                            if (objectChild.Name == "bndbox")
                            {
                                int xmin = int.Parse(objectChild["xmin"].InnerText);
                                int ymin = int.Parse(objectChild["ymin"].InnerText);
                                int xmax = int.Parse(objectChild["xmax"].InnerText);
                                int ymax = int.Parse(objectChild["ymax"].InnerText);

                                CustomShape loadedRect = new CustomShape(xmin,ymin,xmax-xmin,ymax-ymin,id);
                                id++;
                                loadedRect.Label = name;

                                Rectangles.Add(loadedRect);
                                RectanglesView.Add(loadedRect);
                                indexRectangles++;
                                indexRectanglesView++;
                            }
                        }
                    }
                }
            }
        }

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

        /// <summary>
        /// sorts the list of rectangles in ascending order of their name
        /// </summary>
        public void SortList()
        {
            //ObservableCollection<CustomShape> sortedRectangles = new ObservableCollection<CustomShape>
            //    (RectanglesView.OrderBy(resizable => resizable.Label));

            //RectanglesView = sortedRectangles;
            //OnPropertyChanged("RectanglesView");
        }

        /// <summary>
        /// this method filters the list, depending on which label is selected in the combobox
        /// </summary>
        public void FilterName()
        {
            if (SelectedComboBoxItem == "All Labels")
            {
                if (ListViewImage.Contains("grid"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = true;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                else if (ListViewImage.Contains("list"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = true;
                    FilterVisibilitySelected = false;
                }

                RectanglesView.Clear();
                foreach (CustomShape r in Rectangles)
                {
                    RectanglesView.Add(new CustomShape(r));
                }

                RectangleCount = "#" + Rectangles.Count.ToString();
                OnPropertyChanged("Rectangles");
                OnPropertyChanged("RectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("RectangleCount");
                OnPropertyChanged("FilterVisibilitySelected");
                OnPropertyChanged("FilterVisibilitySelectedGallery");
                OnPropertyChanged("FilterVisibilityAllLabels");
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");

            }

            else if (SelectedComboBoxItem != "All Labels")
            {
                if (ListViewImage.Contains("grid"))
                {
                    FilterVisibilitySelectedGallery = false;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = true;
                }

                else if (ListViewImage.Contains("list"))
                {
                    FilterVisibilitySelectedGallery = true;
                    FilterVisibilityAllLabels = false;
                    FilterVisibilityAllLabelsGallery = false;
                    FilterVisibilitySelected = false;
                }

                DefaultLabel = SelectedComboBoxItem;
                RectangleCount = "#" + RectanglesView.Count.ToString();

                RectanglesView.Clear();
                foreach (CustomShape r in Rectangles)
                {
                    if(r.Label == SelectedComboBoxItem)
                    {
                        RectanglesView.Add(new CustomShape(r));
                    }
                }

                OnPropertyChanged("Rectangles");
                OnPropertyChanged("RectanglesView");
                OnPropertyChanged("FilteredRectangles");
                OnPropertyChanged("DefaultLabel");
                OnPropertyChanged("RectangleCount");
                OnPropertyChanged("FilterVisibilitySelected");
                OnPropertyChanged("FilterVisibilitySelectedGallery");
                OnPropertyChanged("FilterVisibilityAllLabels");
                OnPropertyChanged("FilterVisibilityAllLabelsGallery");
            }
        }

        /// <summary>
        /// this method adds all different labels to the combobox
        /// </summary>
        public void ComboBoxNames()
        {
            temp = SelectedComboBoxItem;
            ComboBoxItems.Clear();
            ComboBoxItems.Add("All Labels");
            SelectedComboBoxItem = temp;

            foreach (var rec in Rectangles)
            {
                if (!ComboBoxItems.Contains(rec.Label))
                {
                    ComboBoxItems.Add(rec.Label);
                    OnPropertyChanged("ComboBoxItems");
                }
            }
        }

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

        private ICommand _loadXMLCommand;
        public ICommand LoadXMLCommand
        {
            get
            {
                return _loadXMLCommand ?? (_loadXMLCommand = new CommandHandler(() => LoadXML(), _canExecute));
            }
        }

        /// <summary>
        /// with this method, you can open an xml file from a different location as the loaded image.
        /// </summary>
        private async void LoadXML()
        {
            this.IsEnabled = true;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files | *.xml";

            if (openFileDialog.ShowDialog() == true)
                dst = openFileDialog.FileName;

            if (dst != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(dst);

                foreach (XmlNode node in doc.DocumentElement)
                {

                    if (node.Name == "object")
                    {
                        foreach (XmlNode objectChild in node)
                        {
                            if (objectChild.Name == "name")
                            {
                                name = objectChild.InnerText;
                                RectangleText = name;
                            }

                            if (objectChild.Name == "bndbox")
                            {
                                int xmin = int.Parse(objectChild["xmin"].InnerText);
                                int ymin = int.Parse(objectChild["ymin"].InnerText);
                                int xmax = int.Parse(objectChild["xmax"].InnerText);
                                int ymax = int.Parse(objectChild["ymax"].InnerText);

                                CustomShape loadedRect = new CustomShape(xmin,ymin,xmax-xmin,ymax-ymin,id);
                                id++;
                                loadedRect.Label = name;

                                Rectangles.Add(loadedRect);
                                RectanglesView.Add(loadedRect);
                                indexRectangles++;
                                indexRectanglesView++;
                                OnPropertyChanged("");
                            }
                        }
                    }
                }
            }
            ComboBoxNames();
            SortList();
            await cropImageLabelBegin();


        }


        #endregion

        /// <summary>
        /// Old stuff
        /// </summary>

        public MyICommand DeleteCommand { get; set; }
        public MyICommand DuplicateCommand { get; set; }
        public MyICommand RenameCommand { get; set; }
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// compare rectangle text to each other
        /// 
        /// without function
        /// </summary>
        public int CompareTo(object obj)
        {
            ResizableRectangle resizable = obj as ResizableRectangle;
            if (resizable == null)
            {
                throw new ArgumentException("Object is not Rectangle");
            }
            return this.RectangleText.CompareTo(resizable.RectangleText);
        }



        public ObservableCollection<ResizableRectangle> AllRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();

        public ObservableCollection<ResizableRectangle> AllRectanglesView { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<Polygon> polygonsCollection { get; set; } = new ObservableCollection<Polygon>();
        public ObservableCollection<ResizableRectangle> PixelRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<ResizableRectangle> FilteredRectangles { get; set; } = new ObservableCollection<ResizableRectangle>();
        public ObservableCollection<string> ComboBoxItems { get; set; } = new ObservableCollection<string>();
        //public Stack<ResizableRectangle> undoRectangles { get; set; } = new Stack<ResizableRectangle>();
        //public Stack<string> undoInformation { get; set; } = new Stack<string>();
        //public Stack<ResizableRectangle> redoRectangles { get; set; } = new Stack<ResizableRectangle>();
        //public Stack<string> redoInformation { get; set; } = new Stack<string>();

        #region old Undo for ResizableRectangles
        //private ICommand _undoStackCommand;
        //public ICommand UndoCommand
        //{
        //    get
        //    {
        //        return _undoStackCommand ?? (_undoStackCommand = new CommandHandler(() => Undo(), _canExecute));
        //    }
        //}

        ///// <summary>
        ///// this method undo a rectangle on canvas. 
        ///// </summary>
        //private void Undo()
        //{
        //    if (undoRectangles.Count() > 1 && undoInformation.Count() > 1)
        //    {
        //        // when you have added a rectangle, the undo command will delete the rectangle.
        //        if (undoInformation.Peek() == "Add")
        //        {
        //            var top = undoRectangles.Pop();
        //            var info = undoInformation.Pop();

        //            redoRectangles.Push(top);
        //            redoInformation.Push(info);

        //            top.RectangleFill = System.Windows.Media.Brushes.Blue;
        //            top.RectangleOpacity = 0.07;
        //            AllRectangles.Remove(top);
        //            AllRectanglesView = AllRectangles;
        //            UpdateCropedImage(top);
        //        }

        //        // when you have deleted a rectangle, the undo command will add the rectangle.
        //        if (undoInformation.Peek() == "Delete")
        //        {
        //            var top = undoRectangles.Pop();
        //            var info = undoInformation.Pop();

        //            redoRectangles.Push(top);
        //            redoInformation.Push(info);

        //            top.RectangleFill = System.Windows.Media.Brushes.Blue;
        //            top.RectangleOpacity = 0.07;
        //            AllRectangles.Add(top);
        //            AllRectanglesView = AllRectangles;
        //            UpdateCropedImage(top);
        //        }
        //    }
        //    OnPropertyChanged("");
        //}
        #endregion


        #region old Redo for ResizableRectangles
        //private ICommand _redoStackCommand;
        //public ICommand RedoCommand
        //{
        //    get
        //    {
        //        return _redoStackCommand ?? (_redoStackCommand = new CommandHandler(() => Redo(), _canExecute));
        //    }
        //}

        //private void Redo()
        //{
        //    // Undo the undo command :D
        //    if (redoRectangles.Count() > 1 && redoInformation.Count() > 1)
        //    {
        //        // see undo command documentation above.
        //        if (redoInformation.Peek() == "Add")
        //        {
        //            var top = redoRectangles.Pop();
        //            var info = redoInformation.Pop();

        //            undoRectangles.Push(top);
        //            undoInformation.Push(info);

        //            top.RectangleFill = System.Windows.Media.Brushes.Blue;
        //            top.RectangleOpacity = 0.07;

        //            AllRectangles.Add(top);
        //            AllRectanglesView = AllRectangles;
        //            UpdateCropedImage(top);
        //        }

        //        // see undo documentation above.
        //        if (redoInformation.Peek() == "Delete")
        //        {
        //            var top = redoRectangles.Pop();
        //            var info = redoInformation.Pop();

        //            undoRectangles.Push(top);
        //            undoInformation.Push(info);

        //            top.RectangleFill = System.Windows.Media.Brushes.Blue;
        //            top.RectangleOpacity = 0.07;
        //            AllRectangles.Remove(top);
        //            AllRectanglesView = AllRectangles;
        //            UpdateCropedImage(top);
        //        }
        //    }
        //    OnPropertyChanged("");
        //}
        #endregion

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
            XMLWriter.WritePascalVocToXML(AllRectangles.ToList(), destFileName, 1337, 1337, 3);

            //UpdatePreviews();

            //string destFileName1 = ImagePath.Remove(ImagePath.LastIndexOf('.'));

            //foreach (var rec in AllRectangles)
            //{
            //    string path1 = destFileName1 + @"_Cropped_Images\" + rec.RectangleText + @"\";

            //    if (!Directory.Exists(path1))
            //    {
            //        Directory.CreateDirectory(path1);
            //    }

            //    if (rec.CroppedImage != null)
            //    {
            //        BitmapEncoder encoder = new PngBitmapEncoder();
            //        encoder.Frames.Add(BitmapFrame.Create(rec.CroppedImage));
            //        string filename = path1 + AllRectangles.IndexOf(rec) + ".png";
            //        using (var fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            //        {
            //            encoder.Save(fileStream);
            //        }
            //    }
            //}
        }


        private ICommand _addRectangle;
        public ICommand AddRectangle
        {
            get
            {
                return _addRectangle ?? (_addRectangle = new CommandHandler(() => AddNewRectangle(), _canExecute));
            }
        }


        /// <summary>
        /// this method let us draw a rectangle 
        /// </summary>
        public void AddNewRectangle()
        {
            SortList();
            MyCanvas.Cursor = Cursors.Cross;

            if (SelectedComboBoxItem != "All Labels")
            {
                OnPropertyChanged("AllRectanglesView");
                Enabled = false;
                _rectangleText = SelectedComboBoxItem;
            }

            else
            {
                AllRectanglesView = AllRectangles;
                OnPropertyChanged("AllRectanglesView");
                Enabled = false;
                if (IsChecked == true && DefaultLabel.Length > 0)
                    _rectangleText = _defaultLabel;
                else
                    _rectangleText = "";
            }
        }

        private ICommand _loadImageCommand;
        public ICommand LoadImageCommand
        {
            get
            {
                return _loadImageCommand ?? (_loadImageCommand = new CommandHandler(() => LoadImage(), _canExecute));
            }
        }

        /// <summary>
        /// opens filedialog and let us browse any images which ends with .jpg, .jped, .png and .tiff
        /// </summary>
        private void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files | *.jpg; *.jpeg; *.png; *.tif";

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePath = openFileDialog.FileName;

                //BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
                //Bitmap src;

                //using (MemoryStream outStream = new MemoryStream())
                //{
                //    BitmapEncoder enc = new BmpBitmapEncoder();
                //    enc.Frames.Add(BitmapFrame.Create(bImage));
                //    enc.Save(outStream);
                //    Bitmap bitmap = new Bitmap(outStream);

                //    src = new Bitmap(bitmap);
                //}

                //image = SupportCode.ConvertBmp2Mat(src);

            }



            if (ImagePath != null)
            {
                this.IsEnabled = true;
                AllRectangles.Clear();
                clearUndoRedoStack();
                LoadRectangles();
                ComboBoxNames();
                SortList();
                FilterName();
            }
        }

        public void clearUndoRedoStack()
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
        /// declares a boolean variable that says if you can execute the command "Add Rectangle", "Update Preview",
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

        private ResizableRectangle _selectedResizableRectangle;
        public ResizableRectangle SelectedResizableRectangle
        {
            get
            {
                return _selectedResizableRectangle;
            }

            set
            {
                if(value != null)
                {
                    _selectedResizableRectangle = value;
                    if (value.X < 0)
                    {
                        _selectedResizableRectangle.X += value.X;
                        _selectedResizableRectangle.X = 0;
                    }
                    if (value.Y < 0)
                    {
                        _selectedResizableRectangle.Y += value.Y;
                        _selectedResizableRectangle.Y = 0;
                    }
                    SelectedRectangleFill();
                    DeleteCommand.RaiseCanExecuteChanged();
                    DuplicateCommand.RaiseCanExecuteChanged();
                    RenameCommand.RaiseCanExecuteChanged();
                }
                
            }
        }




        public string temp { get; set; }

        



        /// <summary>
        /// this method rename all files in the listox
        /// </summary>
        private void OnRename()
        {
            foreach(var rec in AllRectangles)
            {
                rec.RectangleText = DefaultLabel;
            }
        }

        /// <summary>
        /// says if you can execute "Can Rename" method
        /// </summary>
        private bool CanRename()
        {
            return SelectedResizableRectangle != null;
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
            AllRectangles.Clear();
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
        /// boolean, which tells if checkbox from default label is checked
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
                SelectedRectangleFill();
                OnPropertyChanged("CropModeChecked");
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
        
        /// <summary>
        /// this method colors the selected rectangle and increases the opacity
        /// </summary>
        public void SelectedRectangleFill()
        {
            if (SelectedResizableRectangle != null && AnnoToolMode == "Object")
            {
                if(CropModeChecked == false)
                {
                    foreach (var rect in AllRectangles)
                    {
                        rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                        rect.RectangleOpacity = 0.07;
                        rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                        rect.ThumbSize = 3;
                        rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
                        rect.Visibility = Visibility.Visible;
                    }
                    SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
                    SelectedResizableRectangle.RectangleOpacity = 0.0;
                    SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.ThumbSize = 3;
                    SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.Visibility = Visibility.Visible;
                }

                if(CropModeChecked == true)
                {
                    foreach (var rect in AllRectangles)
                    {
                        rect.RectangleFill = null;
                        rect.RectangleOpacity = 0.0;
                        rect.ThumbColor = System.Windows.Media.Brushes.Transparent;
                        rect.ThumbSize = 3;
                        rect.ResizeThumbColor = System.Windows.Media.Brushes.Transparent;
                        rect.Visibility = Visibility.Collapsed;

                    }

                    SelectedResizableRectangle.Visibility = Visibility.Visible;
                    SelectedResizableRectangle.RectangleFill = System.Windows.Media.Brushes.LightSalmon;
                    SelectedResizableRectangle.RectangleOpacity = 0.0;
                    SelectedResizableRectangle.ThumbColor = System.Windows.Media.Brushes.Red;
                    SelectedResizableRectangle.ThumbSize = 3;
                    SelectedResizableRectangle.ResizeThumbColor = System.Windows.Media.Brushes.Red;
                }
            }
            
            if(SelectedResizableRectangle == null && CropModeChecked == false)
            {
                foreach (var rect in AllRectangles)
                {
                    rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                    rect.RectangleOpacity = 0.07;
                    rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                    rect.ThumbSize = 3;
                    rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
                    rect.Visibility = Visibility.Visible;
                }
            }
        }

        

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
            Console.WriteLine("delete");
            SelectedResizableRectangle = null;

            foreach (var rect in AllRectangles)
            {
                rect.RectangleFill = System.Windows.Media.Brushes.Blue;
                rect.RectangleOpacity = 0.07;
                rect.ThumbColor = System.Windows.Media.Brushes.LawnGreen;
                rect.ThumbSize = 3;
                rect.ResizeThumbColor = System.Windows.Media.Brushes.Gray;
            }
            Enabled = true;
            MyCanvas.Cursor = Cursors.Arrow;
        }

        //private ICommand _enterCommand;
        //public ICommand EnterCommand
        //{
        //    get
        //    {
        //        return _enterCommand ?? (_enterCommand = new CommandHandler(() => Enter(), _canExecute));
        //    }
        //}

        //public void Enter()
        //{
        //    FilterName();
        //    SortList();
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

        public ResizableRectangle validateResizableRect(ResizableRectangle resizable)
        {
            if (resizable.X < 0)
            {
                resizable.RectangleWidth += resizable.X;
                resizable.X = 0;
            }
            if (resizable.Y < 0)
            {
                resizable.RectangleHeight += resizable.Y;
                resizable.Y = 0;
            }
            if (resizable.X + resizable.RectangleWidth > MyCanvas.ActualWidth) resizable.RectangleWidth = MyCanvas.ActualWidth - resizable.X;
            if (resizable.Y + resizable.RectangleHeight > MyCanvas.ActualHeight) resizable.RectangleHeight = MyCanvas.ActualHeight - resizable.Y;
            return resizable;
        }


        /// <summary>
        /// update only the cropped image of the selected rectangle
        /// </summary>
        /// <param name="resizable"></param>
        public void UpdateCropedImage(ResizableRectangle resizable)
        {
            resizable = validateResizableRect(resizable);
            if (resizable.RectangleHeight > 5 && resizable.RectangleWidth > 5)
            {
                BitmapImage bImage = new BitmapImage(new Uri(MyPreview.Source.ToString()));
                Bitmap src;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    src = new Bitmap(bitmap);
                }


                Mat mat = SupportCode.ConvertBmp2Mat(src);
                OpenCvSharp.Rect rectCrop = new OpenCvSharp.Rect((int)resizable.X, (int)resizable.Y, (int)resizable.RectangleWidth, (int)resizable.RectangleHeight);

                Mat croppedImage = new Mat(mat, rectCrop);
                resizable.CroppedImage = SupportCode.ConvertMat2BmpImg(croppedImage);
            }
                
        }

        public void SelectClickedRectangle(ResizableRectangle resizableRectangle)
        {
            resizableRectangle = validateResizableRect(resizableRectangle);
            SelectedResizableRectangle = resizableRectangle;
            OnPropertyChanged("SelectedResizableRectangle");
        }

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

        #region KeyArrowCommands

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
            if(SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleWidth > 5)
            {
                SelectedResizableRectangle.X = SelectedResizableRectangle.X + 2;
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth - 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
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
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleWidth > 5)
            {
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth - 2;
                UpdateCropedImage(SelectedResizableRectangle);
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
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleHeight > 5)
            {
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight - 2;
                UpdateCropedImage(SelectedResizableRectangle);
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
            if (SelectedResizableRectangle != null && SelectedResizableRectangle.RectangleHeight > 5)
            {
                SelectedResizableRectangle.Y = SelectedResizableRectangle.Y + 2;
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight - 2;
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
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
            if(SelectedResizableRectangle != null)
            {
                SelectedResizableRectangle.X = SelectedResizableRectangle.X - 2;
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth + 2;
                SelectedResizableRectangle = validateResizableRect(SelectedResizableRectangle);
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
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
            if(SelectedResizableRectangle != null)
            {
                SelectedResizableRectangle.RectangleWidth = SelectedResizableRectangle.RectangleWidth + 2;
                SelectedResizableRectangle = validateResizableRect(SelectedResizableRectangle);
                UpdateCropedImage(SelectedResizableRectangle);
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
            if(SelectedResizableRectangle != null)
            {
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight + 2;
                SelectedResizableRectangle = validateResizableRect(SelectedResizableRectangle);
                UpdateCropedImage(SelectedResizableRectangle);
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
            if(SelectedResizableRectangle != null)
            {
                SelectedResizableRectangle.Y = SelectedResizableRectangle.Y - 2;
                SelectedResizableRectangle.RectangleHeight = SelectedResizableRectangle.RectangleHeight + 2;
                SelectedResizableRectangle = validateResizableRect(SelectedResizableRectangle);
                Canvas.SetLeft(SelectedResizableRectangle, SelectedResizableRectangle.X);
                Canvas.SetTop(SelectedResizableRectangle, SelectedResizableRectangle.Y);
                OnPropertyChanged("SelectedResizableRectangle");
                UpdateCropedImage(SelectedResizableRectangle);
            }
        }

        #endregion

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
