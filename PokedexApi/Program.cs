using PokedexApi.Gateways;
using PokedexApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IPokemonService, PokemonService>();
builder.Services.AddScoped<IPokemonGateway, PokemonGateway>();
builder.Services.AddScoped<ITrainerGateway, TrainerGateway>();
builder.Services.AddScoped<ITrainerService, TrainerService>();

builder.Services.AddSingleton(_ =>
{
    var channel = GrpcChannel.ForAddress(builder.Configuration.GetValue<string>("TrainerApiEndpoint"));
    return new PokedexApi.Infrastructure.Grpc.TrainerService.TrainerServiceClient(channel);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
            {
            options.Authority = builder.Configuration.GetValue<string>("Authentication:Authority");
            options.TokenValidationParameters = new TokenValidationParameters 
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                ValidateActor = false,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = "pokedex-api",
                ValidateIssuerSigningKey = true
            };
            options.RequireHttpsMetadata = false;
            });

builder.Services.AddAuthorization(options => 
        {
        options.AddPolicy("Read", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", 
                    "read"));
        options.AddPolicy("Write", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", 
                    "write"));
        });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
