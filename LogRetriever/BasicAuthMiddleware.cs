namespace LogRetriever
{
    using System;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    using Owin;

    public static class BasicAuthMiddleware
    {
        public static void UseBasicAuth(this IAppBuilder app, string username, string password)
        {
            app.Use(
                async (context, next) =>
                {
                    context.Response.OnSendingHeaders(
                        state =>
                        {
                            var response = (IOwinResponse)state;
                            if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
                            {
                                response.Headers["WWW-Authenticate"] = "Basic";
                            }
                        },
                        context.Response);

                    string[] authHeaderValues;
                    if (context.Request.Headers.TryGetValue("Authorization", out authHeaderValues)
                        && authHeaderValues.Length == 1)
                    {
                        var authHeader = AuthenticationHeaderValue.Parse(authHeaderValues[0]);

                        if ("Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
                        {
                            var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter));
                            var parts = parameter.Split(':');

                            var providedUsername = parts[0];
                            var providedPassword = parts[1];

                            if (string.Equals(username, providedUsername, StringComparison.Ordinal)
                                && string.Equals(password, providedPassword, StringComparison.Ordinal))
                            {
                                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                                var identity = new ClaimsIdentity(claims, "Basic");

                                context.Request.User = new ClaimsPrincipal(identity);
                            }
                        }
                    }

                    if (context.Request.User == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ReasonPhrase = "Authentication required.";

                        // TODO: Proper anti-abuse system.
                        await Task.Delay(1000);
                    }
                    else
                    {
                        await next();
                    }
                });
        }
    }
}