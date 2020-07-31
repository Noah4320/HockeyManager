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
        public ActionResult FilterPlayers(IFormCollection data)
        {
            return RedirectToAction("SearchPlayers");
        }


    }
}