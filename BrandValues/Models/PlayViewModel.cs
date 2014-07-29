using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrandValues.Entries;

namespace BrandValues.Models
{
    public class PlayViewModel
    {
        public IEnumerable<BrandValues.Entries.Entry> Entries { get; set; }
        public Entry Entry { get; set; }
        public PostComment PostComment { get; set; }
    }
}