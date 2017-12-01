using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class Afterbody : Geometry, IExtruded
    {
        public float Diameter { get; set; }
        public float AfterbodyLengthDiameterRatio { get; set; }

        public List<CrossSection> CrossSections { get;set;}
        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        public void UpdateSections()
        {
            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = Vector3.back,
                    Distance = 0,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Vector3.zero, Diameter/2, 24)
                },
                new CrossSection
                {
                    Direction = Vector3.back,
                    Distance = AfterbodyLengthDiameterRatio * Diameter,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Vector3.zero, 0, 24),
                    Offset = new Vector3(0, 0.40f * Diameter, 0)
                }
            };
        }
    }
}
