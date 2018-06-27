using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WindowX.WPF.Components;

namespace WindowX.WPF
{
    /// <summary>
    /// The custom WPF window.
    /// </summary>
    public class WindowX : Window
    {

        #region Constructors

        /// <summary>
        /// Static constructor for the WindowX component.
        /// Here we can specify the default styling to use for the window.
        /// </summary>
        static WindowX()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowX), new FrameworkPropertyMetadata(typeof(WindowX)));
        }

        /// <summary>
        /// Create a new WindowX, with the specified title.
        /// </summary>
        /// <param name="title">The title of the WindowX.</param>
        private WindowX(string title)
        {
            // Initialize the window chrome
            InitializeChrome();

            // Set the titlebar height
            TitlebarHeight = 24;

            // Set the title
            Title = title ?? "WindowX";
        }

        /// <summary>
        /// Create a new WindowX.
        /// </summary>
        public WindowX() : this("") { }

        #endregion

        // ----------

        #region Dependency Properties

        public static readonly DependencyProperty BorderSizeProperty =
            DependencyProperty.Register(
                "BorderSize",
                typeof(double),
                typeof(WindowX),
                new FrameworkPropertyMetadata(WhenBorderSizeChanged));

        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register(
                "IsMaximized",
                typeof(bool),
                typeof(WindowX),
                new FrameworkPropertyMetadata(false, WhenIsMaximizedChanged));

        public new static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(object),
                typeof(WindowX),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TitlebarHeightProperty =
            DependencyProperty.Register(
                "TitlebarHeight",
                typeof(double),
                typeof(WindowX),
                new FrameworkPropertyMetadata(WhenTitlebarHeightChanged));

        public static readonly DependencyProperty WindowTypeProperty =
            DependencyProperty.Register(
                "WindowType",
                typeof(WindowXType),
                typeof(WindowX),
                new PropertyMetadata(WindowXType.Window));

        #endregion

        #region Dependency Callbacks

        /// <summary>
        /// Handles the BorderSizeProperty value change.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
        private static void WhenBorderSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Check the new value
            if (e.NewValue.Equals(e.OldValue)) return;
            var newValue = (double)e.NewValue;

            // Get the WindowX
            var wndX = sender as WindowX;
            if (wndX == null) return;

            // Set the window border thickness
            wndX.BorderThickness = new Thickness(newValue);
        }

        /// <summary>
        /// Handles the BorderRadiusProperty value change.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
        private static void WhenBorderRadiusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Check the new value
            if (e.NewValue.Equals(e.OldValue)) return;
            var newValue = (double)e.NewValue;

            // Get the WindowX
            var wndX = sender as WindowX;
            if (wndX == null) return;

            // Check against the window border size
            if (newValue > wndX.BorderSize) return;
        }

        /// <summary>
        /// Handles the IsMaximizedProperty value change.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
        private static void WhenIsMaximizedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Check the new value
            if (e.NewValue.Equals(e.OldValue)) return;

            // Get the WindowX
            var wndX = sender as WindowX;
            if (wndX == null) return;

            // Set the correct border size
            if ((bool)e.NewValue)
                wndX.BorderSize = wndX.BorderSize * 2 - 1;
            else
                wndX.BorderSize = (wndX.BorderSize + 1) / 2;
        }

        /// <summary>
        /// Handles the TitlebarHeightProperty value change.
        /// </summary>
        /// <param name="sender">The DependencyObject on which the property has changed value.</param>
        /// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
        private static void WhenTitlebarHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Check the new value
            if (e.NewValue.Equals(e.OldValue)) return;

            // Get the WindowX
            var wndX = sender as WindowX;
            if (wndX == null) return;

            // Update the WindowX chrome
            var wndXChrome = WindowChrome.GetWindowChrome(wndX);
            if (wndXChrome == null) return;
            wndXChrome.CaptionHeight = (double)e.NewValue;
            WindowChrome.SetWindowChrome(wndX, wndXChrome);
        }

        #endregion

        // ----------

        #region Window Fields

        /// <summary>
        /// The WindowX Windows API message processor
        /// </summary>
        private WindowXProc _windowMessagesProcessor;

        #endregion

        #region Window Properties

        /// <summary>
        /// Gets or sets the WindowX border size.
        /// </summary>
        public double BorderSize
        {
            get { return (double)GetValue(BorderSizeProperty); }
            set { SetValue(BorderSizeProperty, value); }
        }

        /// <summary>
        /// Gets the handle of this WindowX.
        /// </summary>
        public IntPtr Hwnd { get; private set; }

        /// <summary>
        /// Determines if the WindowX is maximized.
        /// </summary>
        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the WindowX title.
        /// </summary>
        public new object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        /// <summary>
        /// Gets or sets the WindowX title bar height.
        /// </summary>
        public double TitlebarHeight
        {
            get { return (double)GetValue(TitlebarHeightProperty); }
            set { SetValue(TitlebarHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the WindowX type.
        /// </summary>
        public WindowXType WindowType
        {
            get { return (WindowXType)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }

        #endregion

        #region Window Initialization

        /// <summary>
        /// Initialize the Win32 Chrome borders.
        /// </summary>
        private void InitializeChrome()
        {
            // Initialize the resize border
            var resizeChrome = new WindowChrome
            {
                GlassFrameThickness = new Thickness(BorderSize),
                CornerRadius = new CornerRadius(0),
                CaptionHeight = TitlebarHeight
            };
            WindowChrome.SetWindowChrome(this, resizeChrome);
        }

        /// <summary>
        /// Overrides the internal call of ApplyTemplate to get access to the WindowX components.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Set the resource dictionary
            _resourceDictionary =
                (ResourceDictionary)Application.LoadComponent(
                    new Uri("/WindowX.WPF;component/Themes/Generic.xaml", UriKind.Relative));

            // Get the window structure components
            _windowBorder = GetTemplateChild(WindowXComponentNames.WindowBorder) as Border;

            // Apply the template
            base.OnApplyTemplate();

            // Set the title
            UpdateBaseWindowTitle();

            // Handle the events
            HandleEvents();
        }

        /// <summary>
        /// Handles the WindowX source initialization.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event arguments.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            // Initialize the window
            base.OnSourceInitialized(e);

            // Get the window handle
            Hwnd = new WindowInteropHelper(this).Handle;

            // Process window messages
            _windowMessagesProcessor = new WindowXProc(this);
            _windowMessagesProcessor.EnableAeroAnimations(Hwnd);
            HwndSource.FromHwnd(Hwnd).AddHook(new HwndSourceHook(_windowMessagesProcessor.WindowProc));
        }

        /// <summary>
        /// Sets the underlying window title.
        /// </summary>
        private void UpdateBaseWindowTitle()
        {
            // Check the title
            if (Title == null) return;

            // Get the string title
            var stringTitle = Title as string;
            if (stringTitle != null)
            {
                base.Title = stringTitle;
                return;
            }

            // Get the UIElement title
            var elementTitle = Title as UIElement;
            if (elementTitle == null)
            {
                base.Title = AppDomain.CurrentDomain.FriendlyName;
                return;
            }

            // Get the SWMWND title
            var titleElement =
                WPFHelper.GetVisualChildByName<FrameworkElement>(elementTitle, WindowXComponentNames.WindowTitle);
            if (titleElement == null)
            {
                base.Title = AppDomain.CurrentDomain.FriendlyName;
                return;
            }
            base.Title = titleElement.Tag as string;
        }


        #endregion

        #region Window Event Handlers

        /// <summary>
        /// Handle the WindowX events.
        /// </summary>
        private void HandleEvents()
        {
            // Handle the WindowX state change
            SizeChanged += WhenWindowXSizeChanged;
        }

        /// <summary>
        /// Handles the WindowX size change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void WhenWindowXSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Check if the window is maximized
            IsMaximized = WindowState == WindowState.Maximized;
        }

        #endregion

        // ----------

        #region Window Components

        /// <summary>
        /// The WindowX resource dictionary.
        /// </summary>
        private ResourceDictionary _resourceDictionary;

        /// <summary>
        /// The WindowX border.
        /// </summary>
        private Border _windowBorder;

        #endregion

    }
}
