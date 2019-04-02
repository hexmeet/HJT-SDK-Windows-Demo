#if AUTOTEST

using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyVideoWin.www
{
    public class WwwServer
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void StartServer()
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler((sender, workerE) =>
            {
                try
                {
                    new WebHostBuilder()
                   .UseKestrel()
                   .UseStartup<Startup>()
                   .UseUrls("http://0.0.0.0:35000")
                   .Build()
                   .Run();
                }
                catch(Exception e)
                {
                    log.InfoFormat("Failed to start www server, exception:{0}", e);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                        System.Windows.Application.Current.MainWindow.Close();
                    });
                }
            });

            bgWorker.RunWorkerAsync();
        }
    }

    public class Startup
    {
        //public void Configure(IApplicationBuilder app)
        //{
        //    app.Run(context => context.Response.WriteAsync("Hello World"));
        //}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            //services.AddMvc(mvcOptions => {
            //    //TODO: make extension method 
            //    var jFormatter = mvcOptions.OutputFormatters.FirstOrDefault(f => f.GetType() == typeof(JsonOutputFormatter)) as JsonOutputFormatter;
            //    jFormatter?.SupportedMediaTypes.Add("application/json");
            //    jFormatter?.SupportedMediaTypes.Add("application/status+json");
            //});
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}

#endif