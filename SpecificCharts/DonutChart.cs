using SimpleXart.ChartBase;
using SkiaSharp;
using System;
using Xamarin.Forms;
using SimpleXart.Util;
using SimpleXart.Converters;
using System.Linq;
using WindowsColor = System.Drawing.Color;

namespace SimpleXart
{
    public class DonutChart : Chart
    {

        //A full circle in degrees
        internal const float FullCircleDegrees = 360f;
        //A full circle in rad
        internal const float FullCircleRad = 2 * (float)Math.PI;

        //Circleradius is the CURRENT angle in rad of the chart. 
        //It depends on a possible current opening animation and the FullAngle provided by the binding
        public float CircleRadius => FullAngle * FullCircleRad * OpenedProportion;

        //The FullAngleProperty binding provides the full angle of the chart in degrees from 0-360.
        //The FullAngle Property itself goes from 0-1, so it can easily be multiplied with a rad value to shorten the CircleRadius.
        public static readonly BindableProperty FullAngleProperty
                = BindableProperty.Create("FullAngle", typeof(float), typeof(DonutChart), FullCircleDegrees);
        public float FullAngle
        {
            get { return ((float)GetValue(FullAngleProperty) / FullCircleDegrees); }
            set { SetValue(FullAngleProperty, value * FullCircleDegrees); }
        }




        //The Color of the chart if the list of Figures is null or empty
        public static readonly BindableProperty PlaceHolderColorProperty
            = BindableProperty.Create("PlaceHolderColor", typeof(WindowsColor), typeof(DonutChart), WindowsColor.Gray);
        public SKColor PlaceHolderColor
        {
            get { return ((WindowsColor)GetValue(PlaceHolderColorProperty)).ToSKColor(); }
            set { SetValue(PlaceHolderColorProperty, value.ToWindowsColor()); }
        }

        //The animated Property gets set directly or by animation whenever the InnerCircleProportion changes.
        //Only this value should be used to draw the chart, for it to be animated.
        protected float InnerCirclePropoertionAnimated { get; set; }
        private const string InnerCircleProportionAnimationHandle = "InnerCircleProportion";
        public static readonly BindableProperty InnerCircleProportionProperty =
            BindableProperty.Create("InnerCircleProportion", typeof(float), typeof(DonutChart), 0.45f, propertyChanged: OnInnerCircleProportionChanged);

        //Animates the InnerCircleProportion
        private static void OnInnerCircleProportionChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = (DonutChart)bindable;
            float oldProportion = (float)oldValue;
            float newProportion = (float)newValue;
            newValue = Math.Max(0, newProportion);
            newValue = Math.Min(1, newProportion);

            //If the animationtime is 0, set it directly and redraw
            if (instance.InnerCircleProportionAnimationTime == 0)
            {
                instance.InnerCirclePropoertionAnimated = newProportion;
                instance.Redraw();
            }
            //If there is an animationtime, animate
            else
            {
                var animation = new Animation((v) =>
                {
                    instance.InnerCirclePropoertionAnimated = (float)v;
                    instance.Redraw();
                }
                , instance.InnerCirclePropoertionAnimated, newProportion);
                if (instance.AnimationIsRunning(InnerCircleProportionAnimationHandle))
                {
                    instance.AbortAnimation(InnerCircleProportionAnimationHandle);
                }
                animation.Commit(instance, InnerCircleProportionAnimationHandle, _animationFramerate, instance.InnerCircleProportionAnimationTime, Easing.CubicInOut);

            }
        }

        public float InnerCircleProportion
        {
            get { return (float)GetValue(InnerCircleProportionProperty); }
            set { SetValue(InnerCircleProportionProperty, value); }
        }

        public static readonly BindableProperty InnerCircleProportionAnimationTimeProperty =
            BindableProperty.Create("InnerCircleProportionAnimationTime", typeof(uint), typeof(DonutChart), 0u);
        public uint InnerCircleProportionAnimationTime
        {
            get { return (uint)GetValue(InnerCircleProportionAnimationTimeProperty); }
            set { SetValue(InnerCircleProportionAnimationTimeProperty, value); }
        }

        public static readonly BindableProperty IsPolygonProperty = BindableProperty.Create("IsPolygon", typeof(bool), typeof(DonutChart), false);
        public bool IsPolygon
        {
            get { return (bool)GetValue(IsPolygonProperty); }
            set { SetValue(IsPolygonProperty, value); }
        }

