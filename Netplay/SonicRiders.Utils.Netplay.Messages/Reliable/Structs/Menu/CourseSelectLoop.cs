using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu
{
    public struct CourseSelectLoop : IReliableMessage, IEquatable<CourseSelectLoop>
    {
        public const int FlagNumBits = 1;
        public const int DeltaNumBits = 2;

        /// <summary>
        /// Change in X selection.
        /// </summary>
        public sbyte DeltaSelectionX;

        /// <summary>
        /// Change in Y selection.
        /// </summary>
        public sbyte DeltaSelectionY;

        /// <summary>
        /// Change in highlighted item.
        /// </summary>
        public sbyte DeltaHighlighted;

        /// <summary>
        /// Change in list scroll offset.
        /// </summary>
        public sbyte DeltaListScroll;

        /// <summary>
        /// Change in submenu selection.
        /// </summary>
        public sbyte SubmenuDeltaSelection;

        /// <summary>
        /// Opens the Submenu.
        /// </summary>
        public byte OpenSubmenu;

        /// <summary>
        /// Closes the Submenu.
        /// </summary>
        public byte CloseSubmenu;

        /// <summary>
        /// Opens the Race Rules menu.
        /// </summary>
        public byte OpenRaceRules;

        /// <summary>
        /// Opens character select if 1.
        /// </summary>
        public byte OpenCharacterSelect;

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.CourseSelectLoop;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            DeltaSelectionX = bitStream.ReadSigned<sbyte>(DeltaNumBits);
            DeltaSelectionY = bitStream.ReadSigned<sbyte>(DeltaNumBits);
            DeltaHighlighted = bitStream.ReadSigned<sbyte>(DeltaNumBits);
            DeltaListScroll = bitStream.ReadSigned<sbyte>(DeltaNumBits);
            SubmenuDeltaSelection = bitStream.ReadSigned<sbyte>(DeltaNumBits);

            OpenSubmenu = bitStream.Read<byte>(FlagNumBits);
            CloseSubmenu = bitStream.Read<byte>(FlagNumBits);
            OpenRaceRules = bitStream.Read<byte>(FlagNumBits);
            OpenCharacterSelect = bitStream.Read<byte>(FlagNumBits);
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteSigned(DeltaSelectionX, DeltaNumBits);
            bitStream.WriteSigned(DeltaSelectionY, DeltaNumBits);
            bitStream.WriteSigned(DeltaHighlighted, DeltaNumBits);
            bitStream.WriteSigned(DeltaListScroll, DeltaNumBits);
            bitStream.WriteSigned(SubmenuDeltaSelection, DeltaNumBits);

            bitStream.Write(OpenSubmenu, FlagNumBits);
            bitStream.Write(CloseSubmenu, FlagNumBits);
            bitStream.Write(OpenRaceRules, FlagNumBits);
            bitStream.Write(OpenCharacterSelect, FlagNumBits);
        }

        public bool IsDefault()
        {
            return this.Equals(new CourseSelectLoop());
        }

        /// <summary>
        /// Adds the contents of two loops together.
        /// </summary>
        public CourseSelectLoop Add(CourseSelectLoop loop)
        {
            return new CourseSelectLoop()
            {
                CloseSubmenu = (byte) (CloseSubmenu + loop.CloseSubmenu),
                DeltaHighlighted = (sbyte) (DeltaHighlighted + loop.DeltaHighlighted),
                DeltaListScroll = (sbyte) (DeltaListScroll + loop.DeltaListScroll),
                DeltaSelectionY = (sbyte) (DeltaSelectionY + loop.DeltaSelectionY),
                DeltaSelectionX = (sbyte) (DeltaSelectionX + loop.DeltaSelectionX),
                OpenSubmenu = (byte) (OpenSubmenu + loop.OpenSubmenu),
                SubmenuDeltaSelection = (sbyte) (SubmenuDeltaSelection + loop.SubmenuDeltaSelection),
                OpenRaceRules = (byte)(OpenRaceRules + loop.OpenRaceRules),
                OpenCharacterSelect = (byte)(OpenCharacterSelect + loop.OpenCharacterSelect)
            };
        }

        /// <summary>
        /// Undoes the cursor movement delta change made by this loop.
        /// </summary>
        public unsafe void Undo(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (task == null)
                return;

            var data = task->TaskData;
            data->SelectionHorizontal = (byte) (data->SelectionHorizontal - DeltaSelectionX);
            data->SelectionVertical = (byte) (data->SelectionVertical - DeltaSelectionY);
            data->ListScrollOffset = (byte)(data->ListScrollOffset - DeltaListScroll);
            data->SubmenuSelection = (byte)(data->SubmenuSelection - SubmenuDeltaSelection);
            data->HighlightedHorizontalItem = (byte) (data->HighlightedHorizontalItem - DeltaHighlighted);

            if (OpenSubmenu > 0)
            {
                task->TaskStatus   = CourseSelectTaskState.Normal;
                data->SubmenuState = MenuState.Exit;
            }
            else if (CloseSubmenu > 0)
            {
                task->TaskStatus   = CourseSelectTaskState.HeroBabylonPicker;
                data->SubmenuState = MenuState.Enter;
            }
            else if (OpenRaceRules > 0)
            {
                task->TaskStatus = CourseSelectTaskState.Normal;
                data->MenuState = MenuState.Enter;
            }
            else if (OpenCharacterSelect > 0)
            {
                task->TaskStatus = CourseSelectTaskState.Normal;
                task->TaskData->NextMenu = 0;
            }
        }

        #region Autogenerated
        public bool Equals(CourseSelectLoop other)
        {
            return DeltaSelectionX == other.DeltaSelectionX && DeltaSelectionY == other.DeltaSelectionY && DeltaHighlighted == other.DeltaHighlighted && DeltaListScroll == other.DeltaListScroll && SubmenuDeltaSelection == other.SubmenuDeltaSelection && OpenSubmenu == other.OpenSubmenu && CloseSubmenu == other.CloseSubmenu && OpenRaceRules == other.OpenRaceRules && OpenCharacterSelect == other.OpenCharacterSelect;
        }

        public override bool Equals(object obj)
        {
            return obj is CourseSelectLoop other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DeltaSelectionX.GetHashCode();
                hashCode = (hashCode * 397) ^ DeltaSelectionY.GetHashCode();
                hashCode = (hashCode * 397) ^ DeltaHighlighted.GetHashCode();
                hashCode = (hashCode * 397) ^ DeltaListScroll.GetHashCode();
                hashCode = (hashCode * 397) ^ SubmenuDeltaSelection.GetHashCode();
                hashCode = (hashCode * 397) ^ OpenSubmenu.GetHashCode();
                hashCode = (hashCode * 397) ^ CloseSubmenu.GetHashCode();
                hashCode = (hashCode * 397) ^ OpenRaceRules.GetHashCode();
                hashCode = (hashCode * 397) ^ OpenCharacterSelect.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public void Dispose() { }

        #endregion
    }
}