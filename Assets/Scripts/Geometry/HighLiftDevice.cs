using Assets.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class HighLiftDevice : Geometry
    {
        public List<CrossSection> CrossSections { get; set; }
        public HighLiftDeviceType HighLiftDeviceType { get; set; }
        public float ExtensionRatio { get; set; }
        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        public HighLiftDevice()
        {
            ExtensionRatio = Constants.Defaults.Wing.FlapExtensionRatio;
        }

        public void UpdateSections(float span, float dihedral, Vector3 offset, float sweep, float tc, float taperRatio, float rootChord)
        {
            span *= Constants.Defaults.Wing.FlapReach;
            sweep = sweep + 2.8f;

            offset += new Vector3(0, -2f, 0);

            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = span/2,
                    Points = GeneratePoints(Vector3.right, Vector3.back,  offset + new Vector3(0, span * dihedral, -span * Mathf.Tan(Mathf.Deg2Rad * sweep)), tc, rootChord * taperRatio)
                },
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = 0,
                    Points = GeneratePoints(Vector3.right, Vector3.back,  offset, tc, rootChord)
                },
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = -span/2,
                    Points = GeneratePoints(Vector3.right, Vector3.back,  offset + new Vector3(0, span * dihedral , -span * Mathf.Tan(Mathf.Deg2Rad * sweep)), tc, rootChord * taperRatio)
                }
            };
        }


        public Vector3[] GeneratePoints(Vector3 normal, Vector3 horizontal, Vector3 pos,  float xx, float c)
        {
            normal.Normalize();
            horizontal.Normalize();

            var vertical = Vector3.Cross(normal, horizontal);

            var data = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Assets\\Config\\" + HighLiftDeviceType.ToString() + "Flaps.dat");

            var x = data
                .Select(z => Convert.ToSingle(z.Split(' ').First()));

            var y = data
                .Select(z => Convert.ToSingle(z.Split(' ').Last()));

            var points = x.Zip(y, (u, v) => u * c * horizontal + v * c * xx * vertical)
                .Select(z => z + pos).ToArray();

            return points;
        }
    }

    public enum HighLiftDeviceType
    {
        Slats,
        Plain,
        Single,
        Double,
        Triple
    }
}


