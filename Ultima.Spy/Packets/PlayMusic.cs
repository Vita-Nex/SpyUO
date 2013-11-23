using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Play Music", UltimaPacketDirection.FromServer, 0x6D )]
	public class PlayMusicPacket : UltimaPacket
	{
		private int _MusicID;

		[UltimaPacketProperty( "Music ID", UltimaPacketPropertyType.Music )]
		public int MusicID
		{
			get { return _MusicID; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			_MusicID = reader.ReadInt16();
		}
	}
}
