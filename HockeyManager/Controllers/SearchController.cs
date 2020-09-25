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


            teams = _context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).ToList();

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

        public ActionResult SearchPlayers()
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.Rank == 0 && x.ApiId != 0).ToList());

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
                filterPlayers = _context.Favourites.Where(x => x.UserId == user.Id).Select(x => x.Player).Include(x => x.PlayerInfo).Where(x => x.PlayerInfo.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).ToList();
                SearchPlayer VMFavouritePlayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).ToList(), filterPlayers);
                return View(VMFavouritePlayers);
            }
            else if (favourite == "No")
            {
                filterPlayers = _context.Players.Include(x => x.PlayerInfo).Where(x => x.PlayerInfo.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).Include(x => x.Favourites).ToList();


                filterPlayers.RemoveAll(x => x.Favourites.Select(y => y.UserId).Contains(user.Id));
                SearchPlayer VMFavouritePlayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).ToList(), filterPlayers);
                return View(VMFavouritePlayers);

            }
            else
            {
                filterPlayers = _context.Players.Include(x => x.PlayerInfo).Where(x => x.PlayerInfo.Position.Contains(position) && x.PlayerInfo.Name.Contains(name) && teamFilter.Contains(x.Team.TeamInfo.Abbreviation)).ToList();
                SearchPlayer VMplayers = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).ToList(), filterPlayers);
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


    }
}