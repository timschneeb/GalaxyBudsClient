using System;
using Avalonia.Controls;

namespace GalaxyBudsClient.Interface
{
    /// <summary>
    /// Represents a taskbar notification area icon (aka "tray icon") on Windows,
    /// and similar task panel notification icons on Linux and Mac.
    /// 
    /// Platform-specific implementations for this interface can be found in 
    /// <c>SQRLDotNetClientUI.Platform.XXX</c>.
    /// 
    /// </summary>
    public interface INotifyIcon
    {
        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText { get; set; }

        /// <summary>
        /// Gets or sets the context- (right-click)-menu for the notify icon.
        /// </summary>
        public ContextMenu ContextMenu { get; set; }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the 
        /// taskbar notification area or not.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Removes the notify icon from the taskbar notification area.
        /// </summary>
        public void Remove();

        /// <summary>
        /// This event is raised when a user clicks on the notification icon.
        /// </summary>
        public event EventHandler<EventArgs> Click;

        /// <summary>
        /// This event is raised when a user doubleclicks on the notification icon.
        /// </summary>
        public event EventHandler<EventArgs> DoubleClick;

        /// <summary>
        /// This event is raised when a user right-clicks on the notification icon.
        /// </summary>
        public event EventHandler<EventArgs> RightClick;
    }
}
