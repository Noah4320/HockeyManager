using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class GameEvent
    {
        [Key]
        public int Id { get; set; }

        public string Event { get; set; }
        public string Period { get; set; }

        public int? PlayerId { get; set; }
        public virtual HMPlayer Player { get; set; }
        public int? GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
