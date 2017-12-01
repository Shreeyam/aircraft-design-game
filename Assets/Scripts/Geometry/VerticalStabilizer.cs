using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class VerticalStabilizer : Geometry
    {
        public List<CrossSection> CrossSections { get; set; }

        public float Height
        {
            get
            {
                return Mathf.Sqrt(Area * AspectRatio);
            }
        }
        public float RootChord
        {
            get
            {
                return (2 * Area) / (0.5f * Height * (TaperRatio + 1));
            }
        }
        public float FuselageDiameter { get; set; }
        public float Area { get; set; }
        public float CentreFuselageLength { get; set; }
        public float AfterbodyLengthDiameterRatio { get; set; }
        public float Sweep { get; set; }
        public float AspectRatio { get; set; }

        public float ThicknessChordRatio { get { return 0.12f; } }
        public const float TaperRatio = 0.35f;

        public VerticalStabilizer()
        {
            CentreFuselageLength = Constants.Defaults.Fuselage.CentreFuselageLength;
            AfterbodyLengthDiameterRatio = Constants.Defaults.Fuselage.AfterbodyLengthDiameterRatio;
            FuselageDiameter = Constants.Defaults.Fuselage.Diameter;
            Sweep = Constants.Defaults.VerticalStabilizer.Sweep;
            Area = Constants.Defaults.VerticalStabilizer.Area;
            AspectRatio = Constants.Defaults.VerticalStabilizer.AspectRatio;

            UpdateSections();
        }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        public float GetXPosition()
        {
            return CentreFuselageLength + (FuselageDiameter * AfterbodyLengthDiameterRatio);
        }

        public virtual Vector3 GetOffset()
        {
            return new Vector3(0,
                0.4f * FuselageDiameter,
                -GetXPosition());
        }

        public void UpdateArea(float S_w, float l_v, float b)
        {
            Area = (Constants.Defaults.VerticalStabilizer.VolumeCoefficient * S_w * b) / l_v;
        }

        public void UpdateSections()
        {
            var offset = GetOffset();

            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = Vector3.up,
                    Distance = Height,
                    Points = Primitives.NACA(Vector3.up, Vector3.forward,  offset + new Vector3(0, FuselageDiameter, -Height * Mathf.Sin(Mathf.Deg2Rad * Sweep)), 0, 0, ThicknessChordRatio, RootChord * TaperRatio, 20)
                },
                new CrossSection
                {
                    Direction = Vector3.up,
                    Distance = 0,
                    Points = Primitives.NACA(Vector3.up, Vector3.forward, Vector3.zero + offset, 0, 0, ThicknessChordRatio, RootChord, 20)
                }
            };
        }

        public override float CalculateArea()
        {
            return 0.5f * RootChord * Height * (1 + TaperRatio);
        }

        public float CalculateSweep(float portion)
        {
            var gamma_full = Mathf.Atan((((Mathf.Tan(Mathf.Deg2Rad * Sweep)
                * Height)
                + (TaperRatio * RootChord)) - RootChord)
                / (Height));

            return ((Sweep * Mathf.Deg2Rad) * (portion) + (gamma_full * (1 - portion))) * Mathf.Rad2Deg;
        }
    }
}
