using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrandValues.Entries
{
    public class PostEntry
    {
        //Individual or team
        [Required]
        public string Type { get; set; }

        //Video, text, image
        [Required]
        public string Format { get; set; }

        public List<string> Values { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Description { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "A name would be very helpful")]
        public string Name { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string UserArea { get; set; }
    }
}