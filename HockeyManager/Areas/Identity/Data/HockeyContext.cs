using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Data
{
    public class HockeyContext : IdentityDbContext<User>
    {
        public HockeyContext(DbContextOptions<HockeyContext> options)
            : base(options)
        {
            
        }

        public virtual DbSet<HMTeamInfo> TeamInfo { get; set; }
        public virtual DbSet<HMTeam> Teams { get; set; }
        public virtual DbSet<HMPlayerInfo> PlayerInfo { get; set; }
        public virtual DbSet<HMPlayer> Players { get; set; }
        public virtual DbSet<Favourites> Favourites { get; set; }
        public virtual DbSet<RuleSet> RuleSets { get; set; }
        public virtual DbSet<PoolList> PoolList { get; set; }
        public virtual DbSet<Pool> Pools { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
