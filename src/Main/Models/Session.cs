namespace Main.Models
{
	public class Session
	{
		public int Id { get; set; }
		public string? twitter_id { get; set; }
		public string? loginsession { get; set; }
		public string? twitter_state { get; set; }
		public string? twitter_code_challenge { get; set; }
		public string? twitch_state { get; set; }
		public DateTime dt { get; set; }
	}
}
