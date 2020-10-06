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
using Newtonsoft.Json;

namespace HockeyManager.Controllers
{
    [Authorize]
    public class PoolController : Controller
    {

        private readonly HockeyContext _context;
        private UserManager<User> _userManager;

        public PoolController(HockeyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pool
        public async Task<ActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var pools = _context.PoolList.Where(x => x.UserId == user.Id).Include(x => x.Pool).ThenInclude(x => x.Teams).ThenInclude(x => x.User);
          
            PoolsViewModel poolsViewModel = new PoolsViewModel();
            poolsViewModel.poolList = pools;

            ViewBag.Message = TempData["Message"];

            return View(poolsViewModel);
        }

        // GET: Pool/Details/5
        public ActionResult Details(int id)
        {
            var pool = _context.Pools.Where(x => x.Id == id).Include(x => x.Teams).ThenInclude(x => x.User)
                .Include(x => x.Teams).ThenInclude(x => x.TeamInfo).FirstOrDefault();
            return View(pool);
        }

        [HttpGet]
        public string GetTeamRoster(int id)
        {
            var players = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.TeamId == id).ToList();

            var result = JsonConvert.SerializeObject(players);
            return result;
        }

        // GET: Pool/ManagePoolTeam?Id=5
        public ActionResult ManagePoolTeam(int id)
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.PoolId == null).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.Rank == 0 && x.ApiId != 0).ToList());

            return View(VMplayers);
        }

        //GET
        public ActionResult GetSelectedTeam(string team, string favourite)
        {
            if (team == null) { team = ""; }

          
            if (favourite == "Yes")
            {
                var results = _context.Favourites.Where(x => x.UserId == _userManager.GetUserId(User)).Select(x => x.Player).Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team)).ToList();
                return PartialView("_PlayerData", results);
            }
            else if (favourite == "No")
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0).Include(x => x.Favourites).ToList();
                results.RemoveAll(x => x.Favourites.Select(y => y.UserId).Contains(_userManager.GetUserId(User)));
                return PartialView("_PlayerData", results);
            }
            else
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0).ToList();
                return PartialView("_PlayerData", results);
            }
        }

        [HttpPost]
        public async Task<string> AddTeam(int poolId, string name, string[] players)
        {
            var anyTeam = _context.Teams.Where(x => x.PoolId == poolId && x.UserId == _userManager.GetUserId(User));
            if (anyTeam.Any())
            {
                return "Team already exists";
            }

            HMTeamInfo teamInfo = new HMTeamInfo();
            teamInfo.Name = name;

            await _context.TeamInfo.AddAsync(teamInfo);
            await _context.SaveChangesAsync();

            HMTeam team = new HMTeam();
            team.TeamInfoId = teamInfo.Id;
            team.PoolId = poolId;
            team.UserId = _userManager.GetUserId(User);

            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var hMPlayersInfo = _context.PlayerInfo.Where(x => players.Contains(x.Id.ToString())).ToList();
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
            return "success";
        }
        // GET: Pool/Create
        public ActionResult CreatePool()
        {
            var ruleSets = _context.RuleSets.ToList();
            return View(new PoolsViewModel { RuleSet = ruleSets});
        }

        // POST: Pool/CreatePool
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePool(Pool pool)
        {

            if (_context.Pools.Any(x => x.Name == pool.Name))
            {
                ViewBag.ErrorMessage = "This pool name already exists";
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            pool.Status = "Active";
            pool.OwnerId = user.Id;
            try
            {
                // TODO: Add insert logic here

                await _context.Pools.AddAsync(pool);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<string> JoinPool(string poolName)
        {
            PoolList joinPool = new PoolList();
            var pool = _context.Pools.FirstOrDefault(x => x.Name == poolName);

            if (pool == null)
            {
                return "Pool doesn't exist";
            }

            var isEnrolled = _context.PoolList.Where(x => x.PoolId == pool.Id && x.UserId == _userManager.GetUserId(User)).Any();
            if (isEnrolled)
            {
                return "You have already joined this pool!";
            }

            joinPool.PoolId = pool.Id;
            joinPool.UserId = _userManager.GetUserId(User);

            await _context.PoolList.AddAsync(joinPool);
            await _context.SaveChangesAsync();

            var poolTeams = _context.Teams.Where(x => x.PoolId == pool.Id);
            if (poolTeams.Count() >= pool.Size)
            {
                return "Pool is full!";
            }

            return "Pool joined successfully!";
        }


            // GET: Pool/Edit/
            public ActionResult Edit()
            {
            var ruleSets = _context.RuleSets.ToList();
            var pools = _context.Pools.Where(x => x.OwnerId == _userManager.GetUserId(User)).Include(x => x.RuleSet).ToList();

            //var count = _context.Teams.Where(x => x.)

            return View(new PoolsViewModel { RuleSet = ruleSets, Pools = pools });
            }

        // POST: Pool/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Pool pool)
        {
            try
            {
                _context.Pools.Attach(pool);
                _context.Entry(pool).Property(x => x.Private).IsModified = true;
                _context.Entry(pool).Property(x => x.RuleSetId).IsModified = true;
                _context.Entry(pool).Property(x => x.Size).IsModified = true;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Pool/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Pool/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public string GetRule()
        {
            int id = int.Parse(Request.Form["id"]);

            var result = _context.RuleSets.Where(x => x.Id == id).Select(x => x.Description);

            return result.First();
        }

        [HttpGet]
        public string[] GetPools()
        {
            var pools = _context.Pools.Where(x => x.Private == false).Select(x => x.Name).ToArray();

            return pools;
        }
    }
}