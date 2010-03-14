using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RaspberryRoad.TempusFugit
{
    public class SpecialEffect
    {
        float time = 0f;
        Model model;
        Position position;
        Trigger trigger;
        Func<float, Matrix> scale;
        Func<float, float> alpha;
        Random r;

        public SpecialEffect(Model model, Position position, Trigger trigger, Func<float, Matrix> scale, Func<float, float> alpha)
        {
            this.model = model;
            this.position = position;
            this.trigger = trigger;
            this.scale = scale;
            this.alpha = alpha;
            r = new Random();
        }

        public bool IsActive()
        {
            return time < 2.5f;
        }

        public void Update(float dt)
        {
            time += dt;
            if (time > 1f)
            {
                if (trigger != null)
                {
                    trigger.Fire();
                    trigger = null;
                }
            }
        }

        public Model GetModel()
        {
            return model;
        }

        public Matrix GetMatrix()
        {
            return
                scale(time) *
                //Matrix.CreateRotationX(time * 200) * Matrix.CreateRotationY(time * 1000) *
                Matrix.CreateRotationX((float)r.NextDouble() * 6.28f) * Matrix.CreateRotationY((float)r.NextDouble() * 6.27f) *
                Matrix.CreateTranslation(new Vector3(position.X, 1, 0));
        }

        public float GetAlpha()
        {
            return alpha(time);
        }
    }
}
