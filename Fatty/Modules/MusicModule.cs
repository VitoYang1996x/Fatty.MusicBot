using Microsoft.Extensions.Options;
using Discord;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;

namespace Fatty.Modules;

public class MusicModule: InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAudioService _audioService;

    public MusicModule(IAudioService audioService)
    {
        _audioService = audioService;
    }

    [SlashCommand("play", "播放音樂 (支援 YouTube, SoundCloud)")]
    public async Task PlayAsync(string query)
    {
        await DeferAsync(); // 因為搜尋音樂需要時間，先告訴 Discord "請稍等"

        // 1. 檢查使用者是否在語音頻道
        var userVoiceState = (Context.User as IVoiceState);
        if (userVoiceState?.VoiceChannel == null)
        {
            await FollowupAsync("❌ 你必須先加入一個語音頻道！");
            return;
        }

        // 2. 取得或建立播放器 (Player)
        // VoteLavalinkPlayer 支援投票跳過功能，適合公開機器人
        var playerOptions = new VoteLavalinkPlayerOptions
        {
            DisconnectOnStop = true, // 播完自動斷線
            SelfDeaf = true // 機器人自己拒聽 (節省頻寬)
        };

        var playerResult = await _audioService.Players
            .RetrieveAsync<VoteLavalinkPlayer, VoteLavalinkPlayerOptions>(
                Context.Guild.Id,
                userVoiceState.VoiceChannel.Id,
                playerFactory: null, // <--- 改成 null，讓它自己處理
                Options.Create(playerOptions)
            );

        if (!playerResult.IsSuccess)
        {
            await FollowupAsync("❌ 無法加入語音頻道 (可能權限不足或是其他錯誤)。");
            return;
        }

        var player = playerResult.Player;

        // 3. 搜尋音樂 (預設搜尋 YouTube)
        var track = await _audioService.Tracks
            .LoadTrackAsync(query, TrackSearchMode.YouTube);

        if (track is null)
        {
            await FollowupAsync($"🔍 找不到關於 `{query}` 的音樂。");
            return;
        }

        // 4. 播放音樂
        await player.PlayAsync(track);

        // 5. 回覆使用者
        await FollowupAsync($"🎵 正在播放： **{track.Title}**");
    }

    [SlashCommand("stop", "停止播放並斷線")]
    public async Task StopAsync()
    {
        // 嘗試取得目前的播放器
        var player = await _audioService.Players.GetPlayerAsync<VoteLavalinkPlayer>(Context.Guild.Id);

        if (player is null)
        {
            await RespondAsync("❌ 目前沒有在播放音樂。");
            return;
        }

        await player.StopAsync();
        await player.DisconnectAsync();
        await RespondAsync("👋 已停止播放並離開頻道。");
    }

}
