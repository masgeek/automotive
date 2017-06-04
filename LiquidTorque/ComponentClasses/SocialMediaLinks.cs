using System;

namespace liquidtorque.ComponentClasses
{
    public class SocialMediaLinks
    {
        public object OpenFacebook(string fbUsername)
        {
            try
            {
                return ("https://www.facebook.com/" + fbUsername);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Facebook launch exception " + ex.Message);
            }
            return false;
        }

        public object OpenTwitter(string twitterUsername)
        {
            try
            {
                return ("https://www.twitter.com/" + twitterUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Twitter launch exceptions " + ex.Message);
            }
            return false;
        }

        public object OpenInstagram(string instagramUsername)
        {
            try
            {
        
               return ("https://www.instagram.com/" + instagramUsername);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Instagram exception " + ex.Message);
            }
            return false;
        }

        public object OpenLinkedin(string linkedinUsername)
        {
            try
            {
                return "https://www.linkedin.com/in/" + linkedinUsername;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Linkedin launching error " + ex.Message);
            }
            return false;
        }
    }
}
