
using Abot.Poco;
using CrawlDaddy.Core.Poco;
using log4net;
using System;
using System.Collections.Generic;
namespace CrawlDaddy.Plugin.Processor
{
    public abstract class CrawlProcessorBase
    {
        static ILog _logger = LogManager.GetLogger(typeof(CrawlProcessorBase).FullName);
        
        protected void PageSave(CrawlContext crawlContext, CrawledPage foundOnPage, ProcessorResult processorResult)
        {
            ProcessorContext processorContext = crawlContext.CrawlBag.GoDaddyProcessorContext;
            DataComponent dataComponent = new DataComponent 
            { 
                ShopperId = processorContext.Domain.ShopperId,
                AttributeId = processorResult.UniqueAttributeId, 
                DomainId = processorContext.Domain.DomainId,
                Attributes = processorResult.Attributes,
                DomainUri = crawlContext.RootUri,
                FoundOnUri = (foundOnPage == null) ? null : foundOnPage.Uri
            };

            try
            {
                processorContext.PrimaryPersistenceProvider.Save(dataComponent);
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Error while trying to save page level data to primary IPersistenceProvider [{0}], will save to backup IPersistenceProvider [{1}]."
																		, processorContext.PrimaryPersistenceProvider.ToString()
																		, processorContext.BackupPersistenceProvider.ToString());
                _logger.Error(e);

                processorContext.BackupPersistenceProvider.Save(dataComponent);
            }
        }

				protected void DomainSave(CrawlContext crawlContext, ProcessorResult processorResult)
				{
					ProcessorContext processorContext = crawlContext.CrawlBag.GoDaddyProcessorContext;
					DataComponent dataComponent = new DataComponent
					{
						ShopperId = processorContext.Domain.ShopperId,
						AttributeId = processorResult.UniqueAttributeId,
						DomainId = processorContext.Domain.DomainId,
						Attributes = processorResult.Attributes,
						DomainUri = crawlContext.RootUri,
						FoundOnUri = null
					};

					try
					{
						processorContext.PrimaryPersistenceProvider.Save(dataComponent);
					}
					catch (Exception e)
					{
						_logger.ErrorFormat("Error while trying to save domain level data to primary IPersistenceProvider [{0}], will save to backup IPersistenceProvider [{1}]."
																, processorContext.PrimaryPersistenceProvider.ToString()
																, processorContext.BackupPersistenceProvider.ToString());
						_logger.Error(e);

						processorContext.BackupPersistenceProvider.Save(dataComponent);
					}
				}
    }
}
