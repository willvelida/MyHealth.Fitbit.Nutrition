using System.Collections.Generic;

namespace MyHealth.Fitbit.Nutrition.Models
{
    public class FoodResponseObject
    {
        public List<Food> foods { get; set; }
        public Goals goals { get; set; }
        public Summary summary { get; set; }
    }
}
