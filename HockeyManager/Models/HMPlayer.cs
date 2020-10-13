using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class HMPlayer
    {
        [Key]
        public int Id { get; set; }
        public int GamesPlayed { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        public int PlusMinus { get; set; }
        public int PenalityMinutes { get; set; }
        public int Saves { get; set; }
        public int Shutouts { get; set; }

        public int ApiId { get; set; }

        public int? TeamId { get; set; }
        public virtual HMTeam Team { get; set; }


        public int? PlayerInfoId { get; set; }
        public virtual HMPlayerInfo PlayerInfo { get; set; }

        [JsonIgnore]
        public virtual ICollection<Favourites> Favourites { get; set; }
    }
}
