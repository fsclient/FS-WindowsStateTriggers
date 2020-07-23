using System;
#if NET5_0
using Microsoft.UI.Xaml;
using UXAdaptiveTrigger = Microsoft.UI.Xaml.AdaptiveTrigger;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using UXAdaptiveTrigger = Windows.UI.Xaml.AdaptiveTrigger;
#endif

namespace WindowsStateTriggers
{
    /// <summary>
    /// Extends the <see cref="WUXAdaptiveTrigger"/> functionality with 
    /// <see cref="ITriggerValue"/> interface implementation 
    /// for <see cref="CompositeStateTrigger"/> usage
    /// </summary>
    public class AdaptiveTrigger : UXAdaptiveTrigger, ITriggerValue
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="AdaptiveTrigger"/> class.
		/// </summary>
        public AdaptiveTrigger()
        {
            RegisterPropertyChangedCallback(MinWindowHeightProperty, OnMinWindowHeightPropertyChanged);
            RegisterPropertyChangedCallback(MinWindowWidthProperty, OnMinWindowWidthPropertyChanged);

            var window = Window.Current;
            if (window != null)
            {
                var weakEvent = new WeakEventListener<AdaptiveTrigger, object, WindowSizeChangedEventArgs>(this)
                {
                    OnEventAction = (instance, s, e) => OnCoreWindowOnSizeChanged(s, e),
                    OnDetachAction = (instance, weakEventListener) => window.SizeChanged -= weakEventListener.OnEvent
                };
                window.SizeChanged += weakEvent.OnEvent;
            }
        }

        private void OnCoreWindowOnSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            IsActive = args.Size.Height >= MinWindowHeight && args.Size.Width >= MinWindowWidth;
        }

        private void OnMinWindowHeightPropertyChanged(object sender, DependencyProperty dp)
        {
            var window = Window.Current;
            if (window != null)
            {
                IsActive = window.Bounds.Height >= MinWindowHeight;
            }
        }

        private void OnMinWindowWidthPropertyChanged(object sender, DependencyProperty dp)
        {
            var window = Window.Current;
            if (window != null)
            {
                IsActive = window.Bounds.Width >= MinWindowWidth;
            }
        }

#region ITriggerValue

        private bool _isActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get => _isActive;
            private set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>
        public event EventHandler? IsActiveChanged;

#endregion ITriggerValue
    }
}
