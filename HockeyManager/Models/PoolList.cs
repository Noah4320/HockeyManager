using HockeyManager.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{
    public class PoolList
    {
        [Key]
        public int Id { get; set; }

        public int? PoolId { get; set; }
        public virtual Pool Pool { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}
