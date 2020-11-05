using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Data;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Controllers
{
    //Only members can access this page
    [Authorize]
    public class MainController : Controller
    {

        private readonly HockeyContext _context;

        public MainController(HockeyContext context)
        {
            _context = context;
        }

        // GET: Main
        public ActionResult Index()
        {
            SearchPlayer players = new SearchPlayer(_context.Teams.Include(x => x.TeamInfo).Where(x => x.ApiId != 0).OrderByDescending(x => x.Points).ToList(), _context.Players.Include(x => x.PlayerInfo).Where(x => x.ApiId != 0).ToList());
            return View(players);
        }

    }
}