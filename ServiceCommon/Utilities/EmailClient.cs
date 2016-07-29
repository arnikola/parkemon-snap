namespace ServiceCommon.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    using SendGrid;

    public static class EmailClient
    {
        //  API Key:
        //      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

        // These are from the Azure management portal
        private const string Username = "azure_fd3984bbdfd9a0cfca340172ff222e68@azure.com";
        private const string Pswd = "h2t4CueMX5cS5jK";

        private static SendGridMessage CreateBill(DateTimeOffset date, decimal amount)
        {
            var dueDate = (date + TimeSpan.FromDays(7)).ToString("dd MMMM yyyy");
            return new SendGridMessage
            {
                From = new MailAddress("billing@otterpop.com"),
                Subject = $"Otterpop invoice for date ending {date.ToString("dd MMMM yyyy")}",
                Html =  $"<p>" +
                        $"Your invoice for Otterpop for the past billing period is ready." +
                        $"<br/>" +
                        $"The total due is: ${amount}, due by {dueDate}." +
                        $"<br/>" +
                        $"A further breakdown of your bill is attached."+
                        $"<br/>" +
                        $"This is an automated message. Do not respond." +
                        $"</p>",
                Text =  $"Your invoice for Otterpop for the past billing period is ready." +
                        $"\n" +
                        $"The total due is: ${amount}, due by {dueDate}." +
                        $"\n" +
                        $"A further breakdown of your bill is attached." +
                        $"\n" +
                        $"This is an automated message. Do not respond.",
            };
        }

        private static SendGridMessage CreateReport(int confirmed, int claimed, DateTimeOffset date)
        {
            return new SendGridMessage
            {
                From = new MailAddress("reports@otterpop.com"),
                Subject = $"Otterpop reports for date ending {date.ToString("dd MMMM yyyy")}",
                Html = $"<p>" +
                       $"Otterpop data for restaurants attached. Hopefully we made some cash this month!" +
                       $"<br/>" +
                       $"Number of coupons confirmed: {confirmed}." +
                       $"<br/>" +
                       $"Number of coupons claimed: {claimed}." +
                       $"<br/>" +
                       $"This is an automated message. Do not respond." +
                       $"</p>",
                Text = $"Otterpop data for restaurants attached. Hopefully we made some cash this month!" +
                       $"\n" +
                       $"Number of coupons confirmed: {confirmed}." +
                       $"\n" +
                       $"Number of coupons claimed: {claimed}." +
                       $"\n" +
                       $"This is an automated message. Do not respond.",
            };
        }

        public static async Task SendBill(string email, decimal amount, Stream invoiceStream)
        {
            var date = DateTimeOffset.UtcNow;
            var message = CreateBill(date, amount);

            // Add multiple addresses to the To field.
            var recipients = new List<string> { email };
            message.AddTo(recipients);

            message.AddAttachment(invoiceStream, "otterpop_" + date.ToString("invoice_dd_MM_yyyy") + ".pdf");

            var credentials = new NetworkCredential(Username, Pswd);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            await transportWeb.DeliverAsync(message);
        }

        public static async Task SendReport(int confirmed, int claimed, Stream csvStream)
        {
            var date = DateTimeOffset.UtcNow;
            var message = CreateReport(confirmed, claimed, date);

            // Add multiple addresses to the To field.
            var recipients = new List<string> { "artem.nikol.usa@hotmail.com", "reuben.bond@gmail.com", "sumeet.e@gmail.com", "aaron.r.bond@gmail.com" };
            message.AddTo(recipients);

            message.AddAttachment(csvStream, "otterpop_" + date.ToString("invoice_dd_MM_yyyy") + ".csv");

            var credentials = new NetworkCredential(Username, Pswd);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            await transportWeb.DeliverAsync(message);
        }
    }
}