using System;
using System.Collections.Generic;
using System.Text;
using linxUnit;

namespace RaspberryRoad.TempusFugit.Tests
{
    public class TimeTests : TestCase
    {
        private Time time;

        public TimeTests(string name)
            : base(name)
        {
            time = new Time();
        }

        public void DefaultConstructor_GlobalTimeCoordinateIsZero()
        {
            // Assert
            Assert.AreEqual(0, time.GlobalTimeCoordinate);
        }

        public void UpdateGameTime_GlobalTimeCoordinateIsIncreasedByFactorOf25()
        {
            // Act
            time.UpdateGameTime(12.34f);

            // Assert
            Assert.AreEqual((int)(12.34f * 25), time.GlobalTimeCoordinate);
        }

        public void JumpTo_GlobalTimeCoordinateIsSetToArgument()
        {
            // Act
            time.JumpTo(12.34f);

            // Assert
            Assert.AreEqual(12, time.GlobalTimeCoordinate);
        }
    }
}
