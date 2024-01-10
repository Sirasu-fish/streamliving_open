namespace Main.Api.Twitter.Dto
{
    public class Twitter_UserMeDto
    {
        public Twitter_UserMeDto_data? data { get; set; }

    }

    public class Twitter_UserMeDto_data
    {
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }
}
