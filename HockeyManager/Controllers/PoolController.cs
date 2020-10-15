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
            var enrolledPool = _context.PoolList.Where(x => x.PoolId == id && _userManager.GetUserId(User) == x.UserId).FirstOrDefault();

            var pool = _context.Pools.Where(x => x.Id == id).Include(x => x.Teams).ThenInclude(x => x.User)
                .Include(x => x.Teams).ThenInclude(x => x.TeamInfo).FirstOrDefault();

            if (pool == null || enrolledPool == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var hasTeam = _context.Teams.Where(x => x.UserId == _userManager.GetUserId(User) && x.PoolId == pool.Id).Any();

            if (hasTeam)
            {
                ViewBag.hasTeam = true;
            }

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

            var pool = _context.Pools.Include(x => x.RuleSet).Where(x => x.Id == id).FirstOrDefault();
            ViewBag.maxForwards = pool.RuleSet.maxForwards;
            ViewBag.maxDefencemen = pool.RuleSet.maxDefensemen;
            ViewBag.maxGoalies = pool.RuleSet.maxGoalies;
            ViewBag.maxPlayers = pool.RuleSet.maxPlayers;

            var teamName = _context.Teams.Include(x => x.TeamInfo).Where(x => x.PoolId == id && x.UserId == _userManager.GetUserId(User)).Select(x => x.TeamInfo.Name).FirstOrDefault();
            ViewBag.teamName = teamName;

            return View(VMplayers);
        }

        //GET
        public ActionResult GetSelectedPlayers(string team, string position, string favourite, string selectedPlayerIds)
        {
            if (team == null) { team = ""; }
            if (position == null) { position = ""; }
            int[] parsedPlayerIds;
            if (selectedPlayerIds == null)
            {
                parsedPlayerIds = new int[1];
                parsedPlayerIds[0] = -1;
            }
            else
            {
                var playerIds = selectedPlayerIds.Split(",");
                playerIds = playerIds.Skip(1).ToArray();
                parsedPlayerIds = Array.ConvertAll(playerIds, s => int.Parse(s));
            }
           

            if (favourite == "Yes")
            {
                var results = _context.Favourites.Where(x => x.UserId == _userManager.GetUserId(User)).Select(x => x.Player).Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.PlayerInfo.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).ToList();
                return PartialView("_PlayerData", results);
            }
            else if (favourite == "No")
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0 && x.PlayerInfo.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).Include(x => x.Favourites).ToList();
                results.RemoveAll(x => x.Favourites.Select(y => y.UserId).Contains(_userManager.GetUserId(User)));
                return PartialView("_PlayerData", results);
            }
            else
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0 && x.PlayerInfo.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).ToList();
                return PartialView("_PlayerData", results);
            }
        }

        [HttpGet]
        public ActionResult GetPoolTeam(int id)
        {
            var result = _context.Players.Include(x => x.Team).Include(x => x.PlayerInfo).Where(x => x.Team.UserId == _userManager.GetUserId(User) && x.Team.PoolId == id).ToList();
            return PartialView("_RosterData", result);
        }


        [HttpPost]
        public async Task<string> AddOrUpdateTeam(int id, string name, string[] players)
        {
            var anyTeam = _context.Teams.Include(x => x.TeamInfo).Where(x => x.PoolId == id && x.UserId == _userManager.GetUserId(User));
           
            var enrolledPool = _context.PoolList.Where(x => x.PoolId == id && _userManager.GetUserId(User) == x.UserId).FirstOrDefault();
            if (enrolledPool == null)
            {
                return "You're not enrolled in this pool!!";
            }

            if (name == null)
            {
                return "Please enter a team name.";
            }

            var ruleId = _context.Pools.Where(x => x.Id == id).FirstOrDefault().RuleSetId;
            if (ruleId == null)
            {
                return "Something went wrong..";
            }

            var rule = _context.RuleSets.Where(x => x.Id == ruleId).FirstOrDefault();

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
                if (forwards > rule.maxForwards)
                {
                    return "Too many forwards!";
                }
                else if (defencemen > rule.maxDefensemen)
                {
                    return "Too many defencemen!";
                }
                else if (goalies > rule.maxGoalies)
                {
                    return "Too many goalies!";
                }

            }
            //Check if team is too big or too small
            if ((forwards + defencemen + goalies) > rule.maxPlayers)
            {
                return "Too many players!";
            }
            else if ((forwards + defencemen + goalies) < rule.maxPlayers)
            {
                return $"{rule.maxPlayers - (forwards + defencemen + goalies)} more players are required.";
            }

            //All validation succeeded

            HMTeam team = new HMTeam();

            if (anyTeam.Any())
            {
                //Update
                team = anyTeam.FirstOrDefault();

                if (team.TeamInfo.Name != name)
                {
                    team.TeamInfo.Name = name;
                    _context.TeamInfo.Attach(team.TeamInfo);
                    _context.Entry(team.TeamInfo).Property(x => x.Name).IsModified = true;
                    await _context.SaveChangesAsync();
                }

                players = players.Skip(1).ToArray();
                int[] newPlayers = Array.ConvertAll(players, s => int.Parse(s));
                var oldPlayers = _context.Players.Include(x => x.Team).Include(x => x.PlayerInfo).Where(x => x.Team.PoolId == id && x.Team.UserId == _userManager.GetUserId(User)).Select(x => x.PlayerInfo.Id).ToArray();
                var sameTeam = Enumerable.SequenceEqual(oldPlayers, newPlayers);

                if (sameTeam)
                {
                    return "name updated";
                }

                var existingPlayers = _context.Players.Include(x => x.Team).Where(x => x.Team.Id == anyTeam.FirstOrDefault().Id).ToList();
                _context.Players.RemoveRange(existingPlayers);
                await _context.SaveChangesAsync();

            } 
            else
            {
                //Add
                HMTeamInfo teamInfo = new HMTeamInfo();
                teamInfo.Name = name;

                await _context.TeamInfo.AddAsync(teamInfo);
                await _context.SaveChangesAsync();

                
                team.TeamInfoId = teamInfo.Id;
                team.PoolId = id;
                team.UserId = _userManager.GetUserId(User);

                await _context.Teams.AddAsync(team);
                await _context.SaveChangesAsync();
            }
     
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
            var currentSize = _context.Teams.Where(x => x.PoolId == pool.Id).Count();
            if (pool.Size < currentSize)
            {
                //ViewBag.SizeError = "Size is too small.";
                return RedirectToAction(nameof(Edit));
            }

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

        [HttpDelete]
        public async Task LeavePool(int id)
        {
            var players = _context.Players.Include(x => x.Team.Pool).Where(x => x.Team.Pool.Id == id && x.Team.UserId == _userManager.GetUserId(User)).ToList();
            _context.Players.RemoveRange(players);
            await _context.SaveChangesAsync();

            var team = _context.Teams.Include(x => x.Pool).Where(x => x.Pool.Id == id && x.UserId == _userManager.GetUserId(User)).FirstOrDefault();
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            var teamInfo = _context.TeamInfo.Where(x => x.Id == team.TeamInfoId).FirstOrDefault();
            _context.TeamInfo.Remove(teamInfo);
            await _context.SaveChangesAsync();

            var poolReference = _context.PoolList.Where(x => x.PoolId == id && x.UserId == _userManager.GetUserId(User)).FirstOrDefault();
            _context.PoolList.Remove(poolReference);
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        public async Task Archive(int poolId)
        {
            var pool = await _context.Pools.FindAsync(poolId);

            pool.Status = "Archived";

            _context.Pools.Update(pool);
            await _context.SaveChangesAsync();
        }

        // POST: Pool/Delete/5
        [HttpDelete]
        public async Task Delete(int poolId)
        {
            try
            {
                var allPlayers = _context.Teams.Where(x => x.PoolId == poolId).SelectMany(x => x.Players).ToList();
                _context.Players.RemoveRange(allPlayers);
                await _context.SaveChangesAsync();

                var allTeams = _context.Teams.Where(x => x.PoolId == poolId).ToList();
                _context.Teams.RemoveRange(allTeams);
                await _context.SaveChangesAsync();

                var allTeamInfos = _context.Teams.Where(x => x.PoolId == poolId).Select(x => x.TeamInfo).ToList();
                _context.TeamInfo.RemoveRange(allTeamInfos);
                await _context.SaveChangesAsync();

                var allUsers = _context.PoolList.Where(x => x.PoolId == poolId).ToList();
                _context.PoolList.RemoveRange(allUsers);
                await _context.SaveChangesAsync();

                var pool = _context.Pools.Find(poolId);
                _context.Pools.Remove(pool);
                await _context.SaveChangesAsync();

            }
            catch(Exception ex)
            {
                var test = ex;
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
            var pools = _context.Pools.Where(x => x.Private == false && x.Status == "Active").Select(x => x.Name).ToArray();

            return pools;
        }
    }
}