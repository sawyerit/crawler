using Commoner.Core.Unity;
using CrawlDaddy.Core;
using CrawlDaddy.Plugin;
using Drone.API.Dig;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CrawlDaddy.Test.Unit.Plugin
{
    [TestFixture]
    public class DigTest
    {

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void GetProviders_NoRegisteredProcessors_ReturnsEmptyCollection()
        {
            Dig dig = Dig.Instance;

            dig.GetWebHostName("mybenchcoach.com");
            
        }
    }

}
