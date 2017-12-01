using Assets.Scripts.Extensions;
using Assets.Scripts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Metrics
{
    /// <summary>
    /// Class that contains static extension methods that evaluate the aerodynamic performance of type Aircraft
    /// </summary>
    public static class AircraftAerodynamics
    {
        /// <summary>
        /// Calculates total C_L of the aircraft, taking into account flaps, wings, and horizontal stabilizers
        /// </summary>
        /// <param name="aircraft">Aircraft type to evaluate</param>
        /// <param name="alpha">Aircraft pitch in degrees</param>
        /// <param name="flaps">Flap extension in the interval [0, 1] to indicate 0-100% flap extension</param>
        /// <returns></returns>
        public static float CalculateC_L(this Aircraft aircraft, float alpha = 0, float flaps = 0)
        {
            var combinedC_L = CalculateCombinedC_L(aircraft, alpha, flaps);

            return combinedC_L.C_Lw + (aircraft.HorizontalStabilizer.Area / aircraft.Wing.Area) * combinedC_L.C_Lh + combinedC_L.C_Lf;
        }

        /// <summary>
        /// Calulacates C_L and returns a structure giving the individual C_L of the flaps and wing
        /// </summary>
        /// <param name="aircraft">Aircraft type to evaluate</param>
        /// <param name="alpha">Aircraft pitch in degrees</param>
        /// <param name="flaps">Flap extension in the interval [0, 1] to indicate 0-100% flap extension</param>
        /// <returns></returns>
        private static CombinedC_L CalculateCombinedC_L(Aircraft aircraft, float alpha = 0, float flaps = 0)
        {
            // Fuselage lift factor

            var F = 1.07f * Mathf.Pow(1 + aircraft.Fuselage.Diameter / aircraft.Wing.Span, 2);

            // is this even a B in the equation sheet?
            var B = Mathf.Sqrt(1 - Mathf.Pow(Constants.Metrics.DesignMachNumber, 2));

            // Efficiency - this one's a guess lol
            var nu = 0.98f;

            var alpha_0 = -2f * Mathf.Deg2Rad;      // Degrees

            var C_Lw = CalculateC_aLMLD(aircraft.Wing, F, B, nu) * (alpha * Mathf.Deg2Rad - alpha_0);
            var C_Lh = CalculateC_aLMLD(aircraft.HorizontalStabilizer, F, B, nu) * (alpha * Mathf.Deg2Rad - alpha_0);
            var C_Lf = CalculateFlapsC_L(aircraft.Wing, flaps);

            return new CombinedC_L
            {
                C_Lw = C_Lw + C_Lf,
                C_Lh = C_Lh,
            };
        }

        /// <summary>
        /// Calculates C_L of the flaps
        /// </summary>
        /// <param name="ld">Wing type that flaps are mounted on</param>
        /// <param name="deployment">Flap extension in the interval [0, 1] to indicate 0-100% flap extension</param>
        /// <returns></returns>
        private static float CalculateFlapsC_L(Wing ld, float deployment = 1)
        {
            var hld = ld.Flaps;

            var d_C_L = 0f;
            var extension = ld.Flaps.ExtensionRatio;

            // Assuming c'/c = 1.1
            switch (hld.HighLiftDeviceType)
            {
                case HighLiftDeviceType.Plain:
                    d_C_L = 0.9f;
                    break;
                case HighLiftDeviceType.Single:
                    d_C_L = 1.3f * extension;
                    break;
                case HighLiftDeviceType.Double:
                    d_C_L = 1.6f * extension;
                    break;
                case HighLiftDeviceType.Triple:
                    d_C_L = 1.9f * extension;
                    break;
            }

            d_C_L *= deployment;

            var S_flapped = ld.RootChord * ((Constants.Defaults.HighLiftDevice.FlapStart) + (Constants.Defaults.HighLiftDevice.FlapEnd)) * ld.TaperRatio * (ld.Span * (Constants.Defaults.HighLiftDevice.FlapEnd - Constants.Defaults.HighLiftDevice.FlapStart));

            var D_C_L = d_C_L * (S_flapped / ld.Area) * Mathf.Cos(ld.CalculateSweep(0.95f) * Mathf.Deg2Rad);

            return D_C_L;
        }

        /// <summary>
        /// Calculate 3D d(C_L)/d(alpha) of a MirroredLiftDevice type
        /// </summary>
        /// <param name="ld">MirroredLiftDevice to evaluate</param>
        /// <param name="F">Fuselage lift factor</param>
        /// <param name="B">sqrt(1-M^2)</param>
        /// <param name="nu">Efficiency factor</param>
        /// <returns></returns>
        private static float CalculateC_aLMLD(MirroredLiftDevice ld, float F, float B, float nu)
        {
            // Assume quarter chord is max thickness
            var sweep = ld.CalculateSweep(0.25f) * Mathf.Deg2Rad;

            //var S_exp_ratio = 2.1f;
            return F * (2 * Mathf.PI * ld.AspectRatio) / (2 + Mathf.Sqrt(4 + Mathf.Pow(ld.AspectRatio * B / nu, 2) * (1 + Mathf.Pow(Mathf.Tan(sweep / B), 2))));
        }

        /// <summary>
        /// Calculate C_L maximum for a MirroredLiftDevice type
        /// </summary>
        /// <param name="ld">MirroredLiftDevice type to evaluate</param>
        /// <param name="C_L_max_2d">2D C_L_Max. 1.6 for NACA2412 Airfoil</param>
        /// <returns></returns>
        private static float CalculateC_LMax(this MirroredLiftDevice ld, float C_L_max_2d = 1.6f)
        {
            // C_L_Max of NACA 2412 = 1.6
            return 0.9f * C_L_max_2d * Mathf.Cos(ld.CalculateSweep(0.25f) * Mathf.Deg2Rad);
        }

        public static float CalculateStaticMargin(this Aircraft aircraft)
        {
            var cgZ = aircraft.CentreOfGravity.z;

            var combinedC_L = CalculateCombinedC_L(aircraft);

            var wingArm = -(cgZ - aircraft.Wing.AerodynamicCentre());
            var horizontalStabilizerArm = (aircraft.HorizontalStabilizer.AerodynamicCentre() - cgZ);

            return ((aircraft.Wing.Area * combinedC_L.C_Lw * wingArm) + (aircraft.HorizontalStabilizer.Area * combinedC_L.C_Lh * horizontalStabilizerArm)) / (aircraft.Wing.Area + aircraft.VerticalStabilizer.Area);
        }

        /// <summary>
        /// Returns X position of the aerodynamic centre of a MirroredLiftDevice type
        /// </summary>
        /// <param name="ld">MirroredLiftDevice to evaluate</param>
        /// <returns>Decimetres</returns>
        public static float AerodynamicCentre(this MirroredLiftDevice ld)
        {
            var lambda = ld.Sweep * Mathf.Deg2Rad;
            var cBar = CalculateMeanAerodynamicChord(ld);
            var YBar = (ld.Span / 6) * ((1 + 2 * lambda) / (1 + lambda));

            var centre = Mathf.Tan(lambda) * YBar + 0.25f * cBar;

            return centre + ld.GetXPosition();
        }

        /// <summary>
        /// Returns the mean aerodynamic chord of a MirroredLiftDevice type
        /// </summary>
        /// <param name="ld">MirroredLiftDevice type to evaluate</param>
        /// <returns>Metres</returns>
        public static float CalculateMeanAerodynamicChord(this MirroredLiftDevice ld)
        {
            var lambda = ld.Sweep * Mathf.Deg2Rad;
            return (0.066667f) * ld.RootChord * (1 + lambda + Mathf.Pow(lambda, 2)) / (1 + lambda);
        }


        public static float AerodynamicCentre(this VerticalStabilizer ld)
        {
            var lambda = ld.Sweep * Mathf.Deg2Rad;
            var cBar = (0.066667f) * ld.RootChord * (1 + lambda + Mathf.Pow(lambda, 2)) / (1 + lambda);
            var YBar = (ld.Height / 3) * ((1 + 2 * lambda) / (1 + lambda));

            var centre = Mathf.Tan(lambda) * YBar + 0.25f * cBar;

            return centre + ld.GetXPosition();
        }

        public static float CalculateC_D0(this Aircraft aircraft, float velocity, float altitude)
        {
            var Re = CalculateRe(velocity, altitude, aircraft.Wing);
            var S_ref = aircraft.Wing.Area / 100f;

            // Skin friction coefficient
            float C_f;

            // Where do these approximations even come from?! Whatever...
            if (Re < 5e5)
            {
                C_f = 1.328f * Mathf.Pow(Re, -0.5f);
            }
            else
            {
                C_f = (0.455f) / (Mathf.Pow(Mathf.Log10(Re), 2.58f) * Mathf.Pow(1 + 0.144f * VelocityToMach(velocity, altitude), 0.65f));
            }

            var fuselageC_D0 = CalculateFuselageC_D0(aircraft.Fuselage, S_ref, Re, C_f);
            var wingC_D0 = CalculateMLDC_D0(aircraft.Wing, S_ref, Re, C_f);
            var hsC_D0 = CalculateMLDC_D0(aircraft.HorizontalStabilizer, S_ref, Re, C_f);
            var vsC_D0 = CalculateVSCD_0(aircraft.VerticalStabilizer, S_ref, Re, C_f);
            var engineC_D0 = aircraft.Wing.Engines.Select(x => CalculateEngineC_D0(x, S_ref, Re, C_f)).Sum();

            // TODO: Landing gear C_D0
            // 0.95 for supercritical airfoils

            var C_L = CalculateC_L(aircraft);
            var M_cr = CalculateM_CR(aircraft.Wing, C_L);
            var M = VelocityToMach(velocity, altitude);

            // 0.8 is a bit of a fudge factor to describe the area of a NACA-2412 airfol as a proportion of total chord area
            float waveC_D0 = 0;
            if (M > M_cr)
            {
                waveC_D0 = 20 * Mathf.Pow(M - M_cr, 4) * (Mathf.Pow(aircraft.Wing.CalculateMeanAerodynamicChord() * 10, 2) * aircraft.Wing.ThicknessChordRatio * 0.9f) / aircraft.Wing.Area;
            }

            return (fuselageC_D0 + wingC_D0 + hsC_D0 + vsC_D0 + engineC_D0) + waveC_D0;
        }

        public static float CalculateRe(float velocity, float altitude, MirroredLiftDevice ld)
        {
            var cBar = CalculateMeanAerodynamicChord(ld);

            // Kinematic viscoscity - interpolated from international standard atmosphere with 3rd order polynomial
            var nu = CalculateKinematicViscosity(altitude);

            return velocity * (ld.CalculateMeanAerodynamicChord() / 2) * Constants.m2ft / nu;
        }

        private static float CalculateKinematicViscosity(float altitude)
        {
            return Convert.ToSingle(((Mathf.Pow(altitude, 3) * (2e-12)) - (Mathf.Pow(altitude, 2) * 1e-8) + (altitude * 0.0001) + 1.5072) * 1e-5);
        }

        private static float CalculateDynamicViscosity(float altitude)
        {
            return CalculateKinematicViscosity(altitude) * CalculateDensity(altitude);
        }

        /// <summary>
        /// Calculate C_D0 for a fuselage
        /// </summary>
        /// <param name="fuselage"></param>
        /// <param name="S_ref"></param>
        /// <param name="Re"></param>
        /// <param name="C_f"></param>
        /// <returns></returns>
        private static float CalculateFuselageC_D0(Fuselage fuselage, float S_ref, float Re, float C_f)
        {
            var S_wet = fuselage.CalculateArea();
            var fuselageLength = 0.1f * (fuselage.CentreFuselage.Length + fuselage.Diameter * (fuselage.NoseLengthDiameterRatio + fuselage.Afterbody.AfterbodyLengthDiameterRatio));

            // Fineness ratio
            var f = fuselageLength / (fuselage.Diameter / 10f);

            // Form factor adjustment
            var FF = 1 + (60f * Mathf.Pow(f, -3)) + f / 400f;

            // Interference estimation
            var Q = 1.3f;

            return (C_f * FF * Q * S_wet) / S_ref;
        }

        /// <summary>
        /// Calculate C_D0 for MirroredLiftDevice types
        /// </summary>
        /// <param name="ld"></param>
        /// <param name="S_ref"></param>
        /// <param name="Re"></param>
        /// <param name="C_f"></param>
        /// <returns></returns>
        private static float CalculateMLDC_D0(MirroredLiftDevice ld, float S_ref, float Re, float C_f)
        {
            // Calculate form factor
            var gamma_quarter = ld.CalculateSweep(0.25f) * Mathf.Deg2Rad;
            var S_wet = ld.Area / 100f;

            var xc = 0.25f;
            var tc = ld.ThicknessChordRatio;
            var FF = (1 + (0.6f / xc * tc) + 100 * Mathf.Pow(tc, 4)) * (1.34f * Mathf.Pow(Constants.Metrics.DesignMachNumber, 0.18f) * Mathf.Pow(Mathf.Cos(gamma_quarter), 0.28f));

            var Q = 1.1f;       // Engines I guess

            return (C_f * FF * Q * S_wet) / S_ref;
        }

        private static float CalculateVSCD_0(VerticalStabilizer vs, float S_ref, float Re, float C_f)
        {
            // Calculate form factor
            var gamma_quarter = vs.CalculateSweep(0.25f) * Mathf.Deg2Rad;
            var S_wet = vs.Area / 100f;

            var xc = 0.25f;
            var tc = vs.ThicknessChordRatio;
            var FF = (1 + (0.6f / xc * tc) + 100 * Mathf.Pow(tc, 4)) * (1.34f * Mathf.Pow(Constants.Metrics.DesignMachNumber, 0.18f) * Mathf.Pow(Mathf.Cos(gamma_quarter), 0.28f));

            return (C_f * FF * S_wet) / S_ref;
        }

        private static float CalculateEngineC_D0(Engine engine, float S_ref, float Re, float C_f)
        {
            var f = engine.Length / engine.Diameter;

            var FF = 1 + (0.35f / f);

            // Q = 0 (no interference)

            return (C_f * FF * engine.Area) / S_ref;
        }

        private static float CalculateUnderCarriageC_D0(UnderCarriage uc, float S_ref, float Re, float C_f)
        {
            return 2.25f * uc.FrontalArea / S_ref;
        }

        private static float VelocityToMach(float velocity, float altitude)
        {
            // Calculate mach 1 from interpolated data
            var M1 = 340.15f - 0.004f * altitude;

            return velocity / M1;
        }

        private static float CalculateDensity(float altitude)
        {
            return 1.225f * Mathf.Exp(-altitude * 0.0001f);
        }

        public static float EvaluateStability(this Aircraft aircraft, float velocity, float altitude)
        {
            var nonDimensionalCg = (aircraft.CentreOfGravity.z - aircraft.Wing.AerodynamicCentre()) / (10 * aircraft.Wing.CalculateMeanAerodynamicChord());

            var V_h = aircraft.HorizontalStabilizer.Area * (aircraft.HorizontalStabilizer.AerodynamicCentre() - aircraft.CentreOfGravity.z) / (aircraft.Wing.Area * aircraft.Wing.CalculateMeanAerodynamicChord() * 10);

            var F = 1.07f * Mathf.Pow(1 + aircraft.Fuselage.Diameter / aircraft.Wing.Span, 2);
            var B = Mathf.Sqrt(1 - Mathf.Pow(Constants.Metrics.DesignMachNumber, 2));
            var nu = 0.98f;

            var dCmdCl = nonDimensionalCg - V_h * (CalculateC_aLMLD(aircraft.HorizontalStabilizer, F, B, nu) / CalculateC_aLMLD(aircraft.Wing, F, B, nu)) * (1 - Constants.Metrics.deda);

            return dCmdCl;
        }

        private static float CalculateM_CR(MirroredLiftDevice ld, float C_L)
        {
            var hsweep = ld.CalculateSweep(0.5f) * Mathf.Deg2Rad;
            var coshsweep = Mathf.Cos(hsweep);

            var M_DD = (Constants.Metrics.KornFactor - (ld.ThicknessChordRatio / coshsweep) - (C_L) / (10 * Mathf.Pow(coshsweep, 2))) / coshsweep;

            return M_DD - 0.108f;
        }

        public static float CalculateRange(this Aircraft aircraft, float velocity, float altitude)
        {
            var W3W4 = (1 - (aircraft.Wing.Fuel.Weight) / (aircraft.Weight)) / (
                    Constants.Metrics.W1W0 *
                    Constants.Metrics.W2W1 *
                    Constants.Metrics.W4W3 *
                    Constants.Metrics.W5W4
                );

            var sfc = (25f / 1000000f * Mathf.Exp(-0.05f * aircraft.Wing.EngineBypassRatio)) * aircraft.Wing.EnginesPerWing * 2;

            var LD = CalculateLD(aircraft, altitude, velocity);

            var range = (velocity * LD / (Constants.g * sfc)) * -Mathf.Log(W3W4);

            return range;
        }

        private static float CalculateLD(this Aircraft aircraft, float altitude, float velocity)
        {
            var rho = CalculateDensity(altitude);
            var C_L = aircraft.Weight * Constants.g / (0.5f * rho * Mathf.Pow(velocity, 2) * aircraft.Wing.Area / 100f);

            var C_D = CalculateC_D0(aircraft, velocity, altitude)
                + (Mathf.Pow(C_L, 2) / (Mathf.PI * aircraft.Wing.AspectRatio * Constants.Metrics.OswaldEfficiency));

            return C_L / C_D;
        }

        private static float CalculateCD(this Aircraft aircraft, float altitude, float velocity)
        {
            var rho = CalculateDensity(altitude);
            var C_L = aircraft.Weight * Constants.g / (0.5f * rho * Mathf.Pow(velocity, 2) * aircraft.Wing.Area / 100f);

            var C_D = CalculateC_D0(aircraft, velocity, altitude)
                + (Mathf.Pow(C_L, 2) / (Mathf.PI * aircraft.Wing.AspectRatio * Constants.Metrics.OswaldEfficiency));

            Debug.Log(CalculateC_D0(aircraft, velocity, altitude));

            return C_D;
        }

        private static float ThrustFraction(float altitude)
        {
            var sigma = Mathf.Sqrt(CalculateDensity(altitude) / CalculateDensity(0));

            if (altitude < 11000)
            {
                return Mathf.Pow(sigma, 0.7f);
            }
            else
            {
                return 1.439f * sigma * 0.7f;
            }
        }

        public static Trimmability EvaluateTrimmability(this Aircraft aircraft, float velocity, float altitude)
        {
            var C_L = CalculateCombinedC_L(aircraft);

            var rho = CalculateDensity(altitude);

            if (0.5f * rho * Mathf.Pow(velocity, 2) * (aircraft.Wing.Area / 100f) < aircraft.Weight * Constants.g)
            {
                return Trimmability.LiftNotAdequate;
            }

            var cg = aircraft.CalculateCentreOfMass(true);

            var momentArm = (cg.z - aircraft.Wing.AerodynamicCentre());
            var moment = momentArm * C_L.C_Lw * aircraft.Wing.Area;

            var horizontalStabilizerArm = (aircraft.HorizontalStabilizer.AerodynamicCentre() - cg.z);
            var requiredC_L = moment / (horizontalStabilizerArm * aircraft.HorizontalStabilizer.Area);

            if (Mathf.Abs(requiredC_L) > CalculateC_LMax(aircraft.HorizontalStabilizer, 0.25f))
            {
                return Trimmability.RotationallyUnstable;
            }

            return Trimmability.Trimmable;

        }

        public static float CalculateMaxSpeed(this Aircraft aircraft, float altitude)
        {
            var q = 0.5f * CalculateDensity(altitude) * aircraft.Wing.Area / 100;

            var interval_pos = 550f;
            var interval_neg = 50f;

            var thrust = ThrustFraction(altitude) * aircraft.Wing.Engines.First().DesignThrust * aircraft.Wing.EnginesPerWing * 2 * 1000;

            for (int i = 0; i < 8; i++)
            {
                var interval_middle = (interval_pos + interval_neg) / 2;

                var drag = CalculateCD(aircraft, altitude, interval_middle) * q * Mathf.Pow(interval_middle, 2);

                var resultant = drag - thrust;

                if (resultant > 0)
                {
                    interval_pos = interval_middle;
                }
                else
                {
                    interval_neg = interval_middle;
                }
            }

            // TODO: Fix this fudge factor?!
            return 0.77f * (interval_pos + interval_neg) / 2;

        }

        /// <summary>
        /// Returns EAS Stall Speed
        /// </summary>
        /// <param name="aircraft">Aircraft type to calculate stall speed for</param>
        /// <returns></returns>
        public static float CalculateStallSpeed(this Aircraft aircraft)
        {
            var C_L_max = CalculateC_LMax(aircraft.Wing);

            var rho = CalculateDensity(0);

            var S = aircraft.Wing.Area / 100f;

            var W = aircraft.Weight;

            return Mathf.Sqrt(2 * W * Constants.g / (rho * S * C_L_max));
        }

        public static float CalculateTakeoffGroundRoll(this Aircraft aircraft)
        {
            var V_TD = 1.1f * CalculateStallSpeed(aircraft);

            return CalculateGroundRoll(aircraft, 0, V_TD, 0.7f, 0.03f);
        }

        public static float CalculateLandingGroundRoll(this Aircraft aircraft)
        {
            var V_TD = 1.15f * CalculateStallSpeed(aircraft);

            return CalculateGroundRoll2(aircraft, 0, V_TD, 1f, 0.3f, 0.1f);
        }

        private static float CalculateGroundRoll(Aircraft aircraft, float V_0, float V_1, float flaps = 1f, float mu = 0.2f, float T_frac = 1f)
        {
            var rho = CalculateDensity(0);
            var W = aircraft.Weight;
            var S = aircraft.Wing.Area / 100f;

            // TODO: Eliminate this HUGE fudge factor!
            var v_avg = aircraft.CalculateStallSpeed() * 0.65f;

            var C_L = CalculateC_L(aircraft, 0, flaps);

            var C_D0 = CalculateC_D0(aircraft, v_avg, 0);

            var K_A = rho / (2 * (W / S)) * (mu * C_L - C_D0 - (Mathf.Pow(C_L, 2) / (Mathf.PI * aircraft.Wing.AspectRatio * Constants.Metrics.OswaldEfficiency)));

            var T = (aircraft.Wing.EngineDesignThrust * 1000f * aircraft.Wing.EnginesPerWing * 2f) * T_frac;
            var K_T = T / (aircraft.Weight * Constants.g) - mu;

            // Got enough variable declarations mate?

            var S_g = Mathf.Log((K_T + K_A * Mathf.Pow(V_1, 2)) / (K_T + K_A * Mathf.Pow(V_0, 2))) / (2 * Constants.g * K_A);

            return S_g;
        }

        private static float CalculateGroundRoll2(Aircraft aircraft, float V_0, float V_1, float flaps = 1f, float mu = 0.2f, float T_frac = 1f)
        {
            var rho = CalculateDensity(0);
            var W = aircraft.Weight;
            var S = aircraft.Wing.Area / 100f;

            // TODO: Eliminate this HUGE fudge factor!
            var v_avg = aircraft.CalculateStallSpeed() * 0.65f;

            var C_L = CalculateC_L(aircraft, 0, flaps);

            var C_D0 = CalculateC_D0(aircraft, v_avg, 0);

            var K_A = rho / (2 * (W / S)) * (mu * C_L - C_D0 - (Mathf.Pow(C_L, 2) / (Mathf.PI * aircraft.Wing.AspectRatio * Constants.Metrics.OswaldEfficiency)));

            var T = (aircraft.Wing.EngineDesignThrust * 1000f * aircraft.Wing.EnginesPerWing * 2f) * T_frac;
            var K_T = T / (aircraft.Weight * Constants.g) + mu;

            // Got enough variable declarations mate?

            var S_g = Mathf.Log((K_T + K_A * Mathf.Pow(V_1, 2)) / (K_T + K_A * Mathf.Pow(V_0, 2))) / (2 * Constants.g * K_A);

            return S_g;
        }


        public static FAR25 EvaluateFAR25(this Aircraft aircraft)
        {
            var V_s = aircraft.CalculateStallSpeed();

            // Takeoff
            {
                var V_cl = V_s * 1.15f;
                var D = CalculateTotalDrag(aircraft, V_cl, 0, 0.7f);
                // 1 engine inoperative
                var T = (aircraft.Wing.EngineDesignThrust * 1000f * aircraft.Wing.EnginesPerWing * 2f - 1) * 0.75f * ((5 + aircraft.Wing.EngineBypassRatio) / (4 + aircraft.Wing.EngineBypassRatio));
                var sgamma = (T - D) / (aircraft.Weight * Constants.g);
                var dhdt = sgamma * V_cl;

                if (dhdt / V_cl < 0.024 + 0.006 * (aircraft.Wing.EnginesPerWing - 1))
                {
                    return FAR25.TakeOffTooShallow;
                }
            }

            // Landing
            {
                var V = 1.5f * V_s;
                var T = (aircraft.Wing.EngineDesignThrust * 1000f * aircraft.Wing.EnginesPerWing * 2f - 1);
                var D = CalculateTotalDrag(aircraft, V, 0, 0.7f);

                var sgamma = (T - D) / (aircraft.Weight * Constants.g);
                var dhdt = sgamma * V;

                if (dhdt / V < 0.021 + 0.006 * (aircraft.Wing.EnginesPerWing - 1))
                {
                    return FAR25.LandingTooSteep;
                }

            }

            return FAR25.Success;
        }

        private static float CalculateTotalDrag(Aircraft aircraft, float velocity, float altitude, float flaps)
        {
            var C_L = CalculateC_L(aircraft, 0, flaps);

            var C_D0 = CalculateC_D0(aircraft, velocity, 0);

            var S_ref = aircraft.Wing.Area / 100;

            var rho = CalculateDensity(altitude);

            var q = 0.5f * rho * S_ref * Mathf.Pow(velocity, 2);

            return q * (C_D0 + (Mathf.Pow(C_L, 2) / (Mathf.PI * aircraft.Wing.AspectRatio)));
        }
    }
}

public enum Trimmability
{
    Trimmable,
    LiftNotAdequate,
    RotationallyUnstable
}

public enum FAR25
{
    Success,
    TakeOffTooShallow,
    LandingTooSteep
}
