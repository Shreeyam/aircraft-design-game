using Assets.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Geometry
{
    public class Fuselage : Geometry
    {
        //public Nose Nose { get; set; }
        public CentreFuselage CentreFuselage { get; set; }
        public Afterbody Afterbody { get; set; }
        public float NoseLengthDiameterRatio { get; set; }

        // Shared Constants
        private float diameter;
        public float Diameter
        {
            get
            {
                return diameter;
            }

            set
            {
                diameter = value;

                //Nose.Diameter = value;
                CentreFuselage.Diameter = value;
                Afterbody.Diameter = value;

                UpdateSections();
            }
        }

        public override float CalculateArea()
        {
            // Convert to m^2
            return Areas.Cylinder(diameter/10, CentreFuselage.Length/10)
                + Areas.Cone(diameter/10, diameter/10 * Afterbody.AfterbodyLengthDiameterRatio);
        }

        public Fuselage()
        {
            //Nose = new Nose();
            CentreFuselage = new CentreFuselage();
            Afterbody = new Afterbody();

            Diameter = Constants.Defaults.Fuselage.Diameter;

            //Nose.Diameter = Diameter;
            //Nose.NoseLengthDiameterRatio = Constants.Defaults.NoseLengthDiameterRatio;
            //Nose.UpdateSections();

            CentreFuselage.Diameter = Diameter;
            CentreFuselage.Length = Constants.Defaults.Fuselage.CentreFuselageLength;
            CentreFuselage.UpdateSections();

            Afterbody.Diameter = Diameter;
            Afterbody.AfterbodyLengthDiameterRatio = Constants.Defaults.Fuselage.AfterbodyLengthDiameterRatio;
            Afterbody.UpdateSections();
        }

        public override IEnumerable<CrossSection> GetCrossSections()
        {
            // Nasty pass by reference bug here!
            //var noseSections = new List<CrossSection>(Nose.GetCrossSections().ToList());
            var centreFuselageSections = new List<CrossSection>(CentreFuselage.GetCrossSections().ToList());
            var afterBodySections = new List<CrossSection>(Afterbody.GetCrossSections().Skip(1).ToList());

            //centreFuselageSections
            //    .ForEach(x => x.Distance += noseSections.Last().Distance);

            //afterBodySections
            //   .ForEach(x => x.Distance += CentreFuselage.Length);

            var crossSections = centreFuselageSections
                .Concat(afterBodySections);

                crossSections.Last().Distance = Diameter * Afterbody.AfterbodyLengthDiameterRatio + CentreFuselage.Length;

            return crossSections;
        }

        public void UpdateSections()
        {
            //Nose.UpdateSections();
            CentreFuselage.UpdateSections();
            Afterbody.UpdateSections();
        }


    }
}
