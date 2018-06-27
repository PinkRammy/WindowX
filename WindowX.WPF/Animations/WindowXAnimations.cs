using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace WindowX.WPF.Animations
{
    /// <summary>
    /// Contains information used by the animations inside the WindowX window.
    /// </summary>
    internal static class WindowXAnimations
    {
        /// <summary>
        /// The time (in seconds) it takes for a fast animation to complete.
        /// </summary>
        public const double FastAnimationDurationSeconds = 0.25;

        /// <summary>
        /// The time (in seconds) it takes for a normal animation to complete.
        /// </summary>
        public const double NormalAnimationDurationSeconds = 0.5;

        /// <summary>
        /// The time (in seconds) it takes for a slow animation to complete.
        /// </summary>
        public const double SlowAnimationDurationSeconds = 1;

        /// <summary>
        /// Initializes the WindowX animation information.
        /// </summary>
        static WindowXAnimations()
        {
            FastAnimationDuration = new Duration(TimeSpan.FromSeconds(FastAnimationDurationSeconds));
            NormalAnimationDuration = new Duration(TimeSpan.FromSeconds(NormalAnimationDurationSeconds));
            SlowAnimationDuration = new Duration(TimeSpan.FromSeconds(SlowAnimationDurationSeconds));
        }

        #region Properties

        /// <summary>
        /// Gets the duration of a fast animation.
        /// </summary>
        public static Duration FastAnimationDuration { get; }

        /// <summary>
        /// Gets the duration of a normal animation.
        /// </summary>
        public static Duration NormalAnimationDuration { get; }

        /// <summary>
        /// Gets the duration of a slow animation.
        /// </summary>

        public static Duration SlowAnimationDuration { get; }

        #endregion

        #region Fade Animations

        /// <summary>
        /// Fades in the specified control.
        /// </summary>
        /// <param name="control">The control to fade in.</param>
        /// <param name="duration">The duration of the fade in.</param>
        /// <param name="callback">The action to take once the control fades in.</param>
        public static void FadeIn(UIElement control, Duration duration, Action callback = null)
        {
            // Fade the control to 0
            FadeTo(control, 1, duration, callback);
        }

        /// <summary>
        /// Fades out the specified control.
        /// </summary>
        /// <param name="control">The control to fade out.</param>
        /// <param name="duration">The duration of the fade out.</param>
        /// <param name="callback">The action to take once the control fades out.</param>
        public static void FadeOut(UIElement control, Duration duration, Action callback = null)
        {
            // Fade the control to 0
            FadeTo(control, 0.0, duration, callback);
        }

        /// <summary>
        /// Fades the specified control to the specified opacity value.
        /// </summary>
        /// <param name="control">The control to fade.</param>
        /// <param name="value">The opacity value to fade the control to.</param>
        /// <param name="duration">The duration of the fade animation.</param>
        /// <param name="callback">The action to take once the control reaches the spcified fade value.</param>
        public static void FadeTo(UIElement control, double value, Duration duration, Action callback = null)
        {
            // Check the given control
            if (control == null) throw new ArgumentNullException(nameof(control));

            // Check the given value
            if (value < 0.0 || value > 1.0)
                throw new ArgumentOutOfRangeException(nameof(value), "The values must be between 0.0 and 1.0.");

            // Check the given duration
            if (duration == null) throw new ArgumentNullException(nameof(duration));

            // Create the animation
            var animation = new DoubleAnimation()
            {
                From = control.Opacity,
                To = value,
                Duration = duration
            };
            animation.Completed += (_, __) => { callback(); };

            // Start the animation
            control.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        #endregion

    }
}
