using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace MachineLearningTrainer.DrawerTool
{
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control designerItem = this.DataContext as Control;
            var rec = ControlOperations.GetParentOfType<UserControl>(designerItem) as ResizableRectangle;

            var drawerviewmodel = (ControlOperations.GetParentOfType<ItemsControl>(designerItem) as ItemsControl).DataContext as DrawerViewModel;

            if (designerItem != null)
            {
                double left = Canvas.GetLeft(designerItem);
                double top = Canvas.GetTop(designerItem);

                if (rec != null)
                {
                    double rXMin = rec.X + e.HorizontalChange;
                    double rYMin = rec.Y + e.VerticalChange;
                    double rXMax = rXMin + rec.RectangleWidth;
                    double rYMax = rYMin + rec.RectangleHeight;
                    if (rXMin >= 0 && rYMin >= 0 && rXMax <= drawerviewmodel.MyCanvas.ActualWidth && rYMax <= drawerviewmodel.MyCanvas.ActualHeight)
                    {
                        Canvas.SetLeft(designerItem, left + e.HorizontalChange);
                        Canvas.SetTop(designerItem, top + e.VerticalChange);
                        rec.X = rXMin;
                        rec.Y = rYMin;
                        //(this.DataContext as DrawerViewModel).MoveRectangle(rec);               
                        
                        //drawerviewmodel.SelectClickedRectangle(rec);
                        drawerviewmodel.UpdateCropedImage(rec);
                    }
                }
            }
        }

    }
}
    

