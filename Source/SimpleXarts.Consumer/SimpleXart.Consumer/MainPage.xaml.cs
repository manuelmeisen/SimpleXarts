using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleXart.Consumer
{

    public class MyFigure : INotifyPropertyChanged
    {

        public float Value { get { return _value; } set { _value = value; ValueDescribtion = _value.ToString(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value")); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ValueDescribtion")); } }

        private float _value;
        public System.Drawing.Color Color { get; set; }
        public string ValueDescribtion { get; set; }
        public string Describtion { get; set; }




        public MyFigure(float value)
        {
            Value = value;
            ValueDescribtion = Value.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class MainPage : ContentPage
    {

        public ObservableCollection<Figure> Data { get; set; } = new ObservableCollection<Figure>()
        {
            new Figure(20)
    {
        Describtion = "Fruit",
        Color = System.Drawing.Color.FromArgb(240, 125, 100)
    },
    new Figure(5)
    {
        Describtion = "Fish",
        Color = System.Drawing.Color.FromArgb(100, 188, 194)
    },
    new Figure(12)
    {
        Describtion = "Sweets",
        Color = System.Drawing.Color.FromArgb(242, 194, 84)
    },
    new Figure(20)
            {
                        Describtion = "Vegetable",
                Color = Xamarin.Forms.Color.FromRgb(142, 215, 131)

            }
        };


        public MyFigure GetRandomFigure()
        {
            Random rng = new Random();
            return new MyFigure(rng.Next(15, 40))
            {
                Color = System.Drawing.Color.FromArgb(rng.Next(255), rng.Next(255), rng.Next(255)),
                Describtion = "New Value!"

            };

        }

        private ObservableCollection<MyFigure> _figures;
        public ObservableCollection<MyFigure> Figures
        {
            get { return _figures; }
            set
            {
                _figures = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Figure> _figuresDefault;
        public ObservableCollection<Figure> FiguresDefault
        {
            get { return _figuresDefault; }
            set
            {
                _figuresDefault = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MyFigure> FigureData { get; set; }
        public ObservableCollection<MyFigure> NoData { get; set; }

        private float rotation;
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                OnPropertyChanged();
            }
        }





        private float inner;
        public float Inner
        {
            get { return inner; }
            set
            {
                inner = value;
                OnPropertyChanged();
            }
        }


        

        public MainPage()
        {


            Inner = 0.3f;
            FigureData = new ObservableCollection<MyFigure>();
            FigureData.Add(new MyFigure(20)
            {
                Color = Color.FromRgb(144, 213, 133),
                Describtion = "Vegetable",
                ValueDescribtion = "20"

            });
            FigureData.Add(new MyFigure(10)
            {
                Color = Color.FromRgb(104, 185, 192),
                Describtion = "Meat",
                ValueDescribtion = "10"
            });
            FigureData.Add(new MyFigure(40)
            {
                Color = Color.FromRgb(243, 127, 100),
                Describtion = "Fruit",
                ValueDescribtion = "40"
            });
            FigureData.Add(new MyFigure(30)
            {
                Color = Color.FromRgb(243, 193, 81),
                Describtion = "Sweets",
                ValueDescribtion = "30"
            });
            FigureData.Add(new MyFigure(8)
            {
                Color = Color.FromRgb(66, 72, 86),
                Describtion = "Spices",
                ValueDescribtion = "8"
            });
            FigureData.Add(new MyFigure(8)
            {
                Color = Color.FromRgb(200, 50, 100),
                Describtion = "Bread",
                ValueDescribtion = "30"
            });
            FigureData.Add(new MyFigure(8)
            {
                Color = Color.FromRgb(50, 100, 20),
                Describtion = "Toxic",
                ValueDescribtion = "30"
            });

            var FigureDataDefault = new ObservableCollection<Figure>();

            FigureDataDefault.Add(new Figure(8)
            {
                Color = Color.FromRgb(50, 100, 20),
                Describtion = "Android"
            });

            FigureDataDefault.Add(new Figure(16)
            {
                Color = Color.FromRgb(200, 10, 20),
                Describtion = "ios"
            });

            InitializeComponent();
            BindingContext = this;

            Figures = FigureData;
            FiguresDefault = FigureDataDefault;

            Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
            {
                Rotation += 0.5f;
                return true;
            });

            //bool makeBigger = false;

            //Device.StartTimer(TimeSpan.FromMilliseconds(3000), () =>
            // {
            //     if (makeBigger) Inner = 0.8f;
            //     else Inner = 0.15f;
            //     makeBigger = !makeBigger;
            //     return true;
            // });

            //bool changeToNull = true;
            //Device.StartTimer(TimeSpan.FromMilliseconds(3000), () =>
            //{
            //    FigureData.Add(GetRandomFigure());
            //    FigureData.Add(GetRandomFigure());
            //    return true;
            //});
            //bool skip = true;
            //Device.StartTimer(TimeSpan.FromMilliseconds(3500), () =>
            //{
            //    if (skip)
            //    {
            //        skip = false;
            //        return true;
            //    }
            //    FigureData.RemoveAt(FigureData.Count / 2);
            //    return true;
            //});

            Device.StartTimer(TimeSpan.FromMilliseconds(1500), () =>
            {

                //
                
                

                var rng = new Random();
                Figures[rng.Next(0, Figures.Count)].Value = rng.Next(10, 80);
                FiguresDefault[rng.Next(0, FigureDataDefault.Count)].Value = rng.Next(10, 80);
                return true;
            });
        }


        private int clicks = 0;
        private void DonutChart_Touch(object sender, SkiaSharp.Views.Forms.SKTouchEventArgs e)
        {
            switch (clicks)
            {
                case 0:
                    Data[0].Value = 30;
                    break;
                case 1:
                    Data.Add(
                    new Figure(20)
                    {
                        Describtion = "Spices",
                        Color = Color.FromRgb(183, 93, 174)
                    }
                );
                    break;
                case 2:
                    Data.RemoveAt(1);
                    break;
            }
            clicks++;
        }

    }


}