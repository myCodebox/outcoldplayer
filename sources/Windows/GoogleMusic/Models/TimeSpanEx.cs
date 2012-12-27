// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Globalization;

    public static class TimeSpanEx
    {
         public static string ToPresentString(this TimeSpan @this)
         {
             return string.Format(CultureInfo.CurrentCulture, "{0:N0}:{1:00}", @this.Subtract(TimeSpan.FromSeconds(@this.Seconds)).TotalMinutes, @this.Seconds);
         }
    }
}