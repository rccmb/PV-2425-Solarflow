namespace SolarflowClient.Models
{
    /// <summary>
    /// Represents an error.
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Code of the error.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the error.
        /// </summary>
        public string Description { get; set; }
    }
}
