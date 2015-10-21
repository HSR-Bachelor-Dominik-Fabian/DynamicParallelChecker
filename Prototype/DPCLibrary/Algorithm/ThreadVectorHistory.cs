using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DPCLibrary.Algorithm
{
    class ThreadVectorHistory : List<VectorEvent>
    {
        /// <summary>
        /// Overriden from List. Adds new VectorEvent to History. If an Event is in the History with the same Clock it's merged
        /// </summary>
        /// <param name="item"></param>
        public new void Add(VectorEvent item)
        {
            
            base.Add(item);
        }

        
    }
}
