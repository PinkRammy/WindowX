using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WindowX.Animations;

namespace WindowX.Components.Notifications
{
    /// <summary>
    /// Notification control used by the WindowX window to show information to the user.
    /// </summary>
    internal class WindowXNotification : HeaderedContentControl
    {

        /// <summary>
        /// The number of seconds to show the WindowX notification.
        /// </summary>
        public const int DefaultTimeToLiveSeconds = 6;

        #region Constructors

        /// <summary>
        /// Static constructor for the WindowX notification.
        /// Here we can specify the default styling to use for the control.
        /// </summary>
        static WindowXNotification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowXNotification), new FrameworkPropertyMetadata(typeof(WindowXNotification)));
        }

        /// <summary>
        /// Creates a new WindowX notification.
        /// </summary>
        /// <param name="content">The content of the WindowX notification.</param>
        /// <param name="title">The title of the WindowX notification.</param>
        /// <param name="type">The type of WindowX notification.</param>
        /// <param name="ttl">The time to live of the WindowX notification.</param>
        public WindowXNotification(object content, object title, WindowXNotificationType type, TimeSpan ttl)
        {
            // Set the notification type
            NotificationType = type;

            // Set the notification title
            Header = IsSupportedTitle(title) ? title : NotificationType.ToString();

            // Set the content
            Content = content;

            // Set the time to live
            _notificationTimeToLive = IsValidTimeToLive(ttl) ? ttl : TimeSpan.FromSeconds(DefaultTimeToLiveSeconds);
        }

        /// <summary>
        /// Creates a new WindowX notification.
        /// </summary>
        public WindowXNotification() : this(null, null, WindowXNotificationType.Information, TimeSpan.Zero) { }

        #endregion

        #region Dependency Properties

        public static readonly DependencyPropertyKey IsClosedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsClosed),
                typeof(bool),
                typeof(WindowXNotification),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsClosedProperty =
            IsClosedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty NotificationTypeProperty =
            DependencyProperty.Register(
                nameof(NotificationType),
                typeof(WindowXNotificationType),
                typeof(WindowXNotification),
                new PropertyMetadata(WindowXNotificationType.Information));

        #endregion

        #region Fields

        /// <summary>
        /// The WindowX notification cancellation token source used for the display of the notificaiton.
        /// </summary>
        private CancellationTokenSource _notificationToken;

        /// <summary>
        /// The WindowX notification task, in which the life of the notification is lived.
        /// </summary>
        private Task _notificationLife;

        /// <summary>
        /// The amount of time to display the notification to the user.
        /// </summary>
        private TimeSpan _notificationTimeToLive;

        #endregion

        #region Events

        public static readonly RoutedEvent ClosedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Closed),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(WindowXNotification));
        
        /// <summary>
        /// Occurs when the WindowX notification is closed.
        /// </summary>
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines if the WindowX notification is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the WindowX notification type.
        /// </summary>
        public WindowXNotificationType NotificationType
        {
            get { return (WindowXNotificationType)GetValue(NotificationTypeProperty); }
            set { SetValue(NotificationTypeProperty, value); }
        }

        #endregion

        #region Notification Methods

        /// <summary>
        /// Closes the WindowX notification.
        /// </summary>
        public void Close()
        {
            // Check if the notification is already closed
            if (IsClosed) return;

            // Stop the notification life
            if(_notificationToken != null)
                _notificationToken.Cancel();
            if (_notificationLife != null)
                _notificationLife.Dispose();

            // Hide the notification
            WindowXAnimations.FadeOut(this, WindowXAnimations.NormalAnimationDuration, WhenNotificationClose);
        }

        /// <summary>
        /// Handles the life of the WindowX notification.
        /// </summary>
        /// <param name="token">The CancellationToken to use for the WindowX notification life.</param>
        /// <returns>The WindowX notification life task.</returns>
        private async Task Live(CancellationToken token)
        {
            // Get the time to live
            var ttl = _notificationTimeToLive.TotalSeconds;

            // Initialize the wait time
            var waitTime = TimeSpan.FromSeconds(1);

            do
            {
                // Check if we need to exit
                if (token.IsCancellationRequested) break;

                try
                {
                    // Wait a second before closing
                    await Task.Delay(waitTime, token);
                }
                catch (OperationCanceledException) { break; }

                // Decrease the time to live
                ttl -= waitTime.TotalSeconds;
            } while (ttl > 0);

            // Close the notification
            Dispatcher.Invoke(Close);
        }

        /// <summary>
        /// Shows the WindowX notification.
        /// </summary>
        public void Show()
        {
            // Check if we are closed
            if (!IsClosed) return;

            // Show the notification
            WindowXAnimations.FadeIn(this, WindowXAnimations.NormalAnimationDuration, WhenNotificationShow);
        }

        /// <summary>
        /// Handles the event that occurs when the notification is closed.
        /// </summary>
        private void WhenNotificationClose()
        {
            // Set the closing flag
            Dispatcher.Invoke(() => IsClosed = true);

            // Trigger the event
            RaiseEvent(new RoutedEventArgs(ClosedEvent));
        }

        /// <summary>
        /// Handles the event that occurs when the notification is shown.
        /// </summary>
        private void WhenNotificationShow()
        {
            // Create the cancellation token
            _notificationToken = new CancellationTokenSource();

            // Start the notification life
            Live(_notificationToken.Token);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Determines if the value of the title is supported by the WindowX notification.
        /// </summary>
        /// <param name="value">The value of the WindowX notification title.</param>
        /// <returns><b>True</b> if the value of the title is a <b>UIElement</b> or <b>string</b>.</returns>
        private bool IsSupportedTitle(object value)
        {
            return value.GetType() == typeof(string) || value.GetType() == typeof(UIElement);
        }

        /// <summary>
        /// Determines if the value of the time to live is valid.
        /// </summary>
        /// <param name="value">The value of the WindowX notification time to live.</param>
        /// <returns><b>True</b> if the value of the time to live is valid.</returns>
        private bool IsValidTimeToLive(TimeSpan value)
        {
            return value != null || value != TimeSpan.Zero;
        }

        #endregion

    }
}
