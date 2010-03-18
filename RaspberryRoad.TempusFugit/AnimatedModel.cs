using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;

namespace RaspberryRoad.TempusFugit
{
    public class AnimatedModel
    {
        public Model Model { get; private set; }
        public AnimationPlayer AnimationPlayer { get; private set; }

        public AnimatedModel(Model model)
        {
            this.Model = model;

            // Look up our custom skinning information.
            SkinningData skinningData = model.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            AnimationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            AnimationPlayer.StartClip(clip);
            AnimationPlayer.Update(TimeSpan.Zero, true, Matrix.Identity);
        }
    }
}
