using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using linxUnit;

namespace RaspberryRoad.TempusFugit.Tests
{
    public class PositionTests: TestCase
    {
        public PositionTests(string name)
            : base(name)
        {
        }

        public void Constructor_CoordinatesAreZero()
        {
            Position position = new Position();

            Assert.AreEqual(0f, position.X);
            Assert.AreEqual(0f, position.Y);
        }

        public void Constructor_InitialValues_CoordinatesEqualInitialValues()
        {
            Position position = new Position(1f, 2f);

            Assert.AreEqual(1f, position.X);
            Assert.AreEqual(2f, position.Y);
        }
    }
}
