namespace Main.Api.Twitter.Dto
{
    public class Twitter_TweetsDto
    {
        public Twitter_TweetsDto_data[]? data { get; set; }
        public Twitter_TweetsDto_includes? includes { get; set; }
    }

    public class Twitter_TweetsDto_data
    {
        public string? created_at { get; set; }
        public Twitter_TweetsDto_publicmetrics? public_metrics { get; set; }
        public Twitter_TweetsDto_referencedtweets[]? referenced_tweets { get; set; }
        public string? author_id { get; set; }
        public string? id { get; set; }
        //public string[]? edit_history_tweet_ids { get; set; }
        public string? text { get; set; }
        public string? in_reply_to_user_id { get; set; }


        public Twitter_TweetsDto_entities? entities { get; set; }

    }

    public class Twitter_TweetsDto_publicmetrics
    {
        public int retweet_count { get; set; }
        public int reply_count { get; set; }
        public int like_count { get; set; }
        public int quote_count { get; set; }
        public int impression_count { get; set; }
    }

    public class Twitter_TweetsDto_referencedtweets
    {
        public string? type { get; set; }
        public string? id { set; get; }
    }

    public class Twitter_TweetsDto_includes
    {
        public Twitter_TweetsDto_users[]? users { get; set; }
        public Twitter_TweetsDto_tweets[]? tweets { get; set; }
    }

    public class Twitter_TweetsDto_users
    {
        public string? username { get; set; }
        public string? pinned_tweet_id { get; set; }
        public string? id { get; set; }
        public string? profile_image_url { get; set; }
        public string? name { get; set; }
    }

    public class Twitter_TweetsDto_tweets
    {
        public string? id { get; set; }
        public Twitter_TweetsDto_referencedtweets[]? referenced_tweets { get; set; }
    }

    public class Twitter_TweetsDto_entities
    {
        //public Twitter_TweetsDto_mentions? mentions { get; set; }
        public string? test { get; set; }
    }

    public class Twitter_TweetsDto_mentions
    {
        public int? start { get; set; }
        public int? end { get; set; }
        public string? username { get; set; }
        public string? id { get; set; }
    }
}
