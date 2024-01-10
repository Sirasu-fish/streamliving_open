jQuery(function() {
    var ini = ".streamer";
    var num = 5;
    var st_open = [];
    var st_close = [];
    var st_container = [];
    var st_prev = [];
    var st_next = [];
    var st_name = [];
    var st_twitter = [];
    var st_youtube = [];
    var st_twitch = [];
    var st_regist = [];
    var st_delete = [];
    var st_msg = [];

    for (var i=1; i<=num; i++)
    {
        st_open.push(jQuery(ini + '.' + String(i) + '.modal-open'));
        st_close.push(jQuery(ini + '.' + String(i) + ' .modal-close'));
        st_container.push(jQuery(ini + '.' + String(i) +'.modal-container'));

        if (!(i == 1)) {
            st_prev.push(jQuery(ini + '.' + String(i) + ' .modal-prev'));
        }
        else
        {
            st_prev.push('');
        }
        if (!(i == num)) {
            st_next.push(jQuery(ini + '.' + String(i) + ' .modal-next'));
        }
        else
        {
            st_next.push('');
        }

        st_name.push(jQuery(ini + '.' + String(i) + ' .name'));
        st_twitter.push(jQuery(ini + '.' + String(i) + ' .twitter'));
        st_youtube.push(jQuery(ini + '.' + String(i) + ' .youtube'));
        st_twitch.push(jQuery(ini + '.' + String(i) + ' .twitch'));
        st_regist.push(jQuery(ini + '.' + String(i) + ' .regist'));
        st_delete.push(jQuery(ini + '.' + String(i) + ' .delete'));
        st_msg.push(jQuery(ini + '.' + String(i) + ' .msg'))
    }

    jQuery(ini + '.modal-open').on('click', function (e) {
        st_open.forEach(function (element, index) {
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                st_container[index].addClass('active');
                return false;
            }
        });
    });

    jQuery(ini + ' .modal-close').on('click', function(e) {
        st_close.forEach(function (element, index) {
            var test = e.target.className;
            var test2 = index + 1;
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                st_msg[index].html("");
                st_container[index].removeClass('active');
                return false;
            }
        });
    });

    jQuery(ini + ' .modal-prev').on('click', function(e) {
        st_prev.forEach(function (element, index) {
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                st_msg[index].html("");
                st_container[index].removeClass('active');
                st_container[index - 1].addClass('active');
                return false;
            }
        });
    });

    jQuery(ini + ' .modal-next').on('click', function (e) {
        st_next.forEach(function (element, index) {
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                st_msg[index].html("");
                st_container[index].removeClass('active');
                st_container[index + 1].addClass('active');
                return false;
            }
        });
    });

    jQuery(ini + ' .regist').on('click', function (e) {
        st_regist.forEach(function (element, index) {
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                RegistStreamer(index + 1
                             , jQuery(st_name[index]).val()
                             , jQuery(st_twitter[index]).val()
                             , jQuery(st_youtube[index]).val()
                             , jQuery(st_twitch[index]).val()
                             , st_msg[index]
                             , st_regist[index] );

            }
        });
    });

    jQuery(ini + ' .delete').on('click', function (e) {
        st_regist.forEach(function (element, index) {
            if (jQuery(e.target).closest('.' + String(index + 1)).length) {
                DeleteStreamer(index + 1
                             , jQuery(st_name[index]).val()
                             , jQuery(st_twitter[index]).val()
                             , jQuery(st_youtube[index]).val()
                             , jQuery(st_twitch[index]).val()
                             , st_msg[index]
                             , st_delete[index] );

            }
        });
    });

    jQuery(document).on('click', function (e) {
        if (!jQuery(e.target).closest('.modal-body').length && !jQuery(e.target).attr('class').includes("modal-open") ) {
            st_container.forEach(function (element, index) {
                st_msg[index].html("");
                element.removeClass('active');
            });
        }
    });

    function RegistStreamer(num, name, twitter, youtube, twitch, msg, regist) {
        jQuery(regist).addClass("disabled");
        jQuery.ajax({
            type: "POST",
            url: "https://localhost/main/Api/RegistStreamer",
            contentType: "application/json",
            data: JSON.stringify({
                "num": num,
                "name": name,
                "twitter": twitter,
                "youtube": youtube,
                "twitch": twitch
            }),
            dataType:"json"
        }).done(function (data) {  
            if (data == null) {
                jQuery(msg).html("登録に失敗しました。");
                return false;
            }

            if (data.result != null && data.result == true)
            {
                jQuery(msg).html("登録しました。");
                jQuery('.' + num + '.content_list_twitter_title').html(name);
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-name').html(name);
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-offline-list-js-item-name').html(name);
                jQuery('.left_nav-online-list-js-item.' + num).css('display');
                jQuery('.left_nav-offline-list-js-item.' + num).css('display');
                jQuery('.content_list_twitter_' + num + ' .content_list_twitter_items').html('');
                if (jQuery('.left_nav-online-list-js-item.' + num).css('display') == 'none' && jQuery('.left_nav-offline-list-js-item.' + num).css('display') == 'none')
                {
                    jQuery('.left_nav-offline-list-js-item.' + num).show();
                }
                if (twitter != "")
                {
                    jQuery('.content_list_twitter_' + num + ' .content_list_twitter_subtitle').html(
                        '<a href="https://twitter.com/' + twitter + '" target="_blank rel="noopener noreferrer">' +
                        '<div><span>@</span>' + twitter + '</div>'
                        );
                    jQuery('.content_list_twitter_' + num + ' .content_list_twitter_id').val(twitter);
                }
                if (youtube != "")
                {
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube');
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').addClass('regist');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube').addClass('regist');
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube-js').html(
                        '<a href="https://www.youtube.com/' + youtube + '" target="_blank" rel="noopener noreferrer">' +
                        '<span class="left_nav-online-list-js-item-youtube false regist"></span>' +
                        '</a>'
                    );
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube-js').html(
                        '<a href="https://www.youtube.com/' + youtube + '" target="_blank" rel="noopener noreferrer">' +
                        '<span class="left_nav-offline-list-js-item-youtube false regist"></span>' +
                        '</a>'
                    );
                }
                else
                {
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').removeClass('regist');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube').removeClass('regist');
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube-js').html('<span class="left_nav-online-list-js-item-youtube false"></span>');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube-js').html('<span class="left_nav-offline-list-js-item-youtube false"></span>');
                }
                if (twitch != "")
                {
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').addClass('regist');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch').addClass('regist');
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch-js').html(
                        '<a href="https://www.twitch.tv/' + twitch + '" target="_blank" rel="noopener noreferrer">' +
                        '<span class="left_nav-online-list-js-item-twitch false regist"></span>' +
                        '</a>'
                    );
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch-js').html(
                        '<a href="https://www.twitch.tv/' + twitch + '" target="_blank" rel="noopener noreferrer">' +
                        '<span class="left_nav-offline-list-js-item-twitch false regist"></span>' +
                        '</a>'
                    );
                }
                else
                {
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').removeClass('regist');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch').removeClass('regist');
                    jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch-js').html('<span class="left_nav-online-list-js-item-twitch false"></span>');
                    jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch-js').html('<span class="left_nav-offline-list-js-item-twitch false"></span>');
                }
                return true;
            }
            else
            {
                if (data.msg != null && data.msg != "") {
                    jQuery(msg).html(data.msg);
                    return false;
                }
                else
                {
                    jQuery(msg).html("登録に失敗しました。");
                    return false;
                }
            }
        }).fail(function () {
            jQuery(msg).html("登録に失敗しました。");
            return false;
        }).always(function () {
            jQuery(regist).removeClass("disabled");
        });
    }

    function DeleteStreamer(num, name, twitter, youtube, twitch, msg, del) {
        jQuery(del).addClass("disabled");
        jQuery.ajax({
            type: "POST",
            url: "https://localhost/main/Api/DeleteStreamer",
            contentType: "application/json",
            data: JSON.stringify({
                "num": num,
                "name": name,
                "twitter": twitter,
                "youtube": youtube,
                "twitch": twitch
            }),
            dataType:"json"
        }).done(function (data) {
            if (data == null) {
                jQuery(msg).html("削除に失敗しました。");
                return false;
            }

            if (data.result != null && data.result == true)
            {
                jQuery(msg).html("削除しました。");
                jQuery('.streamer.' + num + ' .name').val('');
                jQuery('.streamer.' + num + ' .twitter').val('');
                jQuery('.streamer.' + num + ' .youtube').val('');
                jQuery('.streamer.' + num + ' .twitch').val('');
                jQuery('.' + num + '.content_list_twitter_title').html('-');
                jQuery('.content_list_twitter_' + num + ' .content_list_twitter_subtitle').html('-');
                jQuery('.content_list_twitter_' + num + ' .content_list_twitter_id').val('');
                jQuery('.content_list_twitter_' + num + ' .content_list_twitter_items').html('');

                jQuery('.left_nav-online-list-js-item.' + num).hide();
                jQuery('.left_nav-offline-list-js-item.' + num).hide();
                jQuery('.left_nav-online-list-js-item' + num + ' .left_nav-online-list-js-item-name').html('');
                jQuery('.left_nav-online-list-js-item' + num + ' .left_nav-offline-list-js-item-name').html('');
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube').removeClass('regist');
                jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube').removeClass('regist');
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-youtube-js').html('<span class="left_nav-online-list-js-item-youtube false"></span>');
                jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-youtube-js').html('<span class="left_nav-offline-list-js-item-youtube false"></span>');
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch').removeClass('regist');
                jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch').removeClass('regist');
                jQuery('.left_nav-online-list-js-item.' + num + ' .left_nav-online-list-js-item-twitch-js').html('<span class="left_nav-online-list-js-item-twitch false"></span>');
                jQuery('.left_nav-offline-list-js-item.' + num + ' .left_nav-offline-list-js-item-twitch-js').html('<span class="left_nav-offline-list-js-item-twitch false"></span>');
                return true;
            }
            else
            {
                if (data.msg != null && data.msg != "") {
                    jQuery(msg).html(data.msg);
                    return false;
                }
                else
                {
                    jQuery(msg).html("削除に失敗しました。");
                    return false;
                }
            }
        }).fail(function () {
            jQuery(msg).html("削除に失敗しました。");
            return false;
        }).always(function () {
            jQuery(del).removeClass("disabled");
        });
    }
});