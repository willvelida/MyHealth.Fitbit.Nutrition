using AutoMapper;
using MyHealth.Fitbit.Nutrition.Models;
using System.Diagnostics.CodeAnalysis;
using mdl = MyHealth.Common.Models;

namespace MyHealth.Fitbit.Nutrition.Profiles
{
    [ExcludeFromCodeCoverage]
    public class NutritionProfile : Profile
    {
        public NutritionProfile()
        {
            CreateMap<FoodResponseObject, mdl.Nutrition>()
                                .ForMember(
                    dest => dest.Calories,
                    opt => opt.MapFrom(src => src.summary.calories))
                .ForMember(
                    dest => dest.Carbs,
                    opt => opt.MapFrom(src => src.summary.carbs))
                .ForMember(
                    dest => dest.Fat,
                    opt => opt.MapFrom(src => src.summary.fat))
                .ForMember(
                    dest => dest.Fiber,
                    opt => opt.MapFrom(src => src.summary.fiber))
                .ForMember(
                    dest => dest.Protein,
                    opt => opt.MapFrom(src => src.summary.protein))
                .ForMember(
                    dest => dest.Sodium,
                    opt => opt.MapFrom(src => src.summary.sodium))
                .ForMember(
                    dest => dest.WaterInMl,
                    opt => opt.MapFrom(src => src.summary.water));
        }
    }
}
