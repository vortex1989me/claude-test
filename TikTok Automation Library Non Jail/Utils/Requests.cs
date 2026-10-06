using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TikTok_Automation_Library_Non_Jail.Utils
{
	// Token: 0x02000085 RID: 133
	public class Requests
	{
		// Token: 0x06000157 RID: 343 RVA: 0x0005ADB8 File Offset: 0x00058FB8
		public static WebProxy GetProxy(string proxystring)
		{
			WebProxy webProxy = null;
			try
			{
				string[] array = proxystring.Split(new char[] { ':' });
				if (array.Length == 4)
				{
					string text = array[0];
					string text2 = array[1];
					string text3 = array[2];
					string text4 = array[3];
					webProxy = new WebProxy(text + ":" + text2, true);
					webProxy.Credentials = new NetworkCredential(text3, text4);
				}
				else if (array.Length == 2)
				{
					string text5 = array[0];
					string text6 = array[1];
					webProxy = new WebProxy(text5 + ":" + text6, true);
				}
			}
			catch
			{
			}
			return webProxy;
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0005AE50 File Offset: 0x00059050
		public static async Task<string> POST(string target, string data, string XAPI = null)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(target);
			httpWebRequest.Method = "POST";
			if (XAPI != null)
			{
				httpWebRequest.Headers.Add("X-API-KEY", XAPI);
			}
			httpWebRequest.AllowAutoRedirect = false;
			httpWebRequest.ServicePoint.Expect100Continue = false;
			httpWebRequest.Timeout = 10000;
			httpWebRequest.ReadWriteTimeout = 10000;
			httpWebRequest.ContinueTimeout = 10000;
			if (data != null)
			{
				byte[] array = Encoding.UTF8.GetBytes(data);
				Stream stream = await httpWebRequest.GetRequestStreamAsync();
				using (Stream stream2 = stream)
				{
					await stream2.WriteAsync(array, 0, array.Length);
				}
				Stream stream2 = null;
				array = null;
			}
			return await Requests.Full(httpWebRequest);
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0005AEA4 File Offset: 0x000590A4
		public static async Task<string> GET(string target, WebProxy proxy = null)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(target);
			httpWebRequest.Method = "GET";
			httpWebRequest.Proxy = proxy;
			httpWebRequest.ServicePoint.Expect100Continue = false;
			httpWebRequest.AllowAutoRedirect = false;
			httpWebRequest.Timeout = 10000;
			httpWebRequest.ReadWriteTimeout = 10000;
			httpWebRequest.ContinueTimeout = 10000;
			return await Requests.Full(httpWebRequest);
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0005AEF0 File Offset: 0x000590F0
		public static async Task<string> GET(string target, CookieContainer cookies, WebProxy proxy = null)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(target);
			httpWebRequest.Method = "GET";
			httpWebRequest.Proxy = proxy;
			if (cookies != null)
			{
				httpWebRequest.CookieContainer = cookies;
			}
			httpWebRequest.ServicePoint.Expect100Continue = false;
			httpWebRequest.AllowAutoRedirect = false;
			httpWebRequest.Timeout = 10000;
			httpWebRequest.ReadWriteTimeout = 10000;
			httpWebRequest.ContinueTimeout = 10000;
			return await Requests.Full(httpWebRequest);
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0005AF44 File Offset: 0x00059144
		private static async Task<string> Full(WebRequest request)
		{
			int num = 0;
			try
			{
				WebResponse webResponse = await request.GetResponseAsync();
				HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
				string text = await new StreamReader(httpWebResponse.GetResponseStream()).ReadToEndAsync();
				httpWebResponse.Close();
				return text;
			}
			catch (WebException obj)
			{
				num = 1;
			}
			string text2;
			if (num == 1)
			{
				object obj;
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)((WebException)obj).Response)
				{
					text2 = await Requests.ReadResponse(httpWebResponse);
				}
			}
			return text2;
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0005AF88 File Offset: 0x00059188
		private static async Task<string> ReadResponse(HttpWebResponse response)
		{
			if (response.CharacterSet == null)
			{
				using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
				{
					return await streamReader.ReadToEndAsync();
				}
			}
			string text;
			using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet)))
			{
				text = await streamReader.ReadToEndAsync();
			}
			return text;
		}
	}
}
