using Assets.Scripts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Metrics
{
    /// <summary>
    /// Structure used to store aircraft statistics for animations
    /// </summary>
    public class AircraftStats
    {
        public float Range { get; set; }
        public LandingGearAlignmentStatus LandingGearAlignmentStatus { get; set; }
        public float StaticMargin { get; set; }
        public float TakeoffDistance { get; set; }
        public float LandingDistance { get; set; }
        public Trimmability Trimmability { get; set; }
    }
}
