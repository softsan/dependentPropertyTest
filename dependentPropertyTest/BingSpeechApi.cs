using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Xamarin.Forms;
using PCLStorage;


namespace dependentPropertyTest
{
	public class AccessTokenInfo
	{
		public string access_token { get; set; }

		public string token_type { get; set; }

		public string expires_in { get; set; }

		public string scope { get; set; }
	}

	public class SpeechResult
	{
		public string Scenario { get; set; }

		public string Name { get; set; }

		public string Lexical { get; set; }

		public string Confidence { get; set; }
	}

	public class RootObject
	{
		public List<SpeechResult> results { get; set; }
	}

	public class BingSpeechApi
	{
		static IFileSystem LocalFileSystem { get { return FileSystem.Current; } }

		public static readonly string AccessUri = "https://oxford-speech.cloudapp.net/token/issueToken";

		public static string TempFile = "BingSpeechApi.wav";

		//Access token expires every 10 minutes. Renew it every 9 minutes only.
		private const int RefreshTokenDuration = 9;


		private static AccessTokenInfo token;
		private static DateTime expires;

		public static async Task<SpeechResult> SpeechToText ()
		{

			string requestUri = "https://speech.platform.bing.com/recognize";
			//https://api.projectoxford.ai/speech/v0

			/* URI Params. Refer to the README file for more information. */
			// websearch is the other main option.
			requestUri += @"?scenarios=smd";
			// You must use this ID.             
			requestUri += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";
			// We support several other languages.  Refer to README file.  
			requestUri += @"&locale=nl-NL"; //en-US";
			requestUri += @"&device.os=wp7";
			requestUri += @"&version=3.0";
			requestUri += @"&format=json";
			requestUri += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
			requestUri += @"&requestid=" + Guid.NewGuid ();

			string host = @"speech.platform.bing.com";
			string contentType = @"audio/wav; codec=""audio/pcm""; samplerate=16000";


			token = await GetAccessToken ();

			Debug.WriteLine ("Token: {0}\n", token.access_token);

			Debug.WriteLine ("Request Uri: " + requestUri + Environment.NewLine);

			//Use HttpClient in PCL
			HttpClient client = new HttpClient ();
			client.BaseAddress = new Uri (AccessUri);
			client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
			client.DefaultRequestHeaders.TransferEncodingChunked = true;
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Bearer", token.access_token);
			client.DefaultRequestHeaders.Host = host;

			//read file
			var file = await LocalFileSystem.LocalStorage.GetFileAsync (TempFile);

			//send file
			using (var stream = await file.OpenAsync (FileAccess.Read)) {
				var content = new StreamContent (stream);
				content.Headers.ContentType = MediaTypeHeaderValue.Parse (contentType);
				var result = await client.PostAsync (requestUri, content);
				//var t = await result.Content.ReadAsStringAsync ();
				//parse file
				return JsonConvert.DeserializeObject<RootObject> (await result.Content.ReadAsStringAsync ()).results [0];
			}

		}

		/// <summary>
		/// Returns access token to Azure Marketplace
		/// </summary>
		/// <returns></returns>
		public static async Task<AccessTokenInfo> GetAccessToken ()
		{
			//"voice-dictation-app", "b079a0014389449dbf71c891cb65d4bd"

			//return cached if not expired
			if (token != null && expires > DateTime.UtcNow) {
				return token;
			}

			//set expire
			expires = DateTime.UtcNow.AddMinutes (RefreshTokenDuration);

			var request = string.Format ("grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}",
				              WebUtility.UrlEncode ("voice-dictation-app"),
				              WebUtility.UrlEncode ("b079a0014389449dbf71c891cb65d4bd"),
				              WebUtility.UrlEncode ("https://speech.platform.bing.com"));

			// UTF8 contains ASCII
			byte[] bytes = Encoding.UTF8.GetBytes (request);
			var contentType = new ByteArrayContent (bytes);
			contentType.Headers.ContentType = new MediaTypeHeaderValue ("application/x-www-form-urlencoded");
			// Use HttpClient in PCL
			HttpClient client = new HttpClient ();
			client.BaseAddress = new Uri (AccessUri);

			client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/x-www-form-urlencoded"));
			var result = await client.PostAsync (AccessUri, contentType);
			return JsonConvert.DeserializeObject<AccessTokenInfo> (await result.Content.ReadAsStringAsync ());

		}
	}
}



