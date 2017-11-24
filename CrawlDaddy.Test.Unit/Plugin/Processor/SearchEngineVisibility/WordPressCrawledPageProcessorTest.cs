using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using CrawlDaddy.Plugin.Processor.SearchEngineVisibility;
using Moq;
using NUnit.Framework;
using System;

namespace CrawlDaddy.Test.Unit.Plugin.Processor.SearchEngineVisibility
{
    [TestFixture]
    public class WordPressCrawledPageProcessorTest
    {
        WordPressCrawlProcessor _uut;
        CrawlContext _crawlContext;
        CrawledPage _crawledPage;
        Mock<IPersistenceProvider> _fakePrimaryPersistence;
        Mock<IPersistenceProvider> _fakeBackupPersistence;

        [SetUp]
        public void SetUp()
        {
            _uut = new WordPressCrawlProcessor();
            _fakePrimaryPersistence = new Mock<IPersistenceProvider>();
            _fakeBackupPersistence = new Mock<IPersistenceProvider>();

            _crawlContext = new CrawlContext();
            _crawlContext.CrawlBag.GoDaddyProcessorContext = new ProcessorContext
            {
                PrimaryPersistenceProvider = _fakePrimaryPersistence.Object,
                BackupPersistenceProvider = _fakeBackupPersistence.Object,
                Domain = new Domain
                {
                    DomainId = 111
                }
            };
            _crawledPage = new CrawledPage(new Uri("http://a.com/"));
            
        }

        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_IdentifiedByMetaTag_CallPersistenceProvider()
        {
            AssertWordPressCustomer("<meta name='generator' content='WordPress'>");
            AssertWordPressCustomer("<meta        name='generator' content='WordPress'>");
            AssertWordPressCustomer("<meta name='generator' content='WordPress3.1.1.1.1.1.1'>");
            AssertWordPressCustomer("<meta name='generator' content='WordPress'  >");
        }

        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_IdentifiedByAnchorTag_CallPersistenceProvider()
        {
            AssertWordPressCustomer("Powered by <a href='http://www.wordpress.org'>");
            AssertWordPressCustomer("Powered by <a    href='http://www.wordpress.org'>");
            AssertWordPressCustomer("Powered by    <a href   =    'http://www.wordpress.org'>");
            AssertWordPressCustomer("Powered        by <a href='http://www.wordpress.org'     >");
        }

        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_IdentifiedBySnippet_CallPersistenceProvider()
        {
            string content = @"<script type='text/javascript' src='http://www.machinsanity.com/wp-content/themes/response/elements/lib/js/elements.js?ver=3.5.1'></script>
<link rel=""EditURI"" type=""application/rsd+xml"" title=""RSD"" href=""http://www.machinsanity.com/xmlrpc.php?rsd"" />
<link rel=""wlwmanifest"" type=""application/wlwmanifest+xml"" href=""http://www.machinsanity.com/wp-includes/wlwmanifest.xml"" /> 
<meta name=""generator"" content=""WordPress 3.5.1"" />
			<link rel='alternate' type='application/rss+xml' title=""Forums RSS"" href=""http://www.machinsanity.com/wp-content/plugins/mingle-forum/feed.php?topic=all"" /> <style type=""text/css"">.ie8 .container {max-width: 1020px;width:auto;}</style>	<style type=""text/css"">.recentcomments a{display:inline !important;padding:0 !important;margin:0 !important;}</style>
		<style type=""text/css"">
			body { background-image: url( 'http://www.machinsanity.com/wp-content/themes/response/cyberchimps/lib/images/backgrounds/noise.jpg' ); }
		</style>";
            AssertWordPressCustomer(content);

            content = @"<div id=""footer-border""></div>
		<div id=""footer-inner"">
			<span class=""alignleft"">
			Copyright &copy; 2013 <strong><a href=""http://www.developermemes.com/"">Developer Memes</a></strong>
			<div id=""site-generator"">
			
				<small>Proudly powered by <a href=""http://wordpress.org"" target=""_blank"">WordPress</a>. <a href=""http://webtuts.pl/themes/"" title=""GamePress"" target=""_blank"">GamePress</a></small>

			</div><!-- #site-generator -->
			</span>
			<span class=""alignright""><a href=""#"" class=""scrollup"">BACK TO TOP &uarr;</a></span>
		</div>";
            AssertWordPressCustomer(content);
        }

        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_IdentifiedBySnippetLocalized_CallPersistenceProvider()
        {
            string content = @"<script type='text/javascript' src='http://www.machinsanity.com/wp-content/themes/response/elements/lib/js/elements.js?ver=3.5.1'></script>
<link rel=""EditURI"" type=""application/rsd+xml"" title=""RSD"" href=""http://www.machinsanity.com/xmlrpc.php?rsd"" />
<link rel=""wlwmanifest"" type=""application/wlwmanifest+xml"" href=""http://www.machinsanity.com/wp-includes/wlwmanifest.xml"" /> 
<meta name=""generator"" content=""WordPress 3.5.1"" />
			<link rel='alternate' type='application/rss+xml' title=""Forums RSS"" href=""http://www.machinsanity.com/wp-content/plugins/mingle-forum/feed.php?topic=all"" /> <style type=""text/css"">.ie8 .container {max-width: 1020px;width:auto;}</style>	<style type=""text/css"">.recentcomments a{display:inline !important;padding:0 !important;margin:0 !important;}</style>
		<style type=""text/css"">
			body { background-image: url( 'http://www.machinsanity.com/wp-content/themes/response/cyberchimps/lib/images/backgrounds/noise.jpg' ); }
		</style>";
            AssertWordPressCustomer(content);

            content = @"<div id=""footer-border""></div>
		<div id=""footer-inner"">
			<span class=""alignleft"">
			Copyright &copy; 2013 <strong><a href=""http://www.developermemes.com/"">Developer Memes</a></strong>
			<div id=""site-generator"">
			
				<small>Proudly powered by <a href=""http://eu-us.wordpress.org"" target=""_blank"">WordPress</a>. <a href=""http://webtuts.pl/themes/"" title=""GamePress"" target=""_blank"">GamePress</a></small>

			</div><!-- #site-generator -->
			</span>
			<span class=""alignright""><a href=""#"" class=""scrollup"">BACK TO TOP &uarr;</a></span>
		</div>";
            AssertWordPressCustomer(content);
        }


        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_PrimaryIPersistenceProviderThrowsException_BackupIPersistenceProviderCalled()
        {
            _fakePrimaryPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Primary provider down...."));
            _crawledPage.RawContent = "<meta name='generator' content='WordPress'>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(1));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Oh no!! Backup provider down to....")]
        public void ProcessCrawledPage_IsWordPressCustomer_PrimaryIPersistenceProviderAndPrimaryIPersistenceProviderThrowsException()
        {
            _fakePrimaryPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Primary provider down...."));
            _fakeBackupPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Backup provider down to...."));
            _crawledPage.RawContent = "<meta name='generator' content='WordPress'>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);
        }

