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

        [Required(ErrorMessage = "Which Brand Value are you demonstrating?")]
        public ICollection<Value> Values { get; set; }

        [StringLength(600)]
        [Required(AllowEmptyStrings = false)]
        public string Description { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "A name would be very helpful")]
        public string Name { get; set; }

        
        public string UserName { get; set; }

        
        public string UserArea { get; set; }
    }
}