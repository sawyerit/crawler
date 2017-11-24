using CrawlDaddy.Core.Poco;
using StatsG.Client;
using System;

namespace CrawlDaddy.Core
{
    public class StatLogType
    {
        public const string CrawlDaddy_CompletedCrawls = "CrawlDaddy_CompletedCrawls";
        public const string CrawlDaddy_FatalErrorOccured = "CrawlDaddy_FatalErrorOccured";
        public const string CrawlDaddy_ErrorOccuredDuringCrawl = "CrawlDaddy_ErrorOccuredDuringCrawl";
    }

    public class StatsGLoggerAppender
    {
        #region Internal Members

        private string _productAuthId { get; set; }

        private int _productAppId { get; set; }

        private IStatsGLogger _statsGLogger { get; set; }
        CrawlDaddyConfig _config;

        #endregion Internal Members

        #region Constructors

        public StatsGLoggerAppender(CrawlDaddyConfig config)
            : this(config, null)
        {
        }

        public StatsGLoggerAppender(CrawlDaddyConfig config, IStatsGLogger statsGLogger)
        {
            _config = config ?? new CrawlDaddyConfig();
            
            this._statsGLogger = statsGLogger ?? GetDefaultStatsGLoggerInstance(_config);
            this._productAuthId = _config.CrawlDaddyAuthIdForSEV;
            this._productAppId = _config.CrawlDaddyAppIdForSEV;
        }

        private IStatsGLogger GetDefaultStatsGLoggerInstance(CrawlDaddyConfig config)
        {
            string hostUrl = config.StatsGHostConfigName;
            int port = config.StatsGPortConfigName;
            var statsGLogger = new StatsGLogger(hostUrl, port);

            return statsGLogger;
        }

        #endregion Constructors

        #region Public Methods

        public void LogItem(string statsGTypeName, float value = 1)
        {
            if (_config.StatsGEnabled)
            {
                this._statsGLogger.SendMessage(statsGTypeName, value, this._productAuthId, this._productAppId);
            }
        }

        public void LogItemAsync(string statsGTypeName, float value = 1)
        {
            if (_config.StatsGEnabled)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => LogItem(statsGTypeName, value));
            }
        }

        #endregion Public Methods

        #region Static Implementation

        private static volatile StatsGLoggerAppender singleton;
        private static readonly object singletonLock = new object();

        private static StatsGLoggerAppender GetSingleton(CrawlDaddyConfig config)
        {
            // if this is the first time it is being called, we set up the private static members
            if (singleton == null)
            {
                lock (singletonLock)
                {
                    if (singleton == null)
                    {
                        singleton = new StatsGLoggerAppender(config);
                    }
                }
            }

            return singleton;
        }

        public static void LogItem(string statsGTypeName, CrawlDaddyConfig configuration, float value = 1)
        {
            if (configuration.StatsGEnabled)
            {
                StatsGLoggerAppender logger = GetSingleton(configuration);

                logger.LogItem(statsGTypeName, value);
            }
        }

        #endregion Static Implementation
    }
}