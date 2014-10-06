using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrandValues.Entries;

namespace BrandValues.Models
{
    public class BrowseViewModel
    {
        public IEnumerable<BrandValues.Entries.Entry> TeamEntries { get; set; }
        public IEnumerable<BrandValues.Entries.Entry> IndividualEntries { get; set; }
    }
}