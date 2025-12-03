using Discord;
using Discord.Interactions;
using System.Reflection.Metadata;
using System.Text;


namespace Fatty.Modules;

public class QueryModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;

    public QueryModule(InteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [SlashCommand("help","顯示所有指令清單")]
    public async Task HelpAsync()
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("📜 Fatty 機器人指令清單")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        var sb = new StringBuilder();

        foreach(var module in _interactionService.Modules)
        {
            if(!module.SlashCommands.Any()) continue;
            string moduleName = module.Name.Replace("Module","");
            sb.AppendLine($"**📂 {moduleName}**");

            foreach(var command in module.SlashCommands)
            {
                sb.AppendLine($"• `/{command.Name}` - {command.Description}");

                if (command.Parameters.Any())
                {
                    sb.Append($"  參數:");
                    int count = command.Parameters.Count;
                    for (int i = 0 ; i < count; i++)
                    {
                        sb.Append($" `[{command.Parameters[i].Name}]`");
                        if (i == count - 1)
                        {
                            sb.Append($"\n");
                        }
                    }
                }

            }
            sb.AppendLine();
        }

        embedBuilder.WithDescription(sb.ToString());
        embedBuilder.WithFooter($"由 {Context.User.Username} 呼叫");

        await RespondAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("ping", "檢查機器人延遲")]
    public async Task PingAsync()
    {
        await RespondAsync($"🏓 Pong! {Context.Client.Latency} ms");
    }
}
