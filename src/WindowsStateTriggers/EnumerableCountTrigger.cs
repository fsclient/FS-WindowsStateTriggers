// Copyright (c) Morten Nielsen. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace WindowsStateTriggers
{
    public class EnumerableCountTrigger : StateTriggerBase, ITriggerValue
    {
        public IEnumerable Enumerable
        {
            get => (IEnumerable)GetValue(EnumerableProperty);
            set => SetValue(EnumerableProperty, value);
        }

        public static readonly DependencyProperty EnumerableProperty =
            DependencyProperty.Register(nameof(Enumerable), typeof(IEnumerable), typeof(EnumerableCountTrigger),
                new PropertyMetadata(Array.Empty<int>(), OnValuePropertyChanged));

        public int CompareCountTo
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(nameof(CompareCountTo), typeof(int), typeof(EnumerableCountTrigger),
                new PropertyMetadata(0, OnValuePropertyChanged));

        public CountComparison Comparison
        {
            get => (CountComparison)GetValue(ComparisonProperty);
            set => SetValue(ComparisonProperty, value);
        }

        public static readonly DependencyProperty ComparisonProperty =
            DependencyProperty.Register(nameof(Comparison), typeof(CountComparison), typeof(EnumerableCountTrigger),
                new PropertyMetadata(CountComparison.Equal, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs? _)
        {
            var obj = (EnumerableCountTrigger)d;
            obj.IsActive = MatchCount(obj.Enumerable, obj.CompareCountTo, obj.Comparison);


            if (obj.Enumerable is INotifyCollectionChanged notifyCollectionChanged)
            {
                var weakEvent = new WeakEventListener<EnumerableCountTrigger, object, NotifyCollectionChangedEventArgs>(obj)
                {
                    OnEventAction = OnCollectionChanged,
                    OnDetachAction = (trigger, weakEventListener) =>
                    {
                        if (trigger is INotifyCollectionChanged notifyCollectionChangedInner)
                        {
                            notifyCollectionChangedInner.CollectionChanged -= weakEventListener.OnEvent;
                        }
                    }
                };
                notifyCollectionChanged.CollectionChanged += weakEvent.OnEvent;
            }
            else if (obj.Enumerable is INotifyPropertyChanged notifyPropertyChanged)
            {
                var weakEvent = new WeakEventListener<EnumerableCountTrigger, object, PropertyChangedEventArgs>(obj)
                {
                    OnEventAction = OnPropertyChanged,
                    OnDetachAction = (trigger, weakEventListener) =>
                    {
                        if (trigger is INotifyPropertyChanged notifyPropertyChangedInner)
                        {
                            notifyPropertyChanged.PropertyChanged -= weakEventListener.OnEvent;
                        }
                    }
                };
                notifyPropertyChanged.PropertyChanged += weakEvent.OnEvent;
            }
        }

        private static void OnPropertyChanged(EnumerableCountTrigger arg1, object arg2, PropertyChangedEventArgs arg3)
        {
            if (arg3.PropertyName == nameof(ICollection.Count))
            {
                OnValuePropertyChanged(arg1, null);
            }
        }

        private static void OnCollectionChanged(EnumerableCountTrigger arg1, object arg2, NotifyCollectionChangedEventArgs arg3)
        {
            OnValuePropertyChanged(arg1, null);
        }

        public static bool MatchCount(IEnumerable? source, int count, CountComparison comparison)
        {
            if (source == null)
            {
                return false;
            }

            if (source is ICollection collection)
            {
                return comparison switch
                {
                    CountComparison.Equal => collection.Count == count,
                    CountComparison.GreaterThan => collection.Count > count,
                    CountComparison.LessThan => collection.Count < count,
                    _ => throw new InvalidOperationException($"{nameof(CountComparison)}.{comparison} is not supported.")
                };
            }
            if (source is IReadOnlyCollection<object> readOnlyCollection)
            {
                return comparison switch
                {
                    CountComparison.Equal => readOnlyCollection.Count == count,
                    CountComparison.GreaterThan => readOnlyCollection.Count > count,
                    CountComparison.LessThan => readOnlyCollection.Count < count,
                    _ => throw new InvalidOperationException($"{nameof(CountComparison)}.{comparison} is not supported.")
                };
            }

            var enumerator = source.GetEnumerator();
            using var _ = enumerator as IDisposable ?? default(NoopDisposable);

            return comparison switch
            {
                CountComparison.Equal when count == 0 => !enumerator.MoveNext(),
                CountComparison.GreaterThan when count == 0 => enumerator.MoveNext(),

                CountComparison.Equal => CountSlow(enumerator, count + 1) == count,
                CountComparison.GreaterThan => CountSlow(enumerator, count + 1) > count,
                CountComparison.LessThan => CountSlow(enumerator, count) < count,

                _ => throw new InvalidOperationException($"{nameof(CountComparison)}.{comparison} is not supported.")
            };
        }

        public static int CountSlow(IEnumerator source, int limit)
        {
            var count = 0;
            checked
            {
                while (source.MoveNext() || limit-- > 0)
                {
                    count++;
                }
            }
            return count;
        }

        #region ITriggerValue

        private bool m_IsActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get => m_IsActive;
            private set
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    base.SetActive(value);
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>
        public event EventHandler? IsActiveChanged;

        #endregion ITriggerValue

        private struct NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    /// <summary>
    /// Count comparison types
    /// </summary>
    public enum CountComparison
    {
        /// <summary>
        /// Equals
        /// </summary>
        Equal,
        /// <summary>
        /// Less than
        /// </summary>
        LessThan,
        /// <summary>
        /// Greater than
        /// </summary>
        GreaterThan
    }
}
