using System;
using System.Security.Cryptography;

namespace WebToDoAPI.Utils
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private readonly char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        public string Generate(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException(nameof(length));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
            }

            using var rng = RandomNumberGenerator.Create();
            var byteBuffer = new byte[length];

            rng.GetBytes(byteBuffer);

            var count = 0;
            var characterBuffer = new char[length];

            for (var iter = 0; iter < length; iter++)
            {
                var i = byteBuffer[iter] % 87;

                switch (i)
                {
                    case < 10:
                        characterBuffer[iter] = (char)('0' + i);
                        break;
                    case < 36:
                        characterBuffer[iter] = (char)('A' + i - 10);
                        break;
                    case < 62:
                        characterBuffer[iter] = (char)('a' + i - 36);
                        break;
                    default:
                        characterBuffer[iter] = punctuations[i - 62];
                        count++;
                        break;
                }
            }

            if (count >= numberOfNonAlphanumericCharacters)
            {
                return new string(characterBuffer);
            }


            var rand = new Random();

            for (int j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
            {
                int k;
                do
                {
                    k = rand.Next(0, length);
                }
                while (!char.IsLetterOrDigit(characterBuffer[k]));

                characterBuffer[k] = punctuations[rand.Next(0, punctuations.Length)];
            }

            return new string(characterBuffer);
        }
    }
}
