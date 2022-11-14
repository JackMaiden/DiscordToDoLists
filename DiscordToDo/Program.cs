using DiscordToDo;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", false)
               .AddEnvironmentVariables()
               .Build();

Bot.BotTaskAsync(config).GetAwaiter().GetResult(); //Run the Bot Task