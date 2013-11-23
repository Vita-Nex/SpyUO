using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Gump Response", UltimaPacketDirection.FromClient, 0xB1 )]
	public class GumpResponsePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _GumpID;
		
		[UltimaPacketProperty( "Gump ID", "0x{0:X}" )]
		public int GumpID
		{
			get { return _GumpID; }
		}

		private int _ButtonID;
		
		[UltimaPacketProperty( "Button ID" )]
		public int ButtonID
		{
			get { return _ButtonID; }
		}

		private List<int> _Switches;

		[UltimaPacketProperty]
		public List<int> Switches
		{
			get { return _Switches; }
		}

		private List<GumpResponseTextEntry> _TextEntries;

		[UltimaPacketProperty( "Text Entries" )]
		public List<GumpResponseTextEntry> TextEntries
		{
			get { return _TextEntries; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_GumpID = reader.ReadInt32();
			_ButtonID = reader.ReadInt32();

			int switchCount = reader.ReadInt32();
			_Switches = new List<int>( switchCount );

			for ( int i = 0; i < switchCount; i++ )
				_Switches.Add( reader.ReadInt32() );

			int entryCount = reader.ReadInt32();
			_TextEntries = new List<GumpResponseTextEntry>( entryCount );

			for ( int i = 0; i < entryCount; i++ )
				_TextEntries.Add( new GumpResponseTextEntry( reader ) );
		}
	}

	public class GumpResponseTextEntry
	{
		private int _EntryID;

		[UltimaPacketProperty( "Text Entry ID" )]
		public int EntryID
		{
			get { return _EntryID; }
		}

		private string _Text;

		[UltimaPacketProperty( "Text" )]
		public string Text
		{
			get { return _Text; }
		}

		public GumpResponseTextEntry( BigEndianReader reader )
		{
			_EntryID = reader.ReadInt16();
			_Text = reader.ReadUnicodeString();
		}

		public override string ToString()
		{
			return String.Format( "{0}", _EntryID );
		}
	}
}
