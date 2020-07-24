using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class HMTeam
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Division { get; set; }
        public string Conference { get; set; }
        public string Place { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int OvertimeLoses { get; set; }
        public int Points { get; set; }
        public int RegulationWins { get; set; }
        public string logoUrl { get; set; }
        public int ApiId { get; set; }
        public IList<HMPlayer> Players { get; set; }
    }
}
