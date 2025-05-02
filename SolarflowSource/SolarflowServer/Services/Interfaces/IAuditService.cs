namespace SolarflowServer.Services.Interfaces
{
    public interface IAuditService
    {
        /// <summary>
        /// Logs an audit event with the specified details.
        /// </summary>
        /// <param name="userId">The unique identifier of the user who performed the action.</param>
        /// <param name="brief">A brief description of the action performed.</param>
        /// <param name="description">A detailed description of the action performed.</param>
        /// <param name="ipAddress">The IP address from which the action was performed.</param>
        /// <returns>A task that represents the asynchronous logging operation.</returns>
        Task LogAsync(string userId, string brief, string description, string ipAddress);
    }
}
