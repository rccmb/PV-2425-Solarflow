namespace SolarflowClient.Models.ViewModels.Settings
{
    public class ChangeUserModelView
    {
        public string Fullname { get; set; }
        public double GridKWh { get; set; }
        public double SolarKWh { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
