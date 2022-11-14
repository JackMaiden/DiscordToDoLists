using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;

namespace DiscordToDo;
public class Bot
{
    public static async Task BotTaskAsync(IConfigurationRoot config)
    {
        var discordToken = config.GetRequiredSection("token").Value;

        ToDoSingleton singleton = ToDoSingleton.Instance;

        singleton._filePath = config.GetRequiredSection("FilePath").Value;
        singleton._fileName = config.GetRequiredSection("FileName").Value;

        await singleton.Load();

        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = discordToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.GuildMessages | DiscordIntents.GuildMessageReactions | DiscordIntents.GuildMessageTyping,
        });

        var slash = discord.UseSlashCommands();

        slash.RegisterCommands<ToDoCommands>();

        await discord.ConnectAsync();

        while (true)
        {
            Console.WriteLine("Saving Triggered!");
            await singleton.Save();
            await Task.Delay(60000);
        }
        await Task.Delay(-1);
    }

}
