using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public abstract class Geometry
    {
        public virtual float CalculateArea()
        {
            return 0f;
        }

        public float Weight { get; set; }

        // TODO: Consider an alternative data structure for fast inserts. Maybe a LinkedList?
        public virtual IEnumerable<CrossSection> GetCrossSections()
        {
            return null;
        }

        // TODO: Think about whether to split this into a separate class
        // TODO: Actually put it in a separate class (or interface?)
        //public void AddSection(CrossSection section)
        //{
        //    crossSections.Add(section);

        //    crossSections = crossSections
        //        .OrderBy(x => x.Distance)
        //        .ToList();
        //}

        /// <summary>
        /// Stitches two cross sections together. Assumes they have the same number of points
        /// </summary>
        /// <param name="mesh">Mesh object to modify</param>
        /// <param name="capEnds">Determines if the ends are capped</param>
        /// <param name="interp">Bezier interpolation between points</param>
        public virtual void Generate(Mesh mesh, Vector2 uv, bool capEnds = false, int interp = 1)
        {
            var points = GetCrossSections().First().Points.Length;

            // Assumes all cross sections have the same number of points (as they should!)
            var vertices = GetCrossSections()
                .SelectMany(x =>
                    x.Points.Select(y => y + (x.Distance * x.Direction) + x.Offset))
                .ToArray();

            // Number of definitions = (number of cross sections - 1) * (quads per section) * (tris per quad) * (vertices per tri) 
            var triangles = new int[(GetCrossSections().Count() - 1) * 2 * 3 * points];

            // Loop through and connect all cross sections to the next one
            for (var i = 0; i < GetCrossSections().Count() - 1; i++)
            {
                for (var j = 0; j < points; j += 1)
                {
                    triangles[(6 * points * i) + 6 * j] = (mesh.vertexCount + (j + points) % (2 * points)) + i * points;
                    triangles[(6 * points * i) + (6 * j + 1)] = (mesh.vertexCount + (j + 1) % (2 * points)) + i * points;
                    triangles[(6 * points * i) + (6 * j + 2)] = (mesh.vertexCount + j % (2 * points)) + i * points;

                    if (j != points - 1)
                    {
                        triangles[(6 * points * i) + (6 * j + 3)] = (mesh.vertexCount + (j + 1) % (2 * points)) + i * points;
                        triangles[(6 * points * i) + (6 * j + 4)] = (mesh.vertexCount + (j + points) % (2 * points)) + i * points;
                        triangles[(6 * points * i) + (6 * j + 5)] = (mesh.vertexCount + (j + points + 1) % (2 * points)) + i * points;
                    }

                    // Do a last iteration to work around Unity's clockwise triangle definitions
                    else
                    {
                        triangles[(6 * points * i) + (6 * j + 3)] = (mesh.vertexCount + (j) % (2 * points)) + i * points;
                        triangles[(6 * points * i) + (6 * j + 4)] = (mesh.vertexCount + (j + 1) % (2 * points)) + i * points;
                        triangles[(6 * points * i) + (6 * j + 5)] = (mesh.vertexCount + (j + points + 1) % (2 * points)) + i * points;
                    }

                }
            }

            //// TODO: Actually cap the ends lol
            //int capTriangleCount = (GetCrossSections().Count() - 1);
            //int[] capTriangles = new int[capTriangleCount * 3 * 2];

            //if (capEnds)
            //{
            //    for (int i = 0; i < capTriangleCount; i++)
            //    {
            //        capTriangles[i + 2] = mesh.vertexCount + 0;
            //        capTriangles[i + 1] = mesh.vertexCount + i + 1;
            //        capTriangles[i] = mesh.vertexCount + i + 2;

            //        //capTriangles[capTriangleCount * 6 - i] = mesh.vertexCount + capTriangleCount - ;
            //    }
            //}
            //if(capEnds)
            //{
            //    mesh.triangles = mesh.triangles.Concat(capTriangles).ToArray();
            //}

            var totalUvs = new List<Vector2>();
            mesh.GetUVs(0, totalUvs);

            mesh.vertices = mesh.vertices
                .Concat(vertices)
                .ToArray();

            mesh.triangles = mesh.triangles
                .Concat(triangles)
                .ToArray();

            var count = vertices.Count();
            var uvs = Enumerable.Repeat(uv, vertices.Count() ).ToArray();



            var newuvs = totalUvs.Concat(uvs).ToArray();

            mesh.uv = newuvs;
        }
    }
}
