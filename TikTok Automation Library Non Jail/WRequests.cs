using System;
using System.Net;
using System.Threading.Tasks;

namespace TikTok_Automation_Library_Non_Jail
{
	[Obsolete]
	internal class WRequests : HttpWebRequest
	{
		public static async Task<string> POST(string data, string target = null)
		{
			await Task.CompletedTask;
			return "";
		}
	}
}
