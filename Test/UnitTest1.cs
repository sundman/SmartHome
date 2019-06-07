using System;
using Logger;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }


        private class TestEvent : ILoggable
        {
            public string TestString { get; set; }
            public int TestInt { get; set; }
            public Guid TestGuid { get; set; }
        }

        [Test]
        public void Test2()
        {
            var testEvent = new TestEvent()
            {
                TestString = "en textx",
                TestGuid = Guid.NewGuid(),
                TestInt = 42
            };
            var logger = new SplunkLogger();

            logger.Info(this, testEvent);

            logger.FlushLogs();

            
        }
    }
}