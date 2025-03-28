// Launch the filters modal that uses a separate page
$(function () {
    var url = "/ApplyFilters";
    $('#launchFilters').click(function (e) {
        $('.filters-modal').load(url, function (result) {
            $('#filtersModal').modal({ show: true });
        });
    });
});

function SetTopicNavigation(title) {
    $("#navDropdownSelected").html(title + '&nbsp;<span class="caret"></span>'); // Set the dropdown text
    $(".dropdown").addClass("active"); // Set the nav item to be selected
}

function SetDataDropdown(title) {
    $("#viewDataDropdown").html(title + '&nbsp;<span class="caret"></span>'); // Set the dataset dropdown
}

// Polyfill to make .toBlob work against canvas objects where there is no browser support (such as Edge)
if (!HTMLCanvasElement.prototype.toBlob) {
    Object.defineProperty(HTMLCanvasElement.prototype, 'toBlob', {
        value: function (callback, type, quality) {
            var canvas = this;
            setTimeout(function () {

                var binStr = atob(canvas.toDataURL(type, quality).split(',')[1]),
                    len = binStr.length,
                    arr = new Uint8Array(len);

                for (var i = 0; i < len; i++) {
                    arr[i] = binStr.charCodeAt(i);
                }

                callback(new Blob([arr], { type: type || 'image/png' }));

            });
        }
    });
}

function DownloadChartAsImage(chartsWidth, chartsHeight, legendWidth, legendHeight, title) {
    // Legend is HTML and so we need to convert it to a canvas object
    // If there is a legend
    if (document.getElementById('chart1Legend')) {
        html2canvas(document.getElementById('chart1Legend')).then(function (canvas) {
            CreateDownloadChartImage(chartsWidth, chartsHeight, legendWidth, legendHeight, title, canvas);
        });
    } else {
        CreateDownloadChartImage(chartsWidth, chartsHeight, legendWidth, legendHeight, title, null);
    }
    return false;
}

function CreateDownloadChartImage(chartsWidth, chartsHeight, legendWidth, legendHeight, title, legend) {

    // Is there a legend?
    var legendCanvas = null;
    if (legend != null) {
        legendCanvas = legend;
    } else { // Set the size to zero just in case this was not done when passing params
        legendWidth = 0;
        legendHeight = 0;
    }

    var canvasWidth = chartsWidth; // Canvas width is charts
    var canvasHeight = chartsHeight; // Full height of charts

    var padding = 20; // Use some padding to space things out

    // If the legend is beyond a certain width then place it under the charts rather than next to it
    if (legendWidth > 500) {
        canvasHeight = canvasHeight + legendHeight + padding;
    } else {
        canvasWidth = canvasWidth + legendWidth;
    }

    // Create a new canvas
    // Dynamically create the canvas width and height based on the chart and legend
    var downloadCanvas = document.createElement("canvas");
    downloadCanvas.height = canvasHeight;
    downloadCanvas.width = canvasWidth;

    var downloadContext = downloadCanvas.getContext("2d");
    downloadContext.fillStyle = "white";
    downloadContext.fillRect(0, 0, canvasWidth, canvasHeight);

    // Add the charts
    var charts = $('canvas[id^="chart"]'); // There maybe more than one chart!
    var noOfCharts = charts.length; // Number of charts
    var chartWidth = chartsWidth / noOfCharts; // Length of each chart
    var chartHeight = chartsHeight;
    var top = 0;

    // Change the positioning and height of the chart to account for the label
    if (noOfCharts > 1) {
        top = 50;
        chartHeight = chartHeight - top;
    }

    // Iterate the charts and add each one next to the other
    $.each(charts, function (index, value) {

        var left = index * chartWidth; // Calculate position
        var chartId = 'chart' + (index + 1); // Get the chart (might be in value param)
        var chartCanvas = document.getElementById(chartId);

        // We ONLY care about this if there is more than one chart!
        if (noOfCharts > 1) {
            var label = $($(chartCanvas).parent().parent().children()[0]).text(); // Get the label used for this chart by traversing the DOM

            // Add the label to the canvas
            downloadContext.font = "20px Arial";
            downloadContext.fillStyle = "black";
            downloadContext.fillText(label, left, 30);
        }

        downloadContext.drawImage(chartCanvas, left, top, chartWidth - padding, chartHeight); // Add the chart to the canvas

    });

    // Add the legend, place to the side or below charts based on width (if there is one)
    if (legendCanvas != null) {
        if (legendWidth > 500) {
            top = chartsHeight + padding;
            left = 0;
        } else {
            top = 0;
            left = chartsWidth;
        }

        downloadContext.drawImage(legendCanvas, left, top, legendWidth, legendHeight);
    }

    // Create file name from chart title
    var fileName = title;
    fileName = fileName.replace(/[^a-z0-9\s]/gi, '');
    fileName = fileName.replace(/ /g, "_");
    fileName = fileName.toLowerCase();
    fileName = fileName + "_chart.png";

    // Download the file
    downloadCanvas.toBlob(function (blob) {
        saveAs(blob, fileName);
    });

}

