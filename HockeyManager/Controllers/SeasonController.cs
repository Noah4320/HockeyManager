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
            return View();
        }

        // GET: SeasonController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SeasonController/NewSeason
        public ActionResult NewSeason()
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.PoolId == null).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.Rank == 0 && x.ApiId != 0).ToList());

            //The player limits are hardcoded smh
            ViewBag.maxForwards = 12;
            ViewBag.maxDefencemen = 6;
            ViewBag.maxGoalies = 2;
            ViewBag.maxPlayers = 20;
            return View(VMplayers);
        }

        // POST: SeasonController/CreateSeason
        [HttpPost]
        public string CreateSeason(string name, string abbreviation, string[] players, string[] pacific, string[] central, string[] atlantic, string[] metropolitan)
        {

            if (name == null)
            {
                return "Please enter a team name!";
            }

            if (abbreviation == null || abbreviation.Length != 3)
            {
                return "abbreviation must be 3 characters!";
            }

            var hMPlayersInfo = _context.PlayerInfo.Where(x => players.Contains(x.Id.ToString())).ToList();

            var forwards = 0;
            var defencemen = 0;
            var goalies = 0;

            foreach (var player in hMPlayersInfo)
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

            /**

             HMTeamInfo teamInfo = new HMTeamInfo();

             teamInfo.Name = name;
             teamInfo.Abbreviation = abbreviation;
             teamInfo.Division = "";

             await _context.TeamInfo.AddAsync(teamInfo);
             await _context.SaveChangesAsync();


             HMTeam team = new HMTeam();

             team.TeamInfoId = teamInfo.Id;
             //team.SeasonId = "create id";
             team.UserId = _userManager.GetUserId(User);

             await _context.Teams.AddAsync(team);
             await _context.SaveChangesAsync();
             

             List<HMPlayer> hMPlayers = new List<HMPlayer>();

             foreach (var playerInfo in hMPlayersInfo)
             {
                 hMPlayers.Add(new HMPlayer
                 {
                     TeamId = team.Id,
                     PlayerInfoId = playerInfo.Id
                 });
             }

             await _context.Players.AddRangeAsync(hMPlayers);
             await _context.SaveChangesAsync();
            **/
            return "success";
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
