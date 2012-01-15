using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public class DecisionPoint
    {
        public Vector2 Position { get; set; }
        public Func<bool> Condition { get; set; }
    }
}
