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

        public string ChartType { get; set; } = "bar";        

        public List<string> Labels { get; set; }

        public List<int> Data { get; set; }

        public List<string> BackgroundColors { get; set; }

        public List<string> BorderColors { get; set; }

        public string Title { get; set; } = string.Empty;

        public void GenerateDefaultColors(int count)
        {
            var defaultBackgroundColors = new List<string>
            {
                "rgba(255, 99, 132, 0.2)",    
                "rgba(54, 162, 235, 0.2)",    
                "rgba(255, 206, 86, 0.2)",    
                "rgba(75, 192, 192, 0.2)",    
                "rgba(153, 102, 255, 0.2)",   
                "rgba(255, 159, 64, 0.2)"     
            };

            var defaultBorderColors = new List<string>
            {
                "rgba(255, 99, 132, 1)",      
                "rgba(54, 162, 235, 1)",      
                "rgba(255, 206, 86, 1)",      
                "rgba(75, 192, 192, 1)",      
                "rgba(153, 102, 255, 1)",     
                "rgba(255, 159, 64, 1)"       
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
