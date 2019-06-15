using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleXart.Util
{
    internal static class CircleMath
    {

        private const float _startRad = -(float)Math.PI / 2;
        private const float _fullRad = 2f * (float)Math.PI;
        private static SKPoint PolarCoordinates(float radius, float rad) => new SKPoint(radius * (float)Math.Cos(rad), radius * (float)Math.Sin(rad));

        /// <summary>
        /// Creates Circle
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>SKPath in the shape of a circle.</returns>
        internal static SKPath GetCirclePath(float radius)
        {
            var path = new SKPath()
            {
                FillType = SKPathFillType.EvenOdd,
                Convexity = SKPathConvexity.Convex
            };

            path.AddCircle(0, 0, radius);
            return path;
        }

        /// <summary>
        /// Creates a polygon, whichs corners trace a circle.
        /// </summary>
        /// <param name="radius">The distance of the corners to the center.</param>
        /// <param name="steps">How many corners are traced. Minumum 3.</param>
        /// <returns></returns>
        internal static SKPath GetPolygonPath(float radius, int steps)
        {
            var path = new SKPath()
            {
                FillType = SKPathFillType.EvenOdd,
                Convexity = SKPathConvexity.Convex
            };

            //Cant have less than 3 steps
            steps = Math.Max(3,steps);

            //Move to the starting points and then go along the corners.
            path.MoveTo(PolarCoordinates(radius, _startRad));
            for (int i = 0; i < steps; i++)
            {
                path.LineTo(PolarCoordinates(radius, _startRad + _fullRad * i / steps));
            }
            path.Close();
            return path;
        }

        /// <summary>
        /// Creates a pie.
        /// </summary>
        /// <param name="radius">The radius of the pie.</param>
        /// <param name="fromRad"> The angle in rad towards the starting position.</param>
        /// <param name="toRad">The angle in rad towards the ending position.</param>
        /// <returns>SKPath in the shape of a piece of pie.</returns>
        internal static SKPath GetPiePath(float radius, float fromRad, float toRad)
        {
            //Without this shortcut full circles do not get drawn
            if (toRad - fromRad == _fullRad)
            {
                return GetCirclePath(radius);
            }


            bool convex = toRad - fromRad <= Math.PI;
            var path = new SKPath()
            {
                FillType = SKPathFillType.EvenOdd,
                Convexity = (convex ? SKPathConvexity.Convex : SKPathConvexity.Concave)
            };

            SKPoint endPoint = PolarCoordinates(radius, _startRad + toRad);

            //path.MoveTo(0f, 0f);
            path.LineTo(PolarCoordinates(radius, _startRad + fromRad));
            path.ArcTo
                (
                rx: radius,
                ry: radius,
                xAxisRotate: 0f,
                largeArc: (convex ? SKPathArcSize.Small : SKPathArcSize.Large),
                sweep: SKPathDirection.Clockwise,
                x: endPoint.X,
                y: endPoint.Y
                );
            path.Close();

            return path;
        }
    }
}
