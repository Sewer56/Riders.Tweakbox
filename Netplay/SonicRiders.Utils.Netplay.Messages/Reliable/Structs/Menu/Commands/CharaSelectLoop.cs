﻿using System;
using MessagePack;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    /// <summary>
    /// Client -> Host
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public struct CharaSelectLoop : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectLoop;
        public byte[] ToBytes() => Struct.GetBytes(this);

        /// <summary>
        /// The character selected by the current player.
        /// </summary>
        [Key(0)]
        public byte Character;

        /// <summary>
        /// Current board selected by the player.
        /// </summary>
        [Key(1)]
        public byte Board;

        /// <summary>
        /// Current status of the player.
        /// </summary>
        [Key(2)]
        public PlayerStatus Status;

        /// <summary>
        /// Set to true if the player is currently exiting the menu.
        /// </summary>
        [Key(3)]
        public bool IsExitingMenu;

        /// <summary>
        /// Set to true if the player is starting race.
        /// </summary>
        [Key(4)]
        public bool IsStartingRace;

        public CharaSelectLoop(byte character, byte board, PlayerStatus status, bool isExiting, bool isStarting)
        {
            Character = character;
            Board = board;
            Status = status;
            IsExitingMenu = isExiting;
            IsStartingRace = isStarting;
        }

        /// <summary>
        /// Applies the current task to the game.
        /// </summary>
        /// <param name="task">The individual menu task.</param>
        /// <param name="index">The character index.</param>
        public unsafe void ToGame(Task<CharacterSelect, CharacterSelectTaskState>* task, int index)
        {
            ref var player = ref Player.Players[index];
            player.Character   = (Characters)Character;
            player.ExtremeGear = (ExtremeGear)Board;

            // Menu supports only 4 characters
            if (index <= 3)
            {
                task->TaskData->PlayerMenuSelections[index] = CharacterToSelection(Character);

                switch (Status)
                {
                    case PlayerStatus.Inactive:
                    case PlayerStatus.Active:
                    case PlayerStatus.GearSelect:
                    case PlayerStatus.GearDescription:
                        task->TaskData->PlayerStatuses[index] = (byte) Status;
                        break;
                    // Set to an invalid state.
                    // This will look as if the other person has selected the character but 
                    // will not load the model. This is good for now, as it will reduce pressure off us
                    // to implement code to render the model.
                    case PlayerStatus.SetReady:
                    case PlayerStatus.Ready:
                        task->TaskData->PlayerStatuses[index] = 6;
                        break;
                }
                task->TaskData->PlayerMenuSelections[index] = CharacterToSelection(Character);
            }

            // Handle menu exiting and race start.
            if (IsExitingMenu)
            {
                task->TaskStatus = CharacterSelectTaskState.Exiting;
                task->TaskData->MenuState = MenuState.Exit;
            }

            if (IsStartingRace)
            {
                task->TaskStatus = CharacterSelectTaskState.LoadingStage;
                task->TaskData->SelectionIsPerformed = 1;
            }
        }

        /// <summary>
        /// Gets the loop data from the current game instance.
        /// </summary>
        /// <param name="task">The individual menu task.</param>
        public static unsafe CharaSelectLoop FromGame(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            var data = task->TaskData;
            bool isExiting = task->TaskStatus == CharacterSelectTaskState.Exiting && data->MenuState == MenuState.Exit;
            bool isStartingRace = task->TaskStatus == CharacterSelectTaskState.LoadingStage && data->SelectionIsPerformed == 1;

            return new CharaSelectLoop()
            {
                Character = (byte)Player.Players[0].Character,
                Board = (byte)Player.Players[0].ExtremeGear,
                Status = (PlayerStatus)data->PlayerStatuses[0],
                IsExitingMenu = isExiting,
                IsStartingRace = isStartingRace
            };
        }

        /// <summary>
        /// Converts a character index to a menu selection index.
        /// </summary>
        private byte CharacterToSelection(byte character)
        {
            if (character <= (int) Characters.Shadow)
                return character;

            // Super Sonic is not available in the menu.
            return (byte) (character - 1);
        }
    }
}