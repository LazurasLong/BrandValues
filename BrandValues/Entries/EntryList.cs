using System.Collections.Generic;

namespace BrandValues.Entries
{
    public class EntryList
    {
        public IEnumerable<Entry> Entries { get; set; }
        public EntryFilter Filters { get; set; }
    }
}