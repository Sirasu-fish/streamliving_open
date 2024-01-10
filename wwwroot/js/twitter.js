function RequestTwitter() {
    jQuery.ajax({
        type: "POST",
        url: "https://localhost/main/Api/Twitter",
        contentType: "application/json",
        data: JSON.stringify({
            "twitter_id": [jQuery('.content_list_twitter_id').eq(0).val()
                         , jQuery('.content_list_twitter_id').eq(1).val()
                         , jQuery('.content_list_twitter_id').eq(2).val()
                         , jQuery('.content_list_twitter_id').eq(3).val()
                         , jQuery('.content_list_twitter_id').eq(4).val()],
            "tweet_id": [jQuery('.content_list_twitter_items').eq(0).children('.content_list_twitter_items_tweet').eq(0).attr('id')
                       , jQuery('.content_list_twitter_items').eq(1).children('.content_list_twitter_items_tweet').eq(0).attr('id')
                       , jQuery('.content_list_twitter_items').eq(2).children('.content_list_twitter_items_tweet').eq(0).attr('id')
                       , jQuery('.content_list_twitter_items').eq(3).children('.content_list_twitter_items_tweet').eq(0).attr('id')
                       , jQuery('.content_list_twitter_items').eq(4).children('.content_list_twitter_items_tweet').eq(0).attr('id')]
        })
    }).done(function (data) {
        if (typeof data === "undefined" || data == [])
        {
        } else {
            data.forEach(function (item) {
                var body = "";

                for (var i = 0, len = item.tweet.length; i < len; i++)
                {
                    body += "<li id=\"" + item.id[i] + "\" class=\"content_list_twitter_items_tweet js-new-tweet\">";
        
                    body += "<a href=\"https://twitter.com/" + item.user_id[i] + "\" class=\"content_list_twitter_items_tweet_head\" target=\"_blank\" rel=\"noopener noreferrer\">";
        
                    body += "<span class=\"content_list_twitter_items_tweet_head_icon\">";
                    body += "<img src=\"" + item.user_icon[i] + "\">";
                    body += "</span>";
        
                    body += "<span class=\"content_list_twitter_items_tweet_head_name\">";
                    body += item.user_name[i];
                    body += "</span>";
        
                    body += "<span class=\"content_list_twitter_items_tweet_head_id\">";
                    body += "@@" + item.user_id[i];
                    body += "</span>";
        
                    body += "<span class=\"content_list_twitter_items_tweet_head_time\">";
                    body += "    " + item.created_at[i];
                    body += "</span>";
        
                    body += "</a>";
                
                    body += "<a class=\"content_list_twitter_items_tweet_link\" href=\"https://twitter.com/" + item.user_id[i] + "/status/" + item.id[i] + "\" target=\"_blank\" rel=\"noopener noreferrer\">";
                    body += "<div class=\"content_list_twitter_items_tweet_body\">";
                    body += item.tweet[i];
                    body += "</div>";
                    body += "</a>"
        
                    body += "</li>";
                }

                jQuery('.content_list_twitter_items').eq(item.num).children('.content_list_twitter_items_tweet').eq(0).before(
                    body
                );
            });
        }
    }).fail(function () {
        console.error('twitter error');
    }).always(function () {

    });
}

function CreateView(id, user_icon, user_name, user_id, created_at, tweet) {
    var body = "";

    for (var i = 0, len = tweet.length; i < len; i++)
    {
        body += "<li id=\"" + id[i] + "\" class=\"content_list_twitter_items_tweet js-new-tweet\">";

        body += "<a href=\"https://twitter.com/" + user_id[i] + "\" class=\"content_list_twitter_items_tweet_head\" target=\"_blank\" rel=\"noopener noreferrer\">";

        body += "<span class=\"content_list_twitter_items_tweet_head_icon\">";
        body += "<img src=\"" + user_icon[i] + "\">";
        body += "</span>";

        body += "<span class=\"content_list_twitter_items_tweet_head_name\">";
        body += user_name[i];
        body += "</span>";

        body += "<span class=\"content_list_twitter_items_tweet_head_id\">";
        body += "@@" + user_id[i];
        body += "</span>";

        body += "<span class=\"content_list_twitter_items_tweet_head_time\">";
        body += "    " + created_at[i];
        body += "</span>";

        body += "</a>";
        
        body += "<a class=\"content_list_twitter_items_tweet_link\" href=\"https://twitter.com/" + user_id[i] + "/status/" + id[i] + "\" target=\"_blank\" rel=\"noopener noreferrer\">";
        body += "<div class=\"content_list_twitter_items_tweet_body\">";
        body += tweet[i];
        body += "</div>";
        body += "</a>"

        body += "</li>";
    }

    return body;
}

function getTwitter() {
    jQuery.ajax({
        type: "POST",
        url: "https://localhost/main/Api/Twitter",
        contentType: "application/json",
        data: 
            JSON.stringify({
                "twitter_id": [jQuery('.content_list_twitter_id').eq(0).val()
                    , jQuery('.content_list_twitter_id').eq(1).val()
                    , jQuery('.content_list_twitter_id').eq(2).val()
                    , jQuery('.content_list_twitter_id').eq(3).val()
                    , jQuery('.content_list_twitter_id').eq(4).val()]
            })
        }).done(function (data) {
            if (typeof data === "undefined" || data == [])
            {
                setTimeout(function(){}, 10000);
                getTwitter();
            } else {
                data.forEach(function (item) {
                    var body = "";

                    for (var i = 0, len = item.tweet.length; i < len; i++)
                    {
                        body += "<li id=\"" + item.id[i] + "\" class=\"content_list_twitter_items_tweet js-new-tweet\">";
                
                        body += "<a href=\"https://twitter.com/" + item.user_id[i] + "\" class=\"content_list_twitter_items_tweet_head\" target=\"_blank\" rel=\"noopener noreferrer\">";
                
                        body += "<span class=\"content_list_twitter_items_tweet_head_icon\">";
                        body += "<img src=\"" + item.user_icon[i] + "\">";
                        body += "</span>";
                
                        body += "<span class=\"content_list_twitter_items_tweet_head_name\">";
                        body += item.user_name[i];
                        body += "</span>";
                
                        body += "<span class=\"content_list_twitter_items_tweet_head_id\">";
                        body += "@@" + item.user_id[i];
                        body += "</span>";
                
                        body += "<span class=\"content_list_twitter_items_tweet_head_time\">";
                        body += "    " + item.created_at[i];
                        body += "</span>";
                
                        body += "</a>";
                        
                        body += "<a class=\"content_list_twitter_items_tweet_link\" href=\"https://twitter.com/" + item.user_id[i] + "/status/" + item.id[i] + "\" target=\"_blank\" rel=\"noopener noreferrer\">";
                        body += "<div class=\"content_list_twitter_items_tweet_body\">";
                        body += item.tweet[i];
                        body += "</div>";
                        body += "</a>"
                
                        body += "</li>";
                    }

                    jQuery('.content_list_twitter_items').eq(item.num).html(
                        body
                    )
                });
            }
            setInterval(RequestTwitter, 60000);
    }).fail(function () {
        setTimeout(function(){}, 60000);
        getTwitter();
    }).always(function () {
    });
}

jQuery(document).ready(function() {
    for (var i=0; i<5; i++)
    {
        if (jQuery('.content_list_twitter_id').eq(i).val().length)
        {
            jQuery('.content_list_twitter_items').eq(i).html("<li>取得中です。</li>");
        }
    }
    
    getTwitter()
});