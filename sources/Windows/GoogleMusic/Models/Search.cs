// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public static class Search
    {
         public static bool Contains(string title, string search)
         {
             return IndexOf(title, search) >= 0;
         }

         public static int IndexOf(string title, string search)
         {
             if (string.IsNullOrEmpty(title))
             {
                 return -1;
             }

             int index = -1;
             do
             {
                 index = title.IndexOf(search.ToUpper(), index + 1, StringComparison.CurrentCultureIgnoreCase);
                 if ((index == 0) || (index > 0 && char.IsSeparator(title[index - 1])))
                 {
                     return index;
                 }
             }
             while (index >= 0 && index < title.Length);

             return -1;
         }
    }
}