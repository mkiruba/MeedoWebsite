/*
** nopCommerce custom js functions
*/



function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading-block-window').show();
    }
    else {
        $('.ajax-loading-block-window').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error, warning
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog-notifications-success');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-notifications-error');
    }
    else if (messagetype == 'warning') {
        //warning
        container = $('#dialog-notifications-warning');
    }
    else {
        //other
        container = $('#dialog-notifications-success');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }

    container.html(htmlcode);

    var isModal = (modal ? true : false);
    container.dialog({
        modal: isModal,
        width: 350
    });
}
function displayPopupContentFromUrl(url, title, modal, width) {
    var isModal = (modal ? true : false);
    var targetWidth = (width ? width : 550);
    var maxHeight = $(window).height() - 20;

    $('<div></div>').load(url)
        .dialog({
            modal: isModal,
            position: ['center', 20],
            width: targetWidth,
            maxHeight: maxHeight,
            title: title,
            close: function (event, ui) {
                $(this).dialog('destroy').remove();
            }
        });
}

var barNotificationTimeout;
function displayBarNotification(message, messagetype, timeout) {
    clearTimeout(barNotificationTimeout);
    //https://github.com/CodeSeven/toastr
    //types: success, error, warning
    //var cssclass = 'success';
    //if (messagetype == 'success') {
    //    cssclass = 'success';
    //}
    //else if (messagetype == 'error') {
    //    cssclass = 'error';
    //}
    //else if (messagetype == 'warning') {
    //    cssclass = 'warning';
    //}
    ////remove previous CSS classes and notifications
    //$('#bar-notification')
    //    .removeClass('success')
    //    .removeClass('error')
    //    .removeClass('warning');
    //$('#bar-notification .content').remove();

    //we do not encode displayed message
    stopNotification();
    toastr.clear();
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": false,
        "positionClass": "toast-top-center",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    //add new notifications
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p class="content">' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p class="content">' + message[i] + '</p>';
        }
    }
    if (messagetype == 'success') {       
        toastr.options.positionClass = 'toast-top-center';
        toastr.success(htmlcode);
    }
    else if (messagetype == 'error') {
        toastr.options.positionClass = 'toast-top-center';
        toastr.error(htmlcode);
    }
    else if (messagetype == 'warning') {
        toastr.options.positionClass = 'toast-top-center';
        toastr.warning(htmlcode);
    }
    startNotification();
    //$('#bar-notification').append(htmlcode)
    //    .addClass(cssclass)
    //    .fadeIn('slow')
    //    .mouseenter(function () {
    //        clearTimeout(barNotificationTimeout);
    //    });

    //$('#bar-notification .close').unbind('click').click(function () {
    //    $('#bar-notification').fadeOut('slow');
    //});

    ////timeout (if set)
    //if (timeout > 0) {
    //    barNotificationTimeout = setTimeout(function () {
    //        $('#bar-notification').fadeOut('slow');
    //    }, timeout);
    //}
}
//function displayBarNotification(message, messagetype, timeout) {
//    clearTimeout(barNotificationTimeout);

//    //types: success, error, warning
//    var cssclass = 'success';
//    if (messagetype == 'success') {
//        cssclass = 'success';
//    }
//    else if (messagetype == 'error') {
//        cssclass = 'error';
//    }
//    else if (messagetype == 'warning') {
//        cssclass = 'warning';
//    }
//    //remove previous CSS classes and notifications
//    $('#bar-notification')
//        .removeClass('success')
//        .removeClass('error')
//        .removeClass('warning');
//    $('#bar-notification .content').remove();

//    //we do not encode displayed message

//    //add new notifications
//    var htmlcode = '';
//    if ((typeof message) == 'string') {
//        htmlcode = '<p class="content">' + message + '</p>';
//    } else {
//        for (var i = 0; i < message.length; i++) {
//            htmlcode = htmlcode + '<p class="content">' + message[i] + '</p>';
//        }
//    }
//    $('#bar-notification').append(htmlcode)
//        .addClass(cssclass)
//        .fadeIn('slow')
//        .mouseenter(function ()
//            {
//                clearTimeout(barNotificationTimeout);
//            });

//    $('#bar-notification .close').unbind('click').click(function () {
//        $('#bar-notification').fadeOut('slow');
//    });

//    //timeout (if set)
//    if (timeout > 0) {
//        barNotificationTimeout = setTimeout(function () {
//            $('#bar-notification').fadeOut('slow');
//        }, timeout);
//    }
//}

var cities = null, allProducts = null, timer = null, innerhtml = '', interval = 10000;

function displayRecentSalesNotification(productUrl) {

    cities = ["Chennai", "Pune", "Mumbai", "Kolkata", "Bangalore", "Cochin", "Coimbatore", "Madurai"];
    toastr.clear();
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "1000",
        "hideDuration": "100",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    $.ajax({
        cache: false,
        type: "GET",
        url: productUrl,        
        success: function (data) { 
            if (data.success) {
                allProducts = data.products;
                //add new notifications
                startNotification();
            }            
        }        
    });

    ////add new notifications
    //startNotification();
    //var htmlcode = '';  
    //setInterval(function () {   
    //    if (allProducts && allProducts.length > 0) {
    //        var randProductIndex = Math.floor((Math.random() * allProducts.length));
    //        var randCityIndex = Math.floor((Math.random() * cities.length));
    //        var product = allProducts[randProductIndex];
    //        var thumbnail = '<img src="' + product.DefaultPictureModel.ImageUrl + '" alt="' + product.DefaultPictureModel.AlternateText + '" align="left" height="60" width="45">';
    //        htmlcode = thumbnail + '<p class="sales-notification">' + 'Someone from ' + cities[randCityIndex] + ' bought <b>' + product.Name + '</b></p>';
    //        toastr.options.positionClass = 'toast-bottom-right';
    //        toastr.info(htmlcode);                      
    //    }       
    //}, 10000);     
}

function startNotification() {
    if (timer !== null) return;
    timer = setInterval(function () {
        if (allProducts && allProducts.length > 0) {
            var randProductIndex = Math.floor((Math.random() * allProducts.length));
            var randCityIndex = Math.floor((Math.random() * cities.length));
            var product = allProducts[randProductIndex];
            var thumbnail = '<img src="' + product.DefaultPictureModel.ImageUrl + '" alt="' + product.DefaultPictureModel.AlternateText + '" align="left" height="60" width="45">';
            innerhtml = thumbnail + '<p class="sales-notification">' + 'Someone from ' + cities[randCityIndex] + ' bought <b>' + product.Name + '</b></p>';
            toastr.options.positionClass = 'toast-bottom-right';
            toastr.info(innerhtml);            
        }
    }, interval); 
}

function stopNotification() {
    toastr.cl
    clearInterval(timer);
    timer = null;
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};