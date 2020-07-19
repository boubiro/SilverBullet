using System;

namespace SilverBullet.Tesseract
{
    internal static class Extensions
    {

        //string ex
        /// <summary>
        /// Converts the given value to a boolean (or not).
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The default value to consider (optional).</param>
        /// <returns>The converted boolean.</returns>
        public static Boolean ToBoolean(this String value, Boolean defaultValue = false)
        {
            if (!String.IsNullOrEmpty(value) && Boolean.TryParse(value, out var converted))
            {
                return converted;
            }

            return defaultValue;
        }
    }
}
