using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TikTok_Automation_Library_Non_Jail.Utils
{
	// Token: 0x0200007F RID: 127
	internal class CMDUtils
	{
		// Token: 0x0600013E RID: 318 RVA: 0x00059F98 File Offset: 0x00058198
		public static string[] WriteCMD(List<string> lines)
		{
			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.Start();
			foreach (string text in lines)
			{
				process.StandardInput.WriteLine(text);
				process.StandardInput.Flush();
			}
			process.StandardInput.Flush();
			process.StandardInput.Close();
			process.WaitForExit();
			return process.StandardOutput.ReadToEnd().Split(new char[] { '\n' });
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0005A07C File Offset: 0x0005827C
		public static string[] CMDWdaRun(Process process, string line)
		{
			process.StartInfo.FileName = "cmd.exe";
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.Start();
			process.StandardInput.WriteLine(line);
			process.StandardInput.Flush();
			process.StandardInput.Flush();
			return process.StandardOutput.ReadToEnd().Split(new char[] { '\n' });
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0005A110 File Offset: 0x00058310
		public static string[] WriteCMD(string line, int timeoutMs = 10000)
		{
			string[] array;
			using (Process process = new Process())
			{
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.Start();
				process.StandardInput.WriteLine(line);
				process.StandardInput.Close();
				if (!process.WaitForExit(timeoutMs))
				{
					try
					{
						process.Kill();
					}
					catch
					{
					}
					array = null;
				}
				else
				{
					array = process.StandardOutput.ReadToEnd().Split(new char[] { '\n' });
				}
			}
			return array;
		}
	}
}
