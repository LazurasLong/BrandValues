using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BrandValues.Models
{
    public class SiteVersionViewModel
    {
        [Required]
        public string Homepage { get; set; }
    }
}