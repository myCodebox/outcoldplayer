// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections;
    using System.Collections.Generic;

    public class CollectionExtensions
    {
        public static void UpdateCollection<T>(IList<T> collection, IEnumerable newItems, IEnumerable oldItems)
        {
            if (oldItems != null)
            {
                foreach (T songBindingModel in oldItems)
                {
                    if (collection.Contains(songBindingModel))
                    {
                        collection.Remove(songBindingModel);
                    }
                }
            }

            if (newItems != null)
            {
                foreach (T songBindingModel in newItems)
                {
                    if (!collection.Contains(songBindingModel))
                    {
                        collection.Add(songBindingModel);
                    }
                }
            }
        }
    }
}
