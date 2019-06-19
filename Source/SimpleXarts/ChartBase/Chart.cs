using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SimpleXarts.Converters;
using System.Collections.Specialized;
using System.Collections;
using System;
using System.Collections.ObjectModel;
using SimpleXarts.ChartBase;
using System.Linq;

namespace SimpleXarts
{
    public abstract class Chart : SKCanvasView
    {
        //keeps track of the last redraw of the chart, to decline too frequent redraws.
        private DateTime lastRedraw = DateTime.Now;

        // the padding around describtions (towards the chart)
        internal const float DescribtionPadding = 12f;

        // padding inside of the describtions (towards other describtions)
        internal const float DescribtionSpacing = 2f;

        // the framerate of the animations
        protected const uint _animationFramerate = 16;

        // the minimal amount of ticks to pass before a redraw is possible
        protected const int _minTicksBeforeRedraw = 160_000;

        //Represents the opening animation progress of the chart. Goes from 0 (closed) to 1 (completely opened)
        protected float OpenedProportion = 0f;
        protected const string OpeningAnimationHandle = "OpeningHandle";

        // The collection of the wrapper class around the figures.
        // Used to access the values of the figures inside the chart.
        internal KeyedCollection<object, FigureAccess> FigureAccesses { get; set; }


        // the amount of fractional digits to be displayed for the value.
        // Example FractionalDigits=2 => 5.22
        public static readonly BindableProperty FractionalDigitsProperty
            = BindableProperty.Create("FractionalDigits", typeof(int), typeof(Chart), 0);
        public int FractionalDigits
        {
            get { return ((int)GetValue(FractionalDigitsProperty)); }
            set { SetValue(FractionalDigitsProperty, value); }
        }

        // if the changing of values should be animated in the describtion
        public static readonly BindableProperty AnimateVisibleValuesProperty
            = BindableProperty.Create("AnimateVisibleValues", typeof(bool), typeof(Chart), false);
        public bool AnimateVisibleValues
        {
            get { return ((bool)GetValue(AnimateVisibleValuesProperty)); }
            set { SetValue(AnimateVisibleValuesProperty, value); }
        }

        // The position of the describtions. Different Chart types can have different default positions.
        // For example, a donut charts defaults to LeftRight,
        // while a BarChart defaults to Bottom
        public static readonly BindableProperty DescribtionPositionProperty
        = BindableProperty.Create("DescribtionPosition", typeof(DescribtionArea), typeof(Chart), DescribtionArea.Default);
        public DescribtionArea DescribtionPosition
        {
            get { return ((DescribtionArea)GetValue(DescribtionPositionProperty)); }
            set { SetValue(DescribtionPositionProperty, value); }
        }

        // How much space is set aside for the describtions. If the DescribtionPosition is LeftRight
        // the full space is set for both sides.
        public static readonly BindableProperty DescribtionSpaceProperty
                = BindableProperty.Create("DescribtionSpace", typeof(float), typeof(Chart), 175f);
        public float DescribtionSpace
        {
            get { return ((float)GetValue(DescribtionSpaceProperty)); }
            set { SetValue(DescribtionSpaceProperty, value); }
        }

        // The padding of the whole element. 
        public static readonly BindableProperty PaddingProperty
            = BindableProperty.Create("Padding", typeof(float), typeof(Chart), 20f);
        public float Padding
        {
            get { return (float)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        // The rotation of the chart. Does not rotate the describtion
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
        //gets called when the rotation changes.
        private static void OnRotationChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((Chart)bindable).Redraw();
        }

        // The background color. Overwrites the backgroundcolor, so the padding property works.
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

        // sets if the canvas should be drawn with antialiasing.
        public static readonly BindableProperty IsAntiAliasedProperty = BindableProperty.Create("IsAntiAliased", typeof(bool), typeof(Chart), true);
        public bool IsAntiAliased
        {
            get { return (bool)GetValue(IsAntiAliasedProperty); }
            set { SetValue(IsAntiAliasedProperty, value); }
        }

        // Binds the chart entries/figures to the chart.
        public static readonly BindableProperty FiguresProperty
            = BindableProperty.Create("Figures", typeof(ICollection), typeof(Chart), null, propertyChanged: OnFiguresChanged);
        public ICollection Figures
        {
            get { return (ICollection)GetValue(FiguresProperty); }
            set { SetValue(FiguresProperty, value); }
        }

        // Gets called when the whole ICollection of figures changes. Only gets called, if the user notifies of that change.
        // Needs to get the instance of this chart, which then gets called.
        private static void OnFiguresChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var instance = ((Chart)bindable);
            instance.OnFiguresChangedInstance(((ICollection)oldValue));
        }

