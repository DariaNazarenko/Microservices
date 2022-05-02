using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using(var serviseScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviseScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext context, bool isProd)
        {
            if(isProd)
            {
                System.Console.WriteLine("Attempting to apply migrations!");
                try
                {
                    context.Database.Migrate();
                }
                catch(Exception e)
                {
                    System.Console.WriteLine($"Could not run migrations {e.Message}");
                }
            }
           
            if(!context.Platforms.Any())
            {
                System.Console.WriteLine("Seeding data");

                context.Platforms.AddRange(
                    new Platform() {Name = "Dot Net", Publisher = "Microsoft", Cost = "Free"},
                    new Platform() {Name = "Sql Server", Publisher = "Microsoft", Cost = "Free"},
                    new Platform() {Name = "Kubernets", Publisher = "Cloud Native Computing Foundation", Cost = "Free"}
                );

                context.SaveChanges();
            }
            else
            {
                System.Console.WriteLine("--> we are already have data");
            }
        }
    }
}