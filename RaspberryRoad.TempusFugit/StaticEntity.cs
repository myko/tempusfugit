using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RaspberryRoad.Xna.Collision;

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

        public override IEnumerable<Line2> GetCollisionGeometry()
        {
            return Enumerable.Empty<Line2>();
        }
    }
}
