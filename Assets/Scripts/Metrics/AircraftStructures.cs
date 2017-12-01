using Assets.Scripts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Metrics
{
    public static class AircraftStructures
    {
        // Weight calculation methods

        public static float CalculateWeight(this Aircraft aircraft)
        {
            var fuelWeight = aircraft.Wing.Fuel.Weight;
            var passengerWeight = aircraft.CalculateSeats() * Constants.Metrics.PassengerWeight;

            var W_0 = 2 * Constants.kg2lb * (fuelWeight + passengerWeight);               // Same as W_TO.
            var engineWeight = CalculateEngineWeight(aircraft.Wing);
            var totalEngineWeight = 2 * aircraft.Wing.EnginesPerWing * engineWeight;
            var fuselageWeight = CalculateFuselageWeight(aircraft);
            var horizontalStabilizerWeight = CalculateHorizontalStabilizerWeight(aircraft);
            var verticalStabilizerWeight = CalculateVerticalStabilizerWeight(aircraft);

            float wingWeight = 0;
            float undercarriageWeight = 0;
            float flightControlsWeight = 0;
            float electricalSystemsWeight = 0;

            for (int i = 0; i < 10; i++)
            {
                // Fuselage weight
                wingWeight = CalculateWingWeight(aircraft.Wing, W_0, fuelWeight * Constants.kg2lb);
                undercarriageWeight = CalculateUndercarriageWeight(W_0);
                flightControlsWeight = CalculateFlightControlsWeight(W_0);
                electricalSystemsWeight = CalculateElectricalSystemWeight(aircraft, W_0);

                W_0 = fuselageWeight + wingWeight + horizontalStabilizerWeight + verticalStabilizerWeight + totalEngineWeight + (passengerWeight * Constants.kg2lb) + (fuelWeight * Constants.kg2lb) + undercarriageWeight + flightControlsWeight + electricalSystemsWeight;
            }

            aircraft.Fuselage.Weight = (fuselageWeight + flightControlsWeight + electricalSystemsWeight) * Constants.lb2kg;
            aircraft.Wing.Weight = wingWeight * Constants.lb2kg;
            aircraft.HorizontalStabilizer.Weight = horizontalStabilizerWeight * Constants.lb2kg;
            aircraft.VerticalStabilizer.Weight = verticalStabilizerWeight * Constants.lb2kg;
            aircraft.UnderCarriage.Weight = undercarriageWeight * Constants.lb2kg;
            aircraft.Wing.Engines.ForEach(x => x.Weight = engineWeight * Constants.lb2kg);

            aircraft.MaximumTakeoffWeight = W_0 * Constants.lb2kg;

            return W_0 * Constants.lb2kg;
        }

        public static Vector3 CalculateCentreOfMass(this Aircraft aircraft, bool zeroFuel = false)
        {
            var fuselageX = (aircraft.Fuselage.CentreFuselage.Length 
                + aircraft.Fuselage.Diameter * ((0.33f * aircraft.Fuselage.Afterbody.AfterbodyLengthDiameterRatio) - (0.5f * aircraft.Fuselage.NoseLengthDiameterRatio)))/2;

            var wingX = aircraft.Wing.AerodynamicCentre();

            var hsX = aircraft.HorizontalStabilizer.AerodynamicCentre();
            var vsX = aircraft.VerticalStabilizer.AerodynamicCentre();

            var ucX = aircraft.UnderCarriage.CalculateCentre();

            // Haha! nginx/1.4.2
            var engineX = aircraft.Wing.Engines.Select(x => x.GetXPosition()).Sum() / (2 * aircraft.Wing.EnginesPerWing);
            var totalEngineWeight = aircraft.Wing.Engines.Select(x => x.Weight).Sum();

            var totalWeight = aircraft.Fuselage.Weight
                + aircraft.Wing.Weight
                + aircraft.HorizontalStabilizer.Weight
                + aircraft.VerticalStabilizer.Weight
                + aircraft.UnderCarriage.Weight
                + totalEngineWeight;

            var cg = ((fuselageX * aircraft.Fuselage.Weight)
                + (wingX * (aircraft.Wing.Weight))
                + (hsX * aircraft.HorizontalStabilizer.Weight)
                + (vsX * aircraft.VerticalStabilizer.Weight)
                + (ucX * aircraft.UnderCarriage.Weight)
                + (engineX * totalEngineWeight));

            if (!zeroFuel)
            {
                totalWeight += aircraft.Wing.Fuel.Weight;
                cg += wingX * aircraft.Wing.Fuel.Weight;
            }

            cg /= totalWeight;

            if (!zeroFuel)
            {
                aircraft.CentreOfGravity = new Vector3(0, 0, cg);
            }

            return aircraft.CentreOfGravity;
        }

        public static int CalculateSeats(this Aircraft aircraft)
        {
            var seatsAcross = aircraft.SeatsAcross;

            var length = aircraft.Fuselage.CentreFuselage.Length / 10f;   // Converted to metres

            return Mathf.CeilToInt(seatsAcross * length * Constants.Metrics.SeatLengthCoefficient / Constants.Metrics.SeatPitch);

        }

        public static float SeatsToDiameter(int seatsAcross)
        {
            var aisleCount = Convert.ToInt32(Math.Ceiling(Convert.ToSingle(seatsAcross - 6) / 5f));

            return (seatsAcross * Constants.Metrics.SeatWidth) +
                ((seatsAcross + aisleCount + 1) * Constants.Metrics.ArmRestWidth) +
                (aisleCount * Constants.Metrics.AisleWidth);
        }

        // Calculate weight for constituent components

        private static float CalculateFuselageWeight(Aircraft aircraft)
        {
            var l_h = Constants.m2ft * ((aircraft.HorizontalStabilizer.GetXPosition() + aircraft.HorizontalStabilizer.RootChord / 4) -
                 (aircraft.Wing.GetXPosition() + aircraft.Wing.RootChord / 4)) / 10;

            var S_fgs = Mathf.Pow(aircraft.Fuselage.CalculateArea()
                * Constants.ms2fts, 1.2f);

            var Vdlh = Constants.Metrics.DesignDiveSpeedKnots * l_h;

            var r_f = 2 * aircraft.Fuselage.Diameter / 10 * Constants.m2ft;

            // K_f = 1.08 for pressurized cabin

            return 0.021f * 1.08f
                * Mathf.Sqrt(Vdlh / r_f) * S_fgs;
        }

        private static float CalculateWingWeight(Wing wing, float W_0, float W_F)
        {
            var gamma_half = wing.CalculateSweep(0.5f) * Mathf.Deg2Rad;

            var bstr = ((0.1f * wing.Span * Constants.m2ft) * (wing.Area * 0.01f * Constants.ms2fts)) / (Mathf.Cos(gamma_half) * (W_0 - W_F)
                * (0.12f * wing.RootChord * Constants.m2ft / 10));

            return 0.0017f
                * (W_0 - W_F)
                * (Mathf.Pow((0.1f * wing.Span * Constants.m2ft) / Mathf.Cos(gamma_half), 0.75f))
                * (1 + Mathf.Pow((6.3f * Mathf.Cos(gamma_half)) / (0.1f * wing.Span * Constants.m2ft), 0.5f))
                * (Mathf.Pow(1.5f, 0.55f))
                * Mathf.Pow(bstr, 0.3f);
        }

        private static float CalculateEngineWeight(Wing wing)
        {
            return (14.7f * Mathf.Pow(wing.EngineDesignThrust, 1.1f) * Mathf.Exp(-0.045f * wing.EngineBypassRatio));
        }

        private static float CalculateUndercarriageWeight(float W_0)
        {
            var W_main = 2 * (40f + (0.16f * (Mathf.Pow(W_0, 0.75f))) + 0.019f * (W_0) + (0.000015f * Mathf.Pow(W_0, 1.5f)));
            var W_nose = 20f + (0.10f * (Mathf.Pow(W_0, 0.75f))) + (0.000002f * Mathf.Pow(W_0, 1.5f));

            return W_main + W_nose;
        }

        //private static float CalculateElectronicsWeight(Aircraft aircraft)
        //{

        //}

        private static float CalculateHorizontalStabilizerWeight(Aircraft aircraft)
        {
            // Horizontal stabilizer
            // Half chord sweep angle - horizontal stabilizer
            var hstabilizer = aircraft.HorizontalStabilizer;

            var hgamma_half = hstabilizer.CalculateSweep(0.5f) * Mathf.Deg2Rad;
             
            // K_h = 1.1

            var W_h = 1.1f * hstabilizer.Area * 0.01f * Constants.ms2fts * (
                    3.81f * ((Mathf.Pow(hstabilizer.Area * 0.01f, 0.2f) * Constants.Metrics.DesignDiveSpeedKnots) / (1000 * Mathf.Sqrt(Mathf.Cos(hgamma_half)))) - 0.287f
                );

            return W_h;
        }

        private static float CalculateVerticalStabilizerWeight(Aircraft aircraft)
        {
            // Vertical stabilizer

            var vstabilizer = aircraft.VerticalStabilizer;

            var vgamma_full = Mathf.Atan((((Mathf.Tan(Mathf.Deg2Rad * vstabilizer.Sweep)
                * vstabilizer.Height)
                + (VerticalStabilizer.TaperRatio * vstabilizer.RootChord)) - vstabilizer.RootChord)
                / (vstabilizer.Height));

            var vgamma_half = 0.5f * ((vstabilizer.Sweep * Mathf.Deg2Rad) + vgamma_full);

            var W_v = vstabilizer.CalculateArea() * 0.01f * Constants.ms2fts * (
                    3.81f * ((Mathf.Pow(vstabilizer.CalculateArea() * 0.01f * Constants.ms2fts, 0.2f) * Constants.Metrics.DesignDiveSpeedKnots) / (1000 * Mathf.Sqrt(Mathf.Cos(vgamma_half)))) - 0.287f
                );

            if(float.IsNaN(W_v))
            {
                return 0;
            }

            return W_v;
        }

        private static float CalculateElectricalSystemWeight(Aircraft aircraft,float W_0)
        {
            var V_pax = 0.4f * Mathf.Pow(Constants.m2ft, 3) * Mathf.PI * Mathf.Pow(aircraft.Fuselage.Diameter / 20 - 0.8f, 2)
                * (aircraft.Fuselage.CentreFuselage.Length / 10);

            return 10.8f * Mathf.Pow(V_pax, 0.7f) * (1 - 0.018f * Mathf.Pow(V_pax, 0.35f));
        }

        private static float CalculateFlightControlsWeight(float W_0)
        {
            return 0.64f * Mathf.Pow(W_0, 0.666f);
        }


    }
}
