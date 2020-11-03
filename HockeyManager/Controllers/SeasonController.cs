using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Controllers
{
    [Authorize]
    public class SeasonController : Controller
    {
        private readonly HockeyContext _context;
        private readonly UserManager<User> _userManager;

        public SeasonController(HockeyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SeasonController
        public ActionResult Index()
        {
            var seasons = _context.Teams.Include(x => x.TeamInfo).Where(x => x.SeasonId != null && x.UserId == _userManager.GetUserId(User)).ToList();

            return View(seasons);
        }

        // GET: SeasonController/NewSeason
        public ActionResult NewSeason()
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.ApiId != 0).ToList());

            //The player limits are hardcoded smh
            ViewBag.maxForwards = 12;
            ViewBag.maxDefencemen = 6;
            ViewBag.maxGoalies = 2;
            ViewBag.maxPlayers = 20;
            return View(VMplayers);
        }

        // POST: SeasonController/CreateSeason
        [HttpPost]
        public async Task<string> CreateSeason(string name, string abbreviation, string[] players, string[] pacific, string[] central, string[] atlantic, string[] metropolitan)
        {

            if (name == null)
            {
                return "Please enter a team name!";
            }

            if (abbreviation == null || abbreviation.Length != 3)
            {
                return "abbreviation must be 3 characters!";
            }

            abbreviation = abbreviation.ToUpper();

            var hMPlayers = await _context.Players.Include(x => x.PlayerInfo).Where(x => players.Contains(x.PlayerInfoId.ToString()) && x.ApiId != 0).ToListAsync();

            var forwards = 0;
            var defencemen = 0;
            var goalies = 0;

            foreach (var player in hMPlayers)
            {
                //Count every position
                if (player.Position == "C" || player.Position == "LW" || player.Position == "RW")
                {
                    forwards++;
                }
                else if (player.Position == "D")
                {
                    defencemen++;
                }
                else if (player.Position == "G")
                {
                    goalies++;
                }

                //Check to make sure the user hasn't exceeded the player position limit
                if (forwards > 12)
                {
                    return "Too many forwards!";
                }
                else if (defencemen > 6)
                {
                    return "Too many defencemen!";
                }
                else if (goalies > 2)
                {
                    return "Too many goalies!";
                }

            }
            //Check if team is too big or too small
            if ((forwards + defencemen + goalies) > 20)
            {
                return "Too many players!";
            }
            else if ((forwards + defencemen + goalies) < 20)
            {
                return $"{20 - (forwards + defencemen + goalies)} more players are required.";
            }

            //All validation succeeded

            //Create users team and players
            var season = new Season();
            season.UserId = _userManager.GetUserId(User);

            await _context.Seasons.AddAsync(season);
            await _context.SaveChangesAsync();

            HMTeamInfo teamInfo = new HMTeamInfo();

            teamInfo.Name = name;
            teamInfo.Abbreviation = abbreviation;

            await _context.TeamInfo.AddAsync(teamInfo);
            await _context.SaveChangesAsync();


            HMTeam myTeam = new HMTeam();

            myTeam.TeamInfoId = teamInfo.Id;
            myTeam.SeasonId = season.Id;
            myTeam.UserId = _userManager.GetUserId(User);

            var inPacific = pacific.Contains("myTeam");
            var inCentral = central.Contains("myTeam");
            var inAtlantic = atlantic.Contains("myTeam");
            var inMetro = metropolitan.Contains("myTeam");

            if (inPacific)
            {
                myTeam.Division = "Pacific";
                myTeam.Conference = "Western";
            }
            else if (inCentral)
            {
                myTeam.Division = "Central";
                myTeam.Conference = "Western";
            }
            else if (inAtlantic)
            {
                myTeam.Division = "Atlantic";
                myTeam.Conference = "Eastern";
            }
            else if (inMetro)
            {
                myTeam.Division = "Metropolitan";
                myTeam.Conference = "Eastern";
            }

            await _context.Teams.AddAsync(myTeam);
            await _context.SaveChangesAsync();

            List<HMPlayer> newlyGeneratedRoster = new List<HMPlayer>();

            foreach (var player in hMPlayers)
            {
                newlyGeneratedRoster.Add(new HMPlayer
                {
                    Position = player.Position,
                    Overall = player.Overall,
                    TeamId = myTeam.Id,
                    PlayerInfoId = player.PlayerInfoId
                });
            }

            await _context.Players.AddRangeAsync(newlyGeneratedRoster);
            await _context.SaveChangesAsync();

            //Create league teams and players

            var leagueTeams = _context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).ToList();
            List<HMTeam> newlyGeneratedTeams = new List<HMTeam>();

            foreach (var team in leagueTeams)
            {
                var teamInPacific = pacific.Contains(team.TeamInfoId.ToString());
                var teamInCentral = central.Contains(team.TeamInfoId.ToString());
                var teamInAtlantic = atlantic.Contains(team.TeamInfoId.ToString());
                var teamInMetro = metropolitan.Contains(team.TeamInfoId.ToString());

                string division = "N/A";
                string conference = "N/A";

                if (teamInPacific)
                {
                    division = "Pacific";
                    conference = "Western";
                } 
                else if (teamInCentral)
                {
                    division = "Central";
                    conference = "Western";
                }
                else if (teamInAtlantic)
                {
                    division = "Atlantic";
                    conference = "Eastern";
                }
                else if (teamInMetro)
                {
                    division = "Metropolitan";
                    conference = "Eastern";
                }

                newlyGeneratedTeams.Add(new HMTeam
                {
                    Division = division,
                    Conference = conference,
                    //ToDo: This property might not be necessary
                    Place = "1",
                    TeamInfoId = team.TeamInfoId,
                    SeasonId = season.Id
                });
            }

            await _context.Teams.AddRangeAsync(newlyGeneratedTeams);
            await _context.SaveChangesAsync();


            List<HMPlayer> newGeneratedPlayers = new List<HMPlayer>();
            var leaguePlayers = _context.Players.Include(x => x.Team.TeamInfo);
            foreach (var team in newlyGeneratedTeams)
            {
                var teamPlayers = leaguePlayers.Where(x => x.Team.TeamInfoId == team.TeamInfoId).ToList();

                foreach (var player in teamPlayers)
                {
                    newGeneratedPlayers.Add(new HMPlayer
                    {
                        Position = player.Position,
                        Overall = player.Overall,
                        TeamId = team.Id,
                        PlayerInfoId = player.PlayerInfoId
                    });
                }
            }

            await _context.Players.AddRangeAsync(newGeneratedPlayers);
            await _context.SaveChangesAsync();

            //Create Schedule

            Game game = new Game();

            game.Date = DateTime.Now;
            game.HomeTeamId = myTeam.Id;
            game.AwayTeamId = newlyGeneratedTeams[16].Id;

            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            return season.Id.ToString();
        }

        // GET: SeasonController/Hub/5
        public ActionResult Hub(int id)
        {
            var teams = _context.Teams.Include(x => x.TeamInfo).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Where(x => x.SeasonId == id).ToList();
            var myTeam = _context.Teams.Include(x => x.HomeSchedule).Where(x => x.SeasonId == id && x.UserId == _userManager.GetUserId(User)).FirstOrDefault();

            SeasonsViewModel VMteams = new SeasonsViewModel();
            VMteams.Teams = teams;
            VMteams.MyTeam = myTeam;
            return View(VMteams);
        }

        // GET: SeasonController/SimGame/
        public ActionResult SimGame(int gameId)
        {
            var game = _context.Games.Include(x => x.HomeTeam.Players).ThenInclude(x => x.PlayerInfo)
                .Include(x => x.AwayTeam.Players).ThenInclude(x => x.PlayerInfo)
                .Include(x => x.HomeTeam.TeamInfo)
                .Include(x => x.AwayTeam.TeamInfo)
                .Where(x => x.Id == gameId).FirstOrDefault();
            return View(game);
        }

        [HttpGet]
        public ActionResult HomeTeam(int gameId)
        {
            var homeRoster = _context.Games
                .Where(x => x.Id == gameId).SelectMany(x => x.HomeTeam.Players)
                .Include(x => x.PlayerInfo).ToList();

            foreach (var player in homeRoster)
            {
                player.Goals = 0;
                player.Assists = 0;
                player.Points = 0;
                player.TimeOnIce = 0;
                player.Hits = 0;
                player.PlusMinus = 0;
                player.PowerPlayGoals = 0;
                player.PenalityMinutes = 0;
                player.GoalsAgainst = 0;
                player.Saves = 0;
                player.Shutouts = 0;
            }
            return PartialView("_GameRoster", homeRoster);
        }

        [HttpGet]
        public ActionResult AwayTeam(int gameId)
        {
            var awayRoster = _context.Games
                .Where(x => x.Id == gameId).SelectMany(x => x.AwayTeam.Players)
                .Include(x => x.PlayerInfo).ToList();

            foreach (var player in awayRoster)
            {
                player.Goals = 0;
                player.Assists = 0;
                player.Points = 0;
                player.TimeOnIce = 0;
                player.Hits = 0;
                player.PlusMinus = 0;
                player.PowerPlayGoals = 0;
                player.PenalityMinutes = 0;
                player.GoalsAgainst = 0;
                player.Saves = 0;
                player.Shutouts = 0;
            }
            return PartialView("_GameRoster", awayRoster);
        }

        [HttpGet]
        public void SimPeriod(int gameId)
        {
            var homeTeam = _context.Games.Where(x => x.Id == gameId).Select(x => x.HomeTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefault();
            var awayTeam = _context.Games.Where(x => x.Id == gameId).Select(x => x.AwayTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefault();

            decimal homeProb = homeTeam.Players.Sum(x => x.Overall);
            decimal awayProb = awayTeam.Players.Sum(x => x.Overall);

            Random random = new Random(DateTime.Now.Millisecond);

            if (homeProb > awayProb)
            {
                awayProb = Math.Round((awayProb / homeProb) / 2m, 2) * 100;
            } 
            else
            {
                homeProb = Math.Round((homeProb / awayProb) / 2m, 2) * 100;
            }

            int decidingNumber = random.Next(0, 101);


        }

        [HttpGet]
        public async Task<string> FinishGame(int gameId)
        {
            //get team instances
            var homeTeam = _context.Games.Where(x => x.Id == gameId).Select(x => x.HomeTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefault();
            var awayTeam = _context.Games.Where(x => x.Id == gameId).Select(x => x.AwayTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefault();

            //Sum up each team's roster overall
            decimal homeProb = (decimal)homeTeam.Players.Average(x => x.Overall);
            decimal awayProb = (decimal)awayTeam.Players.Average(x => x.Overall);
            string winner = "";
            string loser = "";

            Random random = new Random();
            var randomChance = random.Next(0, 101);
            //Find which team has the odds against them
            //prob will always be under 50%
            if (homeProb > awayProb)
            {
                var prob = Math.Round((awayProb / homeProb) / 2m, 2) * 100;
                if (randomChance < prob)
                {
                    winner = homeTeam.TeamInfo.Name;
                    loser = awayTeam.TeamInfo.Name;
                }
                else
                {
                    winner = awayTeam.TeamInfo.Name;
                    loser = homeTeam.TeamInfo.Name;
                }
            }
            else
            {
                var prob = Math.Round((homeProb / awayProb) / 2m, 2) * 100;
                if (randomChance < prob)
                {
                    winner = homeTeam.TeamInfo.Name;
                    loser = awayTeam.TeamInfo.Name;
                }
                else
                {
                    winner = awayTeam.TeamInfo.Name;
                    loser = homeTeam.TeamInfo.Name;
                }

            }

           


            List<GameEvent> gameEvents = new List<GameEvent>();

            var randomizer1 = new Random();
            var randomDouble1 = randomizer1.NextDouble();

            var result1 = Math.Floor(0 + (8 + 1 - 0) * (Math.Pow(randomDouble1, 2)));

            var randomizer2 = new Random();
            var randomDouble2 = randomizer2.NextDouble();

            var result2 = Math.Floor(0 + (8 + 1 - 0) * (Math.Pow(randomDouble2, 2)));
           
            //Process game scores
            if (result1 > result2)
            {
               
                if (winner == homeTeam.TeamInfo.Name)
                {
                    homeTeam.GamesPlayed += 1;
                    homeTeam.Wins += 1;
                    homeTeam.RegulationWins += 1;
                    homeTeam.Points += 1;

                    awayTeam.GamesPlayed += 1;
                    awayTeam.Loses += 1;

                    for (int i = 0; i < result1; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 712
                        });
                    }

                    for (int i = 0; i < result2; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 847
                        });
                    }

                    await _context.GameEvents.AddRangeAsync(gameEvents);

                }
                else
                {
                    awayTeam.GamesPlayed += 1;
                    awayTeam.Wins += 1;
                    awayTeam.RegulationWins += 1;
                    awayTeam.Points += 1;

                    homeTeam.GamesPlayed += 1;
                    homeTeam.Loses += 1;

                    for (int i = 0; i < result1; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 847
                        });
                    }

                    for (int i = 0; i < result2; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 712
                        });
                    }

                    await _context.GameEvents.AddRangeAsync(gameEvents);

                }
                _context.Teams.Update(homeTeam);
                _context.Teams.Update(awayTeam);
                await _context.SaveChangesAsync();

                return $"{winner}: {result1} {loser}: {result2}";
            }
            else if (result1 < result2)
            {
                

                if (winner == homeTeam.TeamInfo.Name)
                {
                    homeTeam.GamesPlayed += 1;
                    homeTeam.Wins += 1;
                    homeTeam.RegulationWins += 1;
                    homeTeam.Points += 1;

                    awayTeam.GamesPlayed += 1;
                    awayTeam.Loses += 1;

                    for (int i = 0; i < result2; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 712
                        });
                    }

                    for (int i = 0; i < result1; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 847
                        });
                    }

                    await _context.GameEvents.AddRangeAsync(gameEvents);
                }
                else
                {
                    awayTeam.GamesPlayed += 1;
                    awayTeam.Wins += 1;
                    awayTeam.RegulationWins += 1;
                    awayTeam.Points += 2;

                    homeTeam.GamesPlayed += 1;
                    homeTeam.Loses += 1;

                    for (int i = 0; i < result2; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 847
                        });
                    }

                    for (int i = 0; i < result1; i++)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            GameId = gameId,
                            PlayerId = 712
                        });
                    }

                    await _context.GameEvents.AddRangeAsync(gameEvents);
                }
                _context.Teams.Update(homeTeam);
                _context.Teams.Update(awayTeam);
                await _context.SaveChangesAsync();

                return $"{winner}: {result2} {loser}: {result1}";
            }
            else
            {
                return "Game draw!";
            }


        }

        // GET: SeasonController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SeasonController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SeasonController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SeasonController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
