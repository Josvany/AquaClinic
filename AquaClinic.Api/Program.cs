using AquaClinic.Api;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("AquaClinic API iniciando");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
      (context, services, configuration) => configuration
          .ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext()
          .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"),
      true
  );

var app = builder
       .ConfigureServices()
       .ConfigurePipeline();

app.UseSerilogRequestLogging();

// En el entorno de pruebas de integracion la base de datos la prepara el
// propio proyecto de tests (InMemory), asi que no se migra aqui.
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.MigrateAndSeedAsync();
}

app.Run();

public partial class Program { }