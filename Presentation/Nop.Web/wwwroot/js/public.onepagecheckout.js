/*
** nopCommerce one page checkout
*/
/*
** new checkout 
*/
var tab = {
    nextTab: function () {
        var $active = $('.wizard .nav-tabs li.active');
        $active.next().removeClass('disabled');
        $active.next().find('a[data-toggle="tab"]').click();
    },
    prevTab: function () {
        var $active = $('.wizard .nav-tabs li.active'); 
        var isBillingAddSame = $('.opc #opc-shipping').find('#BillingAddressSame');
        
        if (isBillingAddSame.is(":checked")) {// && $active.find('#opc-payment_info').length > 0) {
            $active = $active.prev();
        }       
        $active.prev().removeClass('disabled');
        $active.prev().find('a[data-toggle="tab"]').click();
    }
}

var Checkout = {
    loadWaiting: false,
    failureUrl: false,

    init: function (failureUrl) {
        this.loadWaiting = false;
        this.failureUrl = failureUrl;

        Accordion.disallowAccessToNextSections = true;
    },

    ajaxFailure: function () {
        location.href = Checkout.failureUrl;
    },
    
    _disableEnableAll: function (element, isDisabled) {
        var descendants = element.find('*');
        $(descendants).each(function() {
            if (isDisabled) {
                $(this).attr('disabled', 'disabled');
            } else {
                $(this).removeAttr('disabled');
            }
        });

        if (isDisabled) {
                element.attr('disabled', 'disabled');
            } else {
                element.removeAttr('disabled');
            }
    },

    setLoadWaiting: function (step, keepDisabled) {
        //step = 'payment-info';
        if (step) {
            if (this.loadWaiting) {
                this.setLoadWaiting(false);
            }
            var container = $('#' + step + '-buttons-container');
            container.addClass('disabled');
            container.css('opacity', '.5');
            this._disableEnableAll(container, true);
            $('#' + step + '-please-wait').show();
        } else {
            if (this.loadWaiting) {
                var container = $('#' + this.loadWaiting + '-buttons-container');
                var isDisabled = (keepDisabled ? true : false);
                if (!isDisabled) {
                    container.removeClass('disabled');
                    container.css('opacity', '1');
                }
                this._disableEnableAll(container, isDisabled);
                $('#' + this.loadWaiting + '-please-wait').hide();
            }
        }
        this.loadWaiting = step;
    },

    gotoSection: function (section) {
        var sectionName = 'opc-' + section;
        section = $('#'+sectionName);
        //Todo: 25/01/18 need to navigate to paymentinfo tab
        section.addClass('allow');
        //$('.wizard .nav-tabs li>a' + section).click();
        //Accordion.openSection(section);
        $('.wizard .nav-tabs li a[id="' + sectionName + '"]').removeClass('disabled');
        $('.wizard .nav-tabs li a[id="' + sectionName + '"]').parent().removeClass('disabled');
        $('.wizard .nav-tabs li a[id="' + sectionName +'"]').click()
    },

    back: function () {
        if (this.loadWaiting) return;
        //Accordion.openPrevSection(true, true);
        tab.prevTab();
    },

    setStepResponse: function (response) {
        if (response.update_section) {
            $('#checkout-' + response.update_section.name + '-load').html(response.update_section.html);
        }
        if (response.allow_sections) {
            response.allow_sections.each(function (e) {
                $('#opc-' + e).addClass('allow');
            });
        }
        
        //TODO move it to a new method
        if ($("#billing-address-input").length > 0) {
            var dataValue = $("#billing-address-input").val($(".list-group").find("a.active").data("value"));
            Billing.newAddress(!dataValue);
        }
        if ($("#shipping-address-input").length > 0) {
            Shipping.newAddress(!$('#shipping-address-input').val());
        }

        if (response.goto_section) {
            Checkout.gotoSection(response.goto_section);
            return true;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    }
};





var Billing = {
    form: false,
    saveUrl: false,
    disableBillingAddressCheckoutStep: false,

    init: function (form, saveUrl, disableBillingAddressCheckoutStep) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.disableBillingAddressCheckoutStep = disableBillingAddressCheckoutStep;
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#billing-new-address-form').show();
        } else {
            $('#billing-new-address-form').hide();
        }
        $.event.trigger({ type: "onepagecheckout_billing_address_new" });
    },

    resetSelectedAddress: function () {
        var selectElement = $('#billing-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $.event.trigger({ type: "onepagecheckout_billing_address_reset" }); 
    },

    save: function () {
        //if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('billing');
        
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        //ensure that response.wrong_billing_address is set
        //if not set, "true" is the default value
        if (typeof response.wrong_billing_address == 'undefined') {
            response.wrong_billing_address = false;
        }
        if (Billing.disableBillingAddressCheckoutStep) {
            if (response.wrong_billing_address) {
                Accordion.showSection('#opc-billing');
            } else {
                tab.nextTab();
                //Accordion.hideSection('#opc-billing');
            }
        }


        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }    
};



