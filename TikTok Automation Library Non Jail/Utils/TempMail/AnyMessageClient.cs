using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using TikTok_Automation_Library_Non_Jail.Properties;

namespace TikTok_Automation_Library_Non_Jail.Utils.TempMail
{
	// Token: 0x0200008B RID: 139
	public class AnyMessageClient : IDisposable
	{
		// Token: 0x06000168 RID: 360 RVA: 0x000027AD File Offset: 0x000009AD
		public AnyMessageClient()
		{
			this._httpClient = new HttpClient();
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0005B8D4 File Offset: 0x00059AD4
		public async Task<decimal> GetBalanceAsync()
		{
			return Extensions.Value<decimal>((await this.SendRequestAsync("/user/balance", ""))["balance"]);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0005B918 File Offset: 0x00059B18
		private async Task<JObject> GetAvailableEmailsAsync(string site = "tiktok.com")
		{
			string text = "site=" + HttpUtility.UrlEncode(site);
			TaskAwaiter<JObject> taskAwaiter = this.SendRequestAsync("/email/quantity", text).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<JObject> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<JObject>);
			}
			return (JObject)taskAwaiter.GetResult()["data"];
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0005B964 File Offset: 0x00059B64
		public async Task<AnyMessageClient.EmailData> OrderEmailAsync(string domain = "mail.com", string site = "tiktok.com", string regex = null, string subject = null)
		{
			AnyMessageClient.EmailData emailData = new AnyMessageClient.EmailData();
			string text = "";
			if (domain.Contains("long_"))
			{
				domain = domain.Replace("long_", "");
				text = "longlive-";
				emailData.IsLong = true;
			}
			string text2 = "site=" + HttpUtility.UrlEncode(site) + "&domain=" + HttpUtility.UrlEncode(domain);
			if (!string.IsNullOrEmpty(regex))
			{
				text2 = text2 + "&regex=" + HttpUtility.UrlEncode(regex);
			}
			if (!string.IsNullOrEmpty(subject))
			{
				text2 = text2 + "&subject=" + HttpUtility.UrlEncode(subject);
			}
			text2 += "&soft_id=9445";
			JObject jobject = await this.SendRequestAsync("/" + text + "email/order", text2);
			if ((string)jobject["status"] != "success")
			{
				emailData.Exception = jobject.ToString();
			}
			else if (emailData.IsLong)
			{
				JToken jtoken = ((JArray)jobject["emails"])[0];
				emailData.id = (string)jtoken["id"];
				emailData.email = (string)jtoken["email"];
			}
			else
			{
				emailData.id = (string)jobject["id"];
				emailData.email = (string)jobject["email"];
			}
			this.emailDataC = emailData;
			return emailData;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0005B9C8 File Offset: 0x00059BC8
		public async Task<AnyMessageClient.MessageData> GetMessagesLong(string id)
		{
			string text = "id=" + id;
			JObject jobject = await this.SendRequestAsync("/longlive-email/getmessages", text);
			AnyMessageClient.MessageData messageData = new AnyMessageClient.MessageData();
			if ((string)jobject["status"] != "success")
			{
				messageData.Exception = jobject.ToString();
			}
			else
			{
				JToken jtoken = ((JArray)jobject["data"])[0];
				DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds((long)jtoken["created_at"]).UtcDateTime;
				if ((DateTime.UtcNow - utcDateTime).TotalMinutes > 185.0)
				{
					throw new Exception("wait message");
				}
				messageData.status = (string)jobject["status"];
				messageData.message = (string)jtoken["message"];
				string text2 = (string)jtoken["subject"];
				if (text2.Contains(" is your"))
				{
					messageData.value = text2.Split(new char[] { ' ' })[0];
				}
				else
				{
					messageData.value = this.ParseCode(messageData.message);
				}
			}
			this.messageDataC = messageData;
			return messageData;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0005BA14 File Offset: 0x00059C14
		public async Task<AnyMessageClient.MessageData> GetMessageAsync(string id, bool LL, bool preview = false)
		{
			AnyMessageClient.MessageData messageData;
			if (LL)
			{
				if (Settings.Default.AnymessageMinus1Hour)
				{
					messageData = await this.GetMessagesLong(id);
				}
				else
				{
					messageData = await this.GetLastMessageAsync(id);
				}
			}
			else
			{
				string text = string.Format("id={0}&preview={1}", id, preview ? 1 : 0);
				JObject jobject = await this.SendRequestAsync("/email/getmessage", text);
				AnyMessageClient.MessageData messageData2 = new AnyMessageClient.MessageData();
				if ((string)jobject["status"] != "success")
				{
					messageData2.Exception = jobject.ToString();
				}
				else
				{
					messageData2.status = (string)jobject["status"];
					messageData2.value = (string)jobject["value"];
					messageData2.message = (string)jobject["message"];
				}
				this.messageDataC = messageData2;
				messageData = messageData2;
			}
			return messageData;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0005BA70 File Offset: 0x00059C70
		public async Task<AnyMessageClient.MessageData> GetLastMessageAsync(string id)
		{
			string text = "id=" + id;
			JObject jobject = await this.SendRequestAsync("/longlive-email/getlastmessages", text);
			AnyMessageClient.MessageData messageData = new AnyMessageClient.MessageData();
			if ((string)jobject["status"] != "success")
			{
				messageData.Exception = jobject.ToString();
			}
			else
			{
				JToken jtoken = ((JArray)jobject["data"])[0];
				DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds((long)jtoken["created_at"]).UtcDateTime;
				if ((DateTime.UtcNow - utcDateTime).TotalMinutes > 5.0)
				{
					throw new Exception("wait message");
				}
				messageData.status = (string)jobject["status"];
				messageData.message = (string)jtoken["message"];
				string text2 = (string)jtoken["subject"];
				if (text2.Contains(" is your"))
				{
					messageData.value = text2.Split(new char[] { ' ' })[0];
				}
				else
				{
					messageData.value = this.ParseCode(messageData.message);
				}
			}
			this.messageDataC = messageData;
			return messageData;
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0005BABC File Offset: 0x00059CBC
		public string ParseCode(string data)
		{
			Match match = new Regex(">(\\d+)</label></b>").Match(data);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			match = new Regex("bold\">(\\d+)</p><p").Match(data);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			match = new Regex("0\">(\\d+)</span>").Match(data);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			throw new Exception("code parse exception");
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0005BB54 File Offset: 0x00059D54
		public async Task<AnyMessageClient.EmailData> ReorderByIdAsync(string id, string regex = null, string subject = null)
		{
			string text = "id=" + id;
			if (!string.IsNullOrEmpty(regex))
			{
				text = text + "&regex=" + HttpUtility.UrlEncode(regex);
			}
			if (!string.IsNullOrEmpty(subject))
			{
				text = text + "&subject=" + HttpUtility.UrlEncode(subject);
			}
			JObject jobject = await this.SendRequestAsync("/email/reorder", text);
			AnyMessageClient.EmailData emailData = new AnyMessageClient.EmailData();
			if ((string)jobject["status"] != "success")
			{
				emailData.Exception = jobject.ToString();
			}
			else
			{
				emailData.id = (string)jobject["id"];
				emailData.email = (string)jobject["email"];
			}
			this.emailDataC = emailData;
			return emailData;
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0005BBB0 File Offset: 0x00059DB0
		public AnyMessageClient.EmailData ReorderLL(string id, string email)
		{
			AnyMessageClient.EmailData emailData = new AnyMessageClient.EmailData();
			emailData.id = id;
			emailData.email = email;
			this.emailDataC = emailData;
			return emailData;
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0005BBDC File Offset: 0x00059DDC
		public async Task<AnyMessageClient.EmailData> ReorderByEmailAsync(string email, string regex = null, string subject = null)
		{
			string text = "email=" + email;
			if (!string.IsNullOrEmpty(regex))
			{
				text = text + "&regex=" + HttpUtility.UrlEncode(regex);
			}
			if (!string.IsNullOrEmpty(subject))
			{
				text = text + "&subject=" + HttpUtility.UrlEncode(subject);
			}
			JObject jobject = await this.SendRequestAsync("/email/reorder", text);
			AnyMessageClient.EmailData emailData = new AnyMessageClient.EmailData();
			if ((string)jobject["status"] != "success")
			{
				emailData.Exception = jobject.ToString();
			}
			else
			{
				emailData.id = (string)jobject["id"];
				emailData.email = (string)jobject["email"];
			}
			this.emailDataC = emailData;
			return emailData;
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0005BC38 File Offset: 0x00059E38
		public async Task<bool> CancelEmailAsync(string id)
		{
			string text = "id=" + id;
			JToken jtoken = (await this.SendRequestAsync("/email/cancel", text))["value"];
			return ((jtoken != null) ? jtoken.ToString() : null) == "activation canceled";
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0005BC84 File Offset: 0x00059E84
		private async Task<JObject> SendRequestAsync(string endpoint, string query = "")
		{
			string text = (string.IsNullOrEmpty(query) ? "" : "&");
			string text2 = string.Concat(new string[] { "https://api.anymessage.shop", endpoint, "?token=", this._token, text, query });
			string text3 = await this._httpClient.GetStringAsync(text2);
			JObject jobject = JObject.Parse(text3);
			JToken jtoken = jobject["status"];
			if (((jtoken != null) ? jtoken.ToString() : null) == "error")
			{
				JToken jtoken2 = jobject["value"];
				throw new Exception(string.Concat(new string[]
				{
					"AnyMessage: Response: ",
					text3,
					" || URL: ",
					text2,
					" API Error: ",
					((jtoken2 != null) ? jtoken2.ToString() : null) ?? "unknown error"
				}));
			}
			return jobject;
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000027E6 File Offset: 0x000009E6
		public void Dispose()
		{
			HttpClient httpClient = this._httpClient;
			if (httpClient == null)
			{
				return;
			}
			httpClient.Dispose();
		}

		// Token: 0x04000494 RID: 1172
		private readonly string _token = Settings.Default.AnyMessageApiKey;

		// Token: 0x04000495 RID: 1173
		private readonly HttpClient _httpClient;

		// Token: 0x04000496 RID: 1174
		private const string BaseUrl = "https://api.anymessage.shop";

		// Token: 0x04000497 RID: 1175
		public AnyMessageClient.EmailData emailDataC = new AnyMessageClient.EmailData();

		// Token: 0x04000498 RID: 1176
		public AnyMessageClient.MessageData messageDataC = new AnyMessageClient.MessageData();

		// Token: 0x0200008C RID: 140
		public class EmailData
		{
			// Token: 0x04000499 RID: 1177
			public bool IsLong;

			// Token: 0x0400049A RID: 1178
			public string email;

			// Token: 0x0400049B RID: 1179
			public string id;

			// Token: 0x0400049C RID: 1180
			public string Exception;
		}

		// Token: 0x0200008D RID: 141
		public class MessageData
		{
			// Token: 0x0400049D RID: 1181
			public string status;

			// Token: 0x0400049E RID: 1182
			public string value;

			// Token: 0x0400049F RID: 1183
			public string message;

			// Token: 0x040004A0 RID: 1184
			public string Exception;
		}
	}
}
