using System;

namespace TikTok_Automation_Library_Non_Jail
{
	// Token: 0x0200000B RID: 11
	public static class LogsUtils
	{
		// Token: 0x06000019 RID: 25 RVA: 0x000036B4 File Offset: 0x000018B4
		public static void WriteLog(string Text)
		{
			Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "]: " + Text;
			Logs.concurrent.Enqueue(Text);
		}
	}
}
