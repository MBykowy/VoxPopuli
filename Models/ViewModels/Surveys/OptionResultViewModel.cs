namespace VoxPopuli.Models.ViewModels.Surveys
{
    public class OptionResultViewModel
    {
        public OptionResultViewModel()
        {
            OptionText = string.Empty;
        }

        public int OptionId { get; set; }

        public string OptionText { get; set; }

        public int Count { get; set; }

        public double Percentage { get; set; }
    }
}
