using Microsoft.Xna.Framework;

namespace RaspberryRoad.TempusFugit
{
    public static class Vector2Extensions
    {
        public static Vector2 Normalized(this Vector2 v)
        {
            v.Normalize();
            return v;
        }
    }
}
