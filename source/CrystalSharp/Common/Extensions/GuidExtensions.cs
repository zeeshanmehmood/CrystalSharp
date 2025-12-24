using System;

namespace CrystalSharp.Common.Extensions
{
    public static class GuidExtensions
    {
        extension(Guid)
        {
            public static Guid Create()
            {
                return Guid.CreateVersion7();
            }

            public static Guid Create(DateTimeOffset timestamp)
            {
                return Guid.CreateVersion7(timestamp);
            }

            /// <summary>
            /// Valid formats: "N", "D", "B", "P", or "X". If format is null or an empty string (""), "D" is used.
            /// </summary>
            /// <param name="format"></param>
            /// <returns></returns>
            public static string Create(string format)
            {
                return Create().ToString(format);
            }
        }
    }
}
