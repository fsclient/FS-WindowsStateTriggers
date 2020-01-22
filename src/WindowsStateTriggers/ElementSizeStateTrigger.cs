// Copyright (c) Morten Nielsen. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Windows.UI.Xaml;

namespace WindowsStateTriggers
{
	/// <summary>
	/// Enables a state if element size is changed
	/// </summary>
    public class ElementSizeStateTrigger : StateTriggerBase, ITriggerValue
    {
		/// <summary>
		/// Identifies the <see cref="Element"/> DependencyProperty
		/// </summary>
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(
                "Element",
                typeof(FrameworkElement),
                typeof(ElementSizeStateTrigger),
                new PropertyMetadata(null, OnUIElementPropertyChanged));

		/// <summary>
		/// Identifies the <see cref="MinHeight"/> DependencyProperty
		/// </summary>
        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register(
                "MinHeight",
                typeof(double),
                typeof(ElementSizeStateTrigger),
                new PropertyMetadata(0d, OnMinHeightOrWidthPropertyChanged));

		/// <summary>
		/// Identifies the <see cref="MinWidth"/> DependencyProperty
		/// </summary>
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register(
                "MinWidth",
                typeof(double),
                typeof(ElementSizeStateTrigger),
                new PropertyMetadata(0d, OnMinHeightOrWidthPropertyChanged));

        /// <summary>
        /// Target element, on which trigger subscribes
        /// </summary>
        public FrameworkElement Element
        {
            get => (FrameworkElement)GetValue(ElementProperty);
            set => SetValue(ElementProperty, value);
        }

        /// <summary>
        /// Min height to enable trigger state
        /// </summary>
        public double MinHeight
        {
            get => (double)GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        /// <summary>
        /// Min width to enable trigger state
        /// </summary>
        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        private static void OnMinHeightOrWidthPropertyChanged(DependencyObject dependencyObject, object _)
        {
            if (dependencyObject is ElementSizeStateTrigger trigger
                && trigger.Element is FrameworkElement element)
            {
                trigger.IsActive = element.ActualHeight >= trigger.MinHeight && element.ActualWidth >= trigger.MinWidth;
            }
        }

        private static void OnUIElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FrameworkElement element
                && d is ElementSizeStateTrigger trigger)
            {
                var weakEvent = new WeakEventListener<ElementSizeStateTrigger, object, SizeChangedEventArgs>(trigger)
                {
                    OnEventAction = OnElementSizeChanged,
                    OnDetachAction = (trigger, weakEventListener) => trigger.Element.SizeChanged -= weakEventListener.OnEvent
                };
                element.SizeChanged += weakEvent.OnEvent;
            }
        }

        private static void OnElementSizeChanged(ElementSizeStateTrigger instance, object sender, SizeChangedEventArgs args)
        {
            instance.IsActive = args.NewSize.Height >= instance.MinHeight && args.NewSize.Width >= instance.MinWidth;
        }

        #region ITriggerValue

        private bool isActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get => isActive;
            private set
            {
                if (isActive != value)
                {
                    isActive = value;
                    SetActive(value);
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
