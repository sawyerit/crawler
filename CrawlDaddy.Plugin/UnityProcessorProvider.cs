using Commoner.Core.Unity;
using CrawlDaddy.Core;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace CrawlDaddy.Plugin
{
    public class UnityProcessorProvider : IProcessorProvider
    {
        public IEnumerable<ICrawlProcessor> GetProcessors()
        {
            IEnumerable<ICrawlProcessor> processors = Factory.Container.ResolveAll<ICrawlProcessor>();

            if (processors == null)
                throw new ApplicationException("Unity did not properly load plugins");

            return processors;
        }
    }
}
