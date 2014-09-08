using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BrandValues.Entries;

namespace BrandValues.Models
{
    public class HomeViewModel
    {
        public IEnumerable<BrandValues.Entries.Entry> Entries { get; set; }
        public IEnumerable<BrandValues.Models.Poll> Polls { get; set; }
    }
}