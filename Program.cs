using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScrapeDeltaFlights
{
	class Program
	{
		static void Main(string[] args)
		{
			string airline = "DL";
			string departAirport = "ATL";
			string destAirport = "BOS";
			string outputFile = "data.csv";
			string queryDate = "";

			destAirport = args[0];
			outputFile = args[1];
			queryDate = DateTime.Today.ToString("yyyy-MM-dd");

			Regex regex = new Regex(RegexString);

			string flightUrl = "http://www.flightstats.com/go/FlightAvailability/flightAvailability.do?departure=%28{0}%29&airline=%28{1}%29&arrival=%28{2}%29&connection=&queryDate={3}&queryTime=1&excludedConnectionCodes=&cabinCode=A&numOfSeats=1&queryType=D&fareClassCodes=";
			flightUrl = string.Format(flightUrl, departAirport, airline, destAirport, queryDate);

			Console.WriteLine(flightUrl);

			Task<string> respTask = GetResponse(flightUrl);

			try
			{
				Task.WaitAll(respTask);

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				throw;
			}

			if (!string.IsNullOrWhiteSpace(respTask.Result))
			{
				Console.WriteLine("Result recieved");

				var matches = regex.Matches(respTask.Result);

				Console.WriteLine(matches.Count.ToString() + " matches");

				foreach (var match in matches)
				{
					string lineOut = "";

					Match m = (Match)match;
					int cCount = m.Groups.Count;
					for (int i = 1; i < cCount; i++)
					{
						lineOut += m.Groups[i].Captures[0].Value + ",";
					}

					File.AppendAllText(outputFile, lineOut + DateTime.Now.ToString()+Environment.NewLine);
				}
			}

		}

		private static async Task<string> GetResponse(string url)
		{
			var httpClient = new HttpClient();

			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

			var response = await httpClient.GetAsync(new Uri(url));

			response.EnsureSuccessStatusCode();
			using (var responseStream = await response.Content.ReadAsStreamAsync())
			using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
			using (var streamReader = new StreamReader(decompressedStream))
			{
				return streamReader.ReadToEnd();
			}
		}


		static string RegexString = @"pAirlineCode\s+=\s+""([^""]+?)"";\s+pAirlineName\s+=\s+""Delta\s+Air\s+Lines"";\s+pOperatingAirline\s+=\s+""Delta\s+Air\s+Lines"";\s+pOperatingCode\s+=\s+""DL"";\s+pAirlineName\s+=\s+""Delta\s+Air\s+Lines"";\s+pDepName\s+=\s+""([^""]+?)"";\s+pArrName\s+=\s+""([^""]+?)"";\s+pDepCode\s+=\s+""([^""]+?)"";\s+pArrCode\s+=\s+""([^""]+?)"";\s+depTimeFormatted\s+=\s+""([^""]+?)"";\s+arrTimeFormatted\s+=\s+""([^""]+?)"";\s+currRoute\s+=\s+new\s+AvailableRoute\(\);\s+currFlight\s+=\s+currRoute.addFlight\(""([^""]+?)"",pOperatingCode,\s+pAirlineCode,\s+depTimeFormatted,\s+arrTimeFormatted,\s+""([^""]+?)"",\s+""([^""]+?)"",\s+([^,]+?),\s+([^,]+?),\s+pDepCode,\s+pArrCode,""""\);\s+addAirline\(pAirlineCode,\s+pAirlineName\);\s+addAirline\(pOperatingCode,\s+pOperatingAirline\);\s+addConnection\(pDepCode,\s+pDepName\);\s+addConnection\(pArrCode,\s+pArrName\);\s+currCabin\s+=\s+currFlight.addCabin\(""C"",\s+""Coach""\);\s+currCabin.addFareClass\('Y',\s+'([^']+?)'\);\s+currCabin.addFareClass\('B',\s+'([^']+?)'\);\s+currCabin.addFareClass\('M',\s+'([^']+?)'\);\s+currCabin.addFareClass\('S',\s+'([^']+?)'\);\s+currCabin.addFareClass\('H',\s+'([^']+?)'\);\s+currCabin.addFareClass\('Q',\s+'([^']+?)'\);\s+currCabin.addFareClass\('K',\s+'([^']+?)'\);\s+currCabin.addFareClass\('L',\s+'([^']+?)'\);\s+currCabin.addFareClass\('U',\s+'([^']+?)'\);\s+currCabin.addFareClass\('T',\s+'([^']+?)'\);\s+currCabin.addFareClass\('X',\s+'([^']+?)'\);\s+currCabin.addFareClass\('V',\s+'([^']+?)'\);\s+currCabin.addFareClass\('E',\s+'([^']+?)'\);\s+currCabin\s+=\s+currFlight.addCabin\(""F"",\s+""First""\);\s+currCabin.addFareClass\('F',\s+'([^']+?)'\);\s+currCabin.addFareClass\('P',\s+'([^']+?)'\);\s+currCabin.addFareClass\('A',\s+'([^']+?)'\);\s+currCabin.addFareClass\('G',\s+'([^']+?)'\);\s+currRoute.display\(\);\s+";
	}
}
