using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace RaspberryRoad.Xna.Collision.Tests
{
    [TestFixture]
    public class StaticCollisionDetectorTests
    {
        Rectangle2 rectangle;
        Line2 line;

        [SetUp]
        public void SetUp()
        {
            rectangle = new Rectangle2(-1, 1, 1, -1);
            line = new Line2(0, -10, 0, 10);
        }

        [Test]
        public void FindCollision_RectangleIsOutsideLine_ReturnsNull()
        {
            line = new Line2(-10, -10, 10, -10);
            Assert.IsNull(CollisionDetector.FindCollision(rectangle, line));
        }

        [Test]
        public void FindCollision_RectangleIsInsideVerticalLine_ReturnsAVectorPushingItLeft()
        {
            Assert.AreEqual(new Vector2(-1, 0), CollisionDetector.FindCollision(rectangle, line));
        }

        [Test]
        public void FindCollision_RectangleIsInsideHorizontalLine_ReturnsAVectorPushingItUp()
        {
            line = new Line2(-10, 0, 10, 0);
            Assert.AreEqual(new Vector2(0, 1), CollisionDetector.FindCollision(rectangle, line));
        }
    }
}
