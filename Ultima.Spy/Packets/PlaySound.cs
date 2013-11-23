using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Play Sound", UltimaPacketDirection.FromServer, 0x54 )]
	public class PlaySoundPacket : UltimaPacket
	{
		private int _Flags;

		[UltimaPacketProperty( "Flags", "0x{0:X}" )]
		public int Flags
		{
			get { return _Flags; }
		}

		private int _SoundID;

		[UltimaPacketProperty( "Sound ID", UltimaPacketPropertyType.Sound )]
		public int SoundID
		{
			get { return _SoundID; }
		}

		private int _Volume;

		[UltimaPacketProperty]
		public int Volume
		{
			get { return _Volume; }
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

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			_Flags = reader.ReadByte();
			_SoundID = reader.ReadInt16();
			_Volume = reader.ReadInt16();
			_X = reader.ReadInt16();
			_Y = reader.ReadInt16();
			_Z = reader.ReadInt16();
		}
	}
}
