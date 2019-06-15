using System.ComponentModel;
using System.Drawing;

namespace SimpleXart
{
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