        public Chart()
        {
            var visualElementRepresentation = this as VisualElement;
            visualElementRepresentation.BackgroundColor = Color.Transparent;
            PaintSurface += OnPaintSurface;

            var OpeningAnimation = new Animation((v) => { OpenedProportion = (float)v; Redraw(); }, OpenedProportion, 1, Easing.CubicInOut);
            OpeningAnimation.Commit(this, OpeningAnimationHandle, _animationFramerate, 2000u);
        }

        /// <summary>
        /// Needs to be called when the whole collection of figures changes.
        /// Unsubscribes from old figures, subscribes to new figures.
        /// Closes and opens the chart.
        /// </summary>
        /// <param name="oldValue">The old figures, to unsubscribe from</param>
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

            Redraw();

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

        /// <summary>
        /// Redraws the chart.
        /// </summary>
        /// <param name="force">If forced, will ignore the _minTicksBeforeRedraw value.</param>
        public void Redraw(bool force = false)
        {
            if (lastRedraw.Ticks + 400_000 < DateTime.Now.Ticks || force)
            {
                InvalidateSurface();
                lastRedraw = DateTime.Now;
            }
        }

        /// <summary>
        /// Subscribes to a new list of figures.
        /// This works as long as the Figures collection is a INotifyCOllectionChanged instance.
        /// Makes sure the chart will be notified when figures get added/removed from the collection.
        /// Call this with the new collection, when the Figure collection BindableProperty is set or replaced with a new collection.
        /// </summary>
        /// <param name="newObservable"> The new collection of figures</param>
        private void SubscribeToFigureList(INotifyCollectionChanged newObservable)
        {
            newObservable.CollectionChanged += OnFigureListEntriesChanged;
        }

        /// <summary>
        /// Unsubscribes to a list of figures.
        /// Call this with the old collection, when the Figures collection is replaced completely by a new collection.
        /// </summary>
        /// <param name="oldObservable"> The old collection of figures.</param>
        private void UnSubscribeToFigureList(INotifyCollectionChanged oldObservable)
        {
            oldObservable.CollectionChanged -= OnFigureListEntriesChanged;
        }


        /// <summary>
        /// Gets called when the value of a figure changes.
        /// Animates the change of the value, visible through the chart.
        /// </summary>
        /// <param name="access">The figure access object of the figure whichs value changed.</param>
        private void OnFigureValueChanged(FigureAccess access)
        {
            var valueAnimation = new Animation((v) => { access.ValueDeltaProportion = (float)v; Redraw(); }, access.ValueDeltaProportion, 1, Easing.CubicInOut);
            valueAnimation.Commit(this, access.ValueAnimationHandle, _animationFramerate, 500u);
        }

        /// <summary>
        /// Gets called whenever figures get added/removed from the figure collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFigureListEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var o in e.NewItems)
                    {
                        FigureAccess access = new FigureAccess(o, OnFigureValueChanged, easeInValue: true);
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
                        leaveAnimation.Commit(this, access.EntranceHandle, _animationFramerate, 500u, finished: (v, c) => { FigureAccesses.Remove(o); Redraw(force: true); }) ;

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

        // Todo: as method of the FigureAccessCollection
        private KeyedCollection<object, FigureAccess> GetAccessesFromFigures()
        {
            if (Figures == null) return null;
            var output = new FigureAccessCollection();

            foreach (object figure in Figures)
            {
                output.Add(new FigureAccess(figure, OnFigureValueChanged,easeInValue: false));
            }
            return output;
        }

        /// <summary>
        /// Paints the chart and the describtions on the screen.
        /// </summary>
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(BackgroundColor.ToSKColor());
            DrawSpecific(e.Surface.Canvas, e.Info.Width, e.Info.Height);

            e.Surface.Canvas.ResetMatrix();
            DrawValueLabels(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        /// <summary>
        /// The chart specific implementation of the charts drawing.
        /// </summary>
        protected abstract void DrawSpecific(SKCanvas canvas, int width, int height);

        /// <summary>
        /// Draw the describtions. So far, this is chart unspecific.
        /// </summary>
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

        /// <summary>
        /// Draw the describtion background. So far, this is chart unspecific.
        /// </summary>
        /// <param name="maxDescribtionHeight">Maximal height of the describtions. </param>
        /// <param name="figureAmount">The figure count</param>
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
