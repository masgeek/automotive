using System;
using System.Net;
using System.Net.Mail;
using Twilio;

namespace MessagingLib
{
    public class Messaging
    {
        private static object myObject = new object();

        private static Messaging messaging;

        public static Messaging GetInstance()
        {
            lock (myObject)
            {
                return messaging ?? (messaging = new Messaging());
            }
        }
        /// <summary>
        /// Makes sure the phone number is in the full format plus the country code i.e
        /// +2540xxxxxx
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <param name="message"></param>
        public void SendSms(string phoneNo, string message)
        {
            try
            {
                // Find your Account Sid and Auth Token at twilio.com/user/account
                string AccountSid = "AC2655d63ee48cef76b06fc4ffdbd141a4";
                string AuthToken = "92660fe9bcd50d9c02036364bb0cbf8a";
                string twilioNumber = "+14694163287";

                var twilio = new TwilioRestClient(AccountSid, AuthToken);
                var result = twilio.SendMessage(twilioNumber, phoneNo, message);

                if (result.RestException != null)
                {
                    Console.WriteLine("Twilio error");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        //We can set up smtp for this
        //the google SMTP is not working, dont know why
        public void SendEmail(string receipient, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("no-reply@liquidtorque.com");
                var toAddress = new MailAddress(receipient);
                string fromPassword = "Pinkdog@888";

                var smtp = new SmtpClient
                {
                    //Host = "smtpout.secureserver.net",
                    //Port = 25,
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    //encryption = tls
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    //UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000

                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {

                    //smtp.Send(message);
                    smtp.SendAsync(message, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
