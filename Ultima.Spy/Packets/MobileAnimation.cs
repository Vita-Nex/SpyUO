using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultima.Spy.Packets
{
	public enum AnimationType
	{
		Attack			= 0,
		Parry			= 1,
		Block			= 2,
		Die				= 3,
		Impact			= 4,
		Fidget			= 5,
		Eat				= 6,
		Emote			= 7,
		Alert			= 8,
		TakeOff			= 9,
		Land			= 10,
		Spell			= 11,
		StartCombat		= 12,
		EndCombat		= 13,
		Pillage			= 14,
		Spawn			= 15
	}

	[UltimaPacket( "Mobile Animation", UltimaPacketDirection.FromServer, 0xE2 )]
	public class MobileAnimation : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private AnimationType _AnimationType;

		[UltimaPacketProperty( "Animation Type", "{0:D} - {0}" )]
		public AnimationType AnimationType
		{
			get { return _AnimationType; }
		}

		private int _Action;

		[UltimaPacketProperty]
		public int Action
		{
			get { return _Action; }
		}

		private int _Delay;

		[UltimaPacketProperty]
		public int Delay
		{
			get { return _Delay; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			_Serial = reader.ReadUInt32();
			_AnimationType = (AnimationType) reader.ReadInt16();
			_Action = reader.ReadInt16();
			_Delay = reader.ReadByte();
		}
	}
}
