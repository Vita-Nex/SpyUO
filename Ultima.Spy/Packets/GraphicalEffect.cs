using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	public enum GraphicalEffectType
	{
		SourceToDestination		= 0x0,
		LightningStrike			= 0x1,
		StayWithDestination		= 0x2,
		StayWithSource			= 0x3,
		SpecialEffect			= 0x4,
	}

	[UltimaPacket( "Graphical Effect", UltimaPacketDirection.FromClient, 0x70 )]
	public class GraphicalEffectPacket : UltimaPacket, IUltimaEntity
	{
		private GraphicalEffectType _Type;
		
		[UltimaPacketProperty( "Type", "{0:D} - {0}" )]
		public GraphicalEffectType Type
		{
			get { return _Type; }
		}

		private uint _Source;

		[UltimaPacketProperty( "Source", "0x{0:X}" )]
		public uint Source
		{
			get { return _Source; }
		}

		private uint _Target;

		[UltimaPacketProperty( "Target", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Target; }
		}

		private int _ObjectID;

		[UltimaPacketProperty( "Object ID", "0x{0:X}" )]
		public int ObjectID
		{
			get { return _ObjectID; }
		}

		private int _SourceX;

		[UltimaPacketProperty( "Source X" )]
		public int SourceX
		{
			get { return _SourceX; }
		}

		private int _SourceY;

		[UltimaPacketProperty( "Source X" )]
		public int SourceY
		{
			get { return _SourceY; }
		}

		private int _SourceZ;

		[UltimaPacketProperty( "Source Z" )]
		public int SourceZ
		{
			get { return _SourceZ; }
		}

		private int _TargetX;

		[UltimaPacketProperty( "Target X" )]
		public int TargetX
		{
			get { return _TargetX; }
		}

		private int _TargetY;

		[UltimaPacketProperty( "Target X" )]
		public int TargetY
		{
			get { return _TargetY; }
		}

		private int _TargetZ;

		[UltimaPacketProperty( "Target Z" )]
		public int TargetZ
		{
			get { return _TargetZ; }
		}

		private int _Speed;

		[UltimaPacketProperty]
		public int Speed
		{
			get { return _Speed; }
		}

		private int _Duration;

		[UltimaPacketProperty( "Duration (s)" )]
		public int Duration
		{
			get { return _Duration; }
		}

		private bool _FixedDirection;

		[UltimaPacketProperty( "Fixed Direction" )]
		public bool FixedDirection
		{
			get { return _FixedDirection; }
		}

		private bool _Explode;

		[UltimaPacketProperty]
		public bool Explode
		{
			get { return _Explode; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			_Type = (GraphicalEffectType) reader.ReadByte();
			_Source = reader.ReadUInt32();
			_Target = reader.ReadUInt32();
			_ObjectID = reader.ReadInt16();
			_SourceX = reader.ReadInt16();
			_SourceY = reader.ReadInt16();
			_SourceZ = reader.ReadSByte();
			_TargetX = reader.ReadInt16();
			_TargetY = reader.ReadInt16();
			_TargetZ = reader.ReadSByte();
			_Speed = reader.ReadByte();
			_Duration = reader.ReadByte();
			reader.ReadInt16();
			_FixedDirection = reader.ReadBoolean();
			_Explode = reader.ReadBoolean();
		}
	}
}
