jQuery(function() {
    var open = jQuery('.about.modal-open'),
        close = jQuery('.about .modal-close'),
        container = jQuery('.about.modal-container');

    open.on('click', function() {
        container.addClass('active');
        return false;
    });

    close.on('click', function() {
        container.removeClass('active');
    });

    jQuery(document).on('click', function(e) {
        if (!jQuery(e.target).closest('.modal-body').length) {
            container.removeClass('active');
        }
    });
});