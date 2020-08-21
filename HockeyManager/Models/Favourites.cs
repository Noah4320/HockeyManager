using HockeyManager.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class Favourites
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }

       
        public int? PlayerId { get; set; }
        [ForeignKey("PlayerId")]
        public virtual HMPlayer Player { get; set; }

    }
}
