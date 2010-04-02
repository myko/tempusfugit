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
            suite.add(TestCase.CreateSuite(typeof(TimeTests)));

            TestResult result = new TestResult();
            suite.run(result);

            foreach (var failure in result.failures)
                Console.WriteLine(failure.message + ": " + failure.exception.Message);

            Console.WriteLine(result.summary());
            Console.ReadKey();
        }
    }
}
