using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public abstract class MirroredLiftDevice : Geometry, IExtruded
    {
        public List<CrossSection> CrossSections { get; set; }

        public float Span
        {
            get
            {
                return Mathf.Sqrt(Area * AspectRatio);
            }
        }
        public float Area { get; set; }
        public float AspectRatio { get; set; }
        public float ThicknessChordRatio { get { return 0.12f; } }
        public float RootChord
        {
            get
            {
                return (2 * Area) / (Span * (TaperRatio + 1));
            }
        }
        public float TaperRatio { get; set; }
        public float FuselageDiameter { get; set; }
        public float CentreFuselageLength { get; set; }
        // Degrees
        public float Sweep { get; set; }

        public virtual float Dihedral { get { return 0.4f; } }
        public virtual float FuselageMount { get { return -0.33f; } }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;

        }

        public virtual float GetXPosition()
        {
            return 100f;
        }

        public virtual Vector3 GetOffset()
        {
            return new Vector3(0,
                FuselageMount * FuselageDiameter,
                -GetXPosition());
        }

        public float SpanFromArea(float Area)
        {
            // TODO
            return 1.0f;
        }

        public virtual void UpdateSections()
        {
            var offset = GetOffset();

            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = Span/2,
                    Points = Primitives.NACA(Vector3.right, Vector3.back, offset + new Vector3(0, Span * Dihedral, -Span * Mathf.Tan(Mathf.Deg2Rad * Sweep)), 2, 4, ThicknessChordRatio, RootChord * TaperRatio, 20)
                },
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = 0,
                    Points = Primitives.NACA(Vector3.right, Vector3.back, Vector3.zero + offset, 2, 4, ThicknessChordRatio, RootChord, 20)
                },
                new CrossSection
                {
                    Direction = Vector3.right,
                    Distance = -Span/2,
                    Points = Primitives.NACA(Vector3.right, Vector3.back,  offset + new Vector3(0, Span * Dihedral, -Span * Mathf.Tan(Mathf.Deg2Rad * Sweep)), 2, 4, ThicknessChordRatio, RootChord * TaperRatio, 20)
                }
            };
        }

        public override void Generate(Mesh mesh, Vector2 uv, bool capEnds = false, int interp = 1)
        {
            base.Generate(mesh, uv, true, interp);
        }

        /// <summary>
        /// Calculate sweep at a portion and return it in degrees
        /// </summary>
        /// <param name="portion"></param>
        /// <returns></returns>
        public float CalculateSweep(float portion)
        {
            var opposite = (Mathf.Tan(Mathf.Deg2Rad * Sweep) * (Span / 2)) + (TaperRatio * RootChord) - RootChord;

            var adjacent = Span / 2;

            var gamma_full = Mathf.Atan(opposite / adjacent);

            gamma_full *= Mathf.Rad2Deg;

            return ((Sweep) * (portion) + (gamma_full * (1-portion)));
        }
    }
}
