using System;
using System.Collections.Generic;
using System.Text;
using linxUnit;

namespace RaspberryRoad.TempusFugit.Tests
{
    public class TimeTest : TestCase
    {
        public TimeTest(string name)
            : base(name)
        {
        }

        public void testTime()
        {
            // Arrange
            Time time = new Time();

            // Act

            // Assert
            Assert.AreEqual(0, time.GlobalTimeCoordinate);
        }

        public void testUpdateGameTime()
        {
            // Arrange
            Time time = new Time();

            // Act
            time.UpdateGameTime(12.34f);

            // Assert
            Assert.AreEqual(308, time.GlobalTimeCoordinate);
        }

        public void testJumpTo()
        {
            // Arrange
            Time time = new Time();

            // Act
            time.JumpTo(12.34f);

            // Assert
            Assert.AreEqual(12, time.GlobalTimeCoordinate);
        }
    }
}
