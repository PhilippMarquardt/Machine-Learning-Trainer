using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MachineLearningTrainer.DrawerTool
{
    /// <summary>
    /// Interaktionslogik für PixelTool.xaml
    /// </summary>
    public partial class PixelTool : UserControl
    {
        public PixelTool()
        {
            InitializeComponent();
        }

        //    private System.Windows.Point currentPoint;
        //    private System.Windows.Point startPoint;
        //    private CustomShape rectSelectArea;
        //    private int colorFG { get; set; }
        //    public InkCanvas inkCanvas = new InkCanvas();

        //    private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //    {

        //        this.Focus();
        //        (this.DataContext as DrawerViewModel).AnnoToolMode = "Pixel";
        //        (this.DataContext as DrawerViewModel).MyInkCanvas = cnvInk;
        //        (this.DataContext as DrawerViewModel).MyCanvas = cnvImage;
        //        (this.DataContext as DrawerViewModel).MyPreview = imgPreview;

        //        var gridHeight = gridY0.ActualHeight + gridY1.ActualHeight + gridY2.ActualHeight;
        //        (this.DataContext as DrawerViewModel).ZoomBorderHeight = gridHeight; ;
        //        (this.DataContext as DrawerViewModel).ZoomBorderWidth = gridX.ActualWidth;
        //    }

        //    #region Zoom

        //    private void MenuItem_Reset(object sender, RoutedEventArgs e)
        //    {
        //        if ((this.DataContext as DrawerViewModel).ImagePath != null)
        //            zoomBorder.Reset();
        //    }

        //    private void MenuItem_ZoomOut(object sender, RoutedEventArgs e)
        //    {
        //        zoomBorder.ZoomOut();
        //    }

        //    private void MenuItem_ZoomIn(object sender, RoutedEventArgs e)
        //    {
        //        zoomBorder.ZoomIn();
        //    }

        //    #endregion

        //    private void ImgCamera_MouseDown(object sender, MouseButtonEventArgs e)
        //    {
        //        if ((this.DataContext as DrawerViewModel).Enabled == false)
        //        {
        //            cnvInk.Strokes.Clear();
        //            cnvImage.Cursor = Cursors.Cross;
        //            startPoint = e.GetPosition(cnvImage);
        //            rectSelectArea = new CustomShape(0,0,0);
        //            rectSelectArea.Opacity = 0;

        //            (this.DataContext as DrawerViewModel).PixelRectangles.Add(rectSelectArea);

        //            rectSelectArea.X1 = startPoint.X;
        //            rectSelectArea.Y1 = startPoint.Y;
        //        }

        //        currentPoint = e.GetPosition(cnvImage);
        //        startPoint = e.GetPosition(cnvImage);

        //    }

        //    private void ImgCamera_MouseMove(object sender, MouseEventArgs e)
        //    {
        //        if ((this.DataContext as DrawerViewModel).Enabled == false)

        //        {
        //            cnvImage.Cursor = Cursors.Cross;
        //            if (e.LeftButton == MouseButtonState.Released || rectSelectArea == null)
        //                return;

        //            var pos = e.GetPosition(cnvImage);

        //            var x = Math.Min(pos.X, startPoint.X);
        //            var y = Math.Min(pos.Y, startPoint.Y);

        //            var w = Math.Max(pos.X, startPoint.X) - x;
        //            var h = Math.Max(pos.Y, startPoint.Y) - y;

        //            rectSelectArea.Width = w;
        //            rectSelectArea.Height = h;

        //            rectSelectArea.X1 = x;
        //            rectSelectArea.Y1 = y;

        //            int recStartX = (Convert.ToInt16(x));
        //            int recStartY = (Convert.ToInt16(y));
        //            int recWidth = (Convert.ToInt16(w));
        //            int recHeight = (Convert.ToInt16(h));

        //        }
        //    }

        //    private void ImgCamera_MouseUp(object sender, MouseButtonEventArgs e)
        //    {
        //        if ((this.DataContext as DrawerViewModel).Enabled == false)
        //        {
        //            foreach (var q in (this.DataContext as DrawerViewModel).PixelRectangles)
        //                //q.RectangleMovable = true;
        //            (this.DataContext as DrawerViewModel).Enabled = true;
        //            cnvImage.Cursor = Cursors.Arrow;
        //            (this.DataContext as DrawerViewModel).RectOrMask = 0;

        //            //inkCanvas.Width = rectSelectArea.RectangleWidth;
        //            //inkCanvas.Opacity = 0.3;

        //            //inkCanvas.DefaultDrawingAttributes.Color = Colors.Purple;
        //            //inkCanvas.Height = rectSelectArea.RectangleHeight;
        //            //inkCanvas.Background = System.Windows.Media.Brushes.LawnGreen;
        //            //inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        //            //inkCanvas.IsEnabled = true;

        //            //Canvas.SetLeft(inkCanvas, rectSelectArea.X);
        //            //Canvas.SetTop(inkCanvas, rectSelectArea.Y);

        //            //cnvImage.Children.Add(inkCanvas);

        //        }
        //    }

        //    private void UserControl_KeyUp(object sender, KeyEventArgs e)
        //    {
        //        cnvInk = (this.DataContext as DrawerViewModel).MyInkCanvas;
        //        var viewModel = (this.DataContext as DrawerViewModel);
        //        viewModel.inkCanvas = inkCanvas;

        //        if (e.Key == Key.D)
        //        {
        //            cnvInk.IsEnabled = viewModel.DrawEnabled;
        //            cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
        //            colorFG = 0;
        //            cnvInk.EditingMode = InkCanvasEditingMode.Ink;

        //            foreach (var q in viewModel.PixelRectangles)
        //            {
        //                //q.RectangleMovable = false;
        //                q.Visible = false;
        //            }
        //        }

        //        if (e.Key == Key.F || e.Key == Key.B || e.Key == Key.E || e.Key == Key.OemPlus || e.Key == Key.OemMinus)
        //        {

        //            if (cnvInk.DefaultDrawingAttributes.Height < 3)
        //            {
        //                cnvInk.UseCustomCursor = true;
        //                cnvInk.Cursor = Cursors.Pen;
        //            }

        //            else
        //            {
        //                cnvInk.UseCustomCursor = false;
        //            }

        //            switch (e.Key)
        //            {
        //                case Key.F:
        //                    cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
        //                    colorFG = 0;
        //                    cnvInk.EditingMode = InkCanvasEditingMode.Ink;
        //                    break;
        //                case Key.B:
        //                    cnvInk.DefaultDrawingAttributes.Color = Colors.Red;
        //                    colorFG = 1;
        //                    cnvInk.EditingMode = InkCanvasEditingMode.Ink;
        //                    break;
        //                case Key.E:
        //                    cnvInk.EditingMode = InkCanvasEditingMode.EraseByPoint;
        //                    cnvInk.DefaultDrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Ellipse;
        //                    break;
        //                case Key.OemPlus:
        //                    cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height + 1;
        //                    cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width + 1;
        //                    break;
        //                case Key.OemMinus:
        //                    if (cnvInk.DefaultDrawingAttributes.Height > 1 && cnvInk.DefaultDrawingAttributes.Width > 1)
        //                    {
        //                        cnvInk.DefaultDrawingAttributes.Height = cnvInk.DefaultDrawingAttributes.Height - 1;
        //                        cnvInk.DefaultDrawingAttributes.Width = cnvInk.DefaultDrawingAttributes.Width - 1;
        //                    }
        //                    break;
        //            }
        //        }
        //    }

        //    private void cnvInk_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //    {
        //        if (colorFG == 0)
        //        {
        //            cnvInk.DefaultDrawingAttributes.Color = Colors.Red;
        //            cnvInk.EditingMode = InkCanvasEditingMode.Ink;
        //            colorFG = 1;
        //        }

        //        else
        //        {
        //            cnvInk.DefaultDrawingAttributes.Color = Colors.LawnGreen;
        //            cnvInk.EditingMode = InkCanvasEditingMode.Ink;
        //            colorFG = 0;
        //        }

        //    }

        //    private void CreateSaveBitmap(InkCanvas canvas, string filename)
        //    {
        //        RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
        //         (int)canvas.Width, (int)canvas.Height,
        //         96d, 96d, PixelFormats.Pbgra32);
        //        // needed otherwise the image output is black
        //        canvas.Measure(new System.Windows.Size((int)canvas.Width, (int)canvas.Height));
        //        canvas.Arrange(new System.Windows.Rect(new System.Windows.Size((int)canvas.Width, (int)canvas.Height)));

        //        renderBitmap.Render(canvas);

        //        //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //        PngBitmapEncoder encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

        //        using (FileStream file = File.Create(filename))
        //        {
        //            encoder.Save(file);
        //        }
        //    }
    }
}
 




