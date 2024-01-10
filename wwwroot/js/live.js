function RequestTwitch() {
    jQuery.ajax({
        type: "POST",
        url: "https://localhost/main/Api/Live",
        contentType: "application/json",
        data: JSON.stringify({
            "youtube": [jQuery('.left_nav_youtube_id').eq(0).val()
                , jQuery('.left_nav_youtube_id').eq(1).val()
                , jQuery('.left_nav_youtube_id').eq(2).val()
                , jQuery('.left_nav_youtube_id').eq(3).val()
                , jQuery('.left_nav_youtube_id').eq(4).val()],
            "twitch": [
                , jQuery('.left_nav_twitch_id').eq(0).val()
                , jQuery('.left_nav_twitch_id').eq(1).val()
                , jQuery('.left_nav_twitch_id').eq(2).val()
                , jQuery('.left_nav_twitch_id').eq(3).val()
                , jQuery('.left_nav_twitch_id').eq(4).val()]
        }),
        dataType: "json"
    }).done(function (data) {
        data.forEach(function (item, index) {
            CreateView(
                index + 1
                ,item.twitch
                , item.youtube
            )
        });
    }).fail(function () {
        
    }).always(function () {

    });
}

function CreateView(num, twitch, youtube) {
    if (twitch || youtube)
    {
        jQuery('.left_nav-online-list-js-item.' + num).show();
        jQuery('.left_nav-offline-list-js-item.' + num).hide();
    }
    else
    {
        jQuery('.left_nav-online-list-js-item.' + num).hide();
        jQuery('.left_nav-offline-list-js-item.' + num).show();
    }
    if (twitch)
    {
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').removeClass('false');
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').addClass('true');
    }
    else
    {
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').removeClass('true');
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').addClass('false');
    }
    if (youtube)
    {
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').removeClass('false');
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').addClass('true');
    }
    else
    {
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').removeClass('true');
        jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').addClass('false');
    }
}


function getTwitch() {
    setInterval(RequestTwitch, 2000);
}

jQuery(document).ready(function () {
    getTwitch();
});