using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class HMTeamInfo
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Division { get; set; }
        public string Conference { get; set; }
        public string logoUrl { get; set; }
        [JsonIgnore]
        public virtual ICollection<HMTeam> TeamStats { get; set; }
    }
}
