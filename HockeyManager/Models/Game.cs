using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int? HomeTeamId { get; set; }
        [ForeignKey("HomeTeamId")]
        public virtual HMTeam HomeTeam { get; set; }
        public int? AwayTeamId { get; set; }
        [ForeignKey("AwayTeamId")]
        public virtual HMTeam AwayTeam { get; set; }

        public virtual ICollection<GameEvent> GameEvents { get; set; }
    }
}
