using System;
using System.Text;

namespace CrystalSharp.Common.Utilities
{
    public static class RandomGenerator
    {
        private readonly static string _allowedCharacters;
        private readonly static Random _random;

        static RandomGenerator()
        {
            _allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            _random = new Random();
        }

        public static int GenerateNumber(int min = 0, int max = int.MaxValue)
        {
            return _random.Next(min, max);
        }

        public static string GenerateString(int size = 4)
        {
            StringBuilder builder = new();

            for (int counter = 0; counter < size; counter++)
            {
                int randomCharacter = _random.Next(_allowedCharacters.Length);

                builder.Append(_allowedCharacters[randomCharacter]);
            }

            return builder.ToString();
        }
    }
}
