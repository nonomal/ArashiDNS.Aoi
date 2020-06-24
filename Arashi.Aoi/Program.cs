﻿using System;
using System.Net;
using Arashi.Azure;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Arashi.Aoi
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmd = new CommandLineApplication
                {Name = "Arashi.Aoi", Description = "ArashiDNS.Aoi - Simple Lightweight DNS over HTTPS Server"};

            cmd.HelpOption("-?|-h|--help");
            var ipOption = cmd.Option<string>("-l|--listen <IPEndPoint>", "Set listen ip address and port <127.0.0.1:2020>",
                CommandOptionType.SingleValue);
            var upOption = cmd.Option<string>("-up|--upstream <IPAddress>", "Set upstream ip address <8.8.8.8>",
                CommandOptionType.SingleValue);
            var cacheOption = cmd.Option("--cache", "Set enable caching", CommandOptionType.NoValue);
            var chinaListOption = cmd.Option("--chinalist", "Set enable chinalist", CommandOptionType.NoValue);
            var logOption = cmd.Option("--log", "Set enable log", CommandOptionType.NoValue);

            cmd.OnExecute(() =>
            {
                Console.WriteLine(cmd.Description);
                var ipEndPoint = ipOption.HasValue()
                    ? IPEndPoint.Parse(ipOption.Value())
                    : new IPEndPoint(IPAddress.Loopback, 2020);
                if (upOption.HasValue()) Config.UpStream = IPAddress.Parse(upOption.Value());
                Config.CacheEnable = cacheOption.HasValue();
                Config.ChinaListEnable = chinaListOption.HasValue();
                Config.LogEnable = logOption.HasValue();

                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(AppDomain.CurrentDomain.SetupInformation.ApplicationBase)
                    .ConfigureServices(services => services.AddRouting())
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = 1024;
                        options.Listen(ipEndPoint,
                            listenOptions => listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
                    })
                    .UseStartup<Startup>()
                    .Build();

                host.Run();
            });

            cmd.Execute(args);
        }
    }
}
