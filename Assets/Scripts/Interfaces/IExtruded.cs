using Assets.Scripts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Interfaces
{
    public interface IExtruded
    {
        List<CrossSection> CrossSections { get; set; }
    }
}
