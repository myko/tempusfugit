using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaspberryRoad.TempusFugit
{
    public class StaticEntity: Entity
    {
        private Matrix matrix;

        public StaticEntity(Model model, Matrix matrix)
            : base(model)
        {
            this.matrix = matrix;
        }

        public override Matrix GetMatrix()
        {
            return matrix;
        }
    }
}
