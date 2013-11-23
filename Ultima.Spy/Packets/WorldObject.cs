using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	public enum WorldObjectType
	{
		Tile		= 0,
		Body		= 1,
		Multi		= 2
	}

	public enum WorldObjectAccess
	{
		Player		= 0,
		World		= 1,
	}

	[UltimaPacket( "World Object", UltimaPacketDirection.FromServer, 0xF3 )]
	public class WorldObjectPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private WorldObjectType _DataType;

		[UltimaPacketProperty( "Data Type", "{0:D} - {0}" )]
		public WorldObjectType DataType
		{
			get { return _DataType; }
		}

		private int _ObjectID;

		[UltimaPacketProperty( "Object ID", "0x{0:X}" )]
		public int ObjectID
		{
			get { return _ObjectID; }
		}

		private int _ObjectIDOffset;

		[UltimaPacketProperty( "Object ID Offset" )]
		public int ObjectIDOffset
		{
			get { return _ObjectIDOffset; }
		}

		[UltimaPacketProperty( UltimaPacketPropertyType.Texture )]
		public int Texture
		{
			get { return _ObjectID + _ObjectIDOffset; }
		}

		private int _Amount;

		[UltimaPacketProperty]
		public int Amount
		{
			get { return _Amount; }
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

		private int _LightLevel;


		[UltimaPacketProperty( "Light Level (Quality)" )]
		public int LightLevel
		{
			get { return _LightLevel; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		private bool _HasProperties;

		[UltimaPacketProperty( "Has Properties" )]
		public bool HasProperties
		{
			get { return _HasProperties; }
		}

		private bool _IsVisible;

		[UltimaPacketProperty( "Is Visible" )]
		public bool IsVisible
		{
			get { return _IsVisible; }
		}

		private WorldObjectAccess _Access;

		[UltimaPacketProperty( "Access", "{0:D} - {0}" )]
		public WorldObjectAccess Access
		{
			get { return _Access; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size place holder

			_DataType = (WorldObjectType) reader.ReadByte();
			_Serial = reader.ReadUInt32();
			_ObjectID = reader.ReadInt16();
			_ObjectIDOffset = reader.ReadByte();
			_Amount = reader.ReadInt16();

			reader.ReadInt16(); // Amount again?

			_X = reader.ReadInt16();
			_Y = reader.ReadInt16();
			_Z = reader.ReadSByte();
			_LightLevel = reader.ReadByte();
			_Hue = reader.ReadInt16();

			byte flags = reader.ReadByte();

			if ( ( flags & 0x20 ) > 0 )
				_HasProperties = true;
			else
				_HasProperties = false;

			if ( ( flags & 0x80 ) > 0 )
				_IsVisible = true;
			else
				_IsVisible = false;

			_Access = (WorldObjectAccess) reader.ReadInt16();
		}
	}
}
