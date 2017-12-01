using System;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Extensions;
using System.IO;
using System.Collections.Generic;

namespace Assets.Scripts.Geometry
{
    public static class Primitives
    {
        public static Vector3[] Circle(Vector3 normal, Vector3 horizontal, Vector3 pos, float radius, int points)
        {
            normal.Normalize();
            horizontal.Normalize();

            var vertical = Vector3.Cross(normal, horizontal);
            var circle = new Vector3[points];

            for (var i = 0; i < points; i++)
            {
                circle[i] = pos + radius * ((float)Math.Cos(i * 2 * Math.PI / points) * vertical + (float)Math.Sin(i * 2 * Math.PI / points) * horizontal);
            }

            return circle;
        }

        public static Vector3[] NACA(Vector3 normal, Vector3 horizontal, Vector3 pos, float m, float p, float xx, float c, int n)
        {
            normal.Normalize();
            horizontal.Normalize();

            var vertical = Vector3.Cross(normal, horizontal);

            //m /= 100;
            //p /= 10;


            //var x_top = LinqExtensions.Linspace(0, p * c, n/2).ToList();
            //var x_bottom = LinqExtensions.Linspace(p*c, 1, n / 2)
            //    .Reverse()
            //    .ToList();

            //var y_top = x_top
            //    .Select(z => (m / (Math.Pow(p, 2))) * (2 * p * (z/c) - Math.Pow((z/c), 2)))
            //    .Select(Convert.ToSingle);

            //var y_bottom = x_top
            //    .Select(z => (m / (Math.Pow(1 - p, 2))) * ((1 - 2 * p) + (2 * p * (z / c) - Math.Pow((z / c), 2))))
            //    .Select(Convert.ToSingle);

            //var y = y_top.Concat(y_bottom);
            //var x = x_top.Concat(x_bottom);

            //var points = x.Zip(y, (u, v) => u * horizontal + v * vertical * (c * xx)).ToArray();

            //return points;

            // No one needs to know
            string nacastring;

            if (m == 0 && p == 0)
            {
                nacastring = "00";
            }
            else
            {
                nacastring = "24";
            }


            var data = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Assets\\Config\\naca" + nacastring + "12.dat");

            var x = data
                .Select(z => Convert.ToSingle(z.Split(' ').First()));

            var y = data
                .Select(z => Convert.ToSingle(z.Split(' ').Last()));

            // Don't mind me normalizing this data...
            xx /= 0.12f;

            var points = x.Zip(y, (u, v) => u * c * horizontal + v * c * xx * vertical)
                .Select(z => z + pos).ToArray();

            return points;
        }

        public static List<CrossSection> Cylinder(Vector3 normal, Vector3 horizontal, Vector3 pos, float radius, float height, int points)
        {
            normal.Normalize();
            horizontal.Normalize();

            var vertical = Vector3.Cross(normal, horizontal);

            return new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = vertical,
                    Distance = 0,
                    Points = Circle(normal, horizontal, pos, radius, points)
                },
                new CrossSection
                {
                    Direction = vertical,
                    Distance = height,
                    Points = Circle(normal, horizontal, pos, radius, points)
                }
            };
        }
    }
}
