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
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.Rank == 0 && x.ApiId != 0).ToList());

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
                        TeamId = team.Id,
                        PlayerInfoId = player.PlayerInfoId
                    });
                }
            }

            await _context.Players.AddRangeAsync(newGeneratedPlayers);
            await _context.SaveChangesAsync();

            return season.Id.ToString();
        }

        // GET: SeasonController/Hub/5
        public ActionResult Hub(int id)
        {
            return View();
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
