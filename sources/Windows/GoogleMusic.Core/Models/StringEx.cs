// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Text;

    public static class StringEx
    {
        public static string Normalize(this string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }

            var normalizedValue = new StringBuilder();
            normalizedValue.Append("@");

            for (int i = 0; i < val.Length; i++)
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
