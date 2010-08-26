using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace RaspberryRoad.Xna.Collision.Tests
{
    [TestFixture]
    public class MovingCollisionDetectorTests
    {
        Vector2 position;
        Rectangle2 rectangle;
        Vector2 movement;
        Line2 verticalLine;
        Line2 horizontalLine;

        [SetUp]
        public void SetUp()
        {
            position = new Vector2(0, 0);
            rectangle = new Rectangle2(-1, 1, 1, -1);
            movement = new Vector2(10, 0);
            verticalLine = new Line2(5, -5, 5, 5);
            horizontalLine = new Line2(-5, 5, 5, 5);
        }

        [Test]
        public void Collides_PointStopsShortOfLine_ReturnsFalse()
        {
            var movement = new Vector2(4, 0);

            Assert.IsFalse(CollisionDetector.Collides(position, movement, verticalLine));
        }

        [Test]
        public void Collides_PointMovesAcrossLine_ReturnsTrue()
        {
            Assert.IsTrue(CollisionDetector.Collides(position, movement, verticalLine));
        }

        [Test]
        public void Collides_PointMovesAcrossLineFromBehind_ReturnsFalse()
        {
            var position = new Vector2(10, 0);
            var movement = new Vector2(-10, 0);
            
            Assert.IsFalse(CollisionDetector.Collides(position, movement, verticalLine));
        }

        [Test]
        public void FindCollision_PointStopsShortOfLine_ReturnsNull()
        {
            var movement = new Vector2(4, 0);

            Assert.IsNull(CollisionDetector.FindCollision(position, movement, verticalLine));
        }

        [Test]
        public void FindCollision_PointMovesAcrossLine_Returns0p5()
        {
            Assert.AreEqual(0.5f, CollisionDetector.FindCollision(position, movement, verticalLine));
        }

        [Test]
        public void FindCollision_PointMovesAcrossLineFromBehind_ReturnsNull()
        {
            var position = new Vector2(10, 0);
            var movement = new Vector2(-10, 0);

            Assert.IsNull(CollisionDetector.FindCollision(position, movement, verticalLine));
        }

        [Test]
        public void FindMaxMovement_PointStopsShortOfLine_ReturnsMovement()
        {
            var movement = new Vector2(4, 0);

            Assert.AreEqual(movement, CollisionDetector.FindMaxMovement(position, movement, verticalLine));
        }

        [Test]
        public void FindMaxMovement_PointMovesAcrossLine_ReturnsHalfOfMovement()
        {
            Assert.AreEqual(new Vector2(5, 0), CollisionDetector.FindMaxMovement(position, movement, verticalLine));
        }

        [Test]
        public void FindMaxMovement_PointMovesAcrossLineFromBehind_ReturnsMovement()
        {
            var position = new Vector2(10, 0);
            var movement = new Vector2(-10, 0);

            Assert.AreEqual(movement, CollisionDetector.FindMaxMovement(position, movement, verticalLine));
        }

        [Test]
        public void Collides_RectangleMovesFullyAcrossLine_ReturnsTrue()
        {
            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        [Test]
        public void Collides_RectangleStopsShortOfLine_ReturnsFalse()
        {
            movement = new Vector2(2, 0);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        //       |
        //       |
        //       |
        // +-+  +-+
        // |A|  |B|
        // +-+  +-+
        //       |
        //       |
        //       |
        [Test]
        public void Collides_RectangleMovesHalfwayAcrossLineFromLeft_ReturnsTrue()
        {
            movement = new Vector2(5, 0);

            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        //  |
        //  |
        //  |
        // +-+  +-+
        // |B|  |A|
        // +-+  +-+
        //  |
        //  |
        //  |
        [Test]
        public void Collides_RectangleMovesHalfwayAcrossLineFromRight_ReturnsTrue()
        {
            movement = new Vector2(-5, 0);
            verticalLine = new Line2(-5, 5, -5, -5);

            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        //  
        // +-+  +-+
        // |B|  |A|
        // +-+  +-+
        //  |
        //  |
        //  |
        [Test]
        public void Collides_RectangleMovesHalfwayAcrossHalfLineFromRight_ReturnsTrue()
        {
            movement = new Vector2(-5, 0);
            verticalLine = new Line2(-5, 0, -5, -5);

            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        //       |
        //       |
        //       |
        // +-+  +-+
        // |A|  |B|
        // +-+  +-+
        //       
        [Test]
        public void Collides_RectangleMovesHalfwayAcrossHalfLineFromLeft_ReturnsTrue()
        {
            movement = new Vector2(5, 0);
            verticalLine = new Line2(5, 0, 5, 5);

            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        // +-------+
        // |       |
        // |     | |
        // |   B | |
        // |     | |
        // |       |
        // +-------+
        [Test]
        public void Collides_RectangleEnvelopsLineFromLeft_ReturnsTrue()
        {
            rectangle = new Rectangle2(0, -3, 8, 3);
            verticalLine = new Line2(10, -1, 10, 1);
            movement = new Vector2(4, 0);

            Assert.IsTrue(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        [Test]
        public void FindCollision_RectangleEnvelopsLineFromLeft2_Returns0p25()
        {
            rectangle = new Rectangle2(-50, 25, -25, -25);
            verticalLine = new Line2(0, -10, 0, 10);
            movement = new Vector2(100, 0);

            Assert.AreEqual(0.25f, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        [Test]
        public void Collides_RectangleMovesParallelToLine_ReturnsFalse()
        {
            movement = new Vector2(0, 1);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }
        
        [Test]
        public void Collides_RectangleMovesAboveLine_ReturnsFalse()
        {
            verticalLine = new Line2(5, -10, 5, -5);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        [Test]
        public void Collides_RectangleMovesUnderLine_ReturnsFalse()
        {
            verticalLine = new Line2(5, 5, 5, 10);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        // 
        // 
        // +-----+
        // |     |/
        // |     |
        // |    /|
        // |     |
        // +-----+
        //   
        //
        [Test]
        public void Collides_RectangleMovesIntoDiagonalLineAndOnlyOneCornerEndsUpOnTheOtherSideOfTheLine_ReturnsFalse()
        {
            rectangle = new Rectangle2(-2, 2, 2, -2);
            var diagonalLine = new Line2(5, -1, 8, 1);
            movement = new Vector2(5, 0);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, diagonalLine));
        }

        //       |
        //       |
        //       |
        // +-+ +-+   +-+
        // |A| |B|   |C|
        // +-+ +-+   +-+
        //       |
        //       |
        //       |
        [Test]
        public void FindCollision_RectangleMovesFullyAcrossLineFromLeft_Returns0p4()
        {
            Assert.AreEqual(0.4f, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        //       |
        //       |
        //       |
        // +-+   +-+ +-+
        // |C|   |B| |A|
        // +-+   +-+ +-+
        //       |
        //       |
        //       |
        [Test]
        public void FindCollision_RectangleMovesFullyAcrossLineFromRight_Returns0p4()
        {
            rectangle = new Rectangle2(9, -1, 11, 1);
            movement = new Vector2(-10, 0);
            verticalLine = new Line2(5, 5, 5, -5);

            Assert.AreEqual(0.4f, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        //       |
        //       |
        //       |
        //   +---+-+-+
        //   |   | | |
        //   | B |A|C|
        //   |   | | |
        //   +---+-+-+
        //       |
        //       |
        //       |
        [Test]
        public void FindCollision_RectangleStartsInsideLineMovesLeft_ReturnsMinus1()
        {
            rectangle = new Rectangle2(-2, -2, 2, 2);
            movement = new Vector2(2, 0);
            verticalLine = new Line2(0, -5, 0, 5);

            Assert.AreEqual(-1, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        [Test]
        public void FindCollision_RectangleStartsInsideLineMovesRight_ReturnsMinus1()
        {
            rectangle = new Rectangle2(-2, -2, 2, 2);
            movement = new Vector2(-2, 0);
            verticalLine = new Line2(0, 5, 0, -5);

            Assert.AreEqual(-1, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        [Test]
        public void FindCollision_RectangleStartsInsideLineItFullyEnvelopsMovesLeft_ReturnsMinus1()
        {
            rectangle = new Rectangle2(-2, -2, 2, 2);
            movement = new Vector2(2, 0);
            verticalLine = new Line2(0, -1, 0, 1);

            Assert.AreEqual(-1, CollisionDetector.FindCollision(rectangle, movement, verticalLine).Value.Value);
        }

        [Test]
        public void Collides_RectangleMovesAcrossLineButUnderIt_ReturnsFalse()
        {
            rectangle = new Rectangle2(-20, 20, 20, -20);
            verticalLine = new Line2(50, -50, 50, 50);
            movement = new Vector2(50, -150);

            Assert.IsFalse(CollisionDetector.Collides(rectangle, movement, verticalLine));
        }

        [Test]
        public void FindCollision_RectangleCollidesWithSideOfLine_ReturnsNull()
        {
            rectangle = new Rectangle2(-50, 25, 0, -25);
            horizontalLine = new Line2(50, -50, 100, -50);
            movement = new Vector2(100, -100);

            Assert.IsNull(CollisionDetector.FindCollision(rectangle, movement, horizontalLine));
        }
    }
}
