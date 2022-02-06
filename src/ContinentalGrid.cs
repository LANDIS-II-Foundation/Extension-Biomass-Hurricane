using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    /// <summary>
    /// Represents a line of the form: y = mx + b
    /// Where m is the slope and b is the y-intercept.
    /// </summary>
    public struct Line
    {
        public double Slope { get; set; }
        public double yIntercept { get; set; }

        public Line(Point pt, double slope)
        {
            this.Slope = slope;
            double deltaY = -this.Slope * pt.X;
            this.yIntercept = pt.Y + deltaY;
        }

        public Point GivenXGetPoint(double x)
        {
            return new Point(x, (this.Slope * x) + this.yIntercept);
        }

        public Point GivenYGetPoint(double y)
        {
            double deltaY = this.yIntercept - y;
            return new Point(deltaY / -this.Slope, y);
        }

        public (double, double) GetDistanceAndOffset(Point pt)
        {
            double offset = Math.Abs(this.Slope*pt.X - 1.0 * pt.Y + this.yIntercept) /
                Math.Sqrt(Math.Pow(this.Slope, 2.0) + 1.0);

            double perpandicularSlope = -1.0 / this.Slope;
            Line perpandicularLine = new Line(pt, perpandicularSlope);
            Point nearPoint = this.GetIntersectionPoint(perpandicularLine);
            double dx = nearPoint.X;
            double dy = nearPoint.Y - this.yIntercept;
            double distanceFromYIntercept = Math.Sqrt(dx*dx + dy*dy);

            if(pt.X - dx < 0.0) offset *= -1.0;
            return (distanceFromYIntercept, offset);
        }

        internal Point GetIntersectionPoint(Line otherLine)
        {
            // From https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_the_equations_of_the_lines
            double x = (otherLine.yIntercept - this.yIntercept) / (this.Slope - otherLine.Slope);
            double y = this.GivenXGetPoint(x).Y;
            return new Point(x, y);
        }
    }

    public struct LineSegment
    {
        public double Length { get; set; }
        public double Angle { get; set; }

        public LineSegment(double len, double angle)
        { this.Length = len; this.Angle = angle; }

    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        { this.X = x; this.Y = y; }

        public override string ToString()
        {
            return $"X: {X:F2}, Y: {Y:F2}";
        }

        public static Point operator +(Point startPt, LineSegment line)
        {
            double deltaX = 0.0; double deltaY = 0.0;
            if(Math.Abs(line.Angle - Math.Truncate(line.Angle / 180.0)) < 0.0000001)
            {
                deltaX = 0.0;
                deltaY = line.Length * Math.Cos(line.Angle * Math.PI / 180.0);
            }
            else if(Math.Abs(line.Angle - Math.Truncate((line.Angle + 90.0) / 180.0)) < 0.0000001)
            {
                deltaX = line.Length * Math.Sin(line.Angle * Math.PI / 180.0);
                deltaY = 0.0;
            }
            else
            {
                deltaX = line.Length * Math.Sin(line.Angle * Math.PI / 180.0);
                deltaY = line.Length * Math.Cos(line.Angle * Math.PI / 180.0);
            }

            double newX = startPt.X + deltaX;
            double newY = startPt.Y + deltaY;
            return new Point(newX, newY);
        }

        public static LineSegment operator -(Point fromPt, Point toPoint)
        {
            double dx = fromPt.X - toPoint.X;
            double dy = fromPt.Y - toPoint.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double angle = Math.Atan2(dy, dx);
            return new LineSegment(length, angle);
        }

        public override int GetHashCode()
        {
            return (int)(10.0 * (this.X + this.Y));
        }

        public override bool Equals(object obj)
        {
            return this == (Point) obj;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return (Math.Abs(p2.X - p1.X) < 0.001 &&
                Math.Abs(p2.Y - p1.Y) < 0.001);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public static Point siteIndexToCoordinates(int column, int row)
        {
            int upRow = PlugIn.ModelCore.Landscape.Dimensions.Rows - row;
            return new Point((double)column * PlugIn.ModelCore.CellLength, (double)upRow * PlugIn.ModelCore.CellLength);
        }

        public static IList<int> coordinatesToSiteIndex(Point pt)
        {
            double column = (pt.X / PlugIn.ModelCore.CellLength) + 0.5;
            double row = PlugIn.ModelCore.Landscape.Dimensions.Rows - (pt.Y / PlugIn.ModelCore.CellLength) - 1;
            return new List<int>() { (int)column, (int)row };
        }

        //public double ConvertLatitudeToGridUnits(double lat)
        //{
        //    return (lat - this.GridOriginLatitude) * ContinentalGrid.metersPerDegreeLat;
        //}
    }

    /// <summary>
    /// ContinentalGrid is a class representing a cartesian plane with the origin
    /// set at the lower left point of the raster study area. 
    /// </summary>
    //public class ContinentalGrid
    //{
    //    public static double metersPerDegreeLat = 111000.0;

    //    public double CellSize { get; set; }
    //    public double GridOriginLatitude {get; set;}
    //    public int Columns { get; set; }
    //    public int Rows { get; set; }
    //    //public double StudyAreaWidthMeters { get; set; }
    //    //public double StudyAreaHeightMeters { get; set; }

    //    //public Point CenterPoint { get; private set; }
    //    //public Point CoastNearPoint { get; private set; }

    //    public Line CoastLine { get; private set; }

    //    public ContinentalGrid(double centerLatitude, double cellSize, 
    //        double studyAreaWidthInCells, double studyAreaHeightInCells, 
    //        double centerPtDistanceInland_kilometers)
    //    {
    //        this.CellSize = cellSize;
    //        this.Columns = (int) studyAreaWidthInCells;
    //        this.Rows = (int) studyAreaHeightInCells;
    //        //this.StudyAreaWidthMeters = studyAreaWidthInCells * cellSize;
    //        //this.StudyAreaHeightMeters = studyAreaHeightInCells * cellSize;
    //        //double studyAreaHeightDegreesLatitude = this.StudyAreaHeightMeters / ContinentalGrid.metersPerDegreeLat;
    //        //this.GridOriginLatitude = centerLatitude - studyAreaHeightDegreesLatitude / 2.0;
    //        //this.CenterPoint = 
    //        //    new Point(this.StudyAreaWidthMeters / 2.0, this.StudyAreaHeightMeters / 2.0);
    //        //this.CoastNearPoint = 
    //        //    this.CenterPoint + new LineSegment(1000.0 * centerPtDistanceInland_kilometers, 135.0);

    //        //this.CoastLine = new Line(this.CoastNearPoint, 1.0);


    //        // test coordinate conversion
    //        //var aPoint = this.siteIndexToCoordinates(20, 20);
    //        //var convertBack = this.coordinatesToSiteIndex(aPoint);
    //    }

    /// <summary>
    /// Assumes 0, 0 is at the top left corner and positive row is down.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pt"></param>
    /// <returns>IList where return[0] is x and return[1] is y.</returns>
    //}
}
