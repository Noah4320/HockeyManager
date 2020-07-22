using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Data;
using HockeyManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult Index()
        {
            List<HMTeam> teams = new List<HMTeam>();

            teams = _context.Teams.ToList();

            return View(teams);
        }

        // GET: Search/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

      
    }
}