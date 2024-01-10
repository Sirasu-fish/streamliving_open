namespace Main.Api.StreamerLiving.Dto
{
    public class SL_ResponseTwitterDto
    {
        public int num { get; set; }
        public List<string> id { get; set; }
        public List<string> user_icon { get; set; }
        public List<string> user_name { get; set; }
        public List<string> user_id { get; set; }
        public List<string> created_at { get; set; }
        public List<string> tweet { get; set; }
        public List<int> retweet { get; set; }
        public List<int> reply { get; set; }
        public List<int> like { get; set; }
        public List<int> impression { get; set; }
    }
}
