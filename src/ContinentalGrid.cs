using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    public struct Line
    {
        public double Length { get; set; }
        public double Angle { get; set; }

        public Line(double len, double angle)
        { this.Length = len; this.Angle = angle; }

    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        { this.X = x; this.Y = y; }

        public static Point operator +(Point startPt, Line line)
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
    }

    public class ContinentalGrid
    {
        public static double metersPerDegreeLat = 111000.0;

        public double CellSize { get; set; }
        public double CenterLatitude {get; set;}
        public double StudyAreaWidthMeters { get; set; }
        public double StudyAreaHeightMeters { get; set; }

        public Point CenterPoint { get; private set; }
        public Point CoastNearPoint { get; private set; }

        public double b_coastLine { get; private set; }
        public double b_coastLineLatitude { get; private set; }

        public ContinentalGrid(double centerLatitude, double cellSize, 
            double studyAreaWidthInCells, double studyAreaheightInCells, 
            double centerPtDistanceInland_kilometers)
        {
            this.CenterLatitude = centerLatitude;
            this.CellSize = cellSize;
            this.StudyAreaWidthMeters = studyAreaWidthInCells * cellSize;
            this.StudyAreaHeightMeters = studyAreaheightInCells * cellSize;
            this.CenterPoint = 
                new Point(this.StudyAreaWidthMeters / 2.0, this.StudyAreaHeightMeters / 2.0);
            this.CoastNearPoint = 
                this.CenterPoint + new Line(1000.0 * centerPtDistanceInland_kilometers, 135.0);

            this.b_coastLine = -this.CoastNearPoint.X;
            this.b_coastLineLatitude =
                centerLatitude - (this.CenterPoint.Y - this.b_coastLine) /
                                        metersPerDegreeLat;
        }
    }
}
