jQuery(function() {
    var date = new Date();
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var week = ["日", "月", "火", "水", "木", "金", "土"];
    var dow = week[date.getDay()];
    jQuery(".content_list_schedule_subtitle_date").html(year + "年" + month + "月" + day + "日(" + dow + ")");

    jQuery(".content_list_schedule_subtitle_button-prev").on("click", function() {
        date.setDate(date.getDate() - 1);
        const year = date.getFullYear();
        const month = date.getMonth() + 1;
        const day = date.getDate();
        const dow = week[date.getDay()];
        jQuery(".content_list_schedule_subtitle_date").html(year + "年" + month + "月" + day + "日(" + dow + ")");
    });

    jQuery(".content_list_schedule_subtitle_button-next").on("click", function() {
        date.setDate(date.getDate() + 1);
        const year = date.getFullYear();
        const month = date.getMonth() + 1;
        const day = date.getDate();
        const dow = week[date.getDay()];
        jQuery(".content_list_schedule_subtitle_date").html(year + "年" + month + "月" + day + "日(" + dow + ")");
    });
});