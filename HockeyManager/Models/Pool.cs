﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class Pool
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public bool Private { get; set; }
        [Required]
        public int Size { get; set; }
        public string Status { get; set; }
        public virtual ICollection<PoolList> PoolList { get; set; }

        [Required]
        public int? RuleSetId { get; set; }
        [Required]
        public virtual RuleSet RuleSet { get; set; }
    }
}