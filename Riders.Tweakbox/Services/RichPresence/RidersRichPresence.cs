using System;
using System.IO;
using System.Net;
using DiscordRPC;
using DiscordRPC.IO;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.RichPresence.Common;
using Sewer56.BitStream;
using Sewer56.BitStream.ByteStreams;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using SharpDX.Text;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Services.RichPresence
{
    public class RidersRichPresence : ISingletonService
    {
        // Encryption key for party & lobby id.
        // Do not change.
        private const string PassPhrase = "Argie's The Best Girl <3" +
                                          "I love her very very much!";

        private DiscordRpcClient _discordRpc;
        private System.Threading.Timer _timer;
        private bool _enableRpc = true;
        private EventController _event;
        private NetplayController _netplayController;
        private NetplayMenu _netplayMenu;
        private TitleSequenceTaskState _titleSequenceTaskState;

        private WeakReference<Socket> _last = new WeakReference<Socket>(null);
        private string _serverId;
        private string _partyId;
        private bool _isJoining = false;
        private RaceMode _raceMode;

        public RidersRichPresence(Tweakbox box)
        {
            // Not like you could get this from decompiling anyway. Obfuscation? That sucks.
            _discordRpc = new DiscordRpcClient("849325518288453693", -1, new NullLogger(), true, null); 
            _discordRpc.Initialize();

            _discordRpc.RegisterUriScheme(null, null);
            _discordRpc.Subscribe(EventType.Join);
            _discordRpc.OnJoin += OnJoin;
            box.OnInitialized  += OnInitialized;
        }

        private async void OnJoin(object sender, JoinMessage args)
        {
            if (_netplayController.Socket != null || _isJoining) 
                return;

            _isJoining = true;
            try
            {
                Log.WriteLine($"Joining Lobby via Discord: {args.Secret}");
                var decrypted = Crypto.Decrypt(args.Secret, PassPhrase);
                var details   = LobbyDetails.FromBytes(decrypted);
                try
                {
                    await _netplayMenu.ConnectAsync(IPAddress.Parse(details.IPv4.ToString()).ToString(), details.Port, details.HasPassword);
                }
                catch (Exception e)
                {
                    Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
                }
            }
            finally
            {
                _isJoining = false;
            }
        }

        private unsafe void OnInitialized()
        {
            _event = IoC.Get<EventController>();
            _netplayController = IoC.Get<NetplayController>();
            _netplayMenu = IoC.Get<NetplayMenu>();
            _timer = new System.Threading.Timer(OnTick, null, 0, 5000);
            _event.OnTitleSequence += OnTitleSequence;
            _enableRpc = true;
        }

        private unsafe void OnTitleSequence(Task<TitleSequence, TitleSequenceTaskState>* task)
        {
            _titleSequenceTaskState = task->TaskStatus;
            _raceMode = task->TaskData->RaceMode;
        }

        private unsafe void OnTick(object state)
        {
            if (!_enableRpc) 
                return;

            var richPresence = new DiscordRPC.RichPresence
            {
                Details = GetCurrentDetails(), 
                State = GetCurrentMenu(), 
                Assets = new Assets()
            };

            // Get Image
            if (*State.RaceMode > ActiveRaceMode.Story)
            {
                // Just a small word fix for battle mode.
                if (*State.RaceMode == ActiveRaceMode.BattleStage)
                    richPresence.Details = "Battling";

                // Add timestamp
                var timeStamps = new Timestamps();

                if (!*State.IsPaused)
                {
                    // Do not set timestamp if paused.
                    DateTime levelStartTime = DateTime.UtcNow.Subtract((*State.StageTimer).ToTimeSpan());
                    timeStamps.Start = levelStartTime;
                    richPresence.Timestamps = timeStamps;
                }

                var level = *State.Level;
                
                if (ImageNameDictionary.Images.TryGetValue(level, out string stageAssetName))
                {
                    richPresence.Assets.LargeImageText = Utilities.GetLevelName(level);
                    richPresence.Assets.LargeImageKey = stageAssetName;
                }
            }

            if (_netplayController.Socket != null)
            {
                // Get cached lobby id.
                var socket   = _netplayController.Socket;
                _last.TryGetTarget(out var last);

                if (last != socket)
                {
                    // Cache is invalid.
                    // Note: Discord doesn't like when Server ID is in Party ID, so we pad the beginning of it.
                    _last     = new WeakReference<Socket>(socket);
                    _serverId = TryGetLobbyId(socket);
                    _partyId  = "Party_" + _serverId;
                }

                if (_serverId != null)
                {
                    var netState = socket.State;
                    richPresence.Party = new Party()
                    {
                        ID = _partyId,
                        Max = Constants.MaxRidersNumberOfPlayers,
                        Privacy = Party.PrivacySetting.Public,
                        Size = netState.GetPlayerCount()
                    };

                    richPresence.Secrets = new Secrets()
                    {
                        JoinSecret = _serverId
                    };
                }
            }

            // Send to Discord
            _discordRpc.SetPresence(richPresence);
        }

        private string TryGetLobbyId(Socket socket)
        {
            return TryGetLobbyBytes(socket, out var bytes) 
                ? Crypto.Encrypt(bytes, PassPhrase) 
                : null;
        }

        /// <summary>
        /// Extracts the lobby details from the current socket state.
        /// </summary>
        private bool TryGetLobbyBytes(Socket socket, out byte[] bytes)
        {
            bytes = default;
            if (string.IsNullOrEmpty(socket.HostIp))
                return false;

            var ip = (int) IPAddress.Parse(socket.HostIp).Address;
            var details = new LobbyDetails()
            {
                IPv4 = (uint) IPAddress.NetworkToHostOrder(ip),
                Port = (ushort) socket.HostPort,
                HasPassword = !string.IsNullOrEmpty(socket.Password)
            };

            bytes = details.GetBytes();
            return true;
        }

        /// <summary>
        /// Gets text set directly under game name on Discord.
        /// </summary>
        private unsafe string GetCurrentDetails()
        {
            var result = State.RaceMode->AsString();

            if (_titleSequenceTaskState == TitleSequenceTaskState.CourseSelect)
                result = _raceMode.AsString();

            return result;
        }

        /// <summary>
        /// Retrieves the current state of the game.
        /// </summary>
        private unsafe string GetCurrentMenu() => _titleSequenceTaskState.AsString();

        /// <summary>
        /// Converts the lobby details into a byte array.
        /// </summary>
        internal struct LobbyDetails
        {
            public uint   IPv4;
            public ushort Port;
            public bool   HasPassword;

            public byte[] GetBytes()
            {
                using var memoryStream = new MemoryStream();
                var streamByteStream   = new StreamByteStream(memoryStream);
                var bitStream          = new BitStream<StreamByteStream>(streamByteStream);
                bitStream.Write(IPv4);
                bitStream.Write(Port);
                bitStream.WriteGeneric(HasPassword, 1);
                return memoryStream.ToArray();
            }

            public static LobbyDetails FromBytes(byte[] data)
            {
                var details          = new LobbyDetails();
                var arrayByteStream  = new ArrayByteStream(data);
                var bitStream        = new BitStream<ArrayByteStream>(arrayByteStream);
                details.IPv4 = bitStream.Read<uint>();
                details.Port = bitStream.Read<ushort>();
                details.HasPassword = bitStream.ReadGeneric<bool>(1);
                return details;
            }
        }
    }
}
