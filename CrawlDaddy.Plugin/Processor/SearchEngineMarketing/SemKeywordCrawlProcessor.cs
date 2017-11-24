using Abot.Poco;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TrafficBlazer.BlackWidow.Core;

namespace CrawlDaddy.Plugin.Processor.SearchEngineMarketing
{
    /// <summary>
    /// Determines if a domain is a local service based business by looking for a non 1-800 phone number along with certain keywords in title and description
    /// </summary>
    public class SemKeywordCrawlProcessor : OneHitPerDomainCrawlProcessor
    {
        static ILog _logger = LogManager.GetLogger(typeof(SemKeywordCrawlProcessor).FullName);
        SemList _list;

        public SemKeywordCrawlProcessor(SemList list)
        {
            if (list == null)
                throw new ArgumentNullException("listProvider");

            _list = list;
        }

        protected override ProcessorResult ProcessPage(CrawlContext crawlContext, CrawledPage crawledPage)
        {
            ProcessorResult result = new ProcessorResult();
            result.UniqueAttributeId = 14;

            string title = crawledPage.CsQueryDocument.Select("title").Text();
            title = string.IsNullOrEmpty(title) ? "" : Crop(title.ToLower(), 100);

            string description = crawledPage.CsQueryDocument.Select("meta[name=description]").Attr("content");
            description = string.IsNullOrEmpty(description) ? "" : Crop(description.ToLower(), 300);

						string keywords = crawledPage.CsQueryDocument.Select("meta[name=keywords]").Attr("content");
						keywords = string.IsNullOrEmpty(keywords) ? "" : Crop(keywords.ToLower(), 300);

            string titleDescriptionKeywords = string.Format("{0} {1} {2}",title, description, keywords);

            TextManager manager = new TextManager();
            titleDescriptionKeywords = manager.Remove(titleDescriptionKeywords, Removable.SpecialChars | Removable.ExtraSpaces, Allowable.Hyphen);
            
            string[] wordArray = manager.GetWordsFromText(titleDescriptionKeywords).ToArray();

            Stopwatch timer = Stopwatch.StartNew();
            VerticalKeyword matchedVertical = GetKeywordMatch(wordArray, _list.VerticalKeywords);
            timer.Stop();
            _logger.DebugFormat("Vertical matching took [{0}] millisecs.", timer.ElapsedMilliseconds);

            timer = Stopwatch.StartNew();
            string matchedLocal = GetKeywordMatch(wordArray, _list.LocalKeywords);
            timer.Stop();
            _logger.DebugFormat("Local keyword matching took [{0}] millisecs.", timer.ElapsedMilliseconds);

            timer = Stopwatch.StartNew();
            string matchedCity = GetKeywordMatch(wordArray, _list.Cities);
            timer.Stop();
            _logger.DebugFormat("Local city matching took [{0}] millisecs.", timer.ElapsedMilliseconds);

            timer = Stopwatch.StartNew();
            string matchedState = GetKeywordMatch(wordArray, _list.States);
            timer.Stop();
            _logger.DebugFormat("Local state matching took [{0}] millisecs.", timer.ElapsedMilliseconds);


            if(matchedVertical != null)
            {
                result.Attributes.Add("verticalCategoryId", matchedVertical.CategoryId.ToString());
                result.Attributes.Add("verticalCategoryName", matchedVertical.CategoryName);
                result.Attributes.Add("verticalKeyword", matchedVertical.Text);
                result.IsAHit = true;
            }
            
            if(matchedLocal != null)
            {
                result.Attributes.Add("localKeyword", matchedLocal);
                result.IsAHit = true;
            }

            if (matchedCity != null)
            {
                result.Attributes.Add("localCity", matchedCity);
                result.IsAHit = true;
            }

            if (matchedState != null)
            {
                result.Attributes.Add("localState", matchedState);
                result.IsAHit = true;
            }

            return result;
        }

        private string Crop(string value, int maxChars)
        {
            if (value.Length <= maxChars)
                return value;

            string cropped = "";

            int lastIndex = maxChars - 1;
            int lastIndexOfSpace = value.LastIndexOf(" ", lastIndex, lastIndex);
            if (lastIndexOfSpace > 0)
                cropped = value.Substring(0, lastIndexOfSpace);
            else
                cropped = value.Substring(0, lastIndex);

            return cropped.Trim();
        }

        private T GetKeywordMatch<T>(string[] pageWordArray, Dictionary<string, T> lookupList)
        {
            T foundValue = default(T);
            
            string oneWordPhrase = null;
            string twoWordPhrase = null;
            string threeWordPhrase = null;
            string fourWordPhrase = null;
            string fiveWordPhrase = null;

            for (int i = 0; i < pageWordArray.Length; i++)
			{
                //Check for 1 word phrase match
                oneWordPhrase = pageWordArray[i].Trim();
                if (lookupList.TryGetValue(oneWordPhrase, out foundValue))
                    break;

                //Check for 2 word phrase match
                if ((i + 1) < pageWordArray.Length)
                {
                    twoWordPhrase = GetWordPhrase(pageWordArray, i, 2).Trim();
                    if (lookupList.TryGetValue(twoWordPhrase, out foundValue))
                        break;
                }

                //Check for 3 word phrase match
                if ((i + 2) < pageWordArray.Length)
                {
                    threeWordPhrase = GetWordPhrase(pageWordArray, i, 3);
                    if (lookupList.TryGetValue(threeWordPhrase, out foundValue))
                        break;
                }

                //Check for 4 word phrase match
                if ((i + 3) < pageWordArray.Length)
                {
                    fourWordPhrase = GetWordPhrase(pageWordArray, i, 4);
                    if (lookupList.TryGetValue(fourWordPhrase, out foundValue))
                        break;
                }

                //Check for 5 word phrase match
                if ((i + 4) < pageWordArray.Length)
                {
                    fiveWordPhrase = GetWordPhrase(pageWordArray, i, 5);
                    if (lookupList.TryGetValue(fiveWordPhrase, out foundValue))
                        break;
                }
			} 

            return foundValue;
        }

        private string GetWordPhrase(string[] lookingFor, int startIndex, int count)
        {
            int lastIndex = startIndex + count - 1;
            StringBuilder builder = new StringBuilder();
            builder.Append(lookingFor[startIndex]);
            builder.Append(" ");
            for (int i = startIndex; i < lastIndex; i++)
            {
                builder.Append(lookingFor[i + 1].Trim());
                builder.Append(" ");     
            }
            return builder.ToString().Trim();
        }
    }
}
