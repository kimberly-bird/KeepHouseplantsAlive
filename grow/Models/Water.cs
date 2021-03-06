﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace grow.Models
{
    public class Water
    {
        [Key]
        public int WaterId { get; set; }

        [Required]
        [Display(Name = "Regularity of Watering")]
        public string Regularity { get; set; }


        public virtual ICollection<PlantAudit> PlantAudits { get; set; }
        public virtual List<Plant> Plants { get; set; }
    }
}
