using System.Diagnostics.CodeAnalysis;

namespace MyHealth.Fitbit.Nutrition.Models
{
    [ExcludeFromCodeCoverage]
    public class Unit
    {
        public int id { get; set; }
        public string name { get; set; }
        public string plural { get; set; }
    }
}
