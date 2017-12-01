using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class UnderCarriage
    {
        public List<Wheel> Wheels { get; set; }
        public float Weight { get; internal set; }
        public float FrontalArea { get; internal set; }
        public float StrutHeight { get; set; }

        private int wheelCount;
        public int WheelCount
        {
            get
            {
                return wheelCount;
            }

            set
            {
                wheelCount = Mathf.RoundToInt(value / 2) * 2;
            }
        }

        /// <summary>
        /// Strut count per side
        /// </summary>
        public int StrutCount { get; set; }

        public Vector3 NoseGearPosition
        {
            get
            {
                return new Vector3(0, -45, 15);
            }
        }

        public float RearGearPosition
        {
            get; set;
        }

        public float Width { get; set; }

        public UnderCarriage()
        {
            Width = Constants.Defaults.UnderCarriage.Width;
            StrutCount = Constants.Defaults.UnderCarriage.StrutCount;
            WheelCount = Constants.Defaults.UnderCarriage.WheelCount;
            RearGearPosition = Constants.Defaults.UnderCarriage.RearGearPosition;

            UpdateSections();
        }

        public void Generate(Mesh mesh, Vector2 uv)
        {
            Wheels.ForEach(x => x.Generate(mesh, uv));
        }

        public void UpdateSections()
        {
            Wheels = new List<Wheel>();

            // Nose wheel
            Wheels.Add(new Wheel(new Vector3(0, -StrutHeight, 15)));

            for (int i = 0; i < WheelCount / 2; i++)
            {
                for (int j = 0; j < StrutCount; j++)
                {
                    Wheels.Add(new Wheel(new Vector3(Width + (j * 20), -StrutHeight, RearGearPosition - (10 * i) - (j * 40))));
                    Wheels.Add(new Wheel(new Vector3(-Width - (j * 20), -StrutHeight, RearGearPosition - (10 * i) - (j * 40))));
                }
            }
        }

        public float CalculateCentre()
        {
            return -(0.92f * RearGearPosition + 0.08f * NoseGearPosition.z);
        }


    }

    public class Wheel : Geometry
    {
        public List<CrossSection> CrossSections { get; set; }
        public Vector3 Root { get; set; }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            return CrossSections;
        }

        public Wheel(Vector3 root)
        {
            Root = root;

            CrossSections = new List<CrossSection>
            {
                new CrossSection
                {
                    Distance = -8,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 0, 16),
                    Direction = Vector3.right
                },
                new CrossSection
                {
                    Distance = -8,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 5f, 16),
                    Direction = Vector3.right
                },
                new CrossSection
                {
                    Distance = -4f,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 5f, 16),
                    Direction = Vector3.right
                },new CrossSection
                {
                    Distance = -4f,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 1f, 16),
                    Direction = Vector3.right
                },new CrossSection
                {
                    Distance = 4f,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 1f, 16),
                    Direction = Vector3.right
                },new CrossSection
                {
                    Distance = 4f,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 5f, 16),
                    Direction = Vector3.right
                },new CrossSection
                {
                    Distance = 8,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 5f, 16),
                    Direction = Vector3.right
                },new CrossSection
                {
                    Distance = 8,
                    Points = Primitives.Circle(Vector3.right, Vector3.back, Root, 0, 16),
                    Direction = Vector3.right
                }
            };
        }


    }
}
