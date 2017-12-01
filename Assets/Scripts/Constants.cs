using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    // TODO: give a v_stall constraint instead of a LiftNotAdequate constraint
    public static class Constants
    {
        public class Defaults
        {

            public class Fuselage
            {
                public static float Diameter = 62f;
                public static float CentreFuselageLength = 640f;
                public static float NoseLengthDiameterRatio = 2.0f;
                public static float AfterbodyLengthDiameterRatio = 2f;
            }

            public class Wing
            {

                public static float EngineBypassRatio = 8.0f;
                public static int EnginesPerWing = 1;
                public static float EngineDesignThrust = 100;    // kN
                public static float Sweep = 22f;
                public static float XPercentage = 0.45f;
                public static float ThicknessChordRatio = 12f;
                public static float TaperRatio = 0.2f;
                public static float Area = 50000;
                public static float AspectRatio = 8.5f;
                public static float FuelDensity = 840; // kg/m^3
                public static float FuelVolume = 171.16f;
                public static float FlapReach = 0.75f;
                public static float FlapExtensionRatio = 1.1f;
            }

            public class HorizontalStabilizer
            {
                public static float Sweep = 20f;
                public static float ThicknessChordRatio = 12f;
                public static float TaperRatio = 0.3f;
                public static float AreaRatio = 0.2f;
                public static float AspectRatio = 5f;
            }

            public class VerticalStabilizer
            {
                
                public static float RootChord = 100f;
                public static float TaperRatio = 0.3f;
                //public static float ThicknessChordRatio = 12f;
                public static float Sweep = 50f;
                public static float Height = 80f;
                public static float Area = 2500f;
                public static float AspectRatio = 1.5f;
                public static float VolumeCoefficient = 0.041f;
            }

            public class UnderCarriage
            {
                public static float Width = 50;
                public static int StrutCount = 2;
                public static int WheelCount = 6;
                public static float RearGearPosition = -330f;
            }

            public class HighLiftDevice
            {
                public static float FlapStart = 0.25f;
                public static float FlapEnd = 0.7f;
            }

        }

        public static class Metrics
        {
            public static float SeatWidth = 4.318f;
            public static float ArmRestWidth = 0.508f;
            public static float AisleWidth = 5f;

            public static float DesignDiveSpeedKnots = 580;
            public static float SeatLengthCoefficient = 1f/1.35f;
            public static float SeatPitch = 1.1f;
            public static float PassengerWeight = 90; // Kilograms - Includes checked luggage and cargo
            public static float DesignMachNumber = 0.84f;

            public static float KornFactor = 0.92f;
            public static float W1W0 = 0.98f;
            public static float W2W1 = 0.98f;   
            public static float W4W3 = 0.995f;
            public static float W5W4 = 0.992f;

            public static float OswaldEfficiency = 0.85f;
            public static float CruiseThrust = 0.7f;

            // d(eps)/d(alpha)
            public static float deda = 0.32f;
            public static float VerticalLandingVelocity = 6f; // m/s
        }

        public static class ErrorMessages
        {
            public static string LiftNotAdequate = "Lift is not adequate. Increase wing area or design thrust";
            public static string Unstable = "Aircraft is unstable. Adjust tail area ratio or wing position to make it stable";
            public static string TipBack = "Landing gear could not be positioned automatically. Adjust wing position or increase afterbody length ratio until landing gear can be succcessfully positioned";
        }

        public static float m2ft = 3.281f;
        public static float ms2fts = Convert.ToSingle(Math.Pow(3.281, 2));
        public static float lb2kg = 0.453592f;
        public static float kg2lb = 2.20462f;

        public static float g = 9.81f;
    }
}
