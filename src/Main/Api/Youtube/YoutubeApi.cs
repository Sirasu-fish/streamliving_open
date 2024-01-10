using Main.Data;

namespace Main.Api.Youtube
{
	public class YoutubeApi
	{
		string api_key = "xxxxx"; // apikey
		string redirect_uri = "https://localhost/main/home/Youtube_GetAccessToken"; // https://console.cloud.google.com/apis/credentials/oauthclient?previousPage=%2Fapis%2Fcredentials%3Fproject%3Dsnappy-elf-373601&project=snappy-elf-373601
		string client_id = "xxxxx"; // クライアントID
		string client_secret = "xxxxx"; // クライアントシークレット

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// API: https://developers.google.com/youtube/v3/docs/search/list?hl=ja
        /// </remarks>
        /// <param name="context"></param>
        public string RequestSearchYoutube(MainContext context)
		{
            string url = "https://developers.google.com/youtube/v3/docs/search/list?hl=ja";

            return "";
        }
	}
}