function DownloadDataAsImage(title) {

    // For the chart image we don't want the sorting icons in the column headers
    // This is complicated...
    // We must get the header cells (TH) for each possible sorting class: .sorting, .sorting_asc, .sorting_desc
    // Then, we remove the class... 
    // BUT, we must keep the array of elements in order to add the class back AFTER we have created the image
    var sorting = $(".sorting");
    sorting.each(function (index) {
        $(this).removeClass("sorting");
    });

    var sortingAsc = $(".sorting_asc");
    sortingAsc.each(function (index) {
        $(this).removeClass("sorting_asc");
    });

    var sortingDesc = $(".sorting_desc");
    sortingDesc.each(function (index) {
        $(this).removeClass("sorting_desc");
    });
    
    // Grab the HTML table containing the data, add it to a canvas and download it as an image...
    html2canvas(document.getElementById('allDataTable')).then(function (canvas) {

        // Use width and height from size on screen
        var canvasWidth = $(document.getElementById('allDataTable')).parent().width();
        var canvasHeight = $(document.getElementById('allDataTable')).parent().height();

        // Create a new canvas
        var downloadCanvas = document.createElement("canvas");
        downloadCanvas.height = canvasHeight;
        downloadCanvas.width = canvasWidth;

        var downloadContext = downloadCanvas.getContext("2d");
        downloadContext.fillStyle = "white";
        downloadContext.fillRect(0, 0, canvasWidth, canvasHeight);

        downloadContext.drawImage(canvas, 0, 0, canvasWidth, canvasHeight); // Add the data table to the canvas

        // Create file name from chart title
        var fileName = title;
        fileName = fileName.replace(/[^a-z0-9\s]/gi, '');
        fileName = fileName.replace(/ /g, "_");
        fileName = fileName.toLowerCase();
        fileName = fileName + "_data.png";

        // Download the file
        downloadCanvas.toBlob(function (blob) {
            saveAs(blob, fileName);
        });

        // Add the sorting classes back
        sorting.each(function (index) {
            $(this).addClass("sorting");
        });

        sortingAsc.each(function (index) {
            $(this).addClass("sorting_asc");
        });

        sortingDesc.each(function (index) {
            $(this).addClass("sorting_desc");
        });
    });

    return false;

}

var chartBuildComplete = false;

function ChartAnimationComplete() {
    // This runs when a chart animation finishes...
    // CHALLENGE:
    // There are multiple animations in a chart, not just when it first builds
    // For example, tooltips use animations
    // So, we're only interested in the first animation
    if (chartBuildComplete == false) {
        // Enable the chart download image button, otherwise people can download a chart that is not fully formed
        $('#btnDownloadOptions').removeClass("disabled");
    }

    chartBuildComplete = true;
}