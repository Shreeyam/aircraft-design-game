using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class Areas
    {
        public static float Cone(float r, float h)
        {
            return Mathf.PI * (Mathf.Sqrt(Mathf.Pow(h, 2) + Mathf.Pow(r, 2)));
        }

        public static float Cylinder(float r, float h)
        {
            return 2 * Mathf.PI * r * h;
        }
    }
}
