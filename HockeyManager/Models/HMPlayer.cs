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
        public string Name { get; set; }
        public string Position { get; set; }
        public string Country { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public int PlusMinus { get; set; }
        public string Height { get; set; }
        public long Weight { get; set; }
        public string PenalityMinutes { get; set; }
        public int Saves { get; set; }
        public int Shutouts { get; set; }
        public string HeadShotUrl { get; set; }
        public int ApiId { get; set; }
    }
}
