using System.ComponentModel;
using System.Drawing;

namespace SimpleXarts
{
    /// <summary>
    /// Implements all the properties needed to be bound to the Figures BindableProperty of a chart.
    /// Any type can be bound to Figures, but only the implemented properties will be accessed.
    /// Implements PropertyChanged to update the properties of the figure on the chart.
    /// A constructor is not needed.
    /// 
    /// Properties to implement in a custom class thats bound to Figures:
    /// 
    ///     float Value: Exposes the value of a chart entry.
    ///     string Describtion: Exposes the describtion of a chart entry.
    ///     System.Drawing.Color Color : Exposes the color of a chart entry.
    ///     
    /// </summary>
    public class Figure : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("Value"));
            }
        }

        private string _describtion;
        public string Describtion
        {
            get
            {
                return _describtion;
            }
            set
            {
                _describtion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Describtion"));
            }
        }

        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
            }
        }

        public Figure(float value, string describtion = "")
        {
            Value = value;
            Describtion = describtion;
        }

    }
}
