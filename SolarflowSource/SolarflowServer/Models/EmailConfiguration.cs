namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents the configuration settings required for sending emails.
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Gets or sets the email address from which emails will be sent.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server address used for sending emails.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the port number used to connect to the SMTP server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the username used to authenticate with the SMTP server.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate with the SMTP server.
        /// </summary>
        public string Password { get; set; }
    }
}
