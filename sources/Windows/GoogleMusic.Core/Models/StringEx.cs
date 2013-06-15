// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Text;

    public static class StringEx
    {
        private const string The = "The";

        public static string Normalize(this string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }

            var normalizedValue = new StringBuilder();
            normalizedValue.Append("@");

            var length = val.Length;
            var startIndex = 0;

            if (val.StartsWith(The, StringComparison.CurrentCultureIgnoreCase) 
                && val.Length > (The.Length + 1)
                && char.IsSeparator(val[The.Length]))
            {
                int i = The.Length + 1;
                while (val.Length > i && char.IsSeparator(val[i]))
                {
                    i++;
                }

                if (i < The.Length - 1)
                {
                    startIndex = i;
                }
            }

            for (int i = startIndex; i < length; i++)
            {
                var c = val[i];
                if (!char.IsLetterOrDigit(c))
                {
                    normalizedValue.AppendFormat("@{0}@", c);
                }
                else
                {
                    normalizedValue.Append(char.ToUpper(c));
                }
            }

            return normalizedValue.ToString();
        }
    }
}
