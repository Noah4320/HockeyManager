using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HockeyManager.Models;
using HockeyManager.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HockeyManager.Controllers
{

    public class HomeController : Controller
    {
        private readonly HockeyContext _context;


        public HomeController(HockeyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<ActionResult> ConfigurePlayerRanks()
        {
            var allPlayers = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 10 && (x.Position == "C" || x.Position == "LW" || x.Position == "RW")).ToListAsync();

            List<decimal> oldRanks = new List<decimal>();

            foreach (var player in allPlayers)
            {
                decimal gamesPlayed = Convert.ToDecimal(player.GamesPlayed);
                decimal goals = Convert.ToDecimal(player.Goals);
                decimal assists = Convert.ToDecimal(player.Assists);
                decimal points = Convert.ToDecimal(player.Points);

                decimal test = (0.75m * (goals / gamesPlayed) + 0.70m * (assists / gamesPlayed) + 0.20m * (points / gamesPlayed));
                oldRanks.Add(test);
            }

            
            decimal oldMin = oldRanks.Min();
            decimal oldMax = oldRanks.Max();
            decimal newMax = 99;
            decimal newMin = 65;

            int indexCount = 0;
            foreach (var player in allPlayers)
            {
                decimal oldValue = oldRanks[indexCount];
                player.Rank = (int)Math.Round((((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin, 0);
                _context.Players.Attach(player);
                _context.Entry(player).Property(x => x.Rank).IsModified = true;
                indexCount++;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
