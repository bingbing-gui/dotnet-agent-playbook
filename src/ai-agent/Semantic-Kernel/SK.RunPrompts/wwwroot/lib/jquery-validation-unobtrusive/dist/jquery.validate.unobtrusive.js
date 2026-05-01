/**
 * @license
 * Unobtrusive validation support library for jQuery and jQuery Validate
 * Copyright (c) .NET Foundation. All rights reserved.
 * Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 * @version v4.0.0
 */

/*jslint white: true, browser: true, onevar: true, undef: true, nomen: true, eqeqeq: true, plusplus: true, bitwise: true, regexp: true, newcap: true, immed: true, strict: false */
/*global document: false, jQuery: false */

(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define("jquery.validate.unobtrusive", ['jquery-validation'], factory);
    } else if (typeof module === 'object' && module.exports) {
        // CommonJS-like environments that support module.exports     
        module.exports = factory(require('jquery-validation'));
    } else {
        // Browser global
        jQuery.validator.unobtrusive = factory(jQuery);
    }
}(function ($) {
    var $jQval = $.validator,
        adapters,
        data_validation = "unobtrusiveValidation";

    function setValidationValues(options, ruleName, value) {
        options.rules[ruleName] = value;
        if (options.message) {
            options.messages[ruleName] = options.message;
        }
    }

    function splitAndTrim(value) {
        return value.replace(/^\s+|\s+$/g, "").split(/\s*,\s*/g);
    }

    function escapeAttributeValue(value) {
        // As mentioned on http://api.jquery.com/category/selectors/
        return value.replace(/([!"#$%&'()*+,./:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
    }

    function getModelPrefix(fieldName) {
        return fieldName.substr(0, fieldName.lastIndexOf(".") + 1);
    }

    function appendModelPrefix(value, prefix) {
        if (value.indexOf("*.") === 0) {
            value = value.replace("*.", prefix);
        }
        return value;
    }

    function onError(error, inputElement) {  // 'this' is the form element
        var container = $(this).find("[data-valmsg-for='" + escapeAttributeValue(inputElement[0].name) + "']"),
            replaceAttrValue = container.attr("data-valmsg-replace"),
            replace = replaceAttrValue ? $.parseJSON(replaceAttrValue) !== false : null;

        container.removeClass("field-validation-valid").addClass("field-validation-error");
        error.data("unobtrusiveContainer", container);

        if (replace) {
            container.empty();
            error.removeClass("input-validation-error").appendTo(container);
        }
        else {
            error.hide();
        }
    }

    function onErrors(event, validator) {  // 'this' is the form element
        var container = $(this).find("[data-valmsg-summary=true]"),
            list = container.find("ul");

        if (list && list.length && validator.errorList.length) {
            list.empty();
            container.addClass("validation-summary-errors").removeClass("validation-summary-valid");

            $.each(validator.errorList, function () {
                $("<li />").html(this.message).appendTo(list);
            });
        }
    }

    function onSuccess(error) {  // 'this' is the form element
        var container = error.data("unobtrusiveContainer");

        if (container) {
            var replaceAttrValue = container.attr("data-valmsg-replace"),
                replace = replaceAttrValue ? $.parseJSON(replaceAttrValue) : null;

            container.addClass("field-validation-valid").removeClass("field-validation-error");
            error.removeData("unobtrusiveContainer");

            if (replace) {
                container.empty();
            }
        }
    }

    function onReset(event) {  // 'this' is the form element
        var $form = $(this),
            key = '__jquery_unobtrusive_validation_form_reset';
        if ($form.data(key)) {
            return;
        }
        // Set a flag that indicates we're currently resetting the form.
        $form.data(key, true);
        try {
            $form.data("validator").resetForm();
        } finally {
            $form.removeData(key);
        }

        $form.find(".validation-summary-errors")
            .addClass("validation-summary-valid")
            .removeClass("validation-summary-errors");
        $form.find(".field-validation-error")
            .addClass("field-validation-valid")
            .removeClass("field-validation-error")
            .removeData("unobtrusiveContainer")
            .find(">*")  // If we were using valmsg-replace, get the underlying error
            .removeData("unobtrusiveContainer");
    }

    function validationInfo(form) {
        var $form = $(form),
            result = $form.data(data_validation),
            onResetProxy = $.proxy(onReset, form),
            defaultOptions = $jQval.unobtrusive.options || {},
            execInContext = function (name, args) {
                var func = defaultOptions[name];
                func && $.isFunction(func) && func.apply(form, args);
            };

        if (!result) {
            result = {
                options: {  // options structure passed to jQuery Validate's validate() method
                    errorClass: defaultOptions.errorClass || "input-validation-error",
                    errorElement: defaultOptions.errorElement || "span",
                    errorPlacement: function () {
                        onError.apply(form, arguments);
                        execInContext("errorPlacement", arguments);
                    },
                    invalidHandler: function () {
                        onErrors.apply(form, arguments);
                        execInContext("invalidHandler", arguments);
                    },
                    messages: {},
                    rules: {},
                    success: function () {
                        onSuccess.apply(form, arguments);
                        execInContext("success", arguments);
                    }
                },
                attachValidation: function () {
                    $form
                        .off("reset." + data_validation, onResetProxy)
                        .on("reset." + data_validation, onResetProxy)
                        .validate(this.options);
                },
                validate: function () {  // a validation function that is called by unobtrusive Ajax
                    $form.validate();
                    return $form.valid();
                }
            };
            $form.data(data_validation, result);
        }

        return result;
    }

    $jQval.unobtrusive = {
        adapters: [],

        parseElement: function (element, skipAttach) {
            /// <summary>
            /// Parses a single HTML element for unobtrusive validation attributes.
            /// </summary>
            /// <param name="element" domElement="true">The HTML element to be parsed.</param>
            /// <param name="skipAttach" type="Boolean">[Optional] true to skip attaching the
            /// validation to the form. If parsing just this single element, you should specify true.
            /// If parsing several elements, you should specify false, and manually attach the validation
            /// to the form when you are finished. The default is false.</param>
            var $element = $(element),
                form = $element.parents("form")[0],
                valInfo, rules, messages;

            if (!form) {  // Cannot do client-side validation without a form
                return;
            }

            valInfo = validationInfo(form);
            valInfo.options.rules[element.name] = rules = {};
            valInfo.options.messages[element.name] = messages = {};

            $.each(this.adapters, function () {
                var prefix = "data-val-" + this.name,
                    message = $element.attr(prefix),
                    paramValues = {};

                if (message !== undefined) {  // Compare against undefined, because an empty message is legal (and falsy)
                    prefix += "-";

                    $.each(this.params, function () {
                        paramValues[this] = $element.attr(prefix + this);
                    });

                    this.adapt({
                        element: element,
                        form: form,
                        message: message,
                        params: paramValues,
                        rules: rules,
                        messages: messages
                    });
                }
            });

            $.extend(rules, { "__dummy__": true });

            if (!skipAttach) {
                valInfo.attachValidation();
            }
        },

        parse: function (selector) {
            /// <summary>
            /// Parses all the HTML elements in the specified selector. It looks for input elements decorated
            /// with the [data-val=true] attribute value and enables validation according to the data-val-*
            /// attribute values.
            /// </summary>
            /// <param name="selector" type="String">Any valid jQuery selector.</param>

            // $forms includes all forms in selector's DOM hierarchy (parent, children and self) that have at least one
            // element with data-val=true
            var $selector = $(selector),
                $forms = $selector.parents()
                    .addBack()
                    .filter("form")
                    .add($selector.find("form"))
                    .has("[data-val=true]");

            $selector.find("[data-val=true]").each(function () {
                $jQval.unobtrusive.parseElement(this, true);
            });

            $forms.each(function () {
                var info = validationInfo(this);
                if (info) {
                    info.attachValidation();
                }
            });
        }
    };

    adapters = $jQval.unobtrusive.adapters;

    adapters.add = function (adapterName, params, fn) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="params" type="Array" optional="true">[Optional] An array of parameter names (strings) that will
        /// be extracted from the data-val-nnnn-mmmm HTML attributes (where nnnn is the adapter name, and
        /// mmmm is the parameter name).</param>
        /// <param name="fn" type="Function">The function to call, which adapts the values from the HTML
        /// attributes into jQuery Validate rules and/or messages.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        if (!fn) {  // Called with no params, just a function
            fn = params;
            params = [];
        }
        this.push({ name: adapterName, params: params, adapt: fn });
        return this;
    };

    adapters.addBool = function (adapterName, ruleName) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation rule has no parameter values.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="ruleName" type="String" optional="true">[Optional] The name of the jQuery Validate rule. If not provided, the value
        /// of adapterName will be used instead.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, function (options) {
            setValidationValues(options, ruleName || adapterName, true);
        });
    };

    adapters.addMinMax = function (adapterName, minRuleName, maxRuleName, minMaxRuleName, minAttribute, maxAttribute) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation has three potential rules (one for min-only, one for max-only, and
        /// one for min-and-max). The HTML parameters are expected to be named -min and -max.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute (where nnnn is the adapter name).</param>
        /// <param name="minRuleName" type="String">The name of the jQuery Validate rule to be used when you only
        /// have a minimum value.</param>
        /// <param name="maxRuleName" type="String">The name of the jQuery Validate rule to be used when you only
        /// have a maximum value.</param>
        /// <param name="minMaxRuleName" type="String">The name of the jQuery Validate rule to be used when you
        /// have both a minimum and maximum value.</param>
        /// <param name="minAttribute" type="String" optional="true">[Optional] The name of the HTML attribute that
        /// contains the minimum value. The default is "min".</param>
        /// <param name="maxAttribute" type="String" optional="true">[Optional] The name of the HTML attribute that
        /// contains the maximum value. The default is "max".</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, [minAttribute || "min", maxAttribute || "max"], function (options) {
            var min = options.params.min,
                max = options.params.max;

            if (min && max) {
                setValidationValues(options, minMaxRuleName, [min, max]);
            }
            else if (min) {
                setValidationValues(options, minRuleName, min);
            }
            else if (max) {
                setValidationValues(options, maxRuleName, max);
            }
        });
    };

    adapters.addSingleVal = function (adapterName, attribute, ruleName) {
        /// <summary>Adds a new adapter to convert unobtrusive HTML into a jQuery Validate validation, where
        /// the jQuery Validate validation rule has a single value.</summary>
        /// <param name="adapterName" type="String">The name of the adapter to be added. This matches the name used
        /// in the data-val-nnnn HTML attribute(where nnnn is the adapter name).</param>
        /// <param name="attribute" type="String">[Optional] The name of the HTML attribute that contains the value.
        /// The default is "val".</param>
        /// <param name="ruleName" type="String" optional="true">[Optional] The name of the jQuery Validate rule. If not provided, the value
        /// of adapterName will be used instead.</param>
        /// <returns type="jQuery.validator.unobtrusive.adapters" />
        return this.add(adapterName, [attribute || "val"], function (options) {
            setValidationValues(options, ruleName || adapterName, options.params[attribute]);
        });
    };

    $jQval.addMethod("__dummy__", function (value, element, params) {
        return true;
    });

    $jQval.addMethod("regex", function (value, element, params) {
        var match;
        if (this.optional(element)) {
            return true;
        }

        match = new RegExp(params).exec(value);
        return (match && (match.index === 0) && (match[0].length === value.length));
    });

    $jQval.addMethod("nonalphamin", function (value, element, nonalphamin) {
        var match;
        if (nonalphamin) {
            match = value.match(/\W/g);
            match = match && match.length >= nonalphamin;
        }
        return match;
    });

    if ($jQval.methods.extension) {
        adapters.addSingleVal("accept", "mimtype");
        adapters.addSingleVal("extension", "extension");
    } else {
        // for backward compatibility, when the 'extension' validation method does not exist, such as with versions
        // of JQuery Validation plugin prior to 1.10, we should use the 'accept' method for
        // validating the extension, and ignore mime-type validations as they are not supported.
        adapters.addSingleVal("extension", "extension", "accept");
    }

    adapters.addSingleVal("regex", "pattern");
    adapters.addBool("creditcard").addBool("date").addBool("digits").addBool("email").addBool("number").addBool("url");
    adapters.addMinMax("length", "minlength", "maxlength", "rangelength").addMinMax("range", "min", "max", "range");
    adapters.addMinMax("minlength", "minlength").addMinMax("maxlength", "minlength", "maxlength");
    adapters.add("equalto", ["other"], function (options) {
        var prefix = getModelPrefix(options.element.name),
            other = options.params.other,
            fullOtherName = appendModelPrefix(other, prefix),
            element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

        setValidationValues(options, "equalTo", element);
    });
    adapters.add("required", function (options) {
        // jQuery Validate equates "required" with "mandatory" for checkbox elements
        if (options.element.tagName.toUpperCase() !== "INPUT" || options.element.type.toUpperCase() !== "CHECKBOX") {
            setValidationValues(options, "required", true);
        }
    });
    adapters.add("remote", ["url", "type", "additionalfields"], function (options) {
        var value = {
            url: options.params.url,
            type: options.params.type || "GET",
            data: {}
        },
            prefix = getModelPrefix(options.element.name);

        $.each(splitAndTrim(options.params.additionalfields || options.element.name), function (i, fieldName) {
            var paramName = appendModelPrefix(fieldName, prefix);
            value.data[paramName] = function () {
                var field = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(paramName) + "']");
                // For checkboxes and radio buttons, only pick up values from checked fields.
                if (field.is(":checkbox")) {
                    return field.filter(":checked").val() || field.filter(":hidden").val() || '';
                }
                else if (field.is(":radio")) {
                    return field.filter(":checked").val() || '';
                }
                return field.val();
            };
        });

        setValidationValues(options, "remote", value);
    });
    adapters.add("password", ["min", "nonalphamin", "regex"], function (options) {
        if (options.params.min) {
            setValidationValues(options, "minlength", options.params.min);
        }
        if (options.params.nonalphamin) {
            setValidationValues(options, "nonalphamin", options.params.nonalphamin);
        }
        if (options.params.regex) {
            setValidationValues(options, "regex", options.params.regex);
        }
    });
    adapters.add("fileextensions", ["extensions"], function (options) {
        setValidationValues(options, "extension", options.params.extensions);
    });

    $(function () {
        $jQval.unobtrusive.parse(document);
    });

    return $jQval.unobtrusive;
}));

