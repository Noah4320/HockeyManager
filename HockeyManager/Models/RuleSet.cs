using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class RuleSet
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int maxForwards { get; set; }
        [Required]
        public int maxDefensemen { get; set; }
        [Required]
        public int maxGoalies { get; set; }
        [Required]
        public int maxPlayers { get; set; }

        public virtual ICollection<Pool> Pools { get; set; }
    }
}
