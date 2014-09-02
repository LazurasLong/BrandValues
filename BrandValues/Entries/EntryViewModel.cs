using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrandValues.Entries
{
    public class EntryViewModel
    {
        public IEnumerable<Entry> Latest { get; set; }

        public IEnumerable<Entry> Trending { get; set; }
    }
}