using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SimpleXart.Converters;
using System.Collections.Specialized;
using System.Collections;
using System;
using System.Collections.ObjectModel;
using SimpleXart.ChartBase;
using System.Linq;

namespace SimpleXart
{
    public abstract class Chart : SKCanvasView
    {

        private DateTime lastRedraw = DateTime.Now;

        internal const float DescribtionPadding = 12f;
        internal const float DescribtionSpacing = 2f;

        protected const uint _animationFramerate = 16;

        protected const string OpeningAnimationHandle = "OpeningHandle";

        protected float OpenedProportion = 0f; //Goes from 0-1 and represents the opening of the chart.


        public static readonly BindableProperty FractionalDigitsProperty
            = BindableProperty.Create("FractionalDigits", typeof(int), typeof(Chart), 0);
        public int FractionalDigits
        {
            get { return ((int)GetValue(FractionalDigitsProperty)); }
            set { SetValue(FractionalDigitsProperty, value); }
        }

        public static readonly BindableProperty AnimateVisibleValuesProperty
            = BindableProperty.Create("AnimateVisibleValues", typeof(bool), typeof(Chart), false);
        public bool AnimateVisibleValues
        {
            get { return ((bool)GetValue(AnimateVisibleValuesProperty)); }
            set { SetValue(AnimateVisibleValuesProperty, value); }
        }

        public static readonly BindableProperty DescribtionSpaceProperty
                = BindableProperty.Create("DescribtionSpace", typeof(float), typeof(Chart), 175f);
        public float DescribtionSpace
        {
            get { return ((float)GetValue(DescribtionSpaceProperty)); }
            set { SetValue(DescribtionSpaceProperty, value); }
        }

        public static readonly BindableProperty DescribtionPositionProperty
                = BindableProperty.Create("DescribtionPosition", typeof(DescribtionArea), typeof(Chart), DescribtionArea.Default);
        public DescribtionArea DescribtionPosition
        {
            get { return ((DescribtionArea)GetValue(DescribtionPositionProperty)); }
            set { SetValue(DescribtionPositionProperty, value); }
        }

        public static readonly BindableProperty PaddingProperty
            = BindableProperty.Create("Padding", typeof(float), typeof(Chart), 0f);
        public float Padding
        {
            get { return (float)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        new public static readonly BindableProperty RotationProperty
            = BindableProperty.Create("Rotation", typeof(float), typeof(Chart), 0f, propertyChanged: OnRotationChanged);
        new public float Rotation
        {
            get
            {
                return (float)GetValue(RotationProperty);
            }
            set
            {
                SetValue(RotationProperty, value);
            }
        }
        private static void OnRotationChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Chart)bindable).Redraw();
        }

        new public static readonly BindableProperty BackgroundColorProperty
            = BindableProperty.Create("BackgroundColor", typeof(Color), typeof(Chart), Color.White);
        new public Color BackgroundColor
        {
            get
            {
                return ((Color)GetValue(BackgroundColorProperty));
            }
            set
            {
                SetValue(BackgroundColorProperty, value);
            }
        }

        public static readonly BindableProperty IsAntiAliasedProperty = BindableProperty.Create("IsAntiAliased", typeof(bool), typeof(Chart), true);
        public bool IsAntiAliased
        {
            get { return (bool)GetValue(IsAntiAliasedProperty); }
            set { SetValue(IsAntiAliasedProperty, value); }
        }

        public static readonly BindableProperty FiguresProperty
            = BindableProperty.Create("Figures", typeof(ICollection), typeof(Chart), null, propertyChanged: OnFiguresChanged);

        private static void OnFiguresChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = ((Chart)bindable);
            instance.OnFiguresChangedInstance(((ICollection)oldValue));


            //Redraw
            instance.Redraw();
        }

        public void Redraw()
        {
            if (lastRedraw.Ticks + 400000 < DateTime.Now.Ticks)
            {
                InvalidateSurface();
                lastRedraw = DateTime.Now;
            }
        }

        private void OnFiguresChangedInstance(ICollection oldValue)
        {
            if (oldValue is INotifyCollectionChanged observable)
            {
                UnSubscribeToFigureList(observable);
            }

            //Close Open/Close Animation if running
            if (this.AnimationIsRunning(OpeningAnimationHandle))
            {
                this.AbortAnimation(OpeningAnimationHandle);
            }

            //If its not closed, close and then open
            if (OpenedProportion != 0)
            {
                var ClosingAnimation = new Animation((v) => { OpenedProportion = (float)v; Redraw(); }, OpenedProportion, 0, Easing.CubicInOut);
                ClosingAnimation.Commit(this, OpeningAnimationHandle, _animationFramerate, (uint)(2000 * (OpenedProportion)), finished: (v, c) => l_OpenChart());
            }
            else // If its closed, just open
            {
                l_OpenChart();
            }

            void l_OpenChart()
            {
                if (FigureAccesses != null)
                    foreach (FigureAccess access in FigureAccesses)
                    {
                        access.Unsubscribe();
                    }

                FigureAccesses = GetAccessesFromFigures();
                if (Figures is INotifyCollectionChanged newObservable)
                {
                    SubscribeToFigureList(newObservable);
                }
                var OpeningAnimation = new Animation((v) => { OpenedProportion = (float)v; Redraw(); }, OpenedProportion, 1, Easing.CubicInOut);
                OpeningAnimation.Commit(this, OpeningAnimationHandle, _animationFramerate, (uint)(2000 * (1f - OpenedProportion)));
            }
        }

        private void SubscribeToFigureList(INotifyCollectionChanged newObservable)
        {
            newObservable.CollectionChanged += OnFigureListEntriesChanged;
        }

        private void UnSubscribeToFigureList(INotifyCollectionChanged observable)
        {
            observable.CollectionChanged -= OnFigureListEntriesChanged;
        }

        private void OnFigureValueChanged(FigureAccess access)
        {
            var valueAnimation = new Animation((v) => { access.ValueDeltaProportion = (float)v; Redraw(); }, access.ValueDeltaProportion, 1, Easing.CubicInOut);
            valueAnimation.Commit(this, access.ValueAnimationHandle, _animationFramerate, 500u);
        }

        private void OnFigureListEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var o in e.NewItems)
                    {
                        FigureAccess access = new FigureAccess(o, OnFigureValueChanged);
                        FigureAccesses.Add(access);
                        var entryAnimation = new Animation((v) => { access.Entrance = (float)v; Redraw(); }, 0, 1, Easing.CubicInOut);
                        entryAnimation.Commit(this, access.EntranceHandle, _animationFramerate, 500u);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var o in e.OldItems)
                    {
                        var access = FigureAccesses[o];
                        if (access == null) continue;
                        if (this.AnimationIsRunning(access.EntranceHandle))
                        {
                            this.AbortAnimation(access.EntranceHandle);
                        }
                        access.Unsubscribe();
                        var leaveAnimation = new Animation((v) => { access.Entrance = (float)v; Redraw(); }, access.Entrance, 0, Easing.CubicInOut);
                        leaveAnimation.Commit(this, access.EntranceHandle, _animationFramerate, 500u, finished: (v, c) => FigureAccesses.Remove(o));

                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        private KeyedCollection<object, FigureAccess> GetAccessesFromFigures()
        {
            if (Figures == null) return null;
            var output = new FigureAccessCollection();

            foreach (object figure in Figures)
            {
                output.Add(new FigureAccess(figure, OnFigureValueChanged) { Entrance = 1f });
            }
            return output;
        }

        public ICollection Figures
        {
            get { return (ICollection)GetValue(FiguresProperty); }
            set { SetValue(FiguresProperty, value); }
        }

        internal KeyedCollection<object, FigureAccess> FigureAccesses { get; set; }

        public Chart()
        {
            var visualElementRepresentation = this as VisualElement;
            visualElementRepresentation.BackgroundColor = Color.Transparent;
            PaintSurface += OnPaintSurface;

            var OpeningAnimation = new Animation((v) => { OpenedProportion = (float)v; Redraw(); }, OpenedProportion, 1, Easing.CubicInOut);
            OpeningAnimation.Commit(this, OpeningAnimationHandle, _animationFramerate, 2000u);
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            

            e.Surface.Canvas.Clear(BackgroundColor.ToSKColor());
            DrawSpecific(e.Surface.Canvas, e.Info.Width, e.Info.Height);

            e.Surface.Canvas.ResetMatrix();
            DrawValueLabels(e.Surface.Canvas, e.Info.Width, e.Info.Height);

        }



        protected abstract void DrawSpecific(SKCanvas canvas, int width, int height);


        protected void DrawValueLabels(SKCanvas canvas, int width, int height)
        {
            if (FigureAccesses == null || FigureAccesses.Count == 0) return;

            using (var describtionPaint = new SKPaint()
            {
                TextSize = 20f,
                IsAntialias = IsAntiAliased,
                Color = SKColors.Black,
                IsStroke = false
            })
            {
                //Calculating the maximum line height, that is then used for every line.
                float maxDescribtionHeight = 0f;
                foreach (FigureAccess fig in FigureAccesses)
                {
                    if (!string.IsNullOrWhiteSpace(fig.Describtion))
                    {
                        var describtionBounds = new SKRect();
                        describtionPaint.MeasureText(fig.Describtion, ref describtionBounds);
                        maxDescribtionHeight = Math.Max(maxDescribtionHeight, describtionBounds.Height);
                    }
                }

                DrawDescribtionBackGround(canvas, width, height, maxDescribtionHeight, FigureAccesses.Count);



                for (int i = 0; i < this.FigureAccesses.Count(); i++)
                {
                    FigureAccess figure = FigureAccesses.ElementAt(i);
                    if (!string.IsNullOrEmpty(figure.Describtion))
                    {

                        var describtionBounds = new SKRect();
                        string text = figure.Describtion;
                        describtionPaint.MeasureText(text, ref describtionBounds);
                        maxDescribtionHeight = Math.Max(maxDescribtionHeight, describtionBounds.Height);


                        float xOffset = 0;
                        float yOffset = 0;
                        float xStep = 0;
                        float yStep = 0;
                        int adjustedIndex = i;

                        switch (DescribtionPosition)
                        {
                            case DescribtionArea.Default:
                            case DescribtionArea.LeftAndRight:
                                bool halfDone = i < (this.FigureAccesses.Count() + 1) / 2;
                                adjustedIndex = halfDone ? i : i - this.FigureAccesses.Count() % 2 - this.FigureAccesses.Count() / 2;
                                xOffset = halfDone ? width - Padding - DescribtionSpace + DescribtionPadding : +Padding + DescribtionPadding;
                                yOffset = halfDone ? Padding + DescribtionPadding : +Padding + DescribtionPadding + ((height - 2 * Padding) - Math.Min((this.FigureAccesses.Count() / 2) * maxDescribtionHeight * DescribtionSpacing + DescribtionPadding, height - 2 * Padding));
                                xStep = 0;
                                yStep = maxDescribtionHeight * DescribtionSpacing;
                                break;
                            case DescribtionArea.Right:
                                xOffset = width - Padding - DescribtionSpace + DescribtionPadding;
                                yOffset = Padding + DescribtionPadding;
                                xStep = 0;
                                yStep = maxDescribtionHeight * DescribtionSpacing;
                                break;
                            case DescribtionArea.Left:
                                xOffset = +Padding + DescribtionPadding;
                                yOffset = +Padding + DescribtionPadding;
                                xStep = 0;
                                yStep = maxDescribtionHeight * DescribtionSpacing;
                                break;

                            case DescribtionArea.Top:
                                break;
                            case DescribtionArea.Bottom:
                                break;
                            default:
                                break;
                        }

                        SKRect colorRect = SKRect.Create(
                            x: xOffset + xStep * adjustedIndex,
                            y: yOffset + yStep * adjustedIndex,
                            width: maxDescribtionHeight,
                            height: maxDescribtionHeight
                            );

                        using (var paint = new SKPaint()
                        {
                            IsAntialias = IsAntiAliased,
                            Color = figure.Color,
                            Style = SKPaintStyle.Fill
                        }) canvas.DrawRect(colorRect, paint);

                        canvas.DrawText(text,
                            x: colorRect.Right + 3f,
                            y: colorRect.Bottom,
                            paint: describtionPaint);

                        float valToShow = AnimateVisibleValues ? figure.AnimatedValue : figure.Value;
                        string valText = Math.Round(valToShow, FractionalDigits).ToString("n" + FractionalDigits);

                        canvas.DrawText(
                            text: valText,
                            x: colorRect.Right + describtionBounds.Width + 9f,
                            y: colorRect.Bottom,
                            paint: describtionPaint);

                    }
                }
            }
        }
        private void DrawDescribtionBackGround(SKCanvas canvas, float width, float height, float maxDescribtionHeight, int figureAmount)
        {
            using (var describtionBackGroundPaint = new SKPaint()
            {
                Color = SKColors.DarkGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 4f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 10f, 10f }, 0)
            })
                switch (DescribtionPosition)
                {
                    case DescribtionArea.Default:
                        break;
                    case DescribtionArea.LeftAndRight:

                        int rightVals = (figureAmount + 1) / 2;
                        int leftVals = figureAmount / 2;

                        float rightHeight = Math.Min(rightVals * maxDescribtionHeight * DescribtionSpacing + DescribtionPadding, height - 2 * Padding);
                        float leftHeight = Math.Min(leftVals * maxDescribtionHeight * DescribtionSpacing + DescribtionPadding, height - 2 * Padding);

                        float leftYOffset = (height - 2 * Padding) - leftHeight;

                        canvas.DrawRoundRect(
                            x: width - Padding - DescribtionSpace,
                            y: Padding,
                            w: DescribtionSpace,
                            h: rightHeight,
                            rx: 3, ry: 3,
                            paint: describtionBackGroundPaint);

                        canvas.DrawRoundRect(
                            x: Padding,
                            y: Padding + leftYOffset,
                            w: DescribtionSpace,
                            h: leftHeight,
                            rx: 3, ry: 3,
                            paint: describtionBackGroundPaint);
                        break;
                    case DescribtionArea.Right:
                        canvas.DrawRoundRect(
                            x: width - Padding - DescribtionSpace,
                            y: Padding,
                            w: DescribtionSpace,
                            h: Math.Min(figureAmount * maxDescribtionHeight * DescribtionSpacing + DescribtionPadding, height - 2 * Padding),
                            rx: 3, ry: 3,
                            paint: describtionBackGroundPaint);
                        break;
                    case DescribtionArea.Left:
                        canvas.DrawRoundRect(
                            x: Padding,
                            y: Padding,
                            w: DescribtionSpace,
                            h: Math.Min(figureAmount * maxDescribtionHeight * DescribtionSpacing + DescribtionPadding, height - 2 * Padding),
                            rx: 3, ry: 3,
                            paint: describtionBackGroundPaint);
                        break;
                    case DescribtionArea.Top:
                        break;
                    case DescribtionArea.Bottom:
                        break;
                    default:
                        break;
                }
        }
    }
}
