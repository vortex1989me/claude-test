using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TikTok_Automation_Library_Non_Jail
{
	public class SUtils
	{
		public static string GetDateTime()
		{
			string text = DateTime.UtcNow.ToString("yyyy/MM/dd HH", CultureInfo.InvariantCulture);
			if (text.Contains("/"))
			{
				return text;
			}
			string[] array = DateTime.UtcNow.ToString("yyyy/MM/dd HH").Replace(".", "/").Split(new char[] { ' ' });
			text = "";
			for (int i = 0; i < array.Length; i++)
			{
				if (i == array.Length - 1)
				{
					text = text + " " + array[i];
				}
				else if (i == array.Length - 2)
				{
					text += array[i];
				}
				else
				{
					text = text + array[i] + "/";
				}
			}
			return text;
		}

		public static string sha256(string data)
		{
			HashAlgorithm hashAlgorithm = new SHA256Managed();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data)))
			{
				stringBuilder.Append(b.ToString("x2"));
			}
			return stringBuilder.ToString();
		}

		public static async Task selfdelete()
		{
			await Task.CompletedTask;
		}

		public static async Task<string> Decode(string data)
		{
			await Task.Delay(1);
			string text3;
			try
			{
				string[] array = data.Split(new char[] { '$' });
				string text = "";
				foreach (string text2 in array)
				{
					if (!string.IsNullOrEmpty(text2))
					{
						text = (text2.Contains("999") ? (text + "ghjk{}[]qwertyuiopasdflzxcvbnm1234567890:/'\"\";:?/.>,<!@#$%^&*()-_=+QWERTYUIOPLKJHGFDSAZXCVBNM "[int.Parse(text2.Replace("999", ""))].ToString()) : (text + "ghjk{}[]qwertyuiopasdflzxcvbnm1234567890:/'\"\";:?/.>,<!@#$%^&*()-_=+QWERTYUIOPLKJHGFDSAZXCVBNM "[int.Parse(text2) + 3].ToString()));
					}
				}
				text3 = text;
			}
			catch
			{
				text3 = data;
			}
			return text3;
		}

		public static async Task<string> Encode(string data)
		{
			await Task.Delay(1);
			string text = "";
			string text2;
			try
			{
				for (int i = 0; i < data.Length; i++)
				{
					int num = "ghjk{}[]qwertyuiopasdflzxcvbnm1234567890:/'\"\";:?/.>,<!@#$%^&*()-_=+QWERTYUIOPLKJHGFDSAZXCVBNM ".IndexOf(data[i]);
					if (num >= 3)
					{
						text += string.Format("{0}$", num - 3);
					}
					else
					{
						text += string.Format("999{0}$", num);
					}
				}
				text2 = text;
			}
			catch
			{
				text2 = text;
			}
			return text2;
		}

		public const string chars = "ghjk{}[]qwertyuiopasdflzxcvbnm1234567890:/'\"\";:?/.>,<!@#$%^&*()-_=+QWERTYUIOPLKJHGFDSAZXCVBNM ";
	}
}
