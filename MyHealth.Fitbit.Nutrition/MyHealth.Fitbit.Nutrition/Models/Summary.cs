﻿using System.Diagnostics.CodeAnalysis;

namespace MyHealth.Fitbit.Nutrition.Models
{
    [ExcludeFromCodeCoverage]
    public class Summary
    {
        public int calories { get; set; }
        public double carbs { get; set; }
        public double fat { get; set; }
        public double fiber { get; set; }
        public double protein { get; set; }
        public int sodium { get; set; }
        public int water { get; set; }
    }
}