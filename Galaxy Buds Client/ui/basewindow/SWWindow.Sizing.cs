using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Galaxy_Buds_Client.util;

namespace Galaxy_Buds_Client.ui.basewindow
{
    public partial class SWWindow
    {
        protected virtual Thickness GetDefaultMarginForDpi()
        {
            int currentDPI = SystemHelper.GetCurrentDPI();
            Thickness thickness = new Thickness(8, 8, 8, 8);
            if (currentDPI == 120)
            {
                thickness = new Thickness(7, 7, 4, 5);
            }
            else if (currentDPI == 144)
            {
                thickness = new Thickness(7, 7, 3, 1);
            }
            else if (currentDPI == 168)
            {
                thickness = new Thickness(6, 6, 2, 0);
            }
            else if (currentDPI == 192)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            else if (currentDPI == 240)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            return thickness;
        }

        protected virtual Thickness GetFromMinimizedMarginForDpi()
        {
            int currentDPI = SystemHelper.GetCurrentDPI();
            Thickness thickness = new Thickness(7, 7, 5, 7);
            if (currentDPI == 120)
            {
                thickness = new Thickness(6, 6, 4, 6);
            }
            else if (currentDPI == 144)
            {
                thickness = new Thickness(7, 7, 4, 4);
            }
            else if (currentDPI == 168)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 192)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 240)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            return thickness;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            double width = (double)screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            this.previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            double width = (double)screen.WorkingArea.Width;
            Rectangle workingArea = screen.WorkingArea;
            this.previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
            this.RefreshWindowState();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (base.WindowState == WindowState.Normal)
            {
                this.HeightBeforeMaximize = base.ActualHeight;
                this.WidthBeforeMaximize = base.ActualWidth;
                return;
            }
            if (base.WindowState == WindowState.Maximized)
            {
                Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
                if (this.previousScreenBounds.X != (double)screen.WorkingArea.Width || this.previousScreenBounds.Y != (double)screen.WorkingArea.Height)
                {
                    double width = (double)screen.WorkingArea.Width;
                    Rectangle workingArea = screen.WorkingArea;
                    this.previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
                    this.RefreshWindowState();
                }
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }

            Screen screen = Screen.FromHandle((new WindowInteropHelper(this)).Handle);
            Thickness thickness = new Thickness(0);
            if (this.WindowState != WindowState.Maximized)
            {

                double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
                Rectangle workingArea = screen.WorkingArea;
                this.MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;
                this.MaxWidth = double.PositiveInfinity;
            }
            else
            {
                thickness = this.GetDefaultMarginForDpi();
                if (this.PreviousState == WindowState.Minimized || this.Left == this.positionBeforeDrag.X && this.Top == this.positionBeforeDrag.Y)
                {
                    thickness = this.GetFromMinimizedMarginForDpi();
                }
            }

            this.LayoutRoot.Margin = thickness;
            this.PreviousState = this.WindowState;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!this.isMouseButtonDown)
            {
                return;
            }

            double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
            System.Windows.Point position = e.GetPosition(this);
            System.Diagnostics.Debug.WriteLine(position);
            System.Windows.Point screen = base.PointToScreen(position);
            double x = this.mouseDownPosition.X - position.X;
            double y = this.mouseDownPosition.Y - position.Y;
            if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1)
            {
                double actualWidth = this.mouseDownPosition.X;

                if (this.mouseDownPosition.X <= 0)
                {
                    actualWidth = 0;
                }
                else if (this.mouseDownPosition.X >= base.ActualWidth)
                {
                    actualWidth = this.WidthBeforeMaximize;
                }

                if (base.WindowState == WindowState.Maximized)
                {
                    this.ToggleWindowState();
                    this.Top = (screen.Y - position.Y) / currentDPIScaleFactor;
                    this.Left = (screen.X - actualWidth) / currentDPIScaleFactor;
                    this.CaptureMouse();
                }

                this.isManualDrag = true;

                this.Top = (screen.Y - this.mouseDownPosition.Y) / currentDPIScaleFactor;
                this.Left = (screen.X - actualWidth) / currentDPIScaleFactor;
            }
        }


        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseButtonDown = false;
            this.isManualDrag = false;
            this.ReleaseMouseCapture();
        }

        private void RefreshWindowState()
        {
            if (base.WindowState == WindowState.Maximized)
            {
                this.ToggleWindowState();
                this.ToggleWindowState();
            }
        }

    }
}
