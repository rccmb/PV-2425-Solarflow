using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SolarflowServer.Services
{
    public interface IWindyService
    {
        Task<JObject> GetWeatherDataAsync(double latitude, double longitude);
    }
}