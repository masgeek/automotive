using System;
using Plugin.Messaging;

namespace liquidtorque.ComponentClasses
{
	public class MessagingComponent
	{
		public static bool SendSms(string phoneNumber)
		{
		    try
		    {
		        var smsTask = MessagingPlugin.SmsMessenger;
		        if (smsTask.CanSendSms)
		        {
		            smsTask.SendSms(phoneNumber);

		            return true;
		        }

		        return false;
		    }
		    catch (Exception e)
		    {
		        Console.WriteLine(e.Message);
		        return false;
		    }
		}

        /// <summary>
        /// Trigger phone functions, this may not work on the IPAD
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
		public static bool CallNumber(string phoneNumber)
		{
		    try
		    {
		        var callTask = MessagingPlugin.PhoneDialer;
		        if (callTask.CanMakePhoneCall)
		        {
		            callTask.MakePhoneCall(phoneNumber);
		            return true;
		        }

		        return false;
		    }
		    catch (Exception e)
		    {
		        Console.WriteLine(e.Message);
		        return false;
		    }
		}

        /// <summary>
        /// Send support email or anything email related
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
	    public static bool SendEmail(string emailAddress, string subject = "Vehicle Inquiry")
	    {
	        try
	        {

	            var emailTask = MessagingPlugin.EmailMessenger;
	            if (emailTask.CanSendEmail)
	            {
	                emailTask.SendEmail(emailAddress, subject);
	                return true;
	            }
	        }
	        catch (Exception e)
	        {
	            Console.WriteLine(e.Message);
	        }
	        return false;
	    }
	}
}

