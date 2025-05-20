namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class ChartDataViewModel
    {
        public ChartDataViewModel()
        {
            Labels = new List<string>();
            Data = new List<int>();
            BackgroundColors = new List<string>();
            BorderColors = new List<string>();
        }

        public string ChartType { get; set; } = "bar"; // Default chart type (bar, pie, doughnut, etc.)

        public List<string> Labels { get; set; }

        public List<int> Data { get; set; }

        public List<string> BackgroundColors { get; set; }

        public List<string> BorderColors { get; set; }

        public string Title { get; set; } = string.Empty;

        // Helper method to generate default colors
        public void GenerateDefaultColors(int count)
        {
            var defaultBackgroundColors = new List<string>
            {
                "rgba(255, 99, 132, 0.2)",   // Red
                "rgba(54, 162, 235, 0.2)",   // Blue
                "rgba(255, 206, 86, 0.2)",   // Yellow
                "rgba(75, 192, 192, 0.2)",   // Green
                "rgba(153, 102, 255, 0.2)",  // Purple
                "rgba(255, 159, 64, 0.2)"    // Orange
            };

            var defaultBorderColors = new List<string>
            {
                "rgba(255, 99, 132, 1)",     // Red
                "rgba(54, 162, 235, 1)",     // Blue
                "rgba(255, 206, 86, 1)",     // Yellow
                "rgba(75, 192, 192, 1)",     // Green
                "rgba(153, 102, 255, 1)",    // Purple
                "rgba(255, 159, 64, 1)"      // Orange
            };

            BackgroundColors.Clear();
            BorderColors.Clear();

            for (int i = 0; i < count; i++)
            {
                BackgroundColors.Add(defaultBackgroundColors[i % defaultBackgroundColors.Count]);
                BorderColors.Add(defaultBorderColors[i % defaultBorderColors.Count]);
            }
        }
    }
}
