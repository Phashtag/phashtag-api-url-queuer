using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhashtagApiUrlQueuer
{
	class Program
	{

		private static string authorization64;
		static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Phashtag URL tester 1.0");
			Console.WriteLine("Create a username and password at api.phashtag.com");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("Username: ");
			string username = Console.ReadLine();
			Console.Write("Password: ");
			string password = Console.ReadLine();
			authorization64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));

			Console.Clear();
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Type the URLs for files you wish to scan separated by new lines.");
			Console.WriteLine("To scan multiple at a time, paste/pipe them in all at once.");
			Console.WriteLine("To pipeline API calls, don't use CLI - copy the ScanUrls function.");
			Console.WriteLine("Type exit to exit.");
			Console.ForegroundColor = ConsoleColor.Gray;


			while (true)
			{
				bool exit = false;
				var urls = new List<string>();
				Console.ForegroundColor = ConsoleColor.Gray;
				do
				{
					string command = Console.ReadLine();
					if (command == "exit")
					{
						exit = true;
						break;
					}
					//Otherwise we'll assume it's a URL.
					urls.Add(command);
					Thread.Sleep(10);
				} while (Console.KeyAvailable);


				if (exit) {
					break;
				}

				Console.ForegroundColor = ConsoleColor.White;
				string printResultBuffer = "\n";
				ResultsModel[] results = null;

				try
				{
					results = ScanUrls(urls);
				}
				catch(Exception e){
					Console.WriteLine(e.Message);
					continue;
				}

				for (var i = 0; i < results.Length; i++ )
				{
					try
					{
						var result = results[i];
						Console.WriteLine(Path.GetFileName(urls[i]));
						if (result != null)
						{
							if (result.type == "data")
							{
								foreach (var r in result.data)
								{
									Console.WriteLine("\t PID:" + r.patternId + ",PNAME:" + r.patternName + ",PROB:" + r.probability);
								}
							}
							else
							{
								Console.WriteLine("\t" + result.message);
							}
						}
						else
						{
							Console.WriteLine("\tUnable to get result.");
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("\t" + e.Message);
					}

				} 

			}
		}

		static ResultsModel[] ScanUrls(List<string> urls)
		{

			using (var wc = new WebClient())
			{
				wc.Headers.Add("Authorization", "basic " + authorization64);
				wc.Headers.Add("Content-Type", "application/json; charset=utf-8");

				var MediaModelList = new List<MediaModel>();

				foreach (var url in urls)
				{
					MediaModel sendmodel = new MediaModel
					{
						mediatype = "image",
						uri = url
					};
					MediaModelList.Add(sendmodel);
				}

				//Endpoint actually requires a list...
				MediaModel[] sendmodels = MediaModelList.ToArray();

				string requestbody = JsonConvert.SerializeObject(sendmodels); //Converts the model into json format.
				byte[] postArray = Encoding.UTF8.GetBytes(requestbody); //Now into byte data.

				System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();

				//NOTE - Pattern 0 (in the URL) is the demo pattern. Change it to the pattern you wish to test for.
				byte[] responsebytes = wc.UploadData("http://api.phashtag.com/v1/pattern/0/test", "POST", postArray);
				string responsebody = System.Text.Encoding.Default.GetString(responsebytes);
				return JsonConvert.DeserializeObject<ResultsModel[]>(responsebody);
			}
		}
	}
}
