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
             if (@this.TotalMinutes >= 60)
             {
                 var hours =
                     @this.Subtract(TimeSpan.FromSeconds(@this.Seconds)).Subtract(TimeSpan.FromMinutes(@this.Minutes));
                 return string.Format(
                     CultureInfo.CurrentCulture,
                     "{0:N0}:{1:00}:{2:00}",
                     hours.TotalHours,
                     @this.Subtract(TimeSpan.FromSeconds(@this.Seconds)).Minutes,
                     @this.Seconds);
             }
             else
             {
                 return string.Format(
                     CultureInfo.CurrentCulture,
                     "{0:N0}:{1:00}",
                     @this.Subtract(TimeSpan.FromSeconds(@this.Seconds)).TotalMinutes,
                     @this.Seconds);
             }
         }
    }
}