using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class HMPlayerInfo
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTimeOffset DateOfBirth { get; set; }   
        public string Height { get; set; }
        public long Weight { get; set; }
        public string HeadShotUrl { get; set; }

        [JsonIgnore]
        public virtual ICollection<HMPlayer> PlayerStats { get; set; }
    }
}
