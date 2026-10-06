using System;
using System.Runtime.CompilerServices;

// Token: 0x020000B2 RID: 178
[CompilerGenerated]
internal sealed class Class22
{
	// Token: 0x0600025D RID: 605 RVA: 0x00065B80 File Offset: 0x00063D80
	internal static uint ComputeStringHash(string s)
	{
		uint num;
		if (s != null)
		{
			num = 2166136261U;
			for (int i = 0; i < s.Length; i++)
			{
				num = ((uint)s[i] ^ num) * 16777619U;
			}
		}
		return num;
	}
}
