using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MachineLearningTrainer.DrawerTool
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Control designerItem = this.DataContext as Control;
            var rec = ControlOperations.GetParentOfType<UserControl>(designerItem) as ResizableRectangle;
            var drawerviewmodel = (ControlOperations.GetParentOfType<ItemsControl>(designerItem) as ItemsControl).DataContext as DrawerViewModel;
            if (designerItem != null)
            {
                double deltaVertical, deltaHorizontal;
               
                switch (VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                        designerItem.Height -= deltaVertical;
                        break;
                    case System.Windows.VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                        Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + deltaVertical);
                        designerItem.Height -= deltaVertical;
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                        Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + deltaHorizontal);
                        designerItem.Width -= deltaHorizontal;
                        break;
                    case System.Windows.HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                        designerItem.Width -= deltaHorizontal;
                        break;
                    default:
                        break;
                }
                if (rec != null)
                {
                    rec.RectangleWidth = designerItem.Width;
                    rec.RectangleHeight = designerItem.Height;
                    drawerviewmodel.SelectClickedRectangle(rec);
                    if(rec.RectangleHeight > 5 && rec.RectangleWidth > 5)
                    {
                        drawerviewmodel.UpdateCropedImage(rec);
                    }
                }
            }

            e.Handled = true;
        }
    }
}
