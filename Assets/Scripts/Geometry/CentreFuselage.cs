using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class CentreFuselage : Geometry, IExtruded
    {
        public List<CrossSection> CrossSections { get; set; }
        
        public float Diameter { get; set; }
        public float Length { get; set; }

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
                    Distance = Length,
                    Points = Primitives.Circle(Vector3.back, Vector3.right, Vector3.zero, Diameter/2, 24)
                }
            };
        }
    }
}
