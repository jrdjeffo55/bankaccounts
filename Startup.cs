﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;


namespace BankAccounts

{
    public class Startup

    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)

        {
	        services.AddSession();
            services.AddMvc();
            services.Configure<MySqlOptions>(Configuration.GetSection("DBInfo"));
            services.AddDbContext<bankaccountsContext>(options => options.UseMySql(Configuration["DBInfo:ConnectionString"]));

        }

        public Startup(IHostingEnvironment env)

        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)

        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
	        app.UseSession();

            app.UseMvc(routes =>

            {

                routes.MapRoute(

                    name: "default",

                    template: "{controller=Home}/{action=Index}/{id?}");

            });

        }

    }

}