// SIG // Begin signature block
// SIG // MIIpKQYJKoZIhvcNAQcCoIIpGjCCKRYCAQExDzANBglg
// SIG // hkgBZQMEAgEFADB3BgorBgEEAYI3AgEEoGkwZzAyBgor
// SIG // BgEEAYI3AgEeMCQCAQEEEBDgyQbOONQRoqMAEEvTUJAC
// SIG // AQACAQACAQACAQACAQAwMTANBglghkgBZQMEAgEFAAQg
// SIG // DWoaD7VJuNnFSVFTMCJ3oA0fuEwOVxiEpVKU35D59qmg
// SIG // gg30MIIGcjCCBFqgAwIBAgITMwAABD8V06Npnu+JzAAA
// SIG // AAAEPzANBgkqhkiG9w0BAQwFADB+MQswCQYDVQQGEwJV
// SIG // UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
// SIG // UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBv
// SIG // cmF0aW9uMSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBT
// SIG // aWduaW5nIFBDQSAyMDExMB4XDTI0MTExOTE5NTA1OFoX
// SIG // DTI1MTExMjE5NTA1OFowYzELMAkGA1UEBhMCVVMxEzAR
// SIG // BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
// SIG // bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
// SIG // bjENMAsGA1UEAxMELk5FVDCCAaIwDQYJKoZIhvcNAQEB
// SIG // BQADggGPADCCAYoCggGBAKrK/pPcofXVP3BfQdSqPE+k
// SIG // mqCTYPZCWIfNPatBgWatVqw20RnIAApXDJjLQBYCKbOx
// SIG // YpsssP8pqQ0dmYpWuYg8B0T6T3n3bprZRRNoiw4KGRw1
// SIG // 6DrNg3WWc2ubWCmgPK5qvL5iIeiX71x6rZViXbOoQMcx
// SIG // 64pryR1BfiquU/J/G0W8zlB23677yGA5UOWF7tbQhVPZ
// SIG // hHFXtMvgN1YLGYBKG7ifFAeuEp5tD93iUTWDBC87jWbs
// SIG // ESvl91RfO7uFUTO7dfP3LJOwDQAmzvP5wrU+Tc7AHaG6
// SIG // HVEKEnr72PG+O+BjvUHTBY1zejxe6MRqtH0+te1TdDYs
// SIG // 5JeKgFWvXKgYaZQ3pUNtMY8zpxU6R4afY+//Z3mAWJKJ
// SIG // x/RJy1pCB3wkOzZQ0+r0lcg/7bMG/Y0Cu44+ZJQqz8fV
// SIG // C9/0+81ggRmzRqe8pRMVbeHK7SONglRgu2KR/yBDtgng
// SIG // rM1Vch7JWccnDwZkURgc7y8KHZGE5fR57AzFY55bnKrG
// SIG // 1jXqnQIDAQABo4IBgjCCAX4wHwYDVR0lBBgwFgYKKwYB
// SIG // BAGCN0wIAQYIKwYBBQUHAwMwHQYDVR0OBBYEFCzxnqIr
// SIG // 4/zXEW64dVQ6pVziNXX9MFQGA1UdEQRNMEukSTBHMS0w
// SIG // KwYDVQQLEyRNaWNyb3NvZnQgSXJlbGFuZCBPcGVyYXRp
// SIG // b25zIExpbWl0ZWQxFjAUBgNVBAUTDTQ2NDIyMys1MDMz
// SIG // OTIwHwYDVR0jBBgwFoAUSG5k5VAF04KqFzc3IrVtqMp1
// SIG // ApUwVAYDVR0fBE0wSzBJoEegRYZDaHR0cDovL3d3dy5t
// SIG // aWNyb3NvZnQuY29tL3BraW9wcy9jcmwvTWljQ29kU2ln
// SIG // UENBMjAxMV8yMDExLTA3LTA4LmNybDBhBggrBgEFBQcB
// SIG // AQRVMFMwUQYIKwYBBQUHMAKGRWh0dHA6Ly93d3cubWlj
// SIG // cm9zb2Z0LmNvbS9wa2lvcHMvY2VydHMvTWljQ29kU2ln
// SIG // UENBMjAxMV8yMDExLTA3LTA4LmNydDAMBgNVHRMBAf8E
// SIG // AjAAMA0GCSqGSIb3DQEBDAUAA4ICAQCoh/YE7CpG9ULB
// SIG // TlmokhdbFKSf2m6Ezu6Z+Xmn0mnrqabfSlb2WmW2TqJ2
// SIG // k1sn0fZODP1jwbMkWiEEQ7xIZQdSQnEh1Ht08RXWDMgN
// SIG // GSyxlblGoXSeyojaNB6xbUG4bSKP0sHKHs0tOpEoZA8e
// SIG // PzzJUnbKf8KHm847mm+7IL0l8rF/e749WUJREr4JXRds
// SIG // AIOfgtMCgi3QeN/x9PIKudz3GUNrOag76pbmdLRYzZjx
// SIG // ByofpGhvsU2QkSUBn/5q05gZUbSnC9vdxFzbRDt+OMm2
// SIG // DaAju7JxCOSJ3t74yYHeuiexyaq0mD4ioxGdSh/OZ/AO
// SIG // or0eC5bC/O4Kq6+NTT0epjdygN5alndo6Z0tCO0/Fjap
// SIG // NZPWkEJVx0PFfk0cL13Q5YeTnSBEP8a0dtclfpQxo5Fr
// SIG // Mo3l9lIDoNMBQ3F/dD1Txlz/Az72W9OD9/J6c1q3ysJs
// SIG // kAt4AgPdXX1zoOoPQJlCSmb0G3XBHKGYS8PXbejDNteP
// SIG // Xnms+kCKa0+pmqzNtUxl6eNDf7/+TF3gJP+y8kgh1CVo
// SIG // miLfZX8jrSnQ04UjTxU7l3CDWgyTgXGNta2p5k7C3f9x
// SIG // SsPmeVKb8RwPN1tKkQaureSPwuQCq0GYaJj8gO0MlbAx
// SIG // 7N3pF12UNdb9tNwAnq0Nip6rkke4BNucn3M97xdVVeCA
// SIG // P+xyrbPHcjCCB3owggVioAMCAQICCmEOkNIAAAAAAAMw
// SIG // DQYJKoZIhvcNAQELBQAwgYgxCzAJBgNVBAYTAlVTMRMw
// SIG // EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRt
// SIG // b25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRp
// SIG // b24xMjAwBgNVBAMTKU1pY3Jvc29mdCBSb290IENlcnRp
// SIG // ZmljYXRlIEF1dGhvcml0eSAyMDExMB4XDTExMDcwODIw
// SIG // NTkwOVoXDTI2MDcwODIxMDkwOVowfjELMAkGA1UEBhMC
// SIG // VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT
// SIG // B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
// SIG // b3JhdGlvbjEoMCYGA1UEAxMfTWljcm9zb2Z0IENvZGUg
// SIG // U2lnbmluZyBQQ0EgMjAxMTCCAiIwDQYJKoZIhvcNAQEB
// SIG // BQADggIPADCCAgoCggIBAKvw+nIQHC6t2G6qghBNNLry
// SIG // tlghn0IbKmvpWlCquAY4GgRJun/DDB7dN2vGEtgL8DjC
// SIG // mQawyDnVARQxQtOJDXlkh36UYCRsr55JnOloXtLfm1Oy
// SIG // CizDr9mpK656Ca/XllnKYBoF6WZ26DJSJhIv56sIUM+z
// SIG // RLdd2MQuA3WraPPLbfM6XKEW9Ea64DhkrG5kNXimoGMP
// SIG // LdNAk/jj3gcN1Vx5pUkp5w2+oBN3vpQ97/vjK1oQH01W
// SIG // KKJ6cuASOrdJXtjt7UORg9l7snuGG9k+sYxd6IlPhBry
// SIG // oS9Z5JA7La4zWMW3Pv4y07MDPbGyr5I4ftKdgCz1TlaR
// SIG // ITUlwzluZH9TupwPrRkjhMv0ugOGjfdf8NBSv4yUh7zA
// SIG // IXQlXxgotswnKDglmDlKNs98sZKuHCOnqWbsYR9q4ShJ
// SIG // nV+I4iVd0yFLPlLEtVc/JAPw0XpbL9Uj43BdD1FGd7P4
// SIG // AOG8rAKCX9vAFbO9G9RVS+c5oQ/pI0m8GLhEfEXkwcNy
// SIG // euBy5yTfv0aZxe/CHFfbg43sTUkwp6uO3+xbn6/83bBm
// SIG // 4sGXgXvt1u1L50kppxMopqd9Z4DmimJ4X7IvhNdXnFy/
// SIG // dygo8e1twyiPLI9AN0/B4YVEicQJTMXUpUMvdJX3bvh4
// SIG // IFgsE11glZo+TzOE2rCIF96eTvSWsLxGoGyY0uDWiIwL
// SIG // AgMBAAGjggHtMIIB6TAQBgkrBgEEAYI3FQEEAwIBADAd
// SIG // BgNVHQ4EFgQUSG5k5VAF04KqFzc3IrVtqMp1ApUwGQYJ
// SIG // KwYBBAGCNxQCBAweCgBTAHUAYgBDAEEwCwYDVR0PBAQD
// SIG // AgGGMA8GA1UdEwEB/wQFMAMBAf8wHwYDVR0jBBgwFoAU
// SIG // ci06AjGQQ7kUBU7h6qfHMdEjiTQwWgYDVR0fBFMwUTBP
// SIG // oE2gS4ZJaHR0cDovL2NybC5taWNyb3NvZnQuY29tL3Br
// SIG // aS9jcmwvcHJvZHVjdHMvTWljUm9vQ2VyQXV0MjAxMV8y
// SIG // MDExXzAzXzIyLmNybDBeBggrBgEFBQcBAQRSMFAwTgYI
// SIG // KwYBBQUHMAKGQmh0dHA6Ly93d3cubWljcm9zb2Z0LmNv
// SIG // bS9wa2kvY2VydHMvTWljUm9vQ2VyQXV0MjAxMV8yMDEx
// SIG // XzAzXzIyLmNydDCBnwYDVR0gBIGXMIGUMIGRBgkrBgEE
// SIG // AYI3LgMwgYMwPwYIKwYBBQUHAgEWM2h0dHA6Ly93d3cu
// SIG // bWljcm9zb2Z0LmNvbS9wa2lvcHMvZG9jcy9wcmltYXJ5
// SIG // Y3BzLmh0bTBABggrBgEFBQcCAjA0HjIgHQBMAGUAZwBh
// SIG // AGwAXwBwAG8AbABpAGMAeQBfAHMAdABhAHQAZQBtAGUA
// SIG // bgB0AC4gHTANBgkqhkiG9w0BAQsFAAOCAgEAZ/KGpZjg
// SIG // VHkaLtPYdGcimwuWEeFjkplCln3SeQyQwWVfLiw++MNy
// SIG // 0W2D/r4/6ArKO79HqaPzadtjvyI1pZddZYSQfYtGUFXY
// SIG // DJJ80hpLHPM8QotS0LD9a+M+By4pm+Y9G6XUtR13lDni
// SIG // 6WTJRD14eiPzE32mkHSDjfTLJgJGKsKKELukqQUMm+1o
// SIG // +mgulaAqPyprWEljHwlpblqYluSD9MCP80Yr3vw70L01
// SIG // 724lruWvJ+3Q3fMOr5kol5hNDj0L8giJ1h/DMhji8MUt
// SIG // zluetEk5CsYKwsatruWy2dsViFFFWDgycScaf7H0J/je
// SIG // LDogaZiyWYlobm+nt3TDQAUGpgEqKD6CPxNNZgvAs031
// SIG // 4Y9/HG8VfUWnduVAKmWjw11SYobDHWM2l4bf2vP48hah
// SIG // mifhzaWX0O5dY0HjWwechz4GdwbRBrF1HxS+YWG18NzG
// SIG // GwS+30HHDiju3mUv7Jf2oVyW2ADWoUa9WfOXpQlLSBCZ
// SIG // gB/QACnFsZulP0V3HjXG0qKin3p6IvpIlR+r+0cjgPWe
// SIG // +L9rt0uX4ut1eBrs6jeZeRhL/9azI2h15q/6/IvrC4Dq
// SIG // aTuv/DDtBEyO3991bWORPdGdVk5Pv4BXIqF4ETIheu9B
// SIG // CrE/+6jMpF3BoYibV3FWTkhFwELJm3ZbCoBIa/15n8G9
// SIG // bW1qyVJzEw16UM0xghqNMIIaiQIBATCBlTB+MQswCQYD
// SIG // VQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
// SIG // A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
// SIG // IENvcnBvcmF0aW9uMSgwJgYDVQQDEx9NaWNyb3NvZnQg
// SIG // Q29kZSBTaWduaW5nIFBDQSAyMDExAhMzAAAEPxXTo2me
// SIG // 74nMAAAAAAQ/MA0GCWCGSAFlAwQCAQUAoIGuMBkGCSqG
// SIG // SIb3DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcC
// SIG // AQsxDjAMBgorBgEEAYI3AgEVMC8GCSqGSIb3DQEJBDEi
// SIG // BCB5WXXKXaaSt9+xbf/t4lig2g7bDCo1gJzWQPkQu8lr
// SIG // YjBCBgorBgEEAYI3AgEMMTQwMqAUgBIATQBpAGMAcgBv
// SIG // AHMAbwBmAHShGoAYaHR0cDovL3d3dy5taWNyb3NvZnQu
// SIG // Y29tMA0GCSqGSIb3DQEBAQUABIIBgB0kP6aG7SpiXiEo
// SIG // rK6MXUhwsJY4/z3JK8kPCbwIfDm6GAvmA8Cg1MSk3hzT
// SIG // P6EbsAb1cjCfGSgp3UQTXASprmvCYhkjWfV0MaPm+3NY
// SIG // FafikDcStTR3v2q3ldN959X7hzZjkxyFxQ0pI9zjWRsO
// SIG // RjJeTqR2APDdqhi23T4+twai9oCsVd21RqPF96/DC022
// SIG // G6Y4fXc/+bBVtsMYbpSI6btpLFOJhcHzc+HaONnVuFTk
// SIG // SunmFTdzfgu1Pdgv0Q3eJ7bpFhiChnVLbazkJPrjnkRi
// SIG // 3jjMMYqo1bhDk758G4jiSbNCVJGbGbfxULQuC7pgtNW6
// SIG // /i/rBoOx3xatcCcx3VWr+XZ5lCyiNYDf0sUkrfJ2UroM
// SIG // BS0WrzaF9e4VlMTEhQPIg5ecPwJ0r6FIwDLcbai/5i2s
// SIG // 0RXUnHq3FBcL2XEPJPi/7RnXPOJMpv0p5FSJizu6wAGk
// SIG // +lpKGnTyyNXxYjXvsrfrawE+ilIqyk4Z6bzJHa5M2xvB
// SIG // YZ+da0v1c0m5o6GCF5cwgheTBgorBgEEAYI3AwMBMYIX
// SIG // gzCCF38GCSqGSIb3DQEHAqCCF3AwghdsAgEDMQ8wDQYJ
// SIG // YIZIAWUDBAIBBQAwggFSBgsqhkiG9w0BCRABBKCCAUEE
// SIG // ggE9MIIBOQIBAQYKKwYBBAGEWQoDATAxMA0GCWCGSAFl
// SIG // AwQCAQUABCBDCUSxdxQ9+o5ge9qwIYZr4eDr4QY3uzB8
// SIG // yWFntP1m0AIGaBplO9ROGBMyMDI1MDUwOTAwNTMyNy4z
// SIG // NzlaMASAAgH0oIHRpIHOMIHLMQswCQYDVQQGEwJVUzET
// SIG // MBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVk
// SIG // bW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0
// SIG // aW9uMSUwIwYDVQQLExxNaWNyb3NvZnQgQW1lcmljYSBP
// SIG // cGVyYXRpb25zMScwJQYDVQQLEx5uU2hpZWxkIFRTUyBF
// SIG // U046QTQwMC0wNUUwLUQ5NDcxJTAjBgNVBAMTHE1pY3Jv
// SIG // c29mdCBUaW1lLVN0YW1wIFNlcnZpY2WgghHtMIIHIDCC
// SIG // BQigAwIBAgITMwAAAgJ5UHQhFH24oQABAAACAjANBgkq
// SIG // hkiG9w0BAQsFADB8MQswCQYDVQQGEwJVUzETMBEGA1UE
// SIG // CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEe
// SIG // MBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYw
// SIG // JAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0Eg
// SIG // MjAxMDAeFw0yNTAxMzAxOTQyNDRaFw0yNjA0MjIxOTQy
// SIG // NDRaMIHLMQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSUwIwYDVQQL
// SIG // ExxNaWNyb3NvZnQgQW1lcmljYSBPcGVyYXRpb25zMScw
// SIG // JQYDVQQLEx5uU2hpZWxkIFRTUyBFU046QTQwMC0wNUUw
// SIG // LUQ5NDcxJTAjBgNVBAMTHE1pY3Jvc29mdCBUaW1lLVN0
// SIG // YW1wIFNlcnZpY2UwggIiMA0GCSqGSIb3DQEBAQUAA4IC
// SIG // DwAwggIKAoICAQC3eSp6cucUGOkcPg4vKWKJfEQeshK2
// SIG // ZBsYU1tDWvQu6L9lp+dnqrajIdNeH1HN3oz3iiGoJWuN
// SIG // 2HVNZkcOt38aWGebM0gUUOtPjuLhuO5d67YpQsHBJAWh
// SIG // cve/MVdoQPj1njiAjSiOrL8xFarFLI46RH8NeDhAPXcJ
// SIG // pWn7AIzCyIjZOaJ2DWA+6QwNzwqjBgIpf1hWFwqHvPEe
// SIG // dy0notXbtWfT9vCSL9sdDK6K/HH9HsaY5wLmUUB7SfuL
// SIG // Go1OWEm6MJyG2jixqi9NyRoypdF8dRyjWxKRl2Jxwvbe
// SIG // tlDTio66XliTOckq2RgM+ZocZEb6EoOdtd0XKh3Lzx29
// SIG // AhHxlk+6eIwavlHYuOLZDKodPOVN6j1IJ9brolY6mZbo
// SIG // Q51Oqe5nEM5h/WJX28GLZioEkJN8qOe5P5P2Yx9HoOqL
// SIG // ugX00qCzxq4BDm8xH85HKxvKCO5KikopaRGGtQlXjDyu
// SIG // sMWlrHcySt56DhL4dcVnn7dFvL50zvQlFZMhVoehWSQk
// SIG // kWuUlCCqIOrTe7RbmnbdJosH+7lC+n53gnKy4OoZzuUe
// SIG // qzCnSB1JNXPKnJojP3De5xwspi5tUvQFNflfGTsjZgQA
// SIG // gDBdg/DO0TGgLRDKvZQCZ5qIuXpQRyg37yc51e95z8U2
// SIG // mysU0XnSpWeigHqkyOAtDfcIpq5Gv7HV+da2RwIDAQAB
// SIG // o4IBSTCCAUUwHQYDVR0OBBYEFNoGubUPjP2f8ifkIKvw
// SIG // y1rlSHTZMB8GA1UdIwQYMBaAFJ+nFV0AXmJdg/Tl0mWn
// SIG // G1M1GelyMF8GA1UdHwRYMFYwVKBSoFCGTmh0dHA6Ly93
// SIG // d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY3JsL01pY3Jv
// SIG // c29mdCUyMFRpbWUtU3RhbXAlMjBQQ0ElMjAyMDEwKDEp
// SIG // LmNybDBsBggrBgEFBQcBAQRgMF4wXAYIKwYBBQUHMAKG
// SIG // UGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMv
// SIG // Y2VydHMvTWljcm9zb2Z0JTIwVGltZS1TdGFtcCUyMFBD
// SIG // QSUyMDIwMTAoMSkuY3J0MAwGA1UdEwEB/wQCMAAwFgYD
// SIG // VR0lAQH/BAwwCgYIKwYBBQUHAwgwDgYDVR0PAQH/BAQD
// SIG // AgeAMA0GCSqGSIb3DQEBCwUAA4ICAQCD83aFQUxN37Hk
// SIG // OoJDM1maHFZVUGcqTQcPnOD6UoYRMmDKv0GabHlE82AY
// SIG // gLPuVlukn7HtJPF2z0jnTgAfRMn26JFLPG7O/XbKK25h
// SIG // rBPJ30lBuwjATVt58UA1BWo7lsmnyrur/6h8AFzrXyrX
// SIG // tlvzQYqaRYY9k0UFY5GM+n9YaEEK2D268e+a+HDmWe+t
// SIG // YL2H+9O4Q1MQLag+ciNwLkj/+QlxpXiWou9KvAP0tIk+
// SIG // fH8F3ww5VOTi9aZ9+qPjszw31H4ndtivBZaH5s5boJmH
// SIG // 2JbtMuf2y7hSdJdE0UW2B0FEZPLImemlKhslJNVqEO7R
// SIG // Pgl7c81QuVSO58ffpmbwtSxhYrES3VsPglXn9ODF7Dqm
// SIG // PMG/GysB4o/QkpNUq+wS7bORTNzqHMtH+ord2YSma+1b
// SIG // yWBr/izIKggOCdEzaZDfym12GM6a4S+Iy6AUIp7/KIpA
// SIG // mfWfXrcMK7V7EBzxoezkLREEWI4XtPwpEBntOa1oDH3Z
// SIG // /+dRxsxL0vgya7jNfrO7oizTAln/2ZBYB9ioUeobj5AG
// SIG // L45m2mcKSk7HE5zUReVkILpYKBQ5+X/8jFO1/pZyqzQe
// SIG // I1/oJ/RLoic1SieLXfET9EWZIBjZMZ846mDbp1ynK9Ub
// SIG // NiCjSwmTF509Yn9M47VQsxsv1olQu51rVVHkSNm+rTrL
// SIG // wK1tvhv0mTCCB3EwggVZoAMCAQICEzMAAAAVxedrngKb
// SIG // SZkAAAAAABUwDQYJKoZIhvcNAQELBQAwgYgxCzAJBgNV
// SIG // BAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYD
// SIG // VQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQg
// SIG // Q29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29mdCBS
// SIG // b290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEwMB4X
// SIG // DTIxMDkzMDE4MjIyNVoXDTMwMDkzMDE4MzIyNVowfDEL
// SIG // MAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24x
// SIG // EDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jv
// SIG // c29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9z
// SIG // b2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAwggIiMA0GCSqG
// SIG // SIb3DQEBAQUAA4ICDwAwggIKAoICAQDk4aZM57RyIQt5
// SIG // osvXJHm9DtWC0/3unAcH0qlsTnXIyjVX9gF/bErg4r25
// SIG // PhdgM/9cT8dm95VTcVrifkpa/rg2Z4VGIwy1jRPPdzLA
// SIG // EBjoYH1qUoNEt6aORmsHFPPFdvWGUNzBRMhxXFExN6AK
// SIG // OG6N7dcP2CZTfDlhAnrEqv1yaa8dq6z2Nr41JmTamDu6
// SIG // GnszrYBbfowQHJ1S/rboYiXcag/PXfT+jlPP1uyFVk3v
// SIG // 3byNpOORj7I5LFGc6XBpDco2LXCOMcg1KL3jtIckw+DJ
// SIG // j361VI/c+gVVmG1oO5pGve2krnopN6zL64NF50ZuyjLV
// SIG // wIYwXE8s4mKyzbnijYjklqwBSru+cakXW2dg3viSkR4d
// SIG // Pf0gz3N9QZpGdc3EXzTdEonW/aUgfX782Z5F37ZyL9t9
// SIG // X4C626p+Nuw2TPYrbqgSUei/BQOj0XOmTTd0lBw0gg/w
// SIG // EPK3Rxjtp+iZfD9M269ewvPV2HM9Q07BMzlMjgK8Qmgu
// SIG // EOqEUUbi0b1qGFphAXPKZ6Je1yh2AuIzGHLXpyDwwvoS
// SIG // CtdjbwzJNmSLW6CmgyFdXzB0kZSU2LlQ+QuJYfM2BjUY
// SIG // hEfb3BvR/bLUHMVr9lxSUV0S2yW6r1AFemzFER1y7435
// SIG // UsSFF5PAPBXbGjfHCBUYP3irRbb1Hode2o+eFnJpxq57
// SIG // t7c+auIurQIDAQABo4IB3TCCAdkwEgYJKwYBBAGCNxUB
// SIG // BAUCAwEAATAjBgkrBgEEAYI3FQIEFgQUKqdS/mTEmr6C
// SIG // kTxGNSnPEP8vBO4wHQYDVR0OBBYEFJ+nFV0AXmJdg/Tl
// SIG // 0mWnG1M1GelyMFwGA1UdIARVMFMwUQYMKwYBBAGCN0yD
// SIG // fQEBMEEwPwYIKwYBBQUHAgEWM2h0dHA6Ly93d3cubWlj
// SIG // cm9zb2Z0LmNvbS9wa2lvcHMvRG9jcy9SZXBvc2l0b3J5
// SIG // Lmh0bTATBgNVHSUEDDAKBggrBgEFBQcDCDAZBgkrBgEE
// SIG // AYI3FAIEDB4KAFMAdQBiAEMAQTALBgNVHQ8EBAMCAYYw
// SIG // DwYDVR0TAQH/BAUwAwEB/zAfBgNVHSMEGDAWgBTV9lbL
// SIG // j+iiXGJo0T2UkFvXzpoYxDBWBgNVHR8ETzBNMEugSaBH
// SIG // hkVodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2Ny
// SIG // bC9wcm9kdWN0cy9NaWNSb29DZXJBdXRfMjAxMC0wNi0y
// SIG // My5jcmwwWgYIKwYBBQUHAQEETjBMMEoGCCsGAQUFBzAC
// SIG // hj5odHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2Nl
// SIG // cnRzL01pY1Jvb0NlckF1dF8yMDEwLTA2LTIzLmNydDAN
// SIG // BgkqhkiG9w0BAQsFAAOCAgEAnVV9/Cqt4SwfZwExJFvh
// SIG // nnJL/Klv6lwUtj5OR2R4sQaTlz0xM7U518JxNj/aZGx8
// SIG // 0HU5bbsPMeTCj/ts0aGUGCLu6WZnOlNN3Zi6th542DYu
// SIG // nKmCVgADsAW+iehp4LoJ7nvfam++Kctu2D9IdQHZGN5t
// SIG // ggz1bSNU5HhTdSRXud2f8449xvNo32X2pFaq95W2KFUn
// SIG // 0CS9QKC/GbYSEhFdPSfgQJY4rPf5KYnDvBewVIVCs/wM
// SIG // nosZiefwC2qBwoEZQhlSdYo2wh3DYXMuLGt7bj8sCXgU
// SIG // 6ZGyqVvfSaN0DLzskYDSPeZKPmY7T7uG+jIa2Zb0j/aR
// SIG // AfbOxnT99kxybxCrdTDFNLB62FD+CljdQDzHVG2dY3RI
// SIG // LLFORy3BFARxv2T5JL5zbcqOCb2zAVdJVGTZc9d/HltE
// SIG // AY5aGZFrDZ+kKNxnGSgkujhLmm77IVRrakURR6nxt67I
// SIG // 6IleT53S0Ex2tVdUCbFpAUR+fKFhbHP+CrvsQWY9af3L
// SIG // wUFJfn6Tvsv4O+S3Fb+0zj6lMVGEvL8CwYKiexcdFYmN
// SIG // cP7ntdAoGokLjzbaukz5m/8K6TT4JDVnK+ANuOaMmdbh
// SIG // IurwJ0I9JZTmdHRbatGePu1+oDEzfbzL6Xu/OHBE0ZDx
// SIG // yKs6ijoIYn/ZcGNTTY3ugm2lBRDBcQZqELQdVTNYs6Fw
// SIG // ZvKhggNQMIICOAIBATCB+aGB0aSBzjCByzELMAkGA1UE
// SIG // BhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNV
// SIG // BAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBD
// SIG // b3Jwb3JhdGlvbjElMCMGA1UECxMcTWljcm9zb2Z0IEFt
// SIG // ZXJpY2EgT3BlcmF0aW9uczEnMCUGA1UECxMeblNoaWVs
// SIG // ZCBUU1MgRVNOOkE0MDAtMDVFMC1EOTQ3MSUwIwYDVQQD
// SIG // ExxNaWNyb3NvZnQgVGltZS1TdGFtcCBTZXJ2aWNloiMK
// SIG // AQEwBwYFKw4DAhoDFQBJiUhpCWA/3X/jZyIy0ye6RJwL
// SIG // zqCBgzCBgKR+MHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
// SIG // EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
// SIG // HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAk
// SIG // BgNVBAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAy
// SIG // MDEwMA0GCSqGSIb3DQEBCwUAAgUA68eGjTAiGA8yMDI1
// SIG // MDUwODE5Mzc0OVoYDzIwMjUwNTA5MTkzNzQ5WjB3MD0G
// SIG // CisGAQQBhFkKBAExLzAtMAoCBQDrx4aNAgEAMAoCAQAC
// SIG // AhopAgH/MAcCAQACAhJeMAoCBQDryNgNAgEAMDYGCisG
// SIG // AQQBhFkKBAIxKDAmMAwGCisGAQQBhFkKAwKgCjAIAgEA
// SIG // AgMHoSChCjAIAgEAAgMBhqAwDQYJKoZIhvcNAQELBQAD
// SIG // ggEBAJwHkSfbFQOayNjMdy46s2WITrDdx0w221MuW7i6
// SIG // 9VdGFhewBTxR4kpQm6e6cW9m93Fb/+2/X3LYewCFr+rP
// SIG // 8AFHGblgwdInoamLndvCkeu66K8CBNphHXy0pEcWn/pi
// SIG // QQ9CK6xy6BnmlGbbBkOGVR/VEjuofB6jUa5MXwfmgFdM
// SIG // dOWtBO8T51uszyrTbNWuB700wW4bvqkpVGh057S9kZqQ
// SIG // 6TYdA2EIOpCno/XIOFdMnSNB5GezN+8Io/Q7qX4arNOr
// SIG // ZFnpheZw4EtulcULmJ/h39OgRR8DCWucQFYi6KFNDgTf
// SIG // 2EWyVF9S32lDszZvVXj5gLlL5q0QdrIOyQkqOlExggQN
// SIG // MIIECQIBATCBkzB8MQswCQYDVQQGEwJVUzETMBEGA1UE
// SIG // CBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEe
// SIG // MBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSYw
// SIG // JAYDVQQDEx1NaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0Eg
// SIG // MjAxMAITMwAAAgJ5UHQhFH24oQABAAACAjANBglghkgB
// SIG // ZQMEAgEFAKCCAUowGgYJKoZIhvcNAQkDMQ0GCyqGSIb3
// SIG // DQEJEAEEMC8GCSqGSIb3DQEJBDEiBCCiyvuK94Av5RNL
// SIG // /61kN9waL+ShHMbwQwM+NDkbPcaNVzCB+gYLKoZIhvcN
// SIG // AQkQAi8xgeowgecwgeQwgb0EIPON6gEYB5bLzXLWuUmL
// SIG // 8Zd8xXAsqXksedFyolfMlF/sMIGYMIGApH4wfDELMAkG
// SIG // A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
// SIG // BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
// SIG // dCBDb3Jwb3JhdGlvbjEmMCQGA1UEAxMdTWljcm9zb2Z0
// SIG // IFRpbWUtU3RhbXAgUENBIDIwMTACEzMAAAICeVB0IRR9
// SIG // uKEAAQAAAgIwIgQgP4kOU2HeT9h0AOvr/ZFDZR1KgbVb
// SIG // VU/mmpLBJDamlD0wDQYJKoZIhvcNAQELBQAEggIAGwrn
// SIG // a6ezBq2w5wzqs3Z1i+yykV4l3kMFBdH0D8MA+KV8tUhr
// SIG // YbWXsQ7dj8sEz9+5LfbtBis/MFOOVnIkHjCkbYHY3Aqp
// SIG // 0xQDwK41v9TA9IVystcLlsAJfrX7JCPTYnFQuVUwsr6K
// SIG // qirC9yjVXZWVFQHijIEHWCHKH3Oy0uQ8c+xC2MgnXlIA
// SIG // +bhCYTg4xqNga41OZFcscfzRZpdh0kmLqpPVh2xBGqgA
// SIG // QafMRBjcDKHq8C7aEwpPL8lJCdXe0ihI0Bk6EmxAhuc2
// SIG // aYKJ1CBByz2Lm/ABr2RtAYJqBX8i3eSFO6mcdS5YJorV
// SIG // 4g0rIF1vG1r1TiKdpBuQM3pwZTm4UotI2qQGJ9ZchKL+
// SIG // jtAq8HYQUhKs4sMytEX54fcGZ4Elub2V26TFQM2CmxND
// SIG // WUQmRAn+uTqo6uUYnfFeaUPojnYhR7yDE0HFFkrtkbAc
// SIG // Ea53PzzwwLLrlTkdtUMe+24Peo+230uTWdCaw4vbxq5j
// SIG // IGF6fRmqmEfFABe7+yq0Gxa8hgrAX9deDhZ0LIm0+fSB
// SIG // Ez0cJAnk2vSCTHVy75bmsLfRK+6kDhjb+TOsYasxImET
// SIG // AwRuYLQJQWZ+jNBNnkjA/zwE3HC9R8dG/fF+c6Ir/CSc
// SIG // FBCV1f8jj5OYmiIUiH8nLAHmJxjXPpo3rEm4Hk0Hr/9A
// SIG // GlokdKVb9bIzNmTr73I=
// SIG // End signature block
