using SimpleXarts.Converters;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Reflection;

namespace SimpleXarts.ChartBase
{
    internal class FigureAccess
    {
        internal object Source => _source;
        private object _source;

        private INotifyPropertyChanged _notifier;
        private bool _subscribed = false;

        PropertyInfo ValueProperty, DescribtionProperty, ColorProperty;

        private Func<float> GetValue { get; set; }
        private Func<string> GetValueDescribtion { get; set; }
        private Func<string> GetDescribtion { get; set; }
        private Func<SKColor> GetColor { get; set; }

        private Action<FigureAccess> OnValueChanged;

        private float _value;
        internal float Value
        {
            get
            {
                return _value;
            }
            private set { _value = value; }
        }

        internal string ValueAnimationHandle = Guid.NewGuid().ToString();
        internal float ValueDeltaProportion;
        internal float AnimatedValue => Value * ValueDeltaProportion * Entrance;

        private string _describtion;
        internal string Describtion
        {
            get
            {
                return _describtion;
            }
            private set { _describtion = value; }
        }

        private SKColor _color;
        internal SKColor Color
        {
            get
            {
                return _color.WithAlpha((byte)(_color.Alpha * Entrance));
            }
            private set { _color = value; }
        }

        internal float Entrance { get; set; }
        internal string EntranceHandle { get; set; } = Guid.NewGuid().ToString();

        public FigureAccess(object source, Action<FigureAccess> onValueChanged,bool easeInValue)
        {
            //the chart supplies the onValueChanged action, that gets invoked on the chart whenever the value changes, to animate the change.
            OnValueChanged = onValueChanged;



            // Every Figure gets initialized with its full Value. ValueDeltaProportion is responsible for animating change, not the entrance.
            ValueDeltaProportion = 1f;

            /* When the access gets created because theres a new List of Figures, the chart animates its own opening, so all the figures need to have their full value immediately.
             * When created because a new Figure was added to the list,  easeInValue should be set to true, so the value gets eased in.
             */
            Entrance = easeInValue ? 0f : 1f;

            _source = source;
            _notifier = source as INotifyPropertyChanged;
            Subscribe();
            SetAccessors(source);
            BufferAllValues();

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Figure.Value):
                    {
                        float newValue = GetValue();
                        ValueDeltaProportion = AnimatedValue / newValue;
                        // The chart animates the ValueDeltaProperty, this is done on the chart, instead of this value, because the animation 
                        // handle needs to be executed there.
                        Value = newValue;
                        OnValueChanged?.Invoke(this);
                        break;
                    }
                case nameof(Figure.Describtion):
                    {
                        Describtion = GetDescribtion();
                        break;
                    }
                case nameof(Figure.Color):
                    {
                        Color = GetColor();
                        break;
                    }
            }
        }

        private void Subscribe()
        {
            if (_notifier != null)
            {
                _notifier.PropertyChanged += OnPropertyChanged;
            }
            _subscribed = true;
        }


        internal void Unsubscribe()
        {
            if (_subscribed)
            {
                if (_notifier != null)
                {
                    _notifier.PropertyChanged -= OnPropertyChanged;
                }
                UnsetAccessors();
            }
            _source = null;
            _notifier = null;
        }


        private void SetAccessors(object source)
        {
            ValueProperty = source.GetType().GetProperty(nameof(Figure.Value));
            DescribtionProperty = source.GetType().GetProperty(nameof(Figure.Describtion));
            ColorProperty = source.GetType().GetProperty(nameof(Figure.Color));

            GetValue = () => ValueProperty != null ? (float)ValueProperty.GetValue(_source) : 0f;
            GetDescribtion = () => DescribtionProperty != null ? (string)DescribtionProperty.GetValue(_source) : "";
            GetColor = () => ColorProperty != null ? ((System.Drawing.Color)ColorProperty.GetValue(_source)).ToSKColor() : SKColors.LightGray;
        }

        private void UnsetAccessors()
        {
            ValueProperty = null;
            DescribtionProperty = null;
            ColorProperty = null;
        }

        private void BufferAllValues()
        {
            Value = GetValue();
            Describtion = GetDescribtion();
            Color = GetColor();
        }
    }
}
