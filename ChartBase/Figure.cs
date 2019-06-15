using System.ComponentModel;
using System.Drawing;

namespace SimpleXart
{
    public class Figure
    {
        public float Value { get; set; }
        public string Describtion { get; set; } = "";
        public Color Color { get; set; } = Color.LightGray;

        public Figure(float value)
        {
            Value = value;
        }
    }
}
