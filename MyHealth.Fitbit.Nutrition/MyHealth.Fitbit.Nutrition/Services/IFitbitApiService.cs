using MyHealth.Fitbit.Nutrition.Models;
using System.Threading.Tasks;

namespace MyHealth.Fitbit.Nutrition.Services
{
    public interface IFitbitApiService
    {
        Task<FoodResponseObject> GetFoodLogs(string date);
    }
}
