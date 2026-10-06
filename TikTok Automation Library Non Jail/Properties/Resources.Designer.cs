using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TikTok_Automation_Library_Non_Jail.Properties
{
	// Token: 0x0200009C RID: 156
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000197 RID: 407 RVA: 0x00002050 File Offset: 0x00000250
		internal Resources()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000198 RID: 408 RVA: 0x000028B6 File Offset: 0x00000AB6
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("TikTok_Automation_Library_Non_Jail.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000199 RID: 409 RVA: 0x000028E2 File Offset: 0x00000AE2
		// (set) Token: 0x0600019A RID: 410 RVA: 0x000028E9 File Offset: 0x00000AE9
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x040004FC RID: 1276
		private static ResourceManager resourceMan;

		// Token: 0x040004FD RID: 1277
		private static CultureInfo resourceCulture;
	}
}
