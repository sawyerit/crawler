using CrawlDaddy.Core;
using CrawlDaddy.Core.Poco;
using Moq;
using NUnit.Framework;
using StatsG.Client;

namespace CrawlDaddy.Test.Unit.Core
{
    [TestFixture]
    public class StatsGLogAppenderTest
    {
        private Mock<IStatsGLogger> _fakeStatsGLogger;
        private CrawlDaddyConfig _config;

        [SetUp]
        public void SetUp()
        {
            _fakeStatsGLogger = new Mock<IStatsGLogger>();
            _config = new CrawlDaddyConfig();
        }

        [Test]
        public void Constructor_ConfigIsNull_DoesNotThrowException()
        {
            new StatsGLoggerAppender(null);
        }

        [Test]
        public void Constructor_FakeIStatsGLogger_DoesNotThrowException()
        {
            new StatsGLoggerAppender(_config, _fakeStatsGLogger.Object);
        }


        [Test]
        public void FakeIStatsGLogger_LogItem_DoesNotThrowException()
        {
            StatsGLoggerAppender logger = new StatsGLoggerAppender(_config, _fakeStatsGLogger.Object);
            logger.LogItem("Test");
        }

        [Test]
        public void ConfigValuesEmpty_Disabled_LogItemNotCalled_DoesNotThrowException()
        {
            _config.StatsGEnabled = false;
            _config.StatsGHostConfigName = null;
            _config.StatsGPortConfigName = 0;
            _config.CrawlDaddyAuthIdForSEV = null;
            _config.CrawlDaddyAppIdForSEV = 0;

            _fakeStatsGLogger.Setup(x => x.SendMessage("Test", 1, null, 0));

            StatsGLoggerAppender logger = new StatsGLoggerAppender(_config, _fakeStatsGLogger.Object);
            logger.LogItem("Test");

            _fakeStatsGLogger.Verify(x => x.SendMessage("Test", 1, null, 0), Times.Never());
        }

        [Test]
        public void ConfigValuesEmpty_Enabled_LogItemCalled_DoesNotThrowException()
        {
            _config.StatsGEnabled = true;
            _config.StatsGHostConfigName = null;
            _config.StatsGPortConfigName = 0;
            _config.CrawlDaddyAuthIdForSEV = null;
            _config.CrawlDaddyAppIdForSEV = 0;

            _fakeStatsGLogger.Setup(x => x.SendMessage("Test", 1, null, 0));

            StatsGLoggerAppender logger = new StatsGLoggerAppender(_config, _fakeStatsGLogger.Object);
            logger.LogItem("Test");

            _fakeStatsGLogger.Verify(x => x.SendMessage("Test", 1, null, 0));
        }
    }
}
