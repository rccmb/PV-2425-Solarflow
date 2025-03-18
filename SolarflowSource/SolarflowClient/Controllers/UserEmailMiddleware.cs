namespace SolarflowClient.Controllers
{
    public class UserEmailMiddleware
    {
        private readonly RequestDelegate _next;

        public UserEmailMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userEmail = context.Request.Cookies["UserEmail"];
            if (!string.IsNullOrEmpty(userEmail))
            {
                context.Items["UserEmail"] = userEmail; // Store in request context
            }

            await _next(context);
        }
    }

}
