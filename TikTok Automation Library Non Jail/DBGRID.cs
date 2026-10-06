using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TikTok_Automation_Library_Non_Jail
{
	// Token: 0x02000002 RID: 2
	public static class DBGRID
	{
		// Token: 0x06000001 RID: 1 RVA: 0x0000330C File Offset: 0x0000150C
		public static void SetDeviceName(this DataGridView Main, int i, string name)
		{
			DBGRID.Class1 @class = new DBGRID.Class1();
			@class.Main = Main;
			@class.i = i;
			@class.name = name;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000334C File Offset: 0x0000154C
		public static void SetDeviceUUID(this DataGridView Main, int i, string uuid)
		{
			DBGRID.Class2 @class = new DBGRID.Class2();
			@class.Main = Main;
			@class.i = i;
			@class.uuid = uuid;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000338C File Offset: 0x0000158C
		public static void SetDevicePort(this DataGridView Main, int i, string port)
		{
			DBGRID.Class3 @class = new DBGRID.Class3();
			@class.Main = Main;
			@class.i = i;
			@class.port = port;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000033CC File Offset: 0x000015CC
		public static void SetStatus(this DataGridView Main, int i, string status)
		{
			DBGRID.Class4 @class = new DBGRID.Class4();
			@class.Main = Main;
			@class.i = i;
			@class.status = status;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000340C File Offset: 0x0000160C
		public static void SetNums(this DataGridView Main, int i, string nums)
		{
			DBGRID.Class5 @class = new DBGRID.Class5();
			@class.Main = Main;
			@class.i = i;
			@class.nums = nums;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000344C File Offset: 0x0000164C
		public static void SetWDAStatus(this DataGridView Main, int i, string wdastatus)
		{
			DBGRID.Class6 @class = new DBGRID.Class6();
			@class.Main = Main;
			@class.i = i;
			@class.wdastatus = wdastatus;
			@class.Main.Invoke(new Action(@class.Method0));
		}

		// Token: 0x02000003 RID: 3
		[CompilerGenerated]
		private sealed class Class1
		{
			// Token: 0x06000008 RID: 8 RVA: 0x00002058 File Offset: 0x00000258
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[0].Value = this.name;
			}

			// Token: 0x04000001 RID: 1
			public DataGridView Main;

			// Token: 0x04000002 RID: 2
			public int i;

			// Token: 0x04000003 RID: 3
			public string name;
		}

		// Token: 0x02000004 RID: 4
		[CompilerGenerated]
		private sealed class Class2
		{
			// Token: 0x0600000A RID: 10 RVA: 0x00002086 File Offset: 0x00000286
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[1].Value = this.uuid;
			}

			// Token: 0x04000004 RID: 4
			public DataGridView Main;

			// Token: 0x04000005 RID: 5
			public int i;

			// Token: 0x04000006 RID: 6
			public string uuid;
		}

		// Token: 0x02000005 RID: 5
		[CompilerGenerated]
		private sealed class Class3
		{
			// Token: 0x0600000C RID: 12 RVA: 0x000020B4 File Offset: 0x000002B4
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[2].Value = this.port;
			}

			// Token: 0x04000007 RID: 7
			public DataGridView Main;

			// Token: 0x04000008 RID: 8
			public int i;

			// Token: 0x04000009 RID: 9
			public string port;
		}

		// Token: 0x02000006 RID: 6
		[CompilerGenerated]
		private sealed class Class4
		{
			// Token: 0x0600000E RID: 14 RVA: 0x000020E2 File Offset: 0x000002E2
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[3].Value = this.status;
			}

			// Token: 0x0400000A RID: 10
			public DataGridView Main;

			// Token: 0x0400000B RID: 11
			public int i;

			// Token: 0x0400000C RID: 12
			public string status;
		}

		// Token: 0x02000007 RID: 7
		[CompilerGenerated]
		private sealed class Class5
		{
			// Token: 0x06000010 RID: 16 RVA: 0x00002110 File Offset: 0x00000310
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[4].Value = this.nums;
			}

			// Token: 0x0400000D RID: 13
			public DataGridView Main;

			// Token: 0x0400000E RID: 14
			public int i;

			// Token: 0x0400000F RID: 15
			public string nums;
		}

		// Token: 0x02000008 RID: 8
		[CompilerGenerated]
		private sealed class Class6
		{
			// Token: 0x06000012 RID: 18 RVA: 0x0000213E File Offset: 0x0000033E
			internal void Method0()
			{
				this.Main.Rows[this.i].Cells[5].Value = this.wdastatus;
			}

			// Token: 0x04000010 RID: 16
			public DataGridView Main;

			// Token: 0x04000011 RID: 17
			public int i;

			// Token: 0x04000012 RID: 18
			public string wdastatus;
		}
	}
}
