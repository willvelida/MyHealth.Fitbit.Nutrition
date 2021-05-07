using System.Diagnostics.CodeAnalysis;

namespace MyHealth.Fitbit.Nutrition.Models
{
    [ExcludeFromCodeCoverage]
    public class Food
    {
        public bool isFavorite { get; set; }
        public string logDate { get; set; }
        public object logId { get; set; }
        public LoggedFood loggedFood { get; set; }
        public NutritionalValues nutritionalValues { get; set; }
    }
}
