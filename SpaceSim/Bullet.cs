using Microsoft.Xna.Framework;

namespace SpaceSim
{
    class Bullet : Sphere
    {
        public Vector3 startPos;

        public Bullet(Vector3 position, Vector3 forward, Vector3 down) : base(Matrix.Identity, Color.White, 30, 0, 0.005f)
        {
            this.startPos = position;
            this.Transform *= Matrix.CreateTranslation(startPos);
            this.Transform *= Matrix.CreateTranslation(forward / 6);
            this.Transform *= Matrix.CreateTranslation(down / 20);
        }
    }
}