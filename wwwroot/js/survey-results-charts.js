/**
 * Creates a chart using Chart.js
 * @param {CanvasRenderingContext2D} ctx - The canvas context
 * @param {string} type - Chart type (bar, pie, etc.)
 * @param {string[]} labels - Chart labels
 * @param {number[]} data - Chart data values
 * @param {string[]} backgroundColor - Background colors for data points
 * @param {string[]} borderColor - Border colors for data points
 * @param {string} title - Chart title
 */
function createChart(ctx, type, labels, data, backgroundColor, borderColor, title) {
    // Default colors if not provided
    if (!backgroundColor || backgroundColor.length === 0) {
        backgroundColor = [
            'rgba(255, 99, 132, 0.2)',
            'rgba(54, 162, 235, 0.2)',
            'rgba(255, 206, 86, 0.2)',
            'rgba(75, 192, 192, 0.2)',
            'rgba(153, 102, 255, 0.2)',
            'rgba(255, 159, 64, 0.2)'
        ];
    }

    if (!borderColor || borderColor.length === 0) {
        borderColor = [
            'rgba(255, 99, 132, 1)',
            'rgba(54, 162, 235, 1)',
            'rgba(255, 206, 86, 1)',
            'rgba(75, 192, 192, 1)',
            'rgba(153, 102, 255, 1)',
            'rgba(255, 159, 64, 1)'
        ];
    }

    // Repeat colors if needed
    while (backgroundColor.length < data.length) {
        backgroundColor = backgroundColor.concat(backgroundColor);
    }
    while (borderColor.length < data.length) {
        borderColor = borderColor.concat(borderColor);
    }

    // Get only the colors we need
    backgroundColor = backgroundColor.slice(0, data.length);
    borderColor = borderColor.slice(0, data.length);

    // Create chart configuration based on chart type
    const config = {
        type: type,
        data: {
            labels: labels,
            datasets: [{
                label: 'Responses',
                data: data,
                backgroundColor: backgroundColor,
                borderColor: borderColor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: type === 'pie' || type === 'doughnut',
                    position: 'top',
                },
                title: {
                    display: !!title,
                    text: title || ''
                },
                tooltip: {
                    enabled: true
                }
            }
        }
    };

    // Add specific options based on chart type
    if (type === 'bar') {
        config.options.scales = {
            y: {
                beginAtZero: true,
                ticks: {
                    precision: 0 // Only show integer values
                }
            }
        };
    }

    // Create the chart
    return new Chart(ctx, config);
}

/**
 * Update an existing chart with new data
 * @param {Chart} chart - Chart instance to update
 * @param {string[]} labels - New labels
 * @param {number[]} data - New data values
 */
function updateChart(chart, labels, data) {
    chart.data.labels = labels;
    chart.data.datasets[0].data = data;
    chart.update();
}

/**
 * Export chart data to CSV
 * @param {string} filename - Name for the downloaded file
 * @param {string[]} labels - Chart labels (column headers)
 * @param {number[]} data - Chart data values
 */
function exportToCSV(filename, labels, data) {
    const csvContent = "data:text/csv;charset=utf-8,"
        + "Option,Count\n"
        + labels.map((label, i) => `"${label}",${data[i]}`).join("\n");

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", filename + ".csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

