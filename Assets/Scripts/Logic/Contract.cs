using Assets.Scripts.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Logic
{
    public class Contract
    {
        public List<Requirement> Requirements { get; set; }

        public Contract()
        {
            Requirements = new List<Requirement>();
        }
    }

    public class Requirement
    {
        public string Text { get; set; }
        public Func<Aircraft, bool> Evaluator { get; set; }
    }
}
