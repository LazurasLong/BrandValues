using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrandValues.Entries
{
    public class PostComment
    {
        [Required]
        public string Comment { get; set; }

        [Required]
        public string UserName { get; set; }
        public string UserArea { get; set; }
        public string UserFirstName { get; set; }
        public string UserSurname { get; set; }

        public DateTime CreatedOn { get; set; }


    }
}