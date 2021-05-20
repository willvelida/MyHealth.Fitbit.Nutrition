using System;
using System.Globalization;

namespace MyHealth.Fitbit.Nutrition.Validators
{
    public class DateValidator : IDateValidator
    {
        public bool IsNutritionDateValid(string foodDate)
        {
            bool isDateValid = false;
            string pattern = "yyyy-MM-dd";
            DateTime parsedFoodDate;

            if (DateTime.TryParseExact(foodDate, pattern, null, DateTimeStyles.None, out parsedFoodDate))
            {
                isDateValid = true;
            }

            return isDateValid;
        }
    }
}
