using HockeyManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.ViewModel
{
    public class SearchPlayer
    {
        public IEnumerable<HMTeam> Teams { get; set; }
        public IEnumerable<HMPlayer> Players { get; set; }

        public SearchPlayer(List<HMTeam> teams, List<HMPlayer> players)
        {
            Teams = teams;
            Players = players;
        }
    }
}
