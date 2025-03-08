using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Setbothp;
public class Config
{
    public List<ulong> admins { get; set; } = new List<ulong>();
    public int player_HP { get; set; }
}

[MinimumApiVersion(65)]
public class Setbothp : BasePlugin
{
    public override string ModuleName => "SetPlayerHP";

    public override string ModuleVersion => "v2.0.0";

    public override string ModuleAuthor => "CosmoKotik";

    public List<CCSPlayerController>? PlayersCustomHealth { get; set; }
    public Dictionary<int?, int>? PlayersCustomHealthData { get; set; }
    public List<CCSPlayerController>? PlayersCustomSpeed { get; set; }
    public Dictionary<int?, int>? PlayersCustomSpeedData { get; set; }

    public Config config = new Config();
    public override void Load(bool hotReload)
    {
        var configPath = Path.Join(ModuleDirectory, "Config.json");
        if (!File.Exists(configPath))
        {
            config.admins.Add(76561198831353630);       //CosmoKotik
            config.admins.Add(76561199098150617);       //Messerschmitt
            config.player_HP = 100;
            File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        }
        else 
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));

        PlayersCustomHealth = new List<CCSPlayerController>();
        PlayersCustomSpeed = new List<CCSPlayerController>();
        PlayersCustomHealthData = new Dictionary<int?, int>();
        PlayersCustomSpeedData = new Dictionary<int?, int>();

        Console.WriteLine($"Plugin: {ModuleName} ver:{ModuleVersion} by {ModuleAuthor} has been loaded =)");
    }
    public const int MIN_BOT_HP = 1;
    public const int STANDART_PLAYER_HP = 100;
    public const int MAX_PLAYER_HP = 9999999;
    public void OnConfigReload()
    {
        var configPath = Path.Join(ModuleDirectory, "Config.json");
        config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
    }
    public void SetPlayerHp()
    {
        if (PlayersCustomHealthData == null || PlayersCustomHealth == null)
            return;

        for (int i = 0; i < PlayersCustomHealth.Count; i++)
        {
            CCSPlayerController player = PlayersCustomHealth[i];
            int? userID = player.UserId;
            if (userID == null)
                continue;

            int playerHP = 0;
            PlayersCustomHealthData.TryGetValue(userID, out playerHP);

            if (player.IsValid && !player.IsBot)
            {
                if (playerHP >= MIN_BOT_HP && playerHP <= MAX_PLAYER_HP)
                    player.Pawn.Value.Health = playerHP;
                else if (playerHP < MIN_BOT_HP || playerHP > MAX_PLAYER_HP)
                    player.Pawn.Value.Health = STANDART_PLAYER_HP;
            }
        }
    }
    public void SetPlayerSpeed()
    {
        if (PlayersCustomSpeedData == null || PlayersCustomSpeed == null)
            return;

        for (int i = 0; i < PlayersCustomSpeed.Count; i++)
        {
            CCSPlayerController player = PlayersCustomSpeed[i];
            int? userID = player.UserId;
            if (userID == null)
                continue;

            int playerSpeed = 0;
            PlayersCustomSpeedData.TryGetValue(userID, out playerSpeed);

            if (player.IsValid && !player.IsBot)
            {
                if (playerSpeed >= MIN_BOT_HP && playerSpeed <= MAX_PLAYER_HP)
                    player.Pawn.Value.Speed = playerSpeed;
                else if (playerSpeed < MIN_BOT_HP || playerSpeed > MAX_PLAYER_HP)
                    player.Pawn.Value.Speed = STANDART_PLAYER_HP;
            }
        }
    }
    #region player_hp
    [ConsoleCommand("css_set_player_hp")]
    public void OnCommandSetPlayerHp(CCSPlayerController? controller, CommandInfo command)
    {
        if (PlayersCustomHealthData == null || PlayersCustomHealth == null)
        {
            controller.PrintToChat($" {ChatColors.Red}Error 01");
            return;
        }

        if (controller == null)
        {
            controller.PrintToChat($" {ChatColors.Red}Error 02");
            return;
        }
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            if (Regex.IsMatch(command.GetArg(1), @"\w+"))
            {
                if (GetPlayerByName(command.GetArg(1)) == null)
                {
                    controller.PrintToChat($" {ChatColors.Red}Player is not in game! Please enter a valid player name");
                    return;
                }

                if (Regex.IsMatch(command.GetArg(2), @"^\d+$"))
                {
                    if (int.Parse(command.GetArg(2)).Equals(0))
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Red}Player HP can`t be zero!");
                    else
                    {
                        CCSPlayerController player = GetPlayerByName(command.GetArg(1));
                        int playerHP = int.Parse(command.GetArg(2));

                        if (PlayersCustomHealth.Contains(player))
                        {
                            PlayersCustomHealth.Remove(player);
                            PlayersCustomHealthData.Remove(player.UserId);
                        }

                        PlayersCustomHealth.Add(player);
                        PlayersCustomHealthData.Add(player.UserId, playerHP);

                        //SetPlayerHp();

                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}New Player HP: {ChatColors.Green}{playerHP}");
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Will be set next round");
                    }
                }
                else
                    controller.PrintToChat($" {ChatColors.Red}Incorrect value! Please input correct number");
            }
            else
                controller.PrintToChat($" {ChatColors.Red}Player name missing! Please input player name");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!!!");
    }
    [ConsoleCommand("css_set_team_hp")]
    public void OnCommandSetTeamHp(CCSPlayerController? controller, CommandInfo command)
    {
        if (PlayersCustomHealthData == null || PlayersCustomHealth == null)
            return;

        if (controller == null) return;
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            if (Regex.IsMatch(command.GetArg(1), @"\w+"))
            {
                int playerHP = 0;

                if (Regex.IsMatch(command.GetArg(2), @"^\d+$"))
                {
                    if (int.Parse(command.GetArg(2)).Equals(0))
                    {
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Red}Players HP can`t be zero!");
                        return;
                    }
                    else
                        playerHP = int.Parse(command.GetArg(2));
                }
                else
                    controller.PrintToChat($" {ChatColors.Red}Incorrect value! Please input correct number");

                foreach (CCSPlayerController player in Utilities.GetPlayers())
                {
                    switch (command.GetArg(1))
                    {
                        //2
                        case "T":
                            if (player.TeamNum.Equals(2))
                            {
                                if (PlayersCustomHealth.Contains(player))
                                {
                                    PlayersCustomHealth.Remove(player);
                                    PlayersCustomHealthData.Remove(player.UserId);
                                }

                                PlayersCustomHealth.Add(player);
                                PlayersCustomHealthData.Add(player.UserId, playerHP);
                            }
                            break;
                        //3
                        case "CT":
                            if (player.TeamNum.Equals(3))
                            {
                                if (PlayersCustomHealth.Contains(player))
                                {
                                    PlayersCustomHealth.Remove(player);
                                    PlayersCustomHealthData.Remove(player.UserId);
                                }

                                PlayersCustomHealth.Add(player);
                                PlayersCustomHealthData.Add(player.UserId, playerHP);
                            }
                            break;
                        default:
                            controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Team does not exist");
                            break;
                    }
                }

                //SetPlayerHp();
                controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}New Team HP: {ChatColors.Green}{playerHP}");
                controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Will be set next round");
            }
            else
                controller.PrintToChat($" {ChatColors.Red}Team name missing! Please input player name");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!");
    }
    #endregion
    #region player_speed
    [ConsoleCommand("css_set_player_speed")]
    public void OnCommandSetPlayerSpeed(CCSPlayerController? controller, CommandInfo command)
    {
        if (PlayersCustomSpeedData == null || PlayersCustomSpeed == null)
        {
            controller.PrintToChat($" {ChatColors.Red}Error 01");
            return;
        }

        if (controller == null)
        {
            controller.PrintToChat($" {ChatColors.Red}Error 02");
            return;
        }
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            if (Regex.IsMatch(command.GetArg(1), @"\w+"))
            {
                if (GetPlayerByName(command.GetArg(1)) == null)
                {
                    controller.PrintToChat($" {ChatColors.Red}Player is not in game! Please enter a valid player name");
                    return;
                }

                if (Regex.IsMatch(command.GetArg(2), @"^\d+$"))
                {
                    if (int.Parse(command.GetArg(2)).Equals(0))
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Red}Player Speed can`t be zero!");
                    else
                    {
                        CCSPlayerController player = GetPlayerByName(command.GetArg(1));
                        int playerSpeed = int.Parse(command.GetArg(2));

                        if (PlayersCustomSpeed.Contains(player))
                        {
                            PlayersCustomSpeed.Remove(player);
                            PlayersCustomSpeedData.Remove(player.UserId);
                        }

                        PlayersCustomSpeed.Add(player);
                        PlayersCustomSpeedData.Add(player.UserId, playerSpeed);

                        //SetPlayerHp();

                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}New Player Speed: {ChatColors.Green}{playerSpeed}");
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Will be set next round");
                    }
                }
                else
                    controller.PrintToChat($" {ChatColors.Red}Incorrect value! Please input correct number");
            }
            else
                controller.PrintToChat($" {ChatColors.Red}Player name missing! Please input player name");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!");
    }
    [ConsoleCommand("css_set_team_speed")]
    public void OnCommandSetTeamSpeed(CCSPlayerController? controller, CommandInfo command)
    {
        if (PlayersCustomSpeedData == null || PlayersCustomSpeed == null)
            return;

        if (controller == null) return;
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            if (Regex.IsMatch(command.GetArg(1), @"\w+"))
            {
                int playerSpeed = 0;

                if (Regex.IsMatch(command.GetArg(2), @"^\d+$"))
                {
                    if (int.Parse(command.GetArg(2)).Equals(0))
                    {
                        controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Red}Players Speed can`t be zero!");
                        return;
                    }
                    else
                        playerSpeed = int.Parse(command.GetArg(2));
                }
                else
                    controller.PrintToChat($" {ChatColors.Red}Incorrect value! Please input correct number");

                foreach (CCSPlayerController player in Utilities.GetPlayers())
                {
                    switch (command.GetArg(1))
                    {
                        //2
                        case "T":
                            if (player.TeamNum.Equals(2))
                            {
                                if (PlayersCustomSpeed.Contains(player))
                                {
                                    PlayersCustomSpeed.Remove(player);
                                    PlayersCustomSpeedData.Remove(player.UserId);
                                }

                                PlayersCustomSpeed.Add(player);
                                PlayersCustomSpeedData.Add(player.UserId, playerSpeed);
                            }
                            break;
                        //3
                        case "CT":
                            if (player.TeamNum.Equals(3))
                            {
                                if (PlayersCustomSpeed.Contains(player))
                                {
                                    PlayersCustomSpeed.Remove(player);
                                    PlayersCustomSpeedData.Remove(player.UserId);
                                }

                                PlayersCustomSpeed.Add(player);
                                PlayersCustomSpeedData.Add(player.UserId, playerSpeed);
                            }
                            break;
                        default:
                            controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Team does not exist");
                            break;
                    }
                }

                //SetPlayerHp();
                controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}New Team Speed: {ChatColors.Green}{playerSpeed}");
                controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}PlayerHP {ChatColors.Red}] {ChatColors.Default}Will be set next round");
            }
            else
                controller.PrintToChat($" {ChatColors.Red}Team name missing! Please input player name");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!");
    }
    #endregion
    private CCSPlayerController GetPlayerByName(string name)
    {
        foreach (var player in Utilities.GetPlayers())
            if (player.PlayerName.Equals(name))
                return player;

        return null;
    }
    [ConsoleCommand("css_playerhp_reload")]
    public void OnPlayerikiConfigReload(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller == null) return;
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            OnConfigReload();
            controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}Botiki {ChatColors.Red}] {ChatColors.Olive}...configuration was reloaded. {ChatColors.Green}OK!");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!!!");
    }
    /*[ConsoleCommand("css_playerhp_add_admin")]
    public void OnCommandAddAdmin(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller == null) return;
        if (config.admins.Exists(adminID => adminID == controller.SteamID))
        {
            OnConfigReload();
            controller.PrintToChat($" {ChatColors.Red}[ {ChatColors.Purple}Botiki {ChatColors.Red}] {ChatColors.Olive}...configuration was reloaded. {ChatColors.Green}OK!");
        }
        else
            controller.PrintToChat($" {ChatColors.Red}You are not Admin!!!");
    }*/
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        SetPlayerHp();
        SetPlayerSpeed();
        return HookResult.Continue;
    }
}
