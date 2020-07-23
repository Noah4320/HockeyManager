using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models
{ 
    public class Type
    {
        public string displayName { get; set; }

    }

    public class Stat2
    {
        public string timeOnIce { get; set; }
        public int assists { get; set; }
        public int goals { get; set; }
        public int pim { get; set; }
        public int shots { get; set; }
        public int games { get; set; }
        public int hits { get; set; }
        public int powerPlayGoals { get; set; }
        public int powerPlayPoints { get; set; }
        public string powerPlayTimeOnIce { get; set; }
        public string evenTimeOnIce { get; set; }
        public string penaltyMinutes { get; set; }
        public double faceOffPct { get; set; }
        public double shotPct { get; set; }
        public int gameWinningGoals { get; set; }
        public int overTimeGoals { get; set; }
        public int shortHandedGoals { get; set; }
        public int shortHandedPoints { get; set; }
        public string shortHandedTimeOnIce { get; set; }
        public int blocked { get; set; }
        public int plusMinus { get; set; }
        public int points { get; set; }
        public int shifts { get; set; }
        public string timeOnIcePerGame { get; set; }
        public string evenTimeOnIcePerGame { get; set; }
        public string shortHandedTimeOnIcePerGame { get; set; }
        public string powerPlayTimeOnIcePerGame { get; set; }
        public int ot { get; set; }
        public int shutouts { get; set; }
        public int ties { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int saves { get; set; }
        public int powerPlaySaves { get; set; }
        public int shortHandedSaves { get; set; }
        public int evenSaves { get; set; }
        public int shortHandedShots { get; set; }
        public int evenShots { get; set; }
        public int powerPlayShots { get; set; }
        public double savePercentage { get; set; }
        public double goalAgainstAverage { get; set; }
        public int gamesStarted { get; set; }
        public int shotsAgainst { get; set; }
        public int goalsAgainst { get; set; }
        public double powerPlaySavePercentage { get; set; }
        public double shortHandedSavePercentage { get; set; }
        public double evenStrengthSavePercentage { get; set; }


    }

    public class Split
    {
        public string season { get; set; }
        public Stat2 stat { get; set; }

    }

    public class Stat
    {
        public Type type { get; set; }
        public List<Split> splits { get; set; }

    }

    public class StatsRoot
    {
        public string copyright { get; set; }
        public List<Stat> stats { get; set; }

    }


}
