﻿using System;
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
        public string Place { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int OvertimeLoses { get; set; }
        public int Points { get; set; }
        public int RegulationWins { get; set; }

        public int ApiId { get; set; }

        public int? TeamInfoId { get; set; }
        public virtual HMTeamInfo TeamInfo { get; set; }

        public virtual ICollection<HMPlayer> Players { get; set; }
    }
}