        [Test]
        public void ProcessCrawledPage_IsNotWordPressCustomer_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<a href='http://aaa.bbb.com/'>some link</a><form id='aaa' name='bbb' action='http://aa.bb.com/'></form><label for='ccoo'><label for='cc_'>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotWordPressCustomer_EmptyContent_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotWordPressCustomer_AnchorNoHref_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "Proud <a></a>"; 

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotWordPressCustomer_MetaTagEmpty_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<meta content='post'></meta>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledDomain_DoesNothing()
        {
            _uut.ProcessCrawledDomain(null);
        }

        [Test]
        public void ProcessCrawledPage_IsWordPressCustomer_OnMultiplePages_OnlyScansAndSavesFromFirstPage()
        {
            //Arrange
            _crawledPage.RawContent = "<meta name='generator' content='WordPress'>";
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);//1st page found that is a constant contact customer

            //Act
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            //Assert
            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d => d.AttributeId == 222 && d.DomainId == 111 && d.Attributes.ContainsKey("siteBuilder") && d.Attributes["siteBuilder"] == "BlogWordPress")), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            //Clear all mocks/values
            SetUp();
        }

        private void AssertWordPressCustomer(string html)
        {
            //Arrange
            _crawledPage.RawContent = html;

            //Act
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            //Assert
            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d => d.AttributeId == 222 && d.DomainId == 111 && d.Attributes.ContainsKey("siteBuilder") && d.Attributes["siteBuilder"] == "BlogWordPress")), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            //Clear all mocks/values
            SetUp();
        }
    }
}
