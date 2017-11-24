using Abot.Poco;
using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using CrawlDaddy.Plugin.Processor.SearchEngineMarketing;
using Moq;
using NUnit.Framework;
using System;

namespace CrawlDaddy.Test.Unit.Plugin.Processor.SearchEngineMarketing
{
    [TestFixture]
    public class SemKeywordCrawlProcessorTest
    {
        SemKeywordCrawlProcessor _uut;
        CrawlContext _crawlContext;
        CrawledPage _crawledPage;
        Mock<IPersistenceProvider> _fakePrimaryPersistence;
        SemList _dummySemList;

        [SetUp]
        public void SetUp()
        {
            _dummySemList = new SemList();
            _uut = new SemKeywordCrawlProcessor(_dummySemList);
            _fakePrimaryPersistence = new Mock<IPersistenceProvider>();

            _crawlContext = new CrawlContext();
            _crawlContext.CrawlBag.GoDaddyProcessorContext = new ProcessorContext
            {
                PrimaryPersistenceProvider = _fakePrimaryPersistence.Object,
                BackupPersistenceProvider = null,//no need since this is tested in another child class
                Domain = new Domain
                {
                    DomainId = 111
                }
            };
            _crawledPage = new CrawledPage(new Uri("http://a.com/"));
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor()
        {
            new SemKeywordCrawlProcessor(null);
        }

        [Test]
        public void ProcessCrawledPage_EmptyLists_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_NoMatches_DoesNotCallPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _dummySemList.States.Add("arizona", "arizona");
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId=1, CategoryName="animals", Text="cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_NoPageContent_DoesNotCallPersistenceProvider()
        {
            _crawledPage.RawContent = "";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_AllMatch_CallsPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _dummySemList.States.Add("arizona", "arizona");
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId = 1, CategoryName = "animals", Text = "cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here phoenix dog arizona cow.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 6 &&
                d.Attributes.ContainsKey("localCity") && 
                d.Attributes["localCity"] == "phoenix" && 
                d.Attributes.ContainsKey("localState") &&
                d.Attributes["localState"] == "arizona" &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "dog" &&
                d.Attributes.ContainsKey("verticalCategoryId") &&
                d.Attributes.ContainsKey("verticalCategoryName") &&
                d.Attributes.ContainsKey("verticalKeyword") &&
                d.Attributes["verticalCategoryId"] == "1" &&
                d.Attributes["verticalCategoryName"] == "animals" &&
                d.Attributes["verticalKeyword"] == "cow"
                )), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_TitleAbove100Chars_DoesNotFindFirst100Chars_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>a1aaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee one.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_TitleAbove100Chars_NoSpaces_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaone.</title><metaname=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_TitleAbove100Chars_FindsInFirst100Chars_CallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>aaaaa bbbb one ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_DescriptionAbove300Chars_NoSpaces_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaone.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_DescriptionAbove300Chars_DoesNotFindInFirst300Chars_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee one.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_DescriptionAbove300Chars_FindsInFirst300Chars_CallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"aaaaa bbbb one ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee aaaaa bbbb ccccc ddddd eeeee.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_1WordMatch_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one", "one");
            _crawledPage.RawContent = "<html><head><title>The title is here one.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_2WordMatch_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one two", "one two");
            _crawledPage.RawContent = "<html><head><title>The title is here one two.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one two")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_3WordMatch_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one two three", "one two three");
            _crawledPage.RawContent = "<html><head><title>The title is here one two three.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one two three")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_4WordMatch_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one two three four", "one two three four");
            _crawledPage.RawContent = "<html><head><title>The title is here one two three four.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one two three four")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_5WordMatch_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one two three four five", "one two three four five");
            _crawledPage.RawContent = "<html><head><title>The title is here one two three four five.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "one two three four five")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_6WordMatch_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("one two three four five six", "one two three four five six");
            _crawledPage.RawContent = "<html><head><title>The title is here one two three four five six.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }


        [Test]
        public void ProcessCrawledPage_MatchOnCityInTitle_CallsPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _crawledPage.RawContent = "<html><head><title>The title is here in phoenix.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d => 
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localCity") && 
                d.Attributes["localCity"] == "phoenix")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnCityInDescription_CallsPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here in phoenix.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localCity") &&
                d.Attributes["localCity"] == "phoenix")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnCityInBody_DoesNotCallPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here in phoenix.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnCityButNotItsOwnWord_DoesNotCallPersistenceProvider()
        {
            _dummySemList.Cities.Add("phoenix", "phoenix");
            _crawledPage.RawContent = "<html><head><title>The title is here  inphoenix.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }


        [Test]
        public void ProcessCrawledPage_MatchOnStateInTitle_CallsPersistenceProvider()
        {
            _dummySemList.States.Add("az", "az");
            _crawledPage.RawContent = "<html><head><title>The title is here in az.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localState") &&
                d.Attributes["localState"] == "az")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnStateInDescription_CallsPersistenceProvider()
        {
            _dummySemList.States.Add("az", "az");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here in az.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localState") &&
                d.Attributes["localState"] == "az")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnStateInBody_DoesNotCallPersistenceProvider()
        {
            _dummySemList.States.Add("az", "az");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here in az.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnStateButNotItsOwnWord_DoesNotCallPersistenceProvider()
        {
            _dummySemList.States.Add("az", "az");
            _crawledPage.RawContent = "<html><head><title>The title is here inaz.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }


        [Test]
        public void ProcessCrawledPage_MatchOnLocalKeywordInTitle_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _crawledPage.RawContent = "<html><head><title>The title is here and a dog too.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "dog")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnLocalKeywordInDescription_CallsPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here and a dog too.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 1 &&
                d.Attributes.ContainsKey("localKeyword") &&
                d.Attributes["localKeyword"] == "dog")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnLocalKeywordInBody_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here and dog too.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnLocalKeywordButNotItsOwnWord_DoesNotCallPersistenceProvider()
        {
            _dummySemList.LocalKeywords.Add("dog", "dog");
            _crawledPage.RawContent = "<html><head><title>The title is here anddogtoo</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }


        [Test]
        public void ProcessCrawledPage_MatchOnVerticalKeywordInTitle_CallsPersistenceProvider()
        {
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId = 1, CategoryName = "animals", Text = "cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here and a cow too.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 3 &&
                d.Attributes.ContainsKey("verticalCategoryId") &&
                d.Attributes.ContainsKey("verticalCategoryName") &&
                d.Attributes.ContainsKey("verticalKeyword") &&
                d.Attributes["verticalCategoryId"] == "1" &&
                d.Attributes["verticalCategoryName"] == "animals" &&
                d.Attributes["verticalKeyword"] == "cow")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnVerticalKeywordInDescription_CallsPersistenceProvider()
        {
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId = 1, CategoryName = "animals", Text = "cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here and a cow too.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.Is<DataComponent>(d =>
                d.Attributes.Count == 3 &&
                d.Attributes.ContainsKey("verticalCategoryId") &&
                d.Attributes.ContainsKey("verticalCategoryName") &&
                d.Attributes.ContainsKey("verticalKeyword") &&
                d.Attributes["verticalCategoryId"] == "1" &&
                d.Attributes["verticalCategoryName"] == "animals" &&
                d.Attributes["verticalKeyword"] == "cow")), Times.Exactly(1));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnVerticalKeywordInBody_DoesNotCallPersistenceProvider()
        {
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId = 1, CategoryName = "animals", Text = "cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here and cow too.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }

        [Test]
        public void ProcessCrawledPage_MatchOnVerticalKeywordButNotItsOwnWord_DoesNotCallPersistenceProvider()
        {
            _dummySemList.VerticalKeywords.Add("cow", new VerticalKeyword { CategoryId = 1, CategoryName = "animals", Text = "cow" });
            _crawledPage.RawContent = "<html><head><title>The title is here and cowtoo.</title><meta name=\"description\" content=\"description is here.\"></head><body>The body is here.</body></html>";

            _uut.ProcessCrawledPage(_crawlContext, _crawledPage);

            _fakePrimaryPersistence.Verify(f => f.Save(It.IsAny<DataComponent>()), Times.Exactly(0));
        }
    }
}
