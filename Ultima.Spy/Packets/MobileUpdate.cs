using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Mobile Update", UltimaPacketDirection.FromServer, 0x20 )]
	public class MobileUpdatePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _Body;

		[UltimaPacketProperty( UltimaPacketPropertyType.Body )]
		public int Body
		{
			get { return _Body; }
		}

		private int _X;

		[UltimaPacketProperty]
		public int X
		{
			get { return _X; }
		}

		private int _Y;

		[UltimaPacketProperty]
		public int Y
		{
			get { return _Y; }
		}

		private int _Z;

		[UltimaPacketProperty]
		public int Z
		{
			get { return _Z; }
		}

		private Direction _Direction;

		[UltimaPacketProperty( "Direction", "{0:D} - {0}" )]
		public Direction Direction
		{
			get { return _Direction; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		private bool _IsFrozen;

		[UltimaPacketProperty( "Is Frozen" )]
		public bool IsFrozen
		{
			get { return _IsFrozen; }
		}

		private bool _IsFemale;

		[UltimaPacketProperty( "Is Female" )]
		public bool IsFemale
		{
			get { return _IsFemale; }
		}

		private bool _IsFlying;

		[UltimaPacketProperty( "Is Flying" )]
		public bool IsFlying
		{
			get { return _IsFlying; }
		}

		private bool _HasYellowHealthBar;

		[UltimaPacketProperty( "Has Yellow Health Bar" )]
		public bool HasYellowHealthBar
		{
			get { return _HasYellowHealthBar; }
		}

		private bool _IsIgnoringMobiles;

		[UltimaPacketProperty( "Is Ignoring Mobiles" )]
		public bool IsIgnoringMobiles
		{
			get { return _IsIgnoringMobiles; }
		}

		private bool _IsInWarMode;

		[UltimaPacketProperty( "Is In War Mode" )]
		public bool IsInWarMode
		{
			get { return _IsInWarMode; }
		}

		private bool _IsHidden;

		[UltimaPacketProperty( "Is Hidden" )]
		public bool IsHidden
		{
			get { return _IsHidden; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			_Serial = reader.ReadUInt32();
			_Body = reader.ReadInt16();
			reader.ReadByte();
			_Hue = reader.ReadInt16();

			byte flags = reader.ReadByte();

			if ( ( flags & 0x1 ) > 0 )
				_IsFrozen = true;

			if ( ( flags & 0x2 ) > 0 )
				_IsFemale = true;

			if ( ( flags & 0x4 ) > 0 )
				_IsFlying = true;

			if ( ( flags & 0x8 ) > 0 )
				_HasYellowHealthBar = true;

			if ( ( flags & 0x10 ) > 0 )
				_IsIgnoringMobiles = true;

			if ( ( flags & 0x40 ) > 0 )
				_IsInWarMode = true;

			if ( ( flags & 0x80 ) > 0 )
				_IsHidden = true;

			_X = reader.ReadInt16();
			_Y = reader.ReadInt16();
			reader.ReadInt16();
			_Direction = (Direction) reader.ReadByte();
			_Z = reader.ReadSByte();
		}
	}
}
