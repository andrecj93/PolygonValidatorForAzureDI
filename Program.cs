using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace PolygonValidator
{
    internal static class Program
    {
        private const string testFile = @"C:\Azure DI Test Files\5.pdf.labels.json";

        static void Main()
        {
            string filePath = testFile;
            string json = File.ReadAllText(filePath);
            var document = JsonConvert.DeserializeObject<JsonDocument>(json);

            foreach (var label in document.Labels)
            {
                foreach (var value in label.Value)
                {
                    foreach (var box in value.BoundingBoxes)
                    {
                        var points = PolygonValidator.ConvertToPoints(box);

                        var sortedPoints = PolygonValidator.SortPointsForRectangle(points);
                        if (!PolygonValidator.IsConvex(sortedPoints) || !PolygonValidator.IsClockwise(sortedPoints))
                        {
                            Console.WriteLine();
                            Console.WriteLine($"Label: {label.LabelName}");
                            Console.WriteLine("Not Convex or Clockwise");
                            Console.WriteLine($"Points: {string.Join(" || ", sortedPoints.Select(b => $"x={b.X}; y={b.Y}"))}");
                            Console.WriteLine();
                        }
                    }
                }
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
