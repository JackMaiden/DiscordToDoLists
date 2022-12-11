using DiscordToDo;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

public class ToDoCommands: ApplicationCommandModule
{
    [SlashCommand("ping", "A simple ping command to check bot is alive")]
    public async Task PingCommand(InteractionContext ctx)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Pong!"));
    }

    ToDoSingleton singleton = ToDoSingleton.Instance;
    
    [SlashCommand("createList", "creates a new ToDoList")]
    public async Task CreateTaskList(InteractionContext ctx,
        [Option("Title", "ToDoList Title")] string title,
        [Option("Description", "an optional description for the List")] string? description = "",
        [Option("includeUsers", "should user who added item be shown in list")] bool includeUsers = true)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List Already added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        var message = await ctx.GetOriginalResponseAsync();

        var item = new ToDoList(message.Id, title, ctx.Member.DisplayName, ctx.Member.GuildAvatarUrl ?? ctx.Member.AvatarUrl ?? ctx.Member.DefaultAvatarUrl, description, includeUsers);

        await ctx.EditFollowupAsync(message.Id, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(item)));

        singleton._Lists.Add(channelId, item);
    }

    [SlashCommand("RemoveList", "Removes a ToDoList")]
    public async Task RemoveTaskList(InteractionContext ctx)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List does not exist"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Removing"));

        singleton._Lists.Remove(channelId);

        await ctx.DeleteFollowupAsync(list!._ListMessageId);

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();

        
    }

    [SlashCommand("addItem", "Add an item to the list")]
    public async Task AddTaskItem(InteractionContext ctx, [Option("item", "item to do")] string item, [Option("Amount", "if the task has progression how many stages?", true)] long? amount = 1)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Adding"));

        list!._ToDoList.Add(new ToDo(item, $"<@!{ctx.Member.Id}>", false, amount));

        await ctx.EditFollowupAsync(list._ListMessageId, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(list)));

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("removeItem", "Remove an item from the list")]
    public async Task removeTaskItem(InteractionContext ctx, [Option("item", "item to delete")] long itemlong)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);
        var item = unchecked((int)itemlong);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        if(list!._ToDoList.Count < item)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("item not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Removing"));

        list!._ToDoList.Remove(list!._ToDoList[item-1]);

        await ctx.EditFollowupAsync(list._ListMessageId, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(list)));

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("editItem", "edit an item from a list")]
    public async Task editTaskItem(InteractionContext ctx, [Option("item", "item to modify")] long itemlong, [Option("name", "task item")] string itemName = "", [Option("Amount", "if the task has progression how many stages?", true)] long? amount = -1)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);
        var item = unchecked((int)itemlong);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        if (list!._ToDoList.Count < item)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("item not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Editing"));

        var i = list!._ToDoList[item - 1];

        i.Name = !String.IsNullOrWhiteSpace(itemName) ? itemName : i.Name;
        i.MaxProgress = amount > 0 ? amount : i.MaxProgress;


        await ctx.EditFollowupAsync(list._ListMessageId, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(list)));

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("progressItem", "make progress on an item")]
    public async Task progressTaskItem(InteractionContext ctx, [Option("item", "item to modify")] long itemlong, [Option("Amount", "how many stages to complete", true)] long amount = 0)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);
        var item = unchecked((int)itemlong);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        if (list!._ToDoList.Count < item)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("item not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        var i = list!._ToDoList[item - 1];

        if(i.Progress + amount > i.MaxProgress)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Cannot progress passed max stage"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        i.Progress = i.Progress + amount;

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Updating"));

        await ctx.EditFollowupAsync(list._ListMessageId, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(list)));

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommand("completeItem", "make progress on an item")]
    public async Task completeTaskItem(InteractionContext ctx, [Option("item", "item to modify")] long itemlong)
    {
        var channelId = ctx.Interaction.ChannelId;
        bool listPresent = singleton._Lists.TryGetValue(channelId, out var list);
        var item = unchecked((int)itemlong);

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!listPresent)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("List not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        if (list!._ToDoList.Count < item)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("item not added"));
            await Task.Delay(2000);
            await ctx.DeleteResponseAsync();
            return;
        }

        var i = list!._ToDoList[item - 1];

        i.Progress = (long)i.MaxProgress;
        i.IsComplete = !i.IsComplete;

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Updating"));

        await ctx.EditFollowupAsync(list._ListMessageId, new DiscordWebhookBuilder().AddEmbed(ToDoMessageBuilder(list)));

        await Task.Delay(2000);
        await ctx.DeleteResponseAsync();
    }

    private DiscordEmbed ToDoMessageBuilder(ToDoList toDo)
    {
        string? list = null;
        int count = 1;
        foreach(var item in toDo._ToDoList)
        {
            string completed = item.IsComplete ? "~~" : "";
            string completion = item.MaxProgress > 1 ? $"[{item.Progress}/{item.MaxProgress}] ": "";
            string userInfo = toDo._includeUsers ? $"[**{item.User}**] " : "";
            list = $" {list ?? ""} **{count}.** {userInfo}{completion} {completed}{item.Name}{completed}\r\n";
            count++;
        }

        return new DiscordEmbedBuilder()
        {
            Title = toDo._ListName,
            Author = new EmbedAuthor() { Name = toDo._creationUser, IconUrl = toDo._creationUserImage },
            Description = list??"",
            Footer = new Embed​Footer() { Text = toDo._ListDescription },
            Color = new DiscordColor(10509236),
            Timestamp = DateTime.Now,
        }.Build();
    }
}