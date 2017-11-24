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
    public class ConstantContactCrawledPageProcessorTest
    {
        ConstantContactCrawlProcessor _uut;
        CrawlContext _crawlContext;
        CrawledPage _crawledPage;
        Mock<IPersistenceProvider> _fakePrimaryPersistence;
        Mock<IPersistenceProvider> _fakeBackupPersistence;

        [SetUp]
        public void SetUp()
        {
            _uut = new ConstantContactCrawlProcessor();
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
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByAnchorTag_CallPersistenceProvider()
        {
            AssertConstantContactCustomer("<a href='http://constantcontact.com/'>some link</a>");
            AssertConstantContactCustomer("<a HReF='http://constantcontact.com/'>some link</a>");
            AssertConstantContactCustomer("<a         href  =  'http://constantcontact.com/'>some link");
            AssertConstantContactCustomer("<a href='http://CONSTANTCONTACT.COM/'>some link</a>");
            AssertConstantContactCustomer("<a href='http://aaa.bbconstantcontact.com/'>some link</a>");
            AssertConstantContactCustomer("<a href='http://aaa.bbCONSTANTCONTACT.com/'>some link</a>");
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByFormTag_CallPersistenceProvider()
        {
            AssertConstantContactCustomer("<form action='http://constantcontact.com/'></form>");
            AssertConstantContactCustomer("<form action='http://CONSTANTCONTACT.COM/'></form>");
            AssertConstantContactCustomer("<form action='http://aaa.bbconstantcontact.com/'></form>");
            AssertConstantContactCustomer("<form action='http://aaa.bbCONSTANTCONTACT.com/'></form>");
            AssertConstantContactCustomer("<form name='ccoptin'></form>");
            AssertConstantContactCustomer("<form name='CCOPTIN'></form>");
            AssertConstantContactCustomer("<form id='ccoptin'></form>");
            AssertConstantContactCustomer("<form id='CCOPTIN'></form>");
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByWordPressRegisterDialog_CallPersistenceProvider()
        {
            AssertConstantContactCustomer(@"<div style=""margin-bottom:16px""><label for=""cc_newsletter""><span class=""cc_signup_title"">cctitle</span><input type=""hidden"" name=""cc_newsletter[]"" value=""0"" /> <input type=""checkbox"" id=""cc_newsletter"" name=""cc_newsletter[]"" class=""checkbox"" value=""1"" /> </label><div><input type=""hidden"" name=""cc_newsletter[]"" value=""1"" /></div><div class='description'><p>ccdesc</p></div></div>");
            AssertConstantContactCustomer(@"<div style=""margin-bottom:16px""><label for=""cc_newsletter""><span class=""cc_signup_title"">cctitle</span><input type=""hidden"" name=""CC_NEWSLETTER[]"" value=""0"" /> <INPUT type=""checkbox"" id=""CC_NEWSLETTER"" name=""CC_NEWSLETTER[]"" class=""checkbox"" value=""1"" /> </label><div><input type=""hidden"" name=""cc_newsletter[]"" value=""1"" /></div><div class='description'><p>ccdesc</p></div></div>");
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByWordPressRegisterDialog2_CallPersistenceProvider()
        {
            AssertConstantContactCustomer(@"<div style=""margin-bottom:16px""><label style=""display: block; margin-bottom: 5px;""><span class=""cc_signup_title"" style=""display:block;"">cctitle</span></label><label for=""cc_newsletter_1"" style=""display:block; margin:.25em 0;""><input type=""checkbox"" id=""cc_newsletter"" name=""cc_newsletter[]"" class=""checkbox"" value=""1"" /> General Interest</label><div class='description'><p>ccdesc</p></div></div>");
            AssertConstantContactCustomer(@"<div style=""margin-bottom:16px""><label style=""display: block; margin-bottom: 5px;""><span class=""cc_signup_title"" style=""display:block;"">cctitle</span></label><label for=""cc_newsletter_1"" style=""display:block; margin:.25em 0;""><input type=""checkbox"" id=""CC_NEWSLETTER"" name=""CC_NEWSLETTER[]"" class=""checkbox"" value=""1"" /> General Interest</label><div class='description'><p>ccdesc</p></div></div>");
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedBySignUpWidget_CallPersistenceProvider()
        {
            string content = @"<aside id=""constant_contact_api_widget-2"" class=""widget constant-contact-signup""><h3 class=""widget-title"">cc signup</h3><p>cc description Subscribe to our newsletter</p>
<form action=""http://jvtestcontantcontact.com/wp/?page_id=2"" method=""post"" id=""constant-contact-signup"" name=""constant-contact-signup"">
				<label for=""cc_email"">Email:</label>
				<div class=""input-text-wrap"">
					<input name=""fields[email_address][value]"" id=""cc_email"" value="""" type=""text"">
				</div>
				<div>
					<input name=""cc_newsletter[]"" value=""1"" type=""hidden"">

					<input id=""cc_referral_url"" name=""cc_referral_url"" value=""http%3A%2F%2Fjvtestcontantcontact.com%2Fwp%2F%3Fpage_id%3D2"" type=""hidden"">
					<input id=""cc_redirect_url"" name=""cc_redirect_url"" value="""" type=""hidden"">
					<input name=""constant-contact-signup-submit"" value=""Sign Up"" class=""button submit"" type=""submit"">
					<input name=""uniqueformid"" value=""constant_contact_api_widget-2"" type=""hidden"">
				</div>
			</form></aside>";
            AssertConstantContactCustomer(content);

            content = @"<aside id=""constant_contact_api_widget-2"" class=""widget constant-contact-signup""><h3 class=""widget-title"">cc signup</h3><p>cc description Subscribe to our newsletter</p>
<FORM ACTIon=""http://jvtestcontantcontACT.com/wp/?page_id=2"" method=""post"" id=""constant-contact-signup"" name=""constant-contact-signup"">
				<label for=""cc_email"">Email:</label>
				<div class=""input-text-wrap"">
					<input name=""fields[email_address][value]"" id=""cc_email"" value="""" type=""text"">
				</div>
				<div>
					<input name=""cc_newsletter[]"" value=""1"" type=""hidden"">

					<input id=""cc_referral_url"" name=""cc_referral_url"" value=""http%3A%2F%2Fjvtestcontantcontact.com%2Fwp%2F%3Fpage_id%3D2"" type=""hidden"">
					<input id=""cc_redirect_url"" name=""cc_redirect_url"" value="""" type=""hidden"">
					<input name=""constant-contact-signup-submit"" value=""Sign Up"" class=""button submit"" type=""submit"">
					<input name=""uniqueformid"" value=""constant_contact_api_widget-2"" type=""hidden"">
				</div>
			</form></aside>";
            AssertConstantContactCustomer(content);
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByWordPressEventsWidget_CallPersistenceProvider()
        {
            string content = @"<aside id=""constant_contact_events_widget-2"" class=""widget constant-contact-events""><h3 class=""widget-title"">ccevent</h3>
	<div class=""cc_event_description"">
		<p>ccevent desc</p>
</div><p>There are currently no events.</p></aside>";
            AssertConstantContactCustomer(content);

            content = @"<ASIDE id=""cONStant_contact_events_widget-2"" class=""widget constant-contact-events""><h3 class=""widget-title"">ccevent</h3>
	<div class=""cc_event_description"">
		<p>ccevent desc</p>
</div><p>There are currently no events.</p></aside>"; 
            AssertConstantContactCustomer(content);
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_IdentifiedByWordPressFormWidget_CallPersistenceProvider()
        {
            string content = @"<aside id=""constant contact"" class=""widget widget_gConstantcontact""><h3 class=""widget-title"">ccheader</h3><div>
    <div class=""cc_caption"">Monthly Hints &amp; Tips newsletter</div>
    <div class=""cc_msg""><span id=""gConstantcontact_msg""></span></div>
  <div class=""cc_textbox"">
    <input class=""cc_textbox_class"" name=""gConstantcontact_email"" id=""gConstantcontact_email"" onkeypress=""if(event.keyCode==13) gConstantcontact('http://jvtestcontantcontact.com/wp/wp-content/plugins/constant-contact/gCls/')"" onblur=""if(this.value=='') this.value='Enter email address.';"" onfocus=""if(this.value=='Enter email address.') this.value='';"" value=""Enter email address."" maxlength=""120"" type=""text"">
  </div>
  <div class=""cc_button"">
    <input class=""cc_textbox_button"" name=""gConstantcontact_Button"" onclick=""return gConstantcontact('http://jvtestcontantcontact.com/wp/wp-content/plugins/constant-contact/gCls/')"" id=""gConstantcontact_Button"" value=""Submit"" type=""button"">
  </div>
</div>
</aside>";
            AssertConstantContactCustomer(content);

            content = @"<aside id=""constant contact"" class=""widget widget_gConstantcontact""><h3 class=""widget-title"">ccheader</h3><div>
    <div class=""cc_caption"">Monthly Hints &amp; Tips newsletter</div>
    <div class=""cc_msg""><span id=""gConstantcontact_msg""></span></div>
  <div class=""cc_textbox"">
    <input class=""cc_textbox_class"" name=""gConstantcontact_email"" id=""gConstantcontact_email"" onkeypress=""if(event.keyCode==13) gConstantcontact('http://jvtestcontantcontact.com/wp/wp-content/plugins/constant-contact/gCls/')"" onblur=""if(this.value=='') this.value='Enter email address.';"" onfocus=""if(this.value=='Enter email address.') this.value='';"" value=""Enter email address."" maxlength=""120"" type=""text"">
  </div>
  <div class=""cc_button"">
    <input class=""cc_textbox_button"" name=""gCONSTANTCONTACT_Button"" onclick=""return gConstantcontact('http://jvtestcontantcontact.com/wp/wp-content/plugins/constant-contact/gCls/')"" id=""gConstantcontact_Button"" value=""Submit"" type=""button"">
  </div>
</div>
</aside>";
            AssertConstantContactCustomer(content);
        }

        [Test]
        public void ProcessCrawledPage_IsConstantContactCustomer_PrimaryIPersistenceProviderThrowsException_BackupIPersistenceProviderCalled()
        {
            _fakePrimaryPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Primary provider down...."));
            _crawledPage.RawContent = "<a href='http://constantcontact.com/'>some link</a>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(1));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Oh no!! Backup provider down to....")]
        public void ProcessCrawledPage_IsConstantContactCustomer_PrimaryIPersistenceProviderAndPrimaryIPersistenceProviderThrowsException()
        {
            _fakePrimaryPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Primary provider down...."));
            _fakeBackupPersistence.Setup(f => f.Save(It.IsAny<DataComponent>())).Throws(new Exception("Oh no!! Backup provider down to...."));
            _crawledPage.RawContent = "<a href='http://constantcontact.com/'>some link</a>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);
        }

        [Test]
        public void ProcessCrawledPage_IsNotConstantContactCustomer_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<a href='http://aaa.bbb.com/'>some link</a><form id='aaa' name='bbb' action='http://aa.bb.com/'></form><label for='ccoo'><label for='cc_'>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotConstantContactCustomer_EmptyContent_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotConstantContactCustomer_AnchorNoHref_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<a></a>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_IsNotConstantContactCustomer_FormNoIdNameOrAction_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<form method='post'></form>";

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
        public void ProcessCrawledPage_IsConstantContactCustomer_OnMultiplePages_OnlyScansAndSavesFromFirstPage()
        {
            //Arrange
            _crawledPage.RawContent = "<a href='http://aaa.bbconstantcontact.com/'>some link</a>";
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);//1st page found that is a constant contact customer

            //Act
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            //Assert
            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d => d.AttributeId == 222 && d.DomainId == 111 && d.Attributes.ContainsKey("mailProvider") && d.Attributes["mailProvider"] == "ConstantContact")), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            //Clear all mocks/values
            SetUp();
        }

        private void AssertConstantContactCustomer(string html)
        {
            //Arrange
            _crawledPage.RawContent = html;

            //Act
            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            //Assert
            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d => d.AttributeId == 222 && d.DomainId == 111 && d.Attributes.ContainsKey("mailProvider") && d.Attributes["mailProvider"] == "ConstantContact")), Times.Exactly(1));
            _fakeBackupPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
            //Clear all mocks/values
            SetUp();
        }
    }
}