var Shipping = {
    form: false,
    saveUrl: false,
    sameBilling: false,
    init: function (form, saveUrl, sameBilling) {
        this.form = form;
        this.saveUrl = saveUrl;
        this.sameBilling = sameBilling;
        this.toggleBillingAddress(sameBilling);
    },

    newAddress: function (isNew) {
        if (isNew) {
            this.resetSelectedAddress();
            $('#shipping-new-address-form').show();
        } else {
            $('#shipping-new-address-form').hide();
        }
        $.event.trigger({ type: "onepagecheckout_shipping_address_new" });
    },

    togglePickUpInStore: function (pickupInStoreInput) {        
        if (pickupInStoreInput.checked) {
            $('#pickup-points-form').show();
            $('#shipping-addresses-form').hide();
        }
        else {
            $('#pickup-points-form').hide();
            $('#shipping-addresses-form').show();
        }
    },   
   
    resetSelectedAddress: function () {
        var selectElement = $('#shipping-address-select');
        if (selectElement) {
            selectElement.val('');
        }
        $.event.trigger({ type: "onepagecheckout_shipping_address_reset" });
    },

    save: function () {
        //if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('shipping');
       
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    },

    toggleBillingAddressSame: function (billingAddressSame) {
        sameBilling = billingAddressSame.checked;
        this.toggleBillingAddress(billingAddressSame.checked);
    },

    toggleBillingAddress: function (sameBilling) {
        if (sameBilling) {
            $('.connecting-line').width('30%');
            $('#opc-billing').parent().hide();
        }
        else {
            $('.connecting-line').width('50%');
            $('#opc-billing').parent().show();
        }
    }
};



var ShippingMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    validate: function () {
        var methods = document.getElementsByName('shippingoption');
        if (methods.length==0) {
            alert('Your order cannot be completed at this time as there is no shipping methods available for it. Please make necessary changes in your shipping address.');
            return false;
        }

        for (var i = 0; i< methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify shipping method.');
        return false;
    },
    
    save: function () {
        //if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('shipping-method');
        
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentMethod = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    toggleUseRewardPoints: function (useRewardPointsInput) {
        if (useRewardPointsInput.checked) {
            $('#payment-method-block').hide();
        }
        else {
            $('#payment-method-block').show();
        }
    },

    validate: function () {
        var methods = document.getElementsByName('paymentmethod');
        if (methods.length == 0) {
            alert('Your order cannot be completed at this time as there is no payment methods available for it.');
            return false;
        }
        
        for (var i = 0; i < methods.length; i++) {
            if (methods[i].checked) {
                return true;
            }
        }
        alert('Please specify payment method.');
        return false;
    },
    
    save: function () {
        //if (Checkout.loadWaiting != false) return;
        
        if (this.validate()) {
            Checkout.setLoadWaiting('payment-method');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                data: $(this.form).serialize(),
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        }
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};



var PaymentInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },

    save: function () {
        //if (Checkout.loadWaiting != false) return;
        
        Checkout.setLoadWaiting('payment-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
};
var SaveCheckoutInfo = {
    form: false,
    saveUrl: false,

    init: function (form, saveUrl) {
        this.form = form;
        this.saveUrl = saveUrl;
    },
    save: function () {
        Checkout.setLoadWaiting('checkout-info');
        $.ajax({
            cache: false,
            url: this.saveUrl,
            data: $(this.form).serialize(),
            type: 'post',
            success: this.nextStep,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },
    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },
    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }

        Checkout.setStepResponse(response);
    }
}


var ConfirmOrder = {
    form: false,
    saveUrl: false,
    isSuccess: false,

    init: function (saveUrl, successUrl) { 
        fbq('track', 'AddPaymentInfo');
        this.saveUrl = saveUrl;
        this.successUrl = successUrl;
    },

    save: function () {
        //if (Checkout.loadWaiting != false) return;
        
        //terms of service
        var termOfServiceOk = true;
        if ($('#termsofservice').length > 0) {
            //terms of service element exists
            if (!$('#termsofservice').is(':checked')) {
                $("#terms-of-service-warning-box").dialog();
                termOfServiceOk = false;
            } else {
                termOfServiceOk = true;
            }
        }
        if (termOfServiceOk) {
            Checkout.setLoadWaiting('confirm-order');
            $.ajax({
                cache: false,
                url: this.saveUrl,
                type: 'post',
                success: this.nextStep,
                complete: this.resetLoadWaiting,
                error: Checkout.ajaxFailure
            });
        } else {
            return false;
        }
    },
    
    resetLoadWaiting: function (transport) {
        Checkout.setLoadWaiting(false, ConfirmOrder.isSuccess);
    },

    nextStep: function (response) {
        if (response.error) {
            if ((typeof response.message) == 'string') {
                alert(response.message);
            } else {
                alert(response.message.join("\n"));
            }

            return false;
        }
        
        if (response.redirect) {
            ConfirmOrder.isSuccess = true;
            fbq('track', 'Purchase', { value: response.orderTotal, currency: 'INR' });
            location.href = response.redirect;
            return;
        }
        if (response.success) {
            ConfirmOrder.isSuccess = true;
            window.location = ConfirmOrder.successUrl;
        }

        Checkout.setStepResponse(response);
    }
};