using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Geometry
{
    public class HorizontalStabilizer : MirroredLiftDevice
    {
        public float AreaRatio { get; set; }

        public override float Dihedral
        {
            get
            {
                return 0.1f;
            }
        }

        public override float FuselageMount
        {
            get
            {
                return 0.3f;
            }
        }
        public float AfterbodyLengthDiameterRatio { get; set; }

        public HorizontalStabilizer()
        {
            AfterbodyLengthDiameterRatio = Constants.Defaults.Fuselage.AfterbodyLengthDiameterRatio;

            // TODO?
            //ThicknessChordRatio = Constants.Defaults.HorizontalStabilizer.ThicknessChordRatio;
            Sweep = Constants.Defaults.HorizontalStabilizer.Sweep;
            TaperRatio = Constants.Defaults.HorizontalStabilizer.TaperRatio;
            FuselageDiameter = Constants.Defaults.Fuselage.Diameter;
            CentreFuselageLength = Constants.Defaults.Fuselage.CentreFuselageLength;
            Area = Constants.Defaults.Wing.Area * Constants.Defaults.HorizontalStabilizer.AreaRatio;
            AspectRatio = Constants.Defaults.HorizontalStabilizer.AspectRatio;
            AreaRatio = Constants.Defaults.HorizontalStabilizer.AreaRatio;

            UpdateSections();
        }

        public override float GetXPosition()
        {
            return CentreFuselageLength + (FuselageDiameter * AfterbodyLengthDiameterRatio) - RootChord;
        }
    }
}
