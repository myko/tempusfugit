using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using linxUnit;

namespace RaspberryRoad.TempusFugit.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSuite suite = new TestSuite();

            suite.add(TestCase.CreateSuite(typeof(TimeTest)));

            TestResult result = new TestResult();

            suite.run(result);

            Console.WriteLine(result.summary());
        }
    }
}
