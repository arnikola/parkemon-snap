using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ServiceCommon.Utilities
{
    public static class SmsService
    {
        /// <summary>
        /// The SMS provider endpoint.
        /// </summary>
        private const string Endpoint = "https://rest.nexmo.com/sms/json";

        /// <summary>
        /// The API key.
        /// </summary>
        private const string ApiKey = "31d4ed55";

        /// <summary>
        /// The API secret.
        /// </summary>
        private const string ApiSecret = "1923d2f7";

        /// <summary>
        /// The SMS sender name.
        /// </summary>
        private const string SmsSenderName = "Dr.Band";

        /// <summary>
        /// Send an SMS message with the provided <paramref name="body"/> to the provided <paramref name="phoneNumber"/>.
        /// </summary>
        /// <param name="phoneNumber">
        /// The phone number to send the message to.
        /// </param>
        /// <param name="body">
        /// The message body.
        /// </param>
        /// <returns>
        /// The phone number which the message was sent to.
        /// </returns>
        /// <remarks><paramref name="phoneNumber"/> is an international number without a prefixed '+'.</remarks>
        public static async Task<string> SendSms(string phoneNumber, string body)
        {
            // Build the request.   
            var builder = new UriBuilder(Endpoint);
            var parameters = new Dictionary<string, string>
                             {
                                 { "api_key", ApiKey },
                                 { "api_secret", ApiSecret },
                                 { "from", SmsSenderName },
                                 { "to", phoneNumber },
                                 { "text", body }
                             };
            builder.Query = string.Join("&", parameters.Select(_ =>
                $"{HttpUtility.UrlEncode(_.Key)}={HttpUtility.UrlEncode(_.Value)}"));

            // Send the request.
            var request = builder.Uri;
            var response = await new HttpClient().GetAsync(request);
            response.EnsureSuccessStatusCode();
            return phoneNumber;
        }
     
    }
}
