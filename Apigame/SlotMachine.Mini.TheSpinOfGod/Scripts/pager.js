/*
* jQuery pager plugin
* Version 1.0 (12/22/2008)
* @requires jQuery v1.2.6 or later
*
* Example at: http://jonpauldavies.github.com/JQuery/Pager/PagerDemo.html
*
* Copyright (c) 2008-2009 Jon Paul Davies
* Dual licensed under the MIT and GPL licenses:
* http://www.opensource.org/licenses/mit-license.php
* http://www.gnu.org/licenses/gpl.html
* 
* Read the related blog post and contact the author at http://www.j-dee.com/2008/12/22/jquery-pager-plugin/
*
* This version is far from perfect and doesn't manage it's own state, therefore contributions are more than welcome!
*
* Usage: .pager({ pagenumber: 1, pagecount: 15, buttonClickCallback: PagerClickTest });
*
* Where pagenumber is the visible page number
*       pagecount is the total number of pages to display
*       buttonClickCallback is the method to fire when a pager button is clicked.
*
* buttonClickCallback signiture is PagerClickTest = function(pageclickednumber) 
* Where pageclickednumber is the number of the page clicked in the control.
*
* The included Pager.CSS file is a dependancy but can obviously tweaked to your wishes
* Tested in IE6 IE7 Firefox & Safari. Any browser strangeness, please report.
*/
(function ($) {

    $.fn.pagernew = function (options) {

        var opts = $.extend({}, $.fn.pagernew.defaults, options);

        return this.each(function () {

            // empty out the destination element and then render out the pager with the supplied options
            $(this).empty().append(renderpager(parseInt(options.pagenumber), parseInt(options.pagecount), options.buttonClickCallback));

            // specify correct cursor activity
            $('div#idPage a').mouseover(function () { document.body.style.cursor = "pointer"; }).mouseout(function () { document.body.style.cursor = "auto"; });
        });
    };

    // render and return the pager with the supplied options
    function renderpager(pagenumber, pagecount, buttonClickCallback) {
        if (pagecount < 2)
            return '';

        // setup $pager to hold render
        var $pagernew = $('<div class="mini_paging"><div id="idPage"></div></div>');

        // << &laquo;
        // < &lsaquo;
        // > &rsaquo;
        // >> &raquo;
        // add in the previous and next buttons
        if (pagenumber > 1) {
            $pagernew.find('div#idPage').append(renderButton('prev', '<<', pagenumber, pagecount, buttonClickCallback));
        }

        // pager currently only handles 10 viewable pages ( could be easily parameterized, maybe in next version ) so handle edge cases

        var startPoint = 1;
        var endPoint = 5;

        if (pagenumber > 2) {
            startPoint = pagenumber - 2;
            endPoint = pagenumber + 2;
        }

        if (endPoint > pagecount) {
            startPoint = pagecount - 4;
            endPoint = pagecount;
        }

        if (startPoint < 1) {
            startPoint = 1;
        }

        // loop thru visible pages and render buttons
        for (var page = startPoint; page <= endPoint; page++) {

            var currentButton = $('<a class="mini_page" id="' + page + '" href="javascript:;">' + (page) + '</a>');
            page == pagenumber ? currentButton.addClass('mini_page_select') : currentButton.click(function () { buttonClickCallback(this.firstChild.data); });

            //            currentButton.click(function () { buttonClickCallback(this.firstChild.firstChild.data); });
            currentButton.appendTo($pagernew.find('div#idPage'));
        }

        // render in the next and last buttons before returning the whole rendered control back.
        if (pagenumber < pagecount) {
            $pagernew.find('div#idPage').append(renderButton('next', '>>', pagenumber, pagecount, buttonClickCallback));//.append(renderButton('last', '&raquo;', pagenumber, pagecount, buttonClickCallback));
        }

        return $pagernew;
    }

    // renders and returns a 'specialized' button, ie 'next', 'previous' etc. rather than a page number button
    function renderButton(buttonName, buttonLabel, pagenumber, pagecount, buttonClickCallback) {

        // var $Button = $('<span><a href="javascript:;">' + buttonLabel + '</a></span>');


        var destPage = 1;

        // work out destination page for required button type
        switch (buttonName) {
            case "first":
                destPage = 1;
                break;
            case "prev":
                destPage = pagenumber - 1;
                break;
            case "next":
                destPage = pagenumber + 1;
                break;
            case "last":
                destPage = pagecount;
                break;
        }
        var $Button = $('<a href="javascript:;" class="page_mini" id="' + destPage + '">' + buttonLabel + '</a>');
        $Button.click(function () { buttonClickCallback(destPage); });

        return $Button;
    }

    // pagernew defaults. hardly worth bothering with in this case but used as placeholder for expansion in the next version
    $.fn.pagernew.defaults = {
        pagenumber: 1,
        pagecount: 1
    };

})(jQuery);





