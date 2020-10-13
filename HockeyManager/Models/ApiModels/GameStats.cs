using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HockeyManager.Models.ApiModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<GameRoot>(myJsonResponse); 
    public class GameStatus
    {
        public string abstractGameState { get; set; }
        public string codedGameState { get; set; }
        public string detailedState { get; set; }
        public string statusCode { get; set; }
        public bool startTimeTBD { get; set; }
    }

    public class GameLeagueRecord
    {
        public int wins { get; set; }
        public int losses { get; set; }
        public int ot { get; set; }
        public string type { get; set; }
    }

    public class GameTeam
    {
        public int id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
    }

    public class GameAway
    {
        public GameLeagueRecord leagueRecord { get; set; }
        public int score { get; set; }
        public GameTeam team { get; set; }
    }

    public class GameLeagueRecord2
    {
        public int wins { get; set; }
        public int losses { get; set; }
        public int ot { get; set; }
        public string type { get; set; }
    }

    public class GameTeam2
    {
        public int id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
    }

    public class GameHome
    {
        public GameLeagueRecord2 leagueRecord { get; set; }
        public int score { get; set; }
        public GameTeam2 team { get; set; }
    }

    public class GameTeams
    {
        public GameAway away { get; set; }
        public GameHome home { get; set; }
    }

    public class GameVenue
    {
        public int id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
    }

    public class GameContent
    {
        public string link { get; set; }
    }

    public class Game
    {
        public int gamePk { get; set; }
        public string link { get; set; }
        public string gameType { get; set; }
        public string season { get; set; }
        public DateTime gameDate { get; set; }
        public GameStatus status { get; set; }
        public GameTeams teams { get; set; }
        public GameVenue venue { get; set; }
        public GameContent content { get; set; }
    }

    public class GameDate
    {
        public string date { get; set; }
        public int totalItems { get; set; }
        public int totalEvents { get; set; }
        public int totalGames { get; set; }
        public int totalMatches { get; set; }
        public List<Game> games { get; set; }
        public List<object> events { get; set; }
        public List<object> matches { get; set; }
    }

    public class GameRoot
    {
        public string copyright { get; set; }
        public int totalItems { get; set; }
        public int totalEvents { get; set; }
        public int totalGames { get; set; }
        public int totalMatches { get; set; }
        public int wait { get; set; }
        public List<GameDate> dates { get; set; }
    }


}
