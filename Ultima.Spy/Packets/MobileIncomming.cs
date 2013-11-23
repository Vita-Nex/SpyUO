using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System;

namespace Ultima.Spy.Packets
{
	public enum Direction
	{
		North			= 0x00,
		Right			= 0x01,
		East			= 0x02,
		Down			= 0x03,
		South			= 0x04,
		Left			= 0x05,
		West			= 0x06,
		Up				= 0x07,
		RunningNorth	= 0x80,
		RunningRight	= 0x81,
		RunningEast		= 0x82,
		RunningDown		= 0x83,
		RunningSouth	= 0x84,
		RunningLeft		= 0x85,
		RunningWest		= 0x86,
		RunningUp		= 0x87,
	}

	public enum Notoriety
	{
		Innocent		= 0x1,
		Ally			= 0x2,
		CanBeAttacked	= 0x3,
		Criminal		= 0x4,
		Enemy			= 0x5,
		Murderer		= 0x6,
		Invulenrable	= 0x7
	}
	
	public enum ItemLayer
	{
		Invalid				= 0x0,
		OneHanded			= 0x1,
		TwoHanded			= 0x2,
		Shoes				= 0x3,
		Pants				= 0x4,
		Short				= 0x5,
		Helm				= 0x6,
		Gloves				= 0x7,
		Ring				= 0x8,
		Talisman			= 0x9,
		Neck				= 0xA,
		Hair				= 0xB,
		Waist				= 0xC,
		InnerTorso			= 0xD,
		Bracelet			= 0xE,
		Face				= 0xF,
		FacialHair			= 0x10,
		MiddleTorso			= 0x11,
		Earrings			= 0x12,
		Arms				= 0x13,
		Cloak				= 0x14,
		Backpack			= 0x15,
		OuterTorso			= 0x16,
		OuterLegs			= 0x17,
		InnerLegs			= 0x18,
		Mount				= 0x19,
		ShopBuy				= 0x1A,
		ShopResell			= 0x1B,
		ShopSell			= 0x1C,
		Bank				= 0x1D,
		ShopMax				= 0x1E,
	}

	[UltimaPacket( "Mobile Incomming", UltimaPacketDirection.FromServer, 0x78 )]
	public class MobileIncommingPacket : UltimaPacket, IUltimaEntity
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

		private Notoriety _Notoriety;

		[UltimaPacketProperty( "Notoriety", "{0:D} - {0}" )]
		public Notoriety Notoriety
		{
			get { return _Notoriety; }
		}

		private List<MobileItem> _Items;

		[UltimaPacketProperty]
		public List<MobileItem> Items
		{
			get { return _Items; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_Body = reader.ReadInt16();
			_X = reader.ReadInt16();
			_Y = reader.ReadInt16();
			_Z = reader.ReadSByte();
			_Direction = (Direction) reader.ReadByte();
			_Hue = reader.ReadUInt16();

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

			_Notoriety = (Notoriety) reader.ReadByte();

			// Items
			_Items = new List<MobileItem>();

			uint serial = 0;

			while ( ( serial = reader.ReadUInt32() ) != 0 )
				_Items.Add( new MobileItem( serial, reader ) );
		}
	}

	public class MobileItem
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _ItemID;

		[UltimaPacketProperty( "Item ID", UltimaPacketPropertyType.Texture )]
		public int ItemID
		{
			get { return _ItemID; }
		}

		private ItemLayer _Layer;

		[UltimaPacketProperty( "Layer", "{0:D} - {0}" )]
		public ItemLayer Layer
		{
			get { return _Layer; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		public MobileItem( uint serial, BigEndianReader reader )
		{
			_Serial = serial;
			_ItemID = reader.ReadInt16();
			_Layer = (ItemLayer) reader.ReadByte();

			if ( ( _ItemID & 0x8000 ) != 0 )
				_Hue = reader.ReadInt16();
			else
				_Hue = 0;
		}

		public override string ToString()
		{
			return String.Format( "{0} - {1}", _Serial, _Layer );
		}
	}
}
