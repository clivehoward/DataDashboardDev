/*

IMPORTANT:
This is a fudge!!
addMethod should NOT have the form fields (params) and error message hard coded
It should pull these from the server-side code

*/

$.validator.unobtrusive.adapters.addSingleVal("atleastonerequired", "otherpropertynames");

$.validator.addMethod("atleastonerequired", function (value, element, params) {
    params = "EnquiryForm_TopicArea,EnquiryForm_Geography,EnquiryForm_Industry,EnquiryForm_CompanySize";
    var param = params.toString().split(',');
    var isAllNull = true;
    $.each(param, function (i, val) {
        var valueOfItem = $('#' + val).val().trim();
        if (valueOfItem != '') {
            isAllNull = false;
            return false;
        }
    });
    if (isAllNull) {
        return false;
    }
    else {
        return true;
    }
}, "Please select at least one of the options");