        public static readonly BindableProperty OuterCornersProperty = BindableProperty.Create("OuterCorners", typeof(int), typeof(DonutChart), 7);
        public int OuterCorners
        {
            get { return (int)GetValue(OuterCornersProperty); }
            set { SetValue(OuterCornersProperty, value); }
        }

        public static readonly BindableProperty InnerCornersProperty = BindableProperty.Create("InnerCorners", typeof(int), typeof(DonutChart), 5);
        public int InnerCorners
        {
            get { return (int)GetValue(InnerCornersProperty); }
            set { SetValue(InnerCornersProperty, value); }
        }

        public DonutChart()
        {
            OnInnerCircleProportionChanged(this, InnerCircleProportion, InnerCircleProportion);
        }

        protected override void DrawSpecific(SKCanvas canvas, int width, int height)
        {
            using (new SKAutoCanvasRestore(canvas))
            {
                float radius = 0f;

                switch (DescribtionPosition)
                {
                    case DescribtionArea.Default:
                        radius = Math.Min((width / 2f) - Padding, (height / 2f) - Padding);
                        canvas.Translate(width / 2, height / 2);
                        break;
                    case DescribtionArea.LeftAndRight:
                        radius = Math.Min((width / 2f) - Padding - DescribtionSpace, (height / 2f) - Padding);
                        canvas.Translate(width / 2, height / 2);
                        break;
                    case DescribtionArea.Left:
                        radius = Math.Min((width / 2f) - Padding - DescribtionSpace / 2f, (height / 2f) - Padding);
                        canvas.Translate(DescribtionSpace / 2f + (width / 2f), height / 2f);
                        break;
                    case DescribtionArea.Right:
                        radius = Math.Min((width / 2f) - Padding - DescribtionSpace / 2f, (height / 2f) - Padding);
                        canvas.Translate(-DescribtionSpace / 2f + (width / 2f), height / 2f);
                        break;
                }

                if (Rotation != 0)
                {
                    canvas.RotateDegrees(Rotation);
                }

                //Create the inner and outer clip.
                //If its a polygon
                if (IsPolygon)
                {
                    canvas.ClipPath(CircleMath.GetPolygonPath(InnerCirclePropoertionAnimated * radius, InnerCorners), SKClipOperation.Difference, antialias: IsAntiAliased);
                    canvas.ClipPath(CircleMath.GetPolygonPath(radius, OuterCorners), SKClipOperation.Intersect, antialias: IsAntiAliased);
                }
                //If its a circle
                else
                {
                    canvas.ClipPath(CircleMath.GetCirclePath(InnerCirclePropoertionAnimated * radius), SKClipOperation.Difference, antialias: IsAntiAliased);
                    canvas.ClipPath(CircleMath.GetCirclePath(radius), SKClipOperation.Intersect, antialias: IsAntiAliased);
                }

                //If there are no Figures, draw a placeholder and return
                if (FigureAccesses == null || FigureAccesses.Count == 0)
                {
                    using (var paint = new SKPaint()
                    {
                        Color = PlaceHolderColor,
                        IsAntialias = IsAntiAliased,
                        Style = SKPaintStyle.Fill
                    }) canvas.DrawPath(CircleMath.GetPiePath(radius, 0, CircleRadius), paint);
                    return;
                }


                //Draw the Pie pieces for every figure

                //the sum of all values, adjusted by their animation
                float valueSum = FigureAccesses.Sum(x => x.AnimatedValue);
                //if the sum is 0, then the valuesum is set to 1 because the circle needs a valuesum > 0
                if(valueSum == 0) valueSum  = Math.Max(valueSum,1);

                //The starting position as portion of the full circle angle, going from 0 to 1
                //Gets adjusted after every figure by the portion of the radius that figure demands
                float position = 0;

                foreach (FigureAccess figure in FigureAccesses)
                {
                    //Create The pie shaped path
                    float circlePortion = figure.AnimatedValue / valueSum;
                    SKPath path = CircleMath.GetPiePath
                       (
                       radius: radius,
                       fromRad: position * CircleRadius,
                       toRad: (position + circlePortion) * CircleRadius
                       );

                    //Draw the clipped pie shape
                    using (var paint = new SKPaint()
                    {
                        Color = figure.Color,
                        IsAntialias = IsAntiAliased,
                        Style = SKPaintStyle.Fill
                    }) canvas.DrawPath(path, paint);

                    //Increase the position for the next figure
                    position += circlePortion;
                }
            }
        }

     
        
    }
}
