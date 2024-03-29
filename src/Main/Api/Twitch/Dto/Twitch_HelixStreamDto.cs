﻿namespace Main.Api.Twitch.Dto
{
    public class Twitch_HelixStreamDto
    {
        public Twitch_HelixStream_data[] data { get; set; }
    }

    public class Twitch_HelixStream_data
    {
        public string? id { get; set; }
        public string? user_id { get; set; }
        public string? user_login { get; set; }
        public string? user_name { get; set; }
        public string? game_id { get; set; }
        public string? type { get; set; }
        public string? title { get; set; }
        public string[]? tags { get; set; }
        public int? viewer_count { get; set; }
        public string? started_at { get; set; }
        public string? language { get; set; }
        public string? thumbnail_url { get; set; }
        public bool? is_mature { get; set; }
    }
}
