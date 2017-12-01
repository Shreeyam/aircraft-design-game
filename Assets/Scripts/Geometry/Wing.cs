using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class Wing : MirroredLiftDevice
    {
        public float XPercentage { get; set; }
        public List<Engine> Engines { get; set; }
        public int EnginesPerWing { get; set; }
        public float EngineBypassRatio { get; set; }
        public float EngineDesignThrust { get; set; }
        public Fuel Fuel { get; set; }

        public HighLiftDevice Flaps { get; set; }

        public override float Dihedral
        {
            get
            {
                return 0.06f;
            }
        }

        public Wing()
        {
            //ThicknessChordRatio = Constants.Defaults.Wing.ThicknessChordRatio;
            Sweep = Constants.Defaults.Wing.Sweep;
            TaperRatio = Constants.Defaults.Wing.TaperRatio;
            FuselageDiameter = Constants.Defaults.Fuselage.Diameter;
            CentreFuselageLength = Constants.Defaults.Fuselage.CentreFuselageLength;
            XPercentage = Constants.Defaults.Wing.XPercentage;
            Area = Constants.Defaults.Wing.Area;
            AspectRatio = Constants.Defaults.Wing.AspectRatio;

            EnginesPerWing = Constants.Defaults.Wing.EnginesPerWing;
            EngineBypassRatio = Constants.Defaults.Wing.EngineBypassRatio;
            EngineDesignThrust = Constants.Defaults.Wing.EngineDesignThrust;

            Fuel = new Fuel
            {
                Volume = Constants.Defaults.Wing.FuelVolume
            };

            Flaps = new HighLiftDevice
            {
                HighLiftDeviceType = HighLiftDeviceType.Plain

            };

            UpdateSections();
        }

        public override float GetXPosition()
        {
            return (CentreFuselageLength - RootChord) * XPercentage;

        }

        public override void UpdateSections()
        {
            base.UpdateSections();

            var initial = 0.18f - (0.01f * EnginesPerWing);
            var increment = 0.4f/EnginesPerWing;

            Engines = new List<Engine>();

            for(int i = 0; i < EnginesPerWing; i++)
            {
                Engines.Add(new Engine
                {
                    Root = new Vector3((initial + i * increment) * Span, 2 * (initial + i * increment) * Span * Dihedral, -GetXPosition() - ((2 * (initial + i * increment)) * Span * Mathf.Tan(Sweep * Mathf.Deg2Rad))),
                    BypassRatio = EngineBypassRatio,
                    DesignThrust = EngineDesignThrust
                });

                Engines.Add(new Engine
                {
                    Root = new Vector3(-(initial + i * increment) * Span, 2 * (initial + i * increment) * Span * Dihedral, -GetXPosition() - ((2 * (initial + i * increment)) * Span * Mathf.Tan(Sweep * Mathf.Deg2Rad))),
                    BypassRatio = EngineBypassRatio,
                    DesignThrust = EngineDesignThrust
                });
            }

            Engines.ForEach(x => x.UpdateSections(FuselageDiameter));

            Flaps.UpdateSections(Span, Dihedral, GetOffset(), Sweep, ThicknessChordRatio, TaperRatio, RootChord);
        }
    }

    public class Fuel
    {
        private float volume;
        public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public float Weight
        {
            get { return volume * Constants.Defaults.Wing.FuelDensity; }
            set { volume = value / Constants.Defaults.Wing.FuelDensity; }
        }
    }
}
