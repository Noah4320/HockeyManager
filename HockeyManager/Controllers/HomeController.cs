using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HockeyManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Newtonsoft.Json;
using HockeyManager.Data;

namespace HockeyManager.Controllers
{
    //Only members can access this page
    //[Authorize]
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

        public async Task<IActionResult> FetchApiData()
        {
            var url = "https://statsapi.web.nhl.com/api/v1/teams";
            var httpClient = HttpClientFactory.Create();
            var data = await httpClient.GetStringAsync(url);

            var teams = JsonConvert.DeserializeObject<Root>(data);

            List<HMTeam> hMTeams = new List<HMTeam>();

            foreach (var team in teams.teams)
            {
                hMTeams.Add(new HMTeam
                {
                    ApiId = team.id,
                    Name = team.name,
                    Conference = team.conference.name,
                    Division = team.division.name,
                    logoUrl = $"https://www-league.nhlstatic.com/images/logos/teams-current-primary-light/{team.id}.svg"
                });
            }

            if (_context.Teams.Count() == 0)
            {
                await _context.Teams.AddRangeAsync(hMTeams);
                await _context.SaveChangesAsync();
            }

            //https://statsapi.web.nhl.com/api/v1/teams/1/roster
            //https://nhl.bamcontent.com/images/headshots/current/168x168/8477474.jpg

            return View("Index");
        }
    }
}
