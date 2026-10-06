using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TikTok_Automation_Library_Non_Jail.Security
{
	internal class Checker
	{
		public Checker(string _deviceUUID)
		{
		}

		private async Task CheckTask()
		{
			await Task.CompletedTask;
		}

		private async Task<bool> CheckIt()
		{
			await Task.CompletedTask;
			return true;
		}

		public static async Task<bool> CheckItStatic(string deviceUUID)
		{
			await Task.CompletedTask;
			return true;
		}

		[CompilerGenerated]
		private Task Method0()
		{
			return Task.CompletedTask;
		}

		private string cpu = "";
		private string deviceUUID;
		private string app = "";
		private string unlock = "123";
		public Task CheckerTask;
	}
}
