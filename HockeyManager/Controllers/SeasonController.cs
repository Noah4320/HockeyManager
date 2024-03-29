﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Internal;
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
            DateTime startDate = new DateTime(DateTime.Now.Year, 10, 15);

            season.Date = startDate;
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
                    TeamInfoId = team.TeamInfoId,
                    SeasonId = season.Id
                });
            }

            await _context.Teams.AddRangeAsync(newlyGeneratedTeams);
            await _context.SaveChangesAsync();


            List<HMPlayer> newGeneratedPlayers = new List<HMPlayer>();
            var leaguePlayers = _context.Players.Include(x => x.Team.TeamInfo).Where(x => x.ApiId != 0);
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

            List<Game> games = new List<Game>();
            List<HMTeam> allTeams = new List<HMTeam>();
            allTeams.AddRange(newlyGeneratedTeams);
            allTeams.Add(myTeam);



            if (allTeams.Count % 2 != 0)
            {
                allTeams.Add(new HMTeam
                {

                    TeamInfo = new HMTeamInfo { Name = "Bye" }

                });
            }

            int numDays = (allTeams.Count - 1);
            int halfSize = allTeams.Count / 2;

            List<HMTeam> teams = new List<HMTeam>();

            teams.AddRange(allTeams.Skip(halfSize).Take(halfSize));
            teams.AddRange(allTeams.Skip(1).Take(halfSize - 1).ToArray().Reverse());

            int teamsSize = teams.Count;

            for (int day = 0; day < numDays; day++)
            {

                int teamIdx = day % teamsSize;

                games.Add(new Game
                {
                    Date = startDate.AddDays(day),
                    Complete = false,
                    AwayTeamId = teams[teamIdx].Id,
                    HomeTeamId = allTeams[0].Id               
                });

                for (int idx = 1; idx < halfSize; idx++)
                {
                    int firstTeam = (day + idx) % teamsSize;
                    int secondTeam = (day + teamsSize - idx) % teamsSize;
                    games.Add(new Game
                    {
                        Date = startDate.AddDays(day),
                        Complete = false,
                        AwayTeamId = teams[firstTeam].Id,
                        HomeTeamId = teams[secondTeam].Id                 
                    });
                }
            }

            await _context.Games.AddRangeAsync(games);
            await _context.SaveChangesAsync();

            return season.Id.ToString();
        }

        // GET: SeasonController/Hub/5
        public ActionResult Hub(int id)
        {
            var teams = _context.Teams.Include(x => x.TeamInfo).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Where(x => x.SeasonId == id).OrderByDescending(x => x.Points).ToList();
            var myTeam = _context.Teams.Include(x => x.HomeSchedule).Where(x => x.SeasonId == id && x.UserId != null).FirstOrDefault();
            var season = _context.Seasons.FirstOrDefault(x => x.Id == id);

            SeasonsViewModel VMteams = new SeasonsViewModel();
            VMteams.Teams = teams;
            VMteams.MyTeam = myTeam;
            VMteams.Season = season;

            if (VMteams.Season == null)
            {
                return RedirectToAction("Index", "Season");
            }

            return View(VMteams);
        }

        [HttpGet]
        public ActionResult ApplyStandingsFilter (int seasonId, string filter)
        {

            var teams = _context.Teams.Include(x => x.TeamInfo).Where(x => x.SeasonId == seasonId);

            if (filter == "Eastern")
            {
                teams = teams.Where(x => x.Conference == "Eastern");
            }
            else if (filter == "Western")
            {
                teams = teams.Where(x => x.Conference == "Western");
            }
            else if (filter == "Atlantic")
            {
                teams = teams.Where(x => x.Division == "Atlantic");
            }
            else if (filter == "Metro")
            {
                teams = teams.Where(x => x.Division == "Metropolitan");
            }
            else if (filter == "Central")
            {
                teams = teams.Where(x => x.Division == "Central");
            }
            else if (filter == "Pacific")
            {
                teams = teams.Where(x => x.Division == "Pacific");
            } 

            return PartialView("_Standings", teams.OrderByDescending(x => x.Points).ToList());
        }

        [HttpGet]
        public ActionResult GetCalendarData(int seasonId)
        {
            List<CalendarGames> calendarGames = new List<CalendarGames>();

            try
            {
                var games = _context.Games.Include(x => x.HomeTeam.TeamInfo).Include(x => x.AwayTeam.TeamInfo)
                    .Include(x => x.GameEvents).ThenInclude(x => x.Player)
                    .Where(x => x.HomeTeam.SeasonId == seasonId).ToList();

                foreach (var game in games)
                {
                    bool hasPlayed = game.GameEvents.Count > 0;

                    if (hasPlayed)
                    {
                        calendarGames.Add(new CalendarGames
                        {
                            Id = game.Id,
                            Title = $"{game.AwayTeam.TeamInfo.Abbreviation} {game.GameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == game.AwayTeamId).Count()} {game.HomeTeam.TeamInfo.Abbreviation} {game.GameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == game.HomeTeamId).Count()}",
                            Description = "Game over",
                            StartDate = game.Date.ToString()
                        });
                    }
                    else
                    {
                        calendarGames.Add(new CalendarGames
                        {
                            Id = game.Id,
                            Title = $"{game.AwayTeam.TeamInfo.Abbreviation} @ {game.HomeTeam.TeamInfo.Abbreviation}",
                            Description = "Game hasn't been played",
                            StartDate = game.Date.ToString()
                        });
                    }
                }

                // Processing.  
                JsonResult result = Json(calendarGames);
                return result;
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }

            // Return info.  
            return null;
        }

        [HttpGet]
        public async Task SimToDate(int seasonId, string toDate)
        {
            //Check if it's their season
            if (!_context.Seasons.Where(x => x.Id == seasonId && x.UserId == _userManager.GetUserId(User)).Any())
            {
                return;
            }

            var dateClicked = DateTime.Parse(toDate);
            var games = await _context.Games.Include(x => x.GameEvents).Include(x => x.HomeTeam.TeamInfo).Include(x => x.AwayTeam.TeamInfo).Where(x => x.HomeTeam.SeasonId == seasonId && x.Date < dateClicked).OrderBy(x => x.Date).ToListAsync();

            //Check if there are any games
            if (games.Count > 0)
            {
                foreach (var game in games)
                {
                    if (game.GameEvents.Count == 0)
                    {
                        bool isFinished = false;
                        while (!isFinished)
                        {
                            isFinished = await Simulate(game.Id, 3, 1, 15);
                        }
                    }

                }
                var currentSeason = _context.Seasons.Where(x => x.Id == seasonId).FirstOrDefault();
                
                //only update the date if they clicked a future date and not a past date
                if (dateClicked > currentSeason.Date)
                {
                    currentSeason.Date = games.Last().Date.AddDays(1);
                    await _context.SaveChangesAsync();
                }
            }

            
        }

        [HttpGet]
        public bool CanSimGame(int seasonId, string date)
        {
            var dateClicked = DateTime.Parse(date);

            var gamesToDate = _context.Games.Include(x => x.HomeTeam).Include(x => x.GameEvents).Where(x => x.HomeTeam.SeasonId == seasonId && x.Date < dateClicked).ToList();
            var gamesPlayed = _context.Games.Include(x => x.HomeTeam).Include(x => x.GameEvents).Where(x => x.HomeTeam.SeasonId == seasonId && x.Date <= dateClicked && x.GameEvents.Count > 0).ToList();

            return gamesPlayed.Count() >= gamesToDate.Count();
        }

        // GET: SeasonController/SimGame/
        public ActionResult SimGame(int gameId)
        {

            var game = _context.Games.Include(x => x.HomeTeam.Players).ThenInclude(x => x.PlayerInfo)
                .Include(x => x.AwayTeam.Players).ThenInclude(x => x.PlayerInfo)
                .Include(x => x.HomeTeam.TeamInfo)
                .Include(x => x.AwayTeam.TeamInfo)
                .Include(x => x.GameEvents).ThenInclude(x => x.Player.PlayerInfo)
                .Include(x => x.GameEvents).ThenInclude(x => x.Player).ThenInclude(x => x.Team.TeamInfo)
                .Where(x => x.Id == gameId).FirstOrDefault();

            if (game == null)
            {
                return RedirectToAction("Index", "Season");
            }

            //Has this game been played?
            if (game.GameEvents.Count > 0)
            {
                game.HomeTeam.Players.ForEach(x => x.Goals = game.GameEvents.Where(y => y.Event == "Goal" && y.PlayerId == x.Id).Count());
                game.AwayTeam.Players.ForEach(x => x.Goals = game.GameEvents.Where(y => y.Event == "Goal" && y.PlayerId == x.Id).Count());

                game.HomeTeam.Players.ForEach(x => x.Points = game.GameEvents.Where(y => (y.Event == "Goal" || y.Event == "Assist") && y.PlayerId == x.Id).Count());
                game.AwayTeam.Players.ForEach(x => x.Points = game.GameEvents.Where(y => (y.Event == "Goal" || y.Event == "Assist") && y.PlayerId == x.Id).Count());

                game.HomeTeam.Players.ForEach(x => x.Shots = game.GameEvents.Where(y => y.Event == "Shot" && y.PlayerId == x.Id).Count());
                game.AwayTeam.Players.ForEach(x => x.Shots = game.GameEvents.Where(y => y.Event == "Shot" && y.PlayerId == x.Id).Count());

                return View(game);
            }

            game.HomeTeam.Players.ForEach(x => x.Goals = 0);
            game.HomeTeam.Players.ForEach(x => x.Assists = 0);
            game.HomeTeam.Players.ForEach(x => x.Points = 0);
            game.HomeTeam.Players.ForEach(x => x.PlusMinus = 0);
            game.HomeTeam.Players.ForEach(x => x.PowerPlayGoals = 0);
            game.HomeTeam.Players.ForEach(x => x.Shots = 0);
            game.HomeTeam.Players.ForEach(x => x.Saves = 0);
            game.HomeTeam.Players.ForEach(x => x.GoalsAgainst = 0);
            game.HomeTeam.Players.ForEach(x => x.Hits = 0);
            game.HomeTeam.Players.ForEach(x => x.Shutouts = 0);
            game.HomeTeam.Players.ForEach(x => x.TimeOnIce = 0);

            game.AwayTeam.Players.ForEach(x => x.Goals = 0);
            game.AwayTeam.Players.ForEach(x => x.Assists = 0);
            game.AwayTeam.Players.ForEach(x => x.Points = 0);
            game.AwayTeam.Players.ForEach(x => x.PlusMinus = 0);
            game.AwayTeam.Players.ForEach(x => x.PowerPlayGoals = 0);
            game.AwayTeam.Players.ForEach(x => x.Shots = 0);
            game.AwayTeam.Players.ForEach(x => x.Saves = 0);
            game.AwayTeam.Players.ForEach(x => x.GoalsAgainst = 0);
            game.AwayTeam.Players.ForEach(x => x.Hits = 0);
            game.AwayTeam.Players.ForEach(x => x.Shutouts = 0);
            game.AwayTeam.Players.ForEach(x => x.TimeOnIce = 0);

            return View(game);
        }


        [HttpGet]
        public async Task<ActionResult> GetSimulatedGame(int gameId, bool simFull)
        {
            bool isFinished = false;

            if (simFull)
            {
                while (!isFinished)
                {
                    isFinished = await Simulate(gameId, 3, 1, 15);
                }
            }
            else
            {
                isFinished = await Simulate(gameId, 3, 1, 15);
            }

            //get team instances
            var homeTeam = await _context.Games.Where(x => x.Id == gameId).Select(x => x.HomeTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefaultAsync();
            var awayTeam = await _context.Games.Where(x => x.Id == gameId).Select(x => x.AwayTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefaultAsync();

            var homeGoalie = homeTeam.Players.Where(x => x.Position == "G").FirstOrDefault();
            var awayGoalie = awayTeam.Players.Where(x => x.Position == "G").FirstOrDefault();

            var gameEvents = await _context.GameEvents.Where(x => x.GameId == gameId).ToListAsync();

            //Gather game stats
            Game finishedGame = new Game();
            finishedGame.HomeTeamId = homeTeam.Id;
            finishedGame.HomeTeam = homeTeam;
            finishedGame.AwayTeamId = awayTeam.Id;
            finishedGame.AwayTeam = awayTeam;
            finishedGame.GameEvents = gameEvents;
            finishedGame.Complete = isFinished;

            finishedGame.HomeTeam.Players.ForEach(x => x.Goals = gameEvents.Where(y => y.Event == "Goal" && y.PlayerId == x.Id && y.GameId == gameId).Count());
            finishedGame.AwayTeam.Players.ForEach(x => x.Goals = gameEvents.Where(y => y.Event == "Goal" && y.PlayerId == x.Id && y.GameId == gameId).Count());

            finishedGame.HomeTeam.Players.ForEach(x => x.Assists = gameEvents.Where(y => y.Event == "Assist" && y.PlayerId == x.Id && y.GameId == gameId).Count());
            finishedGame.AwayTeam.Players.ForEach(x => x.Assists = gameEvents.Where(y => y.Event == "Assist" && y.PlayerId == x.Id && y.GameId == gameId).Count());

            finishedGame.HomeTeam.Players.ForEach(x => x.Points = gameEvents.Where(y => (y.Event == "Goal" || y.Event == "Assist") && y.PlayerId == x.Id && y.GameId == gameId).Count());
            finishedGame.AwayTeam.Players.ForEach(x => x.Points = gameEvents.Where(y => (y.Event == "Goal" || y.Event == "Assist") && y.PlayerId == x.Id && y.GameId == gameId).Count());

            finishedGame.HomeTeam.Players.ForEach(x => x.Shots = gameEvents.Where(y => y.Event == "Shot" && y.PlayerId == x.Id && y.GameId == gameId).Count());
            finishedGame.AwayTeam.Players.ForEach(x => x.Shots = gameEvents.Where(y => y.Event == "Shot" && y.PlayerId == x.Id && y.GameId == gameId).Count());

            HMPlayer homeBackup = finishedGame.HomeTeam.Players.Where(x => x.Id != homeGoalie.Id && x.Position == "G").FirstOrDefault();
            HMPlayer awayBackup = finishedGame.AwayTeam.Players.Where(x => x.Id != awayGoalie.Id && x.Position == "G").FirstOrDefault();

            homeGoalie.Saves = gameEvents.Where(x => x.Event == "Shot" && x.Player.TeamId != homeGoalie.TeamId).Count() - gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId != homeGoalie.TeamId).Count();
            awayGoalie.Saves = gameEvents.Where(x => x.Event == "Shot" && x.Player.TeamId != awayGoalie.TeamId).Count() - gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId != awayGoalie.TeamId).Count();

            homeGoalie.GoalsAgainst = gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId != homeGoalie.TeamId && x.GameId == gameId).Count();
            awayGoalie.GoalsAgainst = gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId != awayGoalie.TeamId && x.GameId == gameId).Count();

            homeGoalie.SavePercentage = Math.Round(decimal.Divide((decimal)homeGoalie.Saves, (decimal)homeGoalie.Saves + (decimal)homeGoalie.GoalsAgainst), 3);
            awayGoalie.SavePercentage = Math.Round(decimal.Divide((decimal)awayGoalie.Saves, (decimal)awayGoalie.Saves + (decimal)awayGoalie.GoalsAgainst), 3);

            //Teams might not have a backup
            if (homeBackup != null)
            {
                homeBackup.Saves = 0;
                homeBackup.GoalsAgainst = 0;
            }

            if (awayBackup != null)
            {
                awayBackup.Saves = 0;
                awayBackup.GoalsAgainst = 0;
            }

            return PartialView("_SimStats", finishedGame);
        }

        public async Task<bool> Simulate(int gameId, int maxGoals, int minShots, int maxShots)
        {
            //get team instances
            var homeTeam = await _context.Games.Where(x => x.Id == gameId).Select(x => x.HomeTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefaultAsync();
            var awayTeam = await _context.Games.Where(x => x.Id == gameId).Select(x => x.AwayTeam).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).Include(x => x.TeamInfo).FirstOrDefaultAsync();

            var game = _context.Games.Where(x => x.Id == gameId).FirstOrDefault();

            //Check if it's their season
            if (!_context.Seasons.Where(x => x.Id == homeTeam.SeasonId && x.UserId == _userManager.GetUserId(User)).Any())
            {
                return true;
            }

            //Period check
            int period = 1;
            var wholeGameEvent = _context.GameEvents.Where(x => x.GameId == gameId);

            if (wholeGameEvent.LastOrDefault() != null)
            {
                period += wholeGameEvent.LastOrDefault().Period;
            }

            //randomly choose goalies
            Random r = new Random();
            double goalieOdds = r.NextDouble();

            int homeGoalieId;
            int awayGoalieId;

            if (goalieOdds < 0.75)
            {
                homeGoalieId = homeTeam.Players.OrderByDescending(x => x.Overall).First(x => x.Position == "G").Id;
                awayGoalieId = awayTeam.Players.OrderByDescending(x => x.Overall).First(x => x.Position == "G").Id;
            }
            else
            {
                homeGoalieId = homeTeam.Players.OrderBy(x => x.Overall).First(x => x.Position == "G").Id;
                awayGoalieId = awayTeam.Players.OrderBy(x => x.Overall).First(x => x.Position == "G").Id;
            }

            //get roster stats from last year if less than 10 games played
            List<HMPlayer> homeRoster = new List<HMPlayer>();
            List<HMPlayer> awayRoster = new List<HMPlayer>();

            decimal homeGPGSum = 0;
            decimal awayGPGSum = 0;
            decimal homeSPGSum = 0;
            decimal awaySPGSum = 0;

            if (homeTeam.GamesPlayed < 10)
            {
                homeRoster = await _context.Players.Where(x => x.ApiId != 0 && homeTeam.Players.Select(y => y.PlayerInfoId).Contains(x.PlayerInfoId)).ToListAsync();
                homeGPGSum = homeRoster.Sum(x => decimal.Divide(x.Goals, x.GamesPlayed));
                homeSPGSum = homeRoster.Sum(x => decimal.Divide(x.Shots, x.GamesPlayed));
            }
            else
            {
                homeGPGSum = homeTeam.Players.Sum(x => decimal.Divide(x.Goals, x.GamesPlayed));
                homeSPGSum = homeTeam.Players.Sum(x => decimal.Divide(x.Shots, x.GamesPlayed));
                homeRoster = homeTeam.Players;
            }

            if (awayTeam.GamesPlayed < 10)
            {
                awayRoster = await _context.Players.Where(x => x.ApiId != 0 && awayTeam.Players.Select(y => y.PlayerInfoId).Contains(x.PlayerInfoId)).ToListAsync();
                awayGPGSum = awayRoster.Sum(x => decimal.Divide(x.Goals, x.GamesPlayed));
                awaySPGSum = awayRoster.Sum(x => decimal.Divide(x.Shots, x.GamesPlayed));
            }
            else
            {
                awayGPGSum = awayTeam.Players.Sum(x => decimal.Divide(x.Goals, x.GamesPlayed));
                awaySPGSum = awayTeam.Players.Sum(x => decimal.Divide(x.Shots, x.GamesPlayed));
                awayRoster = awayTeam.Players;
            }

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

            //Randomize scores
            var randomizer1 = new Random();
            var randomDouble1 = randomizer1.NextDouble();

            var result1 = Math.Floor(0 + (maxGoals + 1 - 0) * (Math.Pow(randomDouble1, 2)));

            var randomizer2 = new Random();
            var randomDouble2 = randomizer2.NextDouble();

            var result2 = Math.Floor(0 + (maxGoals + 1 - 0) * (Math.Pow(randomDouble2, 2)));

            double winnerScore = 0;
            double loserScore = 0;

            if (result1 > result2)
            {
                winnerScore = result1;
                loserScore = result2;
            }
            else
            {
                winnerScore = result2;
                loserScore = result1;
            }

            //Randomize shot totals
            int shotResult1 = randomizer1.Next(minShots, maxShots);
            int shotResult2 = randomizer1.Next(minShots, maxShots);

            int winnerShots = 0;
            int loserShots = 0;

            if (shotResult1 > shotResult2)
            {
                winnerShots = shotResult1;
                loserShots = shotResult2;
            }
            else
            {
                winnerShots = shotResult2;
                loserShots = shotResult1;
            }

            //Process game scores
            if (winner == homeTeam.TeamInfo.Name)
            {
                bool isDone = false;

                for (int i = 0; i < winnerScore; i++)
                {
                    gameEvents = PredictGoal(gameId, homeTeam.Players, awayTeam.Players, homeRoster, homeGPGSum, gameEvents, awayGoalieId, period);

                    if (period > 3)
                    {
                        isDone = true;
                        break;
                    }
                }

                for (int i = 0; i < loserScore; i++)
                {
                    if (isDone)
                    {
                        break;
                    }
                    gameEvents = PredictGoal(gameId, awayTeam.Players, homeTeam.Players, awayRoster, awayGPGSum, gameEvents, homeGoalieId, period);
                }


                for (int i = 0; i < winnerShots; i++)
                {
                    gameEvents = PredictShot(gameId, homeTeam.Players, awayTeam.Players, homeRoster, homeSPGSum, gameEvents, awayGoalieId, period);
                }

                for (int i = 0; i < loserShots; i++)
                {
                    gameEvents = PredictShot(gameId, awayTeam.Players, homeTeam.Players, awayRoster, awaySPGSum, gameEvents, homeGoalieId, period);
                }

                await _context.GameEvents.AddRangeAsync(gameEvents);

            }
            else if (winner == awayTeam.TeamInfo.Name)
            {
                bool isDone = false;

                for (int i = 0; i < winnerScore; i++)
                {
                    gameEvents = PredictGoal(gameId, awayTeam.Players, homeTeam.Players, awayRoster, awayGPGSum, gameEvents, homeGoalieId, period);

                    if (period > 3)
                    {
                        isDone = true;
                        break;
                    }
                }

                for (int i = 0; i < loserScore; i++)
                {
                    if (isDone)
                    {
                        break;
                    }

                    gameEvents = PredictGoal(gameId, homeTeam.Players, awayTeam.Players, homeRoster, homeGPGSum, gameEvents, awayGoalieId, period);
                }


                for (int i = 0; i < winnerShots; i++)
                {
                    gameEvents = PredictShot(gameId, awayTeam.Players, homeTeam.Players, awayRoster, awaySPGSum, gameEvents, homeGoalieId, period);
                }

                for (int i = 0; i < loserShots; i++)
                {
                    gameEvents = PredictShot(gameId, homeTeam.Players, awayTeam.Players, homeRoster, homeSPGSum, gameEvents, awayGoalieId, period);
                }

                await _context.GameEvents.AddRangeAsync(gameEvents);
            }

            //Check if game is over
            int homeTotalGoals = gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == homeTeam.Id).Count() + wholeGameEvent.Where(x => x.Event == "Goal" && x.Player.TeamId == homeTeam.Id).Count();
            int awayTotalGoals = gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == awayTeam.Id).Count() + wholeGameEvent.Where(x => x.Event == "Goal" && x.Player.TeamId == awayTeam.Id).Count();
            int currentPeriod = gameEvents.Last().Period;

            bool isFinished = homeTotalGoals != awayTotalGoals && currentPeriod >= 3;

            if (isFinished)
            {
                //Did the game go in overtime
                if (currentPeriod > 3)
                {
                    if (homeTotalGoals > awayTotalGoals)
                    {
                        homeTeam.GamesPlayed += 1;
                        homeTeam.Wins += 1;
                        homeTeam.Points += 2;

                        awayTeam.GamesPlayed += 1;
                        awayTeam.Loses += 1;
                        awayTeam.Points += 1;
                        awayTeam.OvertimeLoses += 1;

                    }
                    else
                    {
                        awayTeam.GamesPlayed += 1;
                        awayTeam.Wins += 1;
                        awayTeam.RegulationWins += 1;
                        awayTeam.Points += 2;

                        homeTeam.GamesPlayed += 1;
                        homeTeam.Loses += 1;
                        homeTeam.Points += 1;
                        homeTeam.OvertimeLoses += 1;
                    }
                }
                else
                {
                    if (homeTotalGoals > awayTotalGoals)
                    {
                        homeTeam.GamesPlayed += 1;
                        homeTeam.Wins += 1;
                        homeTeam.RegulationWins += 1;
                        homeTeam.Points += 2;

                        awayTeam.GamesPlayed += 1;
                        awayTeam.Loses += 1;

                    }
                    else
                    {
                        awayTeam.GamesPlayed += 1;
                        awayTeam.Wins += 1;
                        awayTeam.RegulationWins += 1;
                        awayTeam.Points += 2;

                        homeTeam.GamesPlayed += 1;
                        homeTeam.Loses += 1;
                    }
                }

               

                var homeGoalie = homeTeam.Players.Find(x => x.Id == homeGoalieId);
                var awayGoalie = awayTeam.Players.Find(x => x.Id == awayGoalieId);

                if (gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == homeGoalie.TeamId && x.GameId == gameId).Count() == 0)
                {
                    awayGoalie.Shutouts += 1;
                }

                if (gameEvents.Where(x => x.Event == "Goal" && x.Player.TeamId == awayGoalie.TeamId && x.GameId == gameId).Count() == 0)
                {
                    homeGoalie.Shutouts += 1;
                }

                homeTeam.Players.ForEach(x => x.GamesPlayed += 1);
                awayTeam.Players.ForEach(x => x.GamesPlayed += 1);

                game.Complete = true;
            }

            await _context.SaveChangesAsync();
            return isFinished;
        }


        public List<GameEvent> PredictGoal(int gameId, List<HMPlayer> teamPlayers, List<HMPlayer> opponents, List<HMPlayer> oldRoster, decimal teamGPGSum, List<GameEvent> gameEvents, int opponentGoalieId, int period)
        {
            Random r = new Random();
            double rnd = r.NextDouble();
            int primaryAssist = r.Next(0, 4);
            int secondaryAssist = r.Next(0, 4);
            try
            {
                foreach (var player in teamPlayers)
                {
                    decimal goalsPerGame = 0;
                    if (player.GamesPlayed < 10)
                    {
                        var oldStats = oldRoster.Where(x => x.PlayerInfoId == player.PlayerInfoId).FirstOrDefault();

                        if (oldStats.Goals > 1)
                        {
                            goalsPerGame = (decimal.Divide(oldStats.Goals, oldStats.GamesPlayed)) / teamGPGSum;
                        } else
                        {
                            goalsPerGame = 0m;
                        }
                        
                    }
                    else
                    {
                        if (player.Goals > 1)
                        {
                            goalsPerGame = (decimal.Divide(player.Goals, player.GamesPlayed)) / teamGPGSum;
                        }
                        else
                        {
                            goalsPerGame = 0m;
                        }
                       
                    }

                    if (rnd < (double)goalsPerGame)
                    {
                        var nearestPlayerSkill = teamPlayers.OrderBy(x => Math.Abs((long)x.Overall - player.Overall)).ToList();
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Goal",
                            Period = period,
                            GameId = gameId,
                            PlayerId = player.Id
                        });
                        player.Goals += 1;
                        player.Points += 1;
                        opponents.Find(x => x.Id == opponentGoalieId).GoalsAgainst += 1;
                        opponents.Find(x => x.Id == opponentGoalieId).Saves -= 1;

                        if (primaryAssist > 0)
                        {
                            gameEvents.Add(new GameEvent
                            {
                                Event = "Assist",
                                Period = period,
                                GameId = gameId,
                                PlayerId = nearestPlayerSkill[primaryAssist].Id
                            });

                            nearestPlayerSkill[primaryAssist].Assists += 1;
                            nearestPlayerSkill[primaryAssist].Points += 1;

                            if (secondaryAssist > 0 && secondaryAssist != primaryAssist)
                            {
                                gameEvents.Add(new GameEvent
                                {
                                    Event = "Assist",
                                    Period = period,
                                    GameId = gameId,
                                    PlayerId = nearestPlayerSkill[secondaryAssist].Id
                                });

                                nearestPlayerSkill[secondaryAssist].Assists += 1;
                                nearestPlayerSkill[secondaryAssist].Points += 1;
                            }

                        }

                        break;
                    }

                    rnd -= (double)goalsPerGame;
                }
            }
            catch (InvalidOperationException ex)
            {
                var test = ex.Message;
            }

            return gameEvents;
        }

        public List<GameEvent> PredictShot(int gameId, List<HMPlayer> teamPlayers, List<HMPlayer> opponents, List<HMPlayer> oldRoster, decimal teamSPGSum, List<GameEvent> gameEvents, int opponentGoalieId, int period)
        {
            Random r = new Random();
            double rnd = r.NextDouble();

            try
            {
                foreach (var player in teamPlayers)
                {
                    decimal shotsPerGame = 0;
                    if (player.GamesPlayed < 10)
                    {
                        var oldStats = oldRoster.Where(x => x.PlayerInfoId == player.PlayerInfoId).FirstOrDefault();
                        shotsPerGame = (decimal.Divide(oldStats.Shots, oldStats.GamesPlayed)) / teamSPGSum;
                    }
                    else
                    {
                        shotsPerGame = (decimal.Divide(player.Shots, player.GamesPlayed)) / teamSPGSum;
                    }

                    if (rnd < (double)shotsPerGame)
                    {
                        gameEvents.Add(new GameEvent
                        {
                            Event = "Shot",
                            Period = period,
                            GameId = gameId,
                            PlayerId = player.Id
                        });
                        player.Shots += 1;
                        opponents.Find(x => x.Id == opponentGoalieId).Saves += 1;
                        break;
                    }

                    rnd -= (double)shotsPerGame;
                }
            }
            catch (InvalidOperationException ex)
            {
                var test = ex.Message;
            }

            return gameEvents;
        }

        [HttpGet]
        public async Task<ActionResult> ApplyEventFilter(int gameId, string team, string gameEvent, int period)
        {
            if (team == null)
            {
                team = "";
            }

            if (gameEvent == null)
            {
                gameEvent = "";
            }


            var test = await _context.GameEvents.Include(x => x.Player.PlayerInfo)
                .Include(x => x.Player.Team.TeamInfo)
                .Where(x => x.GameId == gameId && x.Player.Team.TeamInfo.Name.Contains(team) && x.Event.Contains(gameEvent)).ToListAsync();


            if (period > 0)
            {
                var filteredGameEvents = await _context.GameEvents.Include(x => x.Player.PlayerInfo)
                    .Include(x => x.Player.Team.TeamInfo)
                    .Where(x => x.GameId == gameId && x.Player.Team.TeamInfo.Name.Contains(team) && x.Event.Contains(gameEvent) && x.Period == period).ToListAsync();

                return PartialView("_GameEvents", filteredGameEvents);
            } else
            {
                var filteredGameEvents = await _context.GameEvents.Include(x => x.Player.PlayerInfo)
                    .Include(x => x.Player.Team.TeamInfo)
                    .Where(x => x.GameId == gameId && x.Player.Team.TeamInfo.Name.Contains(team) && x.Event.Contains(gameEvent)).ToListAsync();

                return PartialView("_GameEvents", filteredGameEvents);
            }

           
        }

        [HttpDelete]
        public async Task DeleteSeason(int seasonId)
        {
            string userId = _userManager.GetUserId(User);
            var seasonOwner = _context.Seasons.Where(x => x.Id == seasonId && x.UserId == userId).Any();
            

            if (!seasonOwner)
            {
                return;
            }

            var allEvents = await _context.GameEvents.Include(x => x.Player.Team).Where(x => x.Player.Team.SeasonId == seasonId).ToListAsync();
            var allGames = await _context.Games.Include(x => x.HomeTeam).Where(x => x.HomeTeam.SeasonId == seasonId).ToListAsync();
            var allPlayers = await _context.Players.Include(x => x.Team).Where(x => x.Team.SeasonId == seasonId).ToListAsync();
            var allTeams = await _context.Teams.Where(x => x.SeasonId == seasonId).ToListAsync();
            var teamInfo = await _context.Teams.Where(x => x.SeasonId == seasonId && x.UserId == userId).Select(x => x.TeamInfo).FirstOrDefaultAsync();
            var season = await _context.Seasons.FirstOrDefaultAsync(x => x.Id == seasonId);

            _context.GameEvents.RemoveRange(allEvents);
            await _context.SaveChangesAsync();

            _context.Games.RemoveRange(allGames);
            await _context.SaveChangesAsync();

            _context.Players.RemoveRange(allPlayers);
            await _context.SaveChangesAsync();

            _context.Teams.RemoveRange(allTeams);
            await _context.SaveChangesAsync();

            _context.TeamInfo.Remove(teamInfo);
            await _context.SaveChangesAsync();

            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();

        }
    }
}
