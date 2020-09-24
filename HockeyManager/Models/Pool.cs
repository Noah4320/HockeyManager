using HockeyManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [Required(ErrorMessage = "Please select a ruleset.")]
        public int? RuleSetId { get; set; }
        [Required]
        public virtual RuleSet RuleSet { get; set; }

        public string OwnerId { get; set; }
        public User Owner { get; set; }

        public virtual ICollection<HMTeam> Teams { get; set; }

    }
}
