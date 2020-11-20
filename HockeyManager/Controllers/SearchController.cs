using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Controllers
{
    public class SearchController : Controller
    {
        private readonly HockeyContext _context;
        private UserManager<User> _userManager;

        public SearchController(HockeyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Search
        public ActionResult Teams()
        {

            List<HMTeam> teams = new List<HMTeam>();


            teams = _context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderBy(x => x.TeamInfo.Name).ToList();

            return View(teams);
        }


        // GET: Search/Details/5
        public ActionResult Players(int id)
        {
            List<HMPlayer> roster = _context.Players
                .Include(x => x.PlayerInfo)
                .Include(x => x.Team.TeamInfo)
                .Where(x => x.Team.Id == id).ToList();

            return View(roster);
        }


        // GET: Search/Users
        public ActionResult Users()
        {
            List<User> users = new List<User>();
            return View(users);
        }

        [HttpPost]
        public ActionResult Users (string user)
        {
            var users = _context.Users.Where(x => x.UserName.Contains(user)).ToList();
            return View(users);
        }


        [HttpGet]
        public string[] GetUsers()
        {
            var usernames = _context.Users.Select(x => x.UserName).ToArray();

            return usernames;
        }


        // GET: Search/UserDetails/5
        public async Task<ActionResult> UserDetails(string username)
        {
            var user = await _context.Users.Include(x => x.PoolsOwned).ThenInclude(x => x.Pool).ThenInclude(x => x.Teams).ThenInclude(x => x.TeamInfo)
                .Include(x => x.Seasons).ThenInclude(x => x.Teams).ThenInclude(x => x.TeamInfo)
                .Include(x => x.Seasons).ThenInclude(x => x.Teams).ThenInclude(x => x.Players).ThenInclude(x => x.PlayerInfo)
                .Where(x => x.UserName == username).FirstOrDefaultAsync();
            return View(user);
        }

        public ActionResult SearchPlayers()
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderBy(x => x.TeamInfo.Name).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.ApiId != 0).ToList());

            return View(VMplayers);
        }

        [HttpPost]
        public async Task<ActionResult> SearchPlayers(IFormCollection data)
        {
            //ToDo: Known issue with duplicate favourites. Less code should fix this.
            var user = await _userManager.GetUserAsync(User);
            string name = data["Name"];
            string position = data["Position"];
            string favourite = data["Favourite"];

            List<string> teamFilter = new List<string>();

            foreach (var key in data.Keys)
            {
                string value = data[key];
                string[] values = value.Split(",");
                if (values[0] == "on" && key != "Name")
                {
                    teamFilter.Add(key);
                }
            }

            List<HMPlayer> filterPlayers = new List<HMPlayer>();

            if (favourite == "Yes")
            {
                filterPlayers = _context.Favourites.Where(x => x.UserId == user.Id).Select(x => x.Player).Include(x => x.PlayerInfo).Where(x => x.ApiId != 0 && x.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).ToList();
                SearchPlayer VMFavouritePlayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderBy(x => x.TeamInfo.Name).ToList(), filterPlayers);
                return View(VMFavouritePlayers);
            }
            else if (favourite == "No")
            {
                filterPlayers = _context.Players.Include(x => x.PlayerInfo).Where(x => x.ApiId != 0 && x.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).Include(x => x.Favourites).ToList();


                filterPlayers.RemoveAll(x => x.Favourites.Select(y => y.UserId).Contains(user.Id));
                SearchPlayer VMFavouritePlayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderBy(x => x.TeamInfo.Name).ToList(), filterPlayers);
                return View(VMFavouritePlayers);

            }
            else
            {
                filterPlayers = _context.Players.Include(x => x.PlayerInfo).Where(x => x.ApiId != 0 && x.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).ToList();
                SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderBy(x => x.TeamInfo.Name).ToList(), filterPlayers);
                return View(VMplayers);
            }

        }

        [HttpGet]
        public async Task<string[]> getFavourites()
        {
            var user = await _userManager.GetUserAsync(User);
            var players = _context.Favourites.Where(x => x.UserId == user.Id).Select(x => x.PlayerId).ToArray();
            string[] result = Array.ConvertAll(players, x => x.ToString());
            return result;
        }

        [HttpPost]
        public async Task Post()
        {
            var user = await _userManager.GetUserAsync(User);
            string[] favs = Request.Form["fav"];
            string[] nonFavs = Request.Form["nonFav"];
            List<Favourites> favourites = new List<Favourites>();

            foreach (var fav in favs)
            {
                //prevent duplicates
                var currentFavourites = _context.Favourites.Where(x => x.PlayerId == int.Parse(fav) && x.UserId == user.Id);
                if (!currentFavourites.Any())
                {
                    favourites.Add(new Favourites
                    {
                        PlayerId = int.Parse(fav),
                        UserId = user.Id
                    });
                }

            }

            foreach (var nonfav in nonFavs)
            {
                var deFavs = _context.Favourites.Where(x => x.PlayerId == int.Parse(nonfav) && x.UserId == user.Id);

                if (deFavs.Any())
                {
                    _context.Favourites.RemoveRange(deFavs.ToList());
                }
            }

            await _context.Favourites.AddRangeAsync(favourites);
            await _context.SaveChangesAsync();

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
                var results = _context.Favourites.Where(x => x.UserId == _userManager.GetUserId(User)).Select(x => x.Player).Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).ToList();
                return PartialView("_PlayerData", results);
            }
            else if (favourite == "No")
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0 && x.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).Include(x => x.Favourites).ToList();
                results.RemoveAll(x => x.Favourites.Select(y => y.UserId).Contains(_userManager.GetUserId(User)));
                return PartialView("_PlayerData", results);
            }
            else
            {
                var results = _context.Players.Include(x => x.PlayerInfo).Include(x => x.Team.TeamInfo).Where(x => x.Team.TeamInfo.Abbreviation.Contains(team) && x.ApiId != 0 && x.Position.Contains(position) && !parsedPlayerIds.Contains(x.PlayerInfo.Id)).ToList();
                return PartialView("_PlayerData", results);
            }
        }

    }
}