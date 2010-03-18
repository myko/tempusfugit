using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaspberryRoad.TempusFugit
{
    public class Ground: Entity
    {
        public Ground(Model model)
            : base(model)
        {
        }

        public override Matrix GetMatrix()
        {
            return Matrix.Identity;
        }
    }
}
