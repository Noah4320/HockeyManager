using HockeyManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.ViewModel
{
    public class SeasonsViewModel
    {
        public IEnumerable<HMTeam> Teams { get; set; }
        public HMTeam MyTeam { get; set; }
        public virtual Season Season { get; set; }
    }
}
