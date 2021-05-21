using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;


namespace GrpcProduct
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    // webBuilder.ConfigureKestrel(o =>
                    // {
                    //     o.ConfigureHttpsDefaults(o =>
                    //         o.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
                    // });

                     //.UseKestrel(options => options.ListenLocalhost(5000, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2)); 
                    //.UseKestrel(options => options.ConfigureEndpoints());
                    // .UseKestrel(options => 
                    //     {
                    //          // Configure the Url and ports to bind to
                    //          // This overrides calls to UseUrls and the ASPNETCORE_URLS environment variable, but will be 
                    //          // overridden if you call UseIisIntegration() and host behind IIS/IIS Express
                    //           options.Listen(System.Net.IPAddress.Any, 5001);
                    //           options.Listen(System.Net.IPAddress.Any, 5002, listenOptions =>
                    //           {
                    //               listenOptions.UseHttps("localhost.pfx", "123");
                    //           });

                    //          //options.ConfigureEndpoints()
                    //     });
                });


    }




    public static class KestrelServerOptionsExtensions
{
    public static void ConfigureEndpoints(this Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions options)
    {
        var configuration = options.ApplicationServices.GetService(typeof(IConfiguration));
        var environment = (IWebHostEnvironment)options.ApplicationServices.GetService(typeof(IWebHostEnvironment));

        var endpoints = ((IConfiguration)configuration).GetSection("HttpServer:Endpoints")
            .GetChildren()
            .ToDictionary(section => section.Key, section =>
            {
                var endpoint = new EndpointConfiguration(); 
                section.Bind(endpoint); 
                return endpoint;
            });

        foreach (var endpoint in endpoints)
        {
            var config = endpoint.Value;
            var port = config.Port ?? (config.Scheme == "https" ? 443 : 80);

            var ipAddresses = new List<System.Net.IPAddress>();
            if (config.Host == "localhost")
            {
                ipAddresses.Add(System.Net.IPAddress.IPv6Loopback);
                ipAddresses.Add(System.Net.IPAddress.Loopback);
            }
            else if (System.Net.IPAddress.TryParse(config.Host, out var address))
            {
                ipAddresses.Add(address);
            }
            else
            {
                ipAddresses.Add(System.Net.IPAddress.IPv6Any);
            }

            foreach (var address in ipAddresses)
            {
                options.Listen(address, port,
                    listenOptions =>
                    {
                        if (config.Scheme == "https")
                        {
                            var certificate = LoadCertificate(config, environment);
                            listenOptions.UseHttps(certificate);
                        }
                    });
            }
        }
    }



        private static System.Security.Cryptography.X509Certificates.X509Certificate2 LoadCertificate(EndpointConfiguration config, IWebHostEnvironment environment)
        {
            if (config.StoreName != null && config.StoreLocation != null)
            {
                using (var store = new System.Security.Cryptography.X509Certificates.X509Store(config.StoreName, Enum.Parse<StoreLocation>(config.StoreLocation)))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certificate = store.Certificates.Find(
                        X509FindType.FindBySubjectName,
                        config.Host,
                        validOnly: !environment.IsDevelopment());

                    if (certificate.Count == 0)
                    {
                        throw new InvalidOperationException($"Certificate not found for {config.Host}.");
                    }

                    return certificate[0];
                }
            }

            if (config.FilePath != null && config.Password != null)
            {
                return new X509Certificate2(config.FilePath, config.Password);
            }

            throw new InvalidOperationException("No valid certificate configuration found for the current endpoint.");
        }



}



    public class EndpointConfiguration
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Scheme { get; set; }
        public string StoreName { get; set; }
        public string StoreLocation { get; set; }
        public string FilePath { get; set; }
        public string Password { get; set; }
    }




}
