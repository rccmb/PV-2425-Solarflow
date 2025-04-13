namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents an audit log entry that tracks user actions and related metadata.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the audit log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who performed the action.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the action performed by the user.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the action performed.
        /// </summary>
        public string Brief { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the action was performed.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the action was performed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
