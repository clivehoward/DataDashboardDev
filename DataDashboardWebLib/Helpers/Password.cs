using System;

namespace DataDashboardWebLib.Helpers
{
    public static class Password
    {
        public static string Generate()
        {
            Random random = new Random();
            int randomNumber = 0;

            // Prepare an array for the allocated number of digits (12)
            var chars = new char[12];

            // Must have 1 number
            randomNumber = random.Next(0, 10);
            char number = (char)('0' + randomNumber);

            // Must have 1 lower case
            randomNumber = random.Next(0, 26);
            char lower = (char)('a' + randomNumber);

            // Must have 1 upper case
            randomNumber = random.Next(0, 26);
            char upper = (char)('A' + randomNumber);

            // Remaining letters can be anything
            for (int i = 0; i < 12; i++)
            {
                randomNumber = random.Next(0, 61); // Zero to 25

                // Add the predetermined required characters at certain places
                // The rest can be anything alphanumeric
                if (i == 2)
                {
                    chars[i] = lower;
                }
                else if (i == 5)
                {
                    chars[i] = number;
                }
                else if (i == 11)
                {
                    chars[i] = upper;
                }
                else if (randomNumber < 10)
                {
                    chars[i] = (char)('0' + randomNumber);
                }
                else if (randomNumber < 36)
                {
                    chars[i] = (char)('a' + (randomNumber - 10));
                }
                else if (randomNumber < 62)
                {
                    chars[i] = (char)('A' + (randomNumber - 36));
                }
            }

            // Return the new password
            return new string(chars);
        }
    }
}
