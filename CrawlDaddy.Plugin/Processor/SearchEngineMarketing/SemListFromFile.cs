using log4net;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrawlDaddy.Plugin.Processor.SearchEngineMarketing
{
	public class SemListFromFile : SemList
	{
		static ILog _logger = LogManager.GetLogger(typeof(SemListFromFile).FullName);
		static string _dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
		static string _keywordVerticalsFilePath = Path.Combine(_dirPath, "Sem_VerticalKeywords.csv");
		static string _localkeywordsFilePath = Path.Combine(_dirPath, "Sem_LocalKeywords.csv");
		static string _citiesFilePath = Path.Combine(_dirPath, "Sem_Cities.csv");
		static string _statesFilePath = Path.Combine(_dirPath, "Sem_States.csv");

		public SemListFromFile()
			: base()
		{
			VerticalKeywords = GetVerticalKeywords();
			LocalKeywords = GetLocalKeywords();
			Cities = GetCities();
			States = GetStates();
		}

		private Dictionary<string, VerticalKeyword> GetVerticalKeywords()
		{
			Dictionary<string, VerticalKeyword> result = new Dictionary<string, VerticalKeyword>();
			string[] allLines = File.ReadAllLines(_keywordVerticalsFilePath);
			int expectedCsvElements = 3;
			string[] csvElement;
			int currentLine = 0;
			foreach (string line in allLines)
			{
				currentLine++;

				if (line.StartsWith("//"))
					continue;

				csvElement = line.Split(',');
				if (csvElement.Length != expectedCsvElements)
					throw new ApplicationException(string.Format("{0} has more/less than the expected {1} values on line {2}.", _keywordVerticalsFilePath, expectedCsvElements, currentLine));

				try
				{
					string key = csvElement[2].Trim();
					if (!result.ContainsKey(key))
						result.Add(key, new VerticalKeyword { CategoryId = Convert.ToInt32(csvElement[0].Trim()), CategoryName = csvElement[1].Trim(), Text = csvElement[2].Trim() });
				}
				catch (Exception e)
				{
					_logger.ErrorFormat("Encountered an issue while attempting to parse data in file {0}, line {1}.", _keywordVerticalsFilePath, currentLine);
					_logger.Error(e);
				}
			}

			return result;
		}

		private Dictionary<string, string> GetLocalKeywords()
		{
			return GetSingleValueList(_localkeywordsFilePath);
		}

		private Dictionary<string, string> GetCities()
		{
			return GetSingleValueList(_citiesFilePath);
		}

		private Dictionary<string, string> GetStates()
		{
			return GetSingleValueList(_statesFilePath);
		}

		private Dictionary<string, string> GetSingleValueList(string filePath)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			string[] allLines = File.ReadAllLines(filePath);
			int expectedCsvElements = 1;
			string[] csvElement;
			int currentLine = 0;
			foreach (string line in allLines)
			{
				currentLine++;

				if (line.StartsWith("//"))
					continue;

				csvElement = line.Split(',');
				if (csvElement.Length != expectedCsvElements)
					throw new ApplicationException(string.Format("{0} has more/less than the expected {1} values on line {2}.", filePath, expectedCsvElements, currentLine));

				try
				{
					if (!result.ContainsKey(csvElement[0].Trim()))
						result.Add(csvElement[0].Trim(), csvElement[0].Trim());
				}
				catch (Exception e)
				{
					_logger.ErrorFormat("Encountered an issue while attempting to parse data in file {0}, line {1}.", filePath, currentLine);
					_logger.Error(e);
				}
			}

			return result;
		}
	}
}
