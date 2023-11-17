using System.Collections.Generic;
using Newtonsoft.Json;
using NetTopologySuite.Geometries;
using System.Linq;
using System;

namespace PolygonValidator
{
    public class LabelValue
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("boundingBoxes")]
        public List<List<double>> BoundingBoxes { get; set; }
    }

    public class Label
    {
        [JsonProperty("label")]
        public string LabelName { get; set; }

        [JsonProperty("value")]
        public List<LabelValue> Value { get; set; }
    }

    public class JsonDocument
    {
        [JsonProperty("document")]
        public string Document { get; set; }

        [JsonProperty("labels")]
        public List<Label> Labels { get; set; }
    }

    public class BoundingBoxPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public static class PolygonValidator
    {
        public static bool IsConvex(List<BoundingBoxPoint> points)
        {
            var geometryFactory = new GeometryFactory();

            // Ensure the points form a closed ring by adding the first point at the end if necessary
            if (points[0] != points[points.Count - 1])
            {
                points.Add(points[0]);
            }

            var coordinates = points.Select(p => new Coordinate(p.X, p.Y)).ToArray();

            // Create a polygon from the points
            var polygon = geometryFactory.CreatePolygon(coordinates);

            // Check if the polygon is empty or not valid
            if (polygon.IsEmpty || !polygon.IsValid)
            {
                // Handle this case appropriately, maybe return false or throw an exception
                return false;
            }

            // Get the convex hull of the polygon
            var convexHull = polygon.ConvexHull();

            // Compare the polygon with its convex hull
            return polygon.EqualsTopologically(convexHull);
        }


        public static bool IsClockwise(List<BoundingBoxPoint> points)
        {
            double sum = 0;
            for (int i = 0; i < points.Count; i++)
            {
                BoundingBoxPoint p1 = points[i];
                BoundingBoxPoint p2 = points[(i + 1) % points.Count]; // Ensures the last point connects to the first
                sum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            return sum < 0; // Clockwise if sum is negative
        }

        public static List<BoundingBoxPoint> ConvertToPoints(List<double> boundingBox)
        {
            var points = new List<BoundingBoxPoint>();
            for (int i = 0; i < boundingBox.Count; i += 2)
            {
                points.Add(new BoundingBoxPoint { X = boundingBox[i], Y = boundingBox[i + 1] });
            }
            return points;
        }

        public static List<BoundingBoxPoint> SortPointsForRectangle(List<BoundingBoxPoint> points)
        {
            // Ensure there are exactly 4 points for a rectangle
            if (points.Count != 4)
                throw new ArgumentException("Bounding box should have exactly 4 points.");

            // Sort points by Y, then X
            var sortedPoints = points.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();

            // Swap the last two points if necessary to ensure clockwise order
            if (IsTopLeft(sortedPoints[2], sortedPoints[3]))
            {
                (sortedPoints[3], sortedPoints[2]) = (sortedPoints[2], sortedPoints[3]);
            }

            return sortedPoints;
        }

        private static bool IsTopLeft(BoundingBoxPoint a, BoundingBoxPoint b)
        {
            return a.X < b.X || (a.X == b.X && a.Y < b.Y);
        }
    }
}
