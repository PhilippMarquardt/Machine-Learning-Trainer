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

                Canvas.SetLeft(designerItem, left + e.HorizontalChange);
                Canvas.SetTop(designerItem, top + e.VerticalChange);
                if (rec != null)
                {
                    rec.X = rec.X + e.HorizontalChange;
                    rec.Y = rec.Y + e.VerticalChange;
                    //(this.DataContext as DrawerViewModel).MoveRectangle(rec);

                   


                    drawerviewmodel.SelectClickedRectangle(rec);
                    drawerviewmodel.UpdateCropedImage(rec);
                }
            }
        }

    }
}
    

