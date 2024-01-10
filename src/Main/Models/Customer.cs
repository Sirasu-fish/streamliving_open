namespace Main.Models
{
	public class Customer
	{
		public int Id { get; set; }
        public string twitter_id { get; set; }
        public string? twitter_accesstoken { get; set; }
		public string? twitter_refreshtoken { get; set; }
		public string? twitch_accesstoken { get; set; }
		public string? twitch_refreshtoken { get; set; }
	}
}
