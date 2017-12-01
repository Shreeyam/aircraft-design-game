using UnityEngine;

namespace Assets.Scripts.Geometry
{
    /// <summary>
    /// Class that all cross sections should inherit from
    /// </summary>
    public class CrossSection
    {
        public Vector3[] Points { get; set; }

        /// <summary>
        /// Distance of cross section from the origin
        /// </summary>
        public Vector3 Direction { get; set; }
        public Vector3 Offset { get; set; }
        public float Distance { get; set; }
    }

}
