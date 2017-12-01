using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Metrics;

namespace Assets.Scripts.Geometry
{
    public class Aircraft
    {
        public Fuselage Fuselage { get; set; }

        // Will be mirrored on the other side
        public Wing Wing { get; set; }
        public VerticalStabilizer VerticalStabilizer { get; set; }
        public HorizontalStabilizer HorizontalStabilizer { get; set; }
        public UnderCarriage UnderCarriage { get; set; }
        public AircraftStats Stats { get; set; }

        /// <summary>
        /// Z is positive in this instance
        /// </summary>
        public Vector3 CentreOfGravity { get; set; }

        // Horrible hack. Fix!
        public int SeatsAcross { get; set; }

        public float Weight
        {
            get
            {
                var totalEngineWeight = Wing.Engines.Select(x => x.Weight).Sum();

                return Fuselage.Weight
                    + Wing.Weight
                    + Wing.Fuel.Weight
                    + HorizontalStabilizer.Weight
                    + VerticalStabilizer.Weight
                    + UnderCarriage.Weight
                    + totalEngineWeight;
            }
        }

        public float ZeroFuelWeight
        {
            get
            {
                return Weight - Wing.Fuel.Weight;
            }
        }

        public Aircraft()
        {
            Fuselage = new Fuselage();
            HorizontalStabilizer = new HorizontalStabilizer();
            Wing = new Wing();
            VerticalStabilizer = new VerticalStabilizer();
            UnderCarriage = new UnderCarriage();

            SeatsAcross = 10;
        }

        public void Generate(Mesh mesh)
        {
            Update(mesh);

            Wing.Generate(mesh, new Vector2(0.15f, 0));
            Wing.Engines.ForEach(x => x.Generate(mesh, new Vector2(0.25f, 0)));
            Wing.Flaps.Generate(mesh, new Vector2(0.35f, 0));
            Fuselage.Generate(mesh, Vector2.zero);
            HorizontalStabilizer.Generate(mesh, new Vector2(0.45f, 0));
            VerticalStabilizer.Generate(mesh, new Vector2(0.55f, 0));
            UnderCarriage.Generate(mesh, new Vector2(0.65f, 0));
        }

        public void Update(Mesh mesh)
        {
            Wing.UpdateSections();
            Fuselage.UpdateSections();
            HorizontalStabilizer.UpdateSections();
            VerticalStabilizer.UpdateSections();
            UnderCarriage.UpdateSections();
        }




        public float MaximumTakeoffWeight { get; internal set; }

        public LandingGearAlignmentStatus AlignLandingGear(Transform transform)
        {
            // Position of c_g from ground
            // TODO: Make the height of this dynamic
            var h_cg = -66.875f;

            var minStrokeStrutHeight = (Mathf.Pow(Constants.Metrics.VerticalLandingVelocity, 2) / (2 * Constants.g * 0.9f * 2.7f) * 25f);
            var minEngineClearance =  (Wing.Engines.First().Diameter * 10f * 1.9f) + Wing.Engines.First().Root.y + (0.4f * Fuselage.Diameter) - 20f;

            var strutHeight = Mathf.Max(minStrokeStrutHeight, minEngineClearance);

            transform.Translate(new Vector3(0, strutHeight + 3f - transform.position.y));
            UnderCarriage.StrutHeight = strutHeight; 

            // Strut Length Calculator

            for (int i = 0; i < 10; i++)
            {
                var wingStart = Wing.GetXPosition();
                var wingRoot = Wing.RootChord;

                var landingGearStart = CentreOfGravity.z + Mathf.Tan(15 * Mathf.Deg2Rad) * -h_cg;

                if (landingGearStart < wingStart + wingRoot)
                {
                    UnderCarriage.RearGearPosition = -landingGearStart;
                }
                else
                {
                    return LandingGearAlignmentStatus.TipBackNotInRange;
                }

                this.CalculateCentreOfMass();
            }

            float l_n = CentreOfGravity.z + UnderCarriage.NoseGearPosition.z;
            float l_m = -UnderCarriage.RearGearPosition - CentreOfGravity.z;

            var taildownAngle = Mathf.Atan(1f / Fuselage.Afterbody.AfterbodyLengthDiameterRatio) * Mathf.Rad2Deg;
            var tipbackAngle = Mathf.Atan(Mathf.Abs(l_m / h_cg)) * Mathf.Rad2Deg;

            if (tipbackAngle < taildownAngle)
            {
                return LandingGearAlignmentStatus.TipBackSmallerThanTailDown;
            }

            UnderCarriage.StrutCount = 1;

            if (MaximumTakeoffWeight < 200000 * Constants.lb2kg)
            {
                UnderCarriage.WheelCount = 2;
            }
            else if (MaximumTakeoffWeight < 400000 * Constants.lb2kg)
            {
                UnderCarriage.WheelCount = 4;
            }
            else
            {
                UnderCarriage.WheelCount = 6;
                UnderCarriage.StrutCount = 2;
            }

            var delta = Mathf.Asin(h_cg / (l_n * Mathf.Tan(63f * Mathf.Deg2Rad)));
            var width = (l_n + l_m) * Mathf.Tan(delta);

            // TODO: cSet landing gear height

            UnderCarriage.Width = width;
            UnderCarriage.UpdateSections();


            return LandingGearAlignmentStatus.Success;
        }

        public void UpdateTailArea()
        {
            var l_v = ((Fuselage.Diameter * Fuselage.Afterbody.AfterbodyLengthDiameterRatio) + Fuselage.CentreFuselage.Length) - CentreOfGravity.z;

            VerticalStabilizer.UpdateArea(Wing.Area, l_v, Wing.Span);
        }


    }

    public enum LandingGearAlignmentStatus
    {
        Success,
        TipBackSmallerThanTailDown,
        TipBackNotInRange,

    }
}
