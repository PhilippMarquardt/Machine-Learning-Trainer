namespace MachineLearningTrainer.DrawerTool
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// this class contains all methods for zooming the image
    /// </summary>
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        public Point origin;
        private Point start;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
                .Children.First(tr => tr is ScaleTransform);
        }

        private RotateTransform GetRotateTransform(UIElement element)
        {
            return (RotateTransform)((TransformGroup)element.RenderTransform)
                .Children.First(tr => tr is RotateTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }
        

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                RotateTransform rt = new RotateTransform();
                group.Children.Add(rt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += Child_MouseWheel;
                this.MouseLeftButtonDown += Child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += Child_MouseLeftButtonUp;
                this.MouseMove += Child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                    Child_PreviewMouseRightButtonDown);
            }
        }



        public void Reset()
        {
           
            if (child != null)
            {
                var viewmodel = this.DataContext as DrawerViewModel;

                if(viewmodel.MyPreview.ActualWidth > viewmodel.ZoomBorderWidth)
                {
                    // calculate relation between zoomborder and image
                    var relX = viewmodel.ZoomBorderWidth / viewmodel.MyPreview.ActualWidth;

                    // reset zoom
                    var st = GetScaleTransform(child);
                    st.ScaleX = relX;
                    st.ScaleY = relX;

                    // reset pan
                    var tt = GetTranslateTransform(child);
                    tt.X = 0.0;
                    tt.Y = 0.0;
                }

                else
                {
                    // reset zoom
                    var st = GetScaleTransform(child);
                    st.ScaleX = 1.0;
                    st.ScaleY = 1.0;

                    // reset pan
                    var tt = GetTranslateTransform(child);
                    tt.X = 0.0;
                    tt.Y = 0.0;
                }
              
            }
        }

        public void ZoomToRectangle()
        {
            if (child != null)
            {
                if ((this.DataContext as DrawerViewModel).MyPreview == null)
                {
                    this.Reset();
                }

                if((this.DataContext as DrawerViewModel).SelectedResizableRectangle != null)
                {
                    var st = GetScaleTransform(child);
                    var tt = GetTranslateTransform(child);
                    var viewmodel = this.DataContext as DrawerViewModel;
                    tt.X = (viewmodel.MyPreview.ActualWidth / 2) - viewmodel.SelectedResizableRectangle.X * st.ScaleX - viewmodel.SelectedResizableRectangle.RectangleWidth * st.ScaleX / 2;
                    tt.Y = (viewmodel.MyPreview.ActualHeight / 2) - viewmodel.SelectedResizableRectangle.Y * st.ScaleY - viewmodel.SelectedResizableRectangle.RectangleHeight * st.ScaleY / 2;
                    double zeroX = (viewmodel.ZoomBorderWidth - viewmodel.MyPreview.ActualWidth) / 2;
                }
            }
        }

        public void ResetPan()
        {
            if(child != null)
            {
                var st = GetScaleTransform(child);
               
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        #region Child Events

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
           
            var enabled = ((sender as ZoomBorder).DataContext as DrawerViewModel).Enabled;
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? st.ScaleX * (this.DataContext as DrawerViewModel).MyPreview.ActualWidth * 0.08 / 1000 : -st.ScaleX * (this.DataContext as DrawerViewModel).MyPreview.ActualWidth * 0.08 / 1000;
                if (!(e.Delta > 0) && (st.ScaleX < .1 || st.ScaleY < .1))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        public void ZoomOut()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = -.2;
                if ((st.ScaleX < .1 || st.ScaleY < .1))
                    return;

                double abosuluteX;
                double abosuluteY;

                abosuluteX = ((this.DataContext as DrawerViewModel).MyCanvas.ActualWidth) / 2 * st.ScaleX + tt.X;
                abosuluteY = ((this.DataContext as DrawerViewModel).MyCanvas.ActualHeight) / 2 * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - ((this.DataContext as DrawerViewModel).MyCanvas.ActualWidth) / 2 * st.ScaleX;
                tt.Y = abosuluteY - ((this.DataContext as DrawerViewModel).MyCanvas.ActualHeight) / 2 * st.ScaleY;
            }
        }

        public void ZoomIn()
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = .2;
                if ((st.ScaleX < .1 || st.ScaleY < .1))
                    return;

                double abosuluteX;
                double abosuluteY;

                abosuluteX = ((this.DataContext as DrawerViewModel).MyCanvas.ActualWidth) / 2 * st.ScaleX + tt.X;
                abosuluteY = ((this.DataContext as DrawerViewModel).MyCanvas.ActualHeight) / 2 * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - ((this.DataContext as DrawerViewModel).MyCanvas.ActualWidth) / 2 * st.ScaleX;
                tt.Y = abosuluteY - ((this.DataContext as DrawerViewModel).MyCanvas.ActualHeight) / 2 * st.ScaleY;
            }
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var enabled = ((sender as ZoomBorder).DataContext as DrawerViewModel).Enabled;
            if (child != null && enabled)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var enabled = ((sender as ZoomBorder).DataContext as DrawerViewModel).Enabled;
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            var enabled = ((sender as ZoomBorder).DataContext as DrawerViewModel).Enabled;
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }
}

