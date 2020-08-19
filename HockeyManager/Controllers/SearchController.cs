using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Controllers
{
    public class SearchController : Controller
    {
        private readonly HockeyContext _context;

        public SearchController(HockeyContext context)
        {
            _context = context;
        }

        // GET: Search
        public ActionResult Teams()
        {
            List<HMTeam> teams = new List<HMTeam>();

            teams = _context.Teams.ToList();

            return View(teams);
        }


        // GET: Search/Details/5
        public ActionResult Players(int id)
        {
            List<HMPlayer> roster = _context.Players
                .Include(c => c.Team)
                .Where(c => c.Team.Id == id).ToList();

            return View(roster);
        }

        public ActionResult SearchPlayers()
        {
            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.ToList(), _context.Players.ToList());

            return View(VMplayers);
        }

        [HttpPost]
        public ActionResult SearchPlayers(IFormCollection data)
        {

            string name = data["Name"];
            string position = data["Position"];

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

            filterPlayers = _context.Players.Where(x => x.Position.Contains(position) && x.Name.Contains(name) && teamFilter.Contains(x.Team.Abbreviation)).ToList();

            SearchPlayer VMplayers = new SearchPlayer(_context.Teams.ToList(), filterPlayers);

           

            return View(VMplayers);
        }

        [HttpPost]
        public void Post()
        {
             string[] favs = Request.Form["fav"];
             string[] nonFavs = Request.Form["nonFav"];
        }


    }
}