using System;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(HockeyManager.Areas.Identity.IdentityHostingStartup))]
namespace HockeyManager.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<HockeyContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("HockeyContextConnection")));

                services.AddDefaultIdentity<User>()
                    .AddEntityFrameworkStores<HockeyContext>();
            });
        }
    }
}