using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace TikTok_Automation_Library_Non_Jail
{
	// Token: 0x02000009 RID: 9
	public class Logs
	{
		// Token: 0x06000013 RID: 19 RVA: 0x0000348C File Offset: 0x0000168C
		public static void OpenNew(TextBox Log)
		{
			for (;;)
			{
				try
				{
					if (Logs.Logg == null)
					{
						Logs.Logg = Log;
						new Logs();
					}
					else
					{
						Logs.Logg = Log;
					}
				}
				catch
				{
					Thread.Sleep(100);
					continue;
				}
				break;
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000034D4 File Offset: 0x000016D4
		private Logs()
		{
			if (!Directory.Exists("OutData"))
			{
				Directory.CreateDirectory("OutData");
			}
			if (File.Exists("OutData/logs.txt"))
			{
				File.Delete("OutData/logs.txt");
			}
			this.wr = TextWriter.Synchronized(new StreamWriter("OutData/logs.txt", true));
			LogsUtils.WriteLog("The program is running!");
			new Thread(new ThreadStart(this.ThreadWriter))
			{
				IsBackground = true
			}.Start();
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00003554 File Offset: 0x00001754
		private void ThreadWriter()
		{
			for (;;)
			{
				try
				{
					if (Logs.concurrent.Count == 0)
					{
						Thread.Sleep(500);
					}
					else
					{
						if (Logs.Logg.Visible && !Logs.Logg.IsDisposed)
						{
							for (;;)
							{
								Logs.Class7 @class = new Logs.Class7();
								if (!Logs.concurrent.TryDequeue(out @class.text))
								{
									goto IL_00BA;
								}
								for (;;)
								{
									try
									{
										this.wr.WriteLine(@class.text);
										Logs.Logg.Invoke(new MethodInvoker(@class.Method0));
										break;
									}
									catch
									{
										if (!Logs.Logg.Visible || Logs.Logg.IsDisposed)
										{
											goto IL_00BA;
										}
										Thread.Sleep(500);
										continue;
									}
									goto IL_009B;
								}
							}
						}
						IL_009B:
						string text;
						while (Logs.concurrent.TryDequeue(out text))
						{
							try
							{
								this.wr.WriteLine(text);
								continue;
							}
							catch
							{
								continue;
							}
							break;
						}
						IL_00BA:
						this.wr.Flush();
						Thread.Sleep(300);
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x04000013 RID: 19
		private static TextBox Logg = null;

		// Token: 0x04000014 RID: 20
		private TextWriter wr;

		// Token: 0x04000015 RID: 21
		public static ConcurrentQueue<string> concurrent = new ConcurrentQueue<string>();

		// Token: 0x0200000A RID: 10
		[CompilerGenerated]
		private sealed class Class7
		{
			// Token: 0x06000018 RID: 24 RVA: 0x00003664 File Offset: 0x00001864
			internal void Method0()
			{
				TextBox logg = Logs.Logg;
				logg.Text = logg.Text + this.text + Environment.NewLine;
				Logs.Logg.SelectionStart = Logs.Logg.Text.Length;
				Logs.Logg.ScrollToCaret();
			}

			// Token: 0x04000016 RID: 22
			public string text;
		}
	}
}
