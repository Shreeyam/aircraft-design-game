using Assets.Scripts.Extensions;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    /// <summary>
    /// Deprecated class - using geometric nose instead
    /// </summary>
    public class Nose : Geometry, IExtruded
    {
        public List<CrossSection> CrossSections { get; set; }

        public float Diameter { get; set; }
        public float NoseLengthDiameterRatio { get; set; }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        public override float CalculateArea()
        {
            return Areas.Cone(Diameter/10, Diameter/10 * NoseLengthDiameterRatio);
        }

        public void UpdateSections()
        {
            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = Vector3.back,
                    Distance = 0,
                    Offset = new Vector3(0, -0.30f                                                                                           * Diameter, 0),
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Vector3.zero, 0, 24)
                },

                new CrossSection
                {
                    Direction = Vector3.back,
                    Distance = Diameter * NoseLengthDiameterRatio,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Vector3.zero, Diameter/2, 24)
                }
            };
        }
    }
}
