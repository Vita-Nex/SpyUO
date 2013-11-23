using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	public enum MessageType
	{
		Regular			= 0x0,
		System			= 0x1,
		Emote			= 0x2,
		Label			= 0x6,
		Focus			= 0x7,
		Whisper			= 0x8,
		Yell			= 0x9,
		Spell			= 0xA,
		Guild			= 0xD,
		Alliance		= 0xE,
		GMRequest		= 0xF,
		GMResponse		= 0x10,
		Special			= 0x20,
		Encoded			= 0xC0
	}

	[UltimaPacket( "ASCII Message", UltimaPacketDirection.FromServer, 0x1C )]
	public class AsciiMessagePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _Graphics;

		[UltimaPacketProperty( "Graphics", "0x{0:X}" )]
		public int Graphics
		{
			get { return _Graphics; }
		}

		private MessageType _Type;

		[UltimaPacketProperty( "Type", "{0:D} - {0}" )]
		public MessageType Type
		{
			get { return _Type; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		private int _Font;

		[UltimaPacketProperty]
		public int Font
		{
			get { return _Font; }
		}

		private string _EntityName;

		[UltimaPacketProperty( "Name" )]
		public string EntityName
		{
			get { return _EntityName; }
		}

		private string _Message;

		[UltimaPacketProperty]
		public string Message
		{
			get { return _Message; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			int length = reader.ReadInt16() - 44;

			_Serial = reader.ReadUInt32();
			_Graphics = reader.ReadInt16();
			_Type = (MessageType) reader.ReadByte();
			_Hue = reader.ReadInt16();
			_Font = reader.ReadInt16();
			_EntityName = reader.ReadAsciiString( 30 );
			_Message = reader.ReadAsciiString( length );
		}
	}
}
