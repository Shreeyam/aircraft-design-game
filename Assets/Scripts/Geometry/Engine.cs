using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class Engine : Geometry, IExtruded
    {
        public List<CrossSection> CrossSections { get; set; }

        public Vector3 Root { get; set; }
        public float BypassRatio { get; set; }
        public float DesignThrust { get; set; } // kN
        public float Diameter
        {
            get
            {
                return 1.5f * scale;
            }
        }

        public float Length
        {
            get
            {
                return CrossSections.Last().Distance / 10;
            }
        }

        public float Area
        {
            get
            {
                return Mathf.PI * Mathf.Pow(Diameter / 2, 2) * Length;
            }
        }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        private float scale
        {
            get { return (1.2f / 36f) * Mathf.Sqrt(DesignThrust) * Mathf.Exp(0.04f * BypassRatio); }
        }

        public void UpdateSections(float fuselageDiameter)
        {
            var lengthScale = (0.22f) * (0.49f * Mathf.Pow(DesignThrust, 0.4f) * 0.9f);
            var offset = new Vector3(0, -18 * scale - (0.4f * fuselageDiameter), 0);

            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Distance = 0,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 0, 24)
                },
                new CrossSection
                {
                    Distance = 6 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 4, 24)
                },
                new CrossSection
                {
                    Distance = 6 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 14 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 0 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 14 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 0 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 15 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 20 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 18 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 40 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 18 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 60 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 14 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 50 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 12 * scale, 24)
                },
                new CrossSection
                {
                    Distance = 73 * lengthScale,
                    Direction = Vector3.back,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Root + offset, 0, 24)
                }
            };
        }

        public float GetXPosition()
        {
            var scale = (1.5f / 36f) * Mathf.Sqrt(DesignThrust) * Mathf.Exp(0.04f * BypassRatio);
            var lengthScale = (0.25f) * (0.49f * Mathf.Pow(DesignThrust, 0.4f) * 0.9f);
            var offset = new Vector3(0, -18 * scale - 26f, 0);

            return -Root.z + (73 * lengthScale);
        }
    }
}
