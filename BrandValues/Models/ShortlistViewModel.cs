using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrandValues.Entries;

namespace BrandValues.Models
{
    public class ShortlistViewModel
    {
        public IEnumerable<BrandValues.Entries.ShortlistedEntry> TeamEntries { get; set; }
        public IEnumerable<BrandValues.Entries.ShortlistedEntry> IndividualEntries { get; set; }
    }
}