using SimpleXarts;
using SimpleXarts.ChartBase;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SimpleXarts
{
    public class BarChart : Chart
    {
        #region BarPaddingProperty
        public static readonly BindableProperty BarPaddingProperty = BindableProperty.Create("BarPadding", typeof(int), typeof(BarChart), 0, propertyChanged: OnBarPaddingChanged);

        private static void OnBarPaddingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = (BarChart)bindable;
            instance.Redraw();
        }

        public int BarPadding
        {
            get { return (int)GetValue(BarPaddingProperty); }
            set { SetValue(BarPaddingProperty, value); }
        }

        #endregion
        #region BarOrientation

        public static readonly BindableProperty BarOrientationProperty = BindableProperty.Create("BarOrientation", typeof(BarOrientation), typeof(BarChart), BarOrientation.Up, propertyChanged: OnBarOrientationChanged);

        private static void OnBarOrientationChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = (BarChart)bindable;
            instance.Redraw();
        }

        public BarOrientation BarOrientation
        {
            get { return (BarOrientation)GetValue(BarOrientationProperty); }
            set { SetValue(BarOrientationProperty, value); }
        }

        #endregion


        protected override void DrawSpecific(SKCanvas canvas, int width, int height)
        {

            float widthCut = 0f;
            float startPositionX = 0f;
            float rotationPivotX = 0f;
            float rotationPivotY = 0f;

            switch (DescribtionPosition)
            {
                case DescribtionArea.Default:
                    widthCut = width - 2 * Padding;
                    rotationPivotX = width / 2;
                    rotationPivotY = height / 2;
                    break;
                case DescribtionArea.LeftAndRight:
                    widthCut = width - 2 * Padding - 2 * DescribtionSpace;
                    startPositionX = DescribtionSpace;
                    rotationPivotX = width / 2;
                    rotationPivotY = height / 2;
                    break;
                case DescribtionArea.Left:
                    widthCut = width - 2 * Padding - DescribtionSpace;
                    startPositionX = DescribtionSpace;
                    rotationPivotX = (widthCut / 2) + startPositionX;
                    rotationPivotY = height / 2;
                    break;
                case DescribtionArea.Right:
                    widthCut = width - 2 * Padding - DescribtionSpace;
                    rotationPivotX = widthCut / 2;
                    rotationPivotY = height / 2;
                    break;
            }

            if (Rotation != 0)
            {
                canvas.RotateDegrees(Rotation, rotationPivotX, rotationPivotY);
            }


            //the sum of all values, adjusted by their animation
            float maxValue = 0;

            if (FigureAccesses == null || FigureAccesses.Count == 0)
            {
                return;
            }

            maxValue = FigureAccesses.Max(fig => fig.AnimatedValue);

            //The starting position as portion of the full circle angle, going from 0 to 1
            //Gets adjusted after every figure by the portion of the radius that figure demands
            float position = 0;
            float barWidth = widthCut / FigureAccesses.Count;
            float barHeight;

            foreach (FigureAccess figure in FigureAccesses)
            {
                
                if (BarOrientation== BarOrientation.Up || BarOrientation == BarOrientation.Down)
                {
                    barHeight = (height - 2 * Padding) * (figure.AnimatedValue / maxValue);
                }
                else
                {
                    barHeight = (widthCut) * (figure.AnimatedValue / maxValue);
                    barWidth = (height - 2 * Padding) / FigureAccesses.Count;
                }
               
                using (var paint = new SKPaint()
                {
                    Color = figure.Color,
                    IsAntialias = IsAntiAliased,
                    Style = SKPaintStyle.Fill
                })
                {
                    switch (BarOrientation)
                    {
                        case BarOrientation.Up:
                            canvas.DrawRect(
                                    x: startPositionX + Padding + BarPadding + position * barWidth,
                                    y: height - Padding - barHeight,
                                    w: barWidth - 2 * BarPadding,
                                    h: barHeight,
                                    paint: paint);
                            break;
                        case BarOrientation.Down:
                            canvas.DrawRect(
                                    x: startPositionX + Padding + BarPadding + position * barWidth,
                                    y: Padding,
                                    w: barWidth - 2 * BarPadding,
                                    h: barHeight,
                                    paint: paint);
                            break;
                        case BarOrientation.Left:
                            canvas.DrawRect(
                                    x: widthCut -Padding -barHeight,
                                    y: Padding + position * barWidth,
                                    w: barHeight,
                                    h: barWidth -2 * BarPadding,
                                    paint: paint);
                            break;
                        case BarOrientation.Right:
                            canvas.DrawRect(
                                    x: startPositionX + Padding,
                                    y: Padding + position * barWidth,
                                    w: barHeight,
                                    h: barWidth - 2 * BarPadding,
                                    paint: paint);
                            break;
                        default:
                            break;
                    }
                }
                //Increase the position for the next figure
                position += 1;
            }
        }
    }

}