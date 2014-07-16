using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrandValues.Entries
{
    public class PostEntry
    {
        public string Type { get; set; }

        public string Format { get; set; }

        public string Thumbnail { get; set; }

        public List<string> Values = new List<string>();

        [Required(AllowEmptyStrings = false)]
        public string Description { get; set; }

        public string UserName { get; set; }

        public string UserArea { get; set; }
    }
}