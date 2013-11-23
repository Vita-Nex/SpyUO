using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Generic Gump", UltimaPacketDirection.FromServer, 0xB0 )]
	public class GenericGumpPacket : UltimaPacket, IUltimaEntity
	{
		protected uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		protected int _GumpID;

		[UltimaPacketProperty( "Gump ID", "0x{0:X}" )]
		public int GumpID
		{
			get { return _GumpID; }
		}

		protected int _X;

		[UltimaPacketProperty]
		public int X
		{
			get { return _X; }
		}

		protected int _Y;

		[UltimaPacketProperty]
		public int Y
		{
			get { return _Y; }
		}

		protected List<GumpEntry> _Entries;

		[UltimaPacketProperty]
		public List<GumpEntry> Entries
		{
			get { return _Entries; }
		}

		protected List<string> _Text;

		[UltimaPacketProperty]
		public List<string> Text
		{
			get { return _Text; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_GumpID = reader.ReadInt32();
			_X = reader.ReadInt32();
			_Y = reader.ReadInt32();

			string layout = reader.ReadAsciiString();

			int textLength = reader.ReadInt16();
			_Text = new List<string>( textLength );

			for ( int i = 0; i < textLength; i++ )
			{
				_Text.Add( reader.ReadAsciiString() );
			}

			Parse( layout );
		}

		protected void Parse( string layout )
		{
			_Entries = new List<GumpEntry>();
			layout = layout.Replace( "}", "" );
			string[] splt = layout.Substring( 1 ).Split( '{' );

			foreach ( string s in splt )
			{
				try
				{
					string[] commands = SplitCommands( s );

					GumpEntry entry = GumpEntry.Create( commands, this );

					if ( entry != null )
						_Entries.Add( entry );
				}
				catch { }
			}
		}

		private static string[] SplitCommands( string s )
		{
			s = s.Trim();

			List<string> ret = new List<string>();
			bool stringCmd = false;
			string command;
			int start = 0;

			for ( int i = 0; i < s.Length; i++ )
			{
				char ch = s[ i ];

				if ( ch == ' ' || ch == '\t' )
				{
					if ( !stringCmd )
					{
						command = s.Substring( start, i - start );

						if ( !String.IsNullOrEmpty( command ) )
							ret.Add( command );

						start = i + 1;
					}
				}
				else if ( ch == '@' )
				{
					stringCmd = !stringCmd;
				}
			}

			command = s.Substring( start, s.Length - start );

			if ( !String.IsNullOrEmpty( command ) )
				ret.Add( command );

			return ret.ToArray();
		}
	}

	public abstract class GumpEntry
	{
		public static GumpEntry Create( string[] commands, GenericGumpPacket parent )
		{
			string command = commands[ 0 ].ToLower();

			if ( command.StartsWith( "kr_" ) )
				command = command.Substring( 3, command.Length - 3 );

			switch ( command )
			{
				case "nomove":
				return new GumpNotDragable( commands, parent );
				case "noclose":
				return new GumpNotClosable( commands, parent );
				case "nodispose":
				return new GumpNotDisposable( commands, parent );
				case "noresize":
				return new GumpNotResizable( commands, parent );
				case "checkertrans":
				return new GumpAlphaRegion( commands, parent );
				case "resizepic":
				return new GumpBackground( commands, parent );
				case "button":
				return new GumpButton( commands, parent );
				case "checkbox":
				return new GumpCheck( commands, parent );
				case "group":
				return new GumpGroup( commands, parent );
				case "htmlgump":
				return new GumpHtml( commands, parent );
				case "xmfhtmlgump":
				return new GumpHtmlLocalized( commands, parent );
				case "xmfhtmlgumpcolor":
				return new GumpHtmlLocalizedColor( commands, parent );
				case "xmfhtmltok":
				return new GumpHtmlLocalizedArgs( commands, parent );
				case "gumppic":
				return new GumpImage( commands, parent );
				case "gumppictiled":
				return new GumpImageTiled( commands, parent );
				case "buttontileart":
				return new GumpImageTiledButton( commands, parent );
				case "tilepic":
				return new GumpItem( commands, parent );
				case "tilepichue":
				return new GumpItemColor( commands, parent );
				case "text":
				return new GumpLabel( commands, parent );
				case "croppedtext":
				return new GumpLabelCropped( commands, parent );
				case "page":
				return new GumpPage( commands, parent );
				case "radio":
				return new GumpRadio( commands, parent );
				case "textentry":
				return new GumpTextEntry( commands, parent );
				case "tooltip":
				return new GumpTooltip( commands, parent );
				case "mastergump":
				return null;

				default:
				throw new ArgumentException();
			}
		}

		private string[] _Commands;

		[UltimaPacketProperty]
		public string[] Commands
		{
			get { return _Commands; }
		}

		private GenericGumpPacket _Parent;

		public GenericGumpPacket Parent
		{
			get { return _Parent; }
		}

		public GumpEntry( string[] commands, GenericGumpPacket parent )
		{
			_Commands = commands;
			_Parent = parent;
		}

		public int GetInt32( int n )
		{
			return Int32.Parse( _Commands[ n ] );
		}

		public uint GetUInt32( int n )
		{
			return UInt32.Parse( _Commands[ n ] );
		}

		public bool GetBoolean( int n )
		{
			return GetInt32( n ) != 0;
		}

		public string GetString( int n )
		{
			string cmd = _Commands[ n ];
			return cmd.Substring( 1, cmd.Length - 2 );
		}

		public string GetText( int n )
		{
			return _Parent.Text[ n ];
		}

		public static string Format( bool b )
		{
			return b ? "true" : "false";
		}

		public static string Format( string s )
		{
			return s.Replace( "\t", "\\t" );
		}

		public abstract string GetRunUOLine();
	}

	public class GumpNotDragable : GumpEntry
	{
		public GumpNotDragable( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
		}

		public override string GetRunUOLine()
		{
			return "Dragable = false;";
		}

		public override string ToString()
		{
			return "Not Dragable";
		}
	}

	public class GumpNotClosable : GumpEntry
	{
		public GumpNotClosable( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
		}

		public override string GetRunUOLine()
		{
			return "Closable = false;";
		}

		public override string ToString()
		{
			return "Not Closable";
		}
	}

	public class GumpNotDisposable : GumpEntry
	{
		public GumpNotDisposable( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
		}

		public override string GetRunUOLine()
		{
			return "Disposable = false;";
		}

		public override string ToString()
		{
			return "Not Disposable";
		}
	}

	public class GumpNotResizable : GumpEntry
	{
		public GumpNotResizable( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
		}

		public override string GetRunUOLine()
		{
			return "Resizable = false;";
		}

		public override string ToString()
		{
			return "Not Resizable";
		}
	}

	public class GumpAlphaRegion : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }

		public GumpAlphaRegion( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddAlphaRegion( {0}, {1}, {2}, {3} );", _X, _Y, _Width, _Height );
		}

		public override string ToString()
		{
			return string.Format( "Alpha Region: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\"",
				_X, _Y, _Width, _Height );
		}
	}

	public class GumpBackground : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private int _GumpId;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public int GumpId { get { return _GumpId; } }

		public GumpBackground( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_GumpId = GetInt32( 3 );
			_Width = GetInt32( 4 );
			_Height = GetInt32( 5 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddBackground( {0}, {1}, {2}, {3}, 0x{4:X} );", _X, _Y, _Width, _Height, _GumpId );
		}

		public override string ToString()
		{
			return string.Format( "Background: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", GumpId: \"0x{4:X}\"",
				_X, _Y, _Width, _Height, _GumpId );
		}
	}

	public class GumpButton : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _NormalId;
		private int _PressedId;
		private int _ButtonId;
		private int _Type;
		private int _Param;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int NormalId { get { return _NormalId; } }
		public int PressedId { get { return _PressedId; } }
		public int ButtonId { get { return _ButtonId; } }
		public int Type { get { return _Type; } }
		public int Param { get { return _Param; } }

		public GumpButton( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_NormalId = GetInt32( 3 );
			_PressedId = GetInt32( 4 );
			_Type = GetInt32( 5 );
			_Param = GetInt32( 6 );
			_ButtonId = GetInt32( 7 );
		}

		public override string GetRunUOLine()
		{
			string type = _Type == 0 ? "GumpButtonType.Page" : "GumpButtonType.Reply";
			return string.Format( "AddButton( {0}, {1}, 0x{2:X}, 0x{3:X}, {4}, {5}, {6} );",
				_X, _Y, _NormalId, _PressedId, _ButtonId, type, _Param );
		}

		public override string ToString()
		{
			return string.Format( "Button: \"X: \"{0}\", Y: \"{1}\", NormalId: \"0x{2:X}\", PressedId: \"0x{3:X}\", ButtonId: \"{4}\", Type: \"{5}\", Param: \"{6}\"",
				_X, _Y, _NormalId, _PressedId, _ButtonId, _Type, _Param );
		}
	}

	public class GumpCheck : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _InactiveId;
		private int _ActiveId;
		private bool _InitialState;
		private int _SwitchId;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int InactiveId { get { return _InactiveId; } }
		public int ActiveId { get { return _ActiveId; } }
		public bool InitialState { get { return _InitialState; } }
		public int SwitchId { get { return _SwitchId; } }

		public GumpCheck( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_InactiveId = GetInt32( 3 );
			_ActiveId = GetInt32( 4 );
			_InitialState = GetBoolean( 5 );
			_SwitchId = GetInt32( 6 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddCheck( {0}, {1}, 0x{2:X}, 0x{3:X}, {4}, {5} );",
				_X, _Y, _InactiveId, _ActiveId, Format( _InitialState ), _SwitchId );
		}

		public override string ToString()
		{
			return string.Format( "Check: X: \"{0}\", Y: \"{1}\", InactiveId: \"0x{2:X}\", ActiveId: \"0x{3:X}\", InitialState: \"{4}\", SwitchId: \"{5}\"",
				_X, _Y, _InactiveId, _ActiveId, _InitialState, _SwitchId );
		}
	}

	public class GumpGroup : GumpEntry
	{
		private int _Group;

		public int Group { get { return _Group; } }

		public GumpGroup( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_Group = GetInt32( 1 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddGroup( {0} );", _Group );
		}

		public override string ToString()
		{
			return string.Format( "Group: \"{0}\"", _Group );
		}
	}

	public class GumpHtml : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private string _Text;
		private bool _Background;
		private bool _Scrollbar;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public string Text { get { return _Text; } }
		public bool Background { get { return _Background; } }
		public bool Scrollbar { get { return _Scrollbar; } }

		public GumpHtml( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Text = GetText( GetInt32( 5 ) );
			_Background = GetBoolean( 6 );
			_Scrollbar = GetBoolean( 7 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddHtml( {0}, {1}, {2}, {3}, \"{4}\", {5}, {6} );",
				_X, _Y, _Width, _Height, Format( _Text ), Format( _Background ), Format( _Scrollbar ) );
		}

		public override string ToString()
		{
			return string.Format( "Html: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Text: \"{4}\", Background: \"{5}\", Scrollbar: \"{6}\"",
				_X, _Y, _Width, _Height, _Text, _Background, _Scrollbar );
		}
	}

	public class GumpHtmlLocalized : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private uint _Number;
		private bool _Background;
		private bool _Scrollbar;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public uint Number { get { return _Number; } }
		public bool Background { get { return _Background; } }
		public bool Scrollbar { get { return _Scrollbar; } }

		public GumpHtmlLocalized( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Number = GetUInt32( 5 );

			if ( commands.Length < 8 )
			{
				_Background = false;
				_Scrollbar = false;
			}
			else
			{
				_Background = GetBoolean( 6 );
				_Scrollbar = GetBoolean( 7 );
			}
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddHtmlLocalized( {0}, {1}, {2}, {3}, {4}, {5}, {6} );",
				_X, _Y, _Width, _Height, _Number, Format( _Background ), Format( _Scrollbar ) );
		}

		public override string ToString()
		{
			return string.Format( "HtmlLocalized: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Number: \"{4}\", Background: \"{5}\", Scrollbar: \"{6}\"",
				_X, _Y, _Width, _Height, _Number, _Background, _Scrollbar );
		}
	}

	public class GumpHtmlLocalizedColor : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private uint _Number;
		private int _Color;
		private bool _Background;
		private bool _Scrollbar;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public uint Number { get { return _Number; } }
		public int Color { get { return _Color; } }
		public bool Background { get { return _Background; } }
		public bool Scrollbar { get { return _Scrollbar; } }

		public GumpHtmlLocalizedColor( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Number = GetUInt32( 5 );
			_Background = GetBoolean( 6 );
			_Scrollbar = GetBoolean( 7 );
			_Color = GetInt32( 8 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddHtmlLocalized( {0}, {1}, {2}, {3}, {4}, 0x{5:X}, {6}, {7} );",
				_X, _Y, _Width, _Height, _Number, _Color, Format( _Background ), Format( _Scrollbar ) );
		}

		public override string ToString()
		{
			return string.Format( "HtmlLocalizedColor: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Number: \"{4}\", Color: \"0x{5:X}\", \"Background: \"{6}\", Scrollbar: \"{7}\"",
				_X, _Y, _Width, _Height, _Number, _Color, _Background, _Scrollbar );
		}
	}

	public class GumpHtmlLocalizedArgs : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private uint _Number;
		private string _Args;
		private int _Color;
		private bool _Background;
		private bool _Scrollbar;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public uint Number { get { return _Number; } }
		public string Args { get { return _Args; } }
		public int Color { get { return _Color; } }
		public bool Background { get { return _Background; } }
		public bool Scrollbar { get { return _Scrollbar; } }

		public GumpHtmlLocalizedArgs( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Background = GetBoolean( 5 );
			_Scrollbar = GetBoolean( 6 );
			_Color = GetInt32( 7 );
			_Number = GetUInt32( 8 );
			_Args = GetString( 9 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddHtmlLocalized( {0}, {1}, {2}, {3}, {4}, \"{5}\", 0x{6:X}, {7}, {8} );",
				_X, _Y, _Width, _Height, _Number, Format( _Args ), _Color, Format( _Background ), Format( _Scrollbar ) );
		}

		public override string ToString()
		{
			return string.Format( "HtmlLocalizedArgs: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Number: \"{4}\", Args: \"{5}\", Color: \"0x{6:X}\", \"Background: \"{7}\", Scrollbar: \"{8}\"",
				_X, _Y, _Width, _Height, _Number, _Args, _Color, _Background, _Scrollbar );
		}
	}

	public class GumpImage : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _GumpId;
		private int _Color;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int GumpId { get { return _GumpId; } }
		public int Color { get { return _Color; } }

		public GumpImage( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_GumpId = GetInt32( 3 );

			if ( commands.Length > 4 )
				_Color = Int32.Parse( commands[ 4 ].Substring( 4 ) );
			else
				_Color = 0;
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddImage( {0}, {1}, 0x{2:X}{3} );",
				_X, _Y, _GumpId, _Color != 0 ? ", 0x" + _Color.ToString( "X" ) : "" );
		}

		public override string ToString()
		{
			return string.Format( "Image: \"X: \"{0}\", Y: \"{1}\", GumpId: \"0x{2:X}\", Color: \"0x{3:X}\"",
				_X, _Y, _GumpId, _Color );
		}
	}

	public class GumpImageTiled : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private int _GumpId;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public int GumpId { get { return _GumpId; } }

		public GumpImageTiled( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_GumpId = GetInt32( 5 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddImageTiled( {0}, {1}, {2}, {3}, 0x{4:X} );",
				_X, _Y, _Width, _Height, _GumpId );
		}

		public override string ToString()
		{
			return string.Format( "ImageTiled: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", GumpId: \"0x{4:X}\"",
				_X, _Y, _Width, _Height, _GumpId );
		}
	}

	public class GumpImageTiledButton : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _NormalID;
		private int _PressedID;
		private int _ButtonID;
		private int _Type;
		private int _Param;

		private int _ItemID;
		private int _Hue;
		private int _Width;
		private int _Height;

		public int X { get { return _X; } }
		public int Y { get { return _Y; } }
		public int NormalID { get { return _NormalID; } }
		public int PressedID { get { return _PressedID; } }
		public int ButtonID { get { return _ButtonID; } }
		public int Type { get { return _Type; } }
		public int Param { get { return _Param; } }

		public int ItemID { get { return _ItemID; } }
		public int Hue { get { return _Hue; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }

		public GumpImageTiledButton( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_NormalID = GetInt32( 3 );
			_PressedID = GetInt32( 4 );
			_Type = GetInt32( 5 );
			_Param = GetInt32( 6 );
			_ButtonID = GetInt32( 7 );
			_ItemID = GetInt32( 8 );
			_Hue = GetInt32( 9 );
			_Width = GetInt32( 10 );
			_Height = GetInt32( 11 );
		}

		public override string GetRunUOLine()
		{
			string type = ( _Type == 0 ? "GumpButtonType.Page" : "GumpButtonType.Reply" );
			return String.Format( "AddImageTiledButton( {0}, {1}, 0x{2:X}, 0x{3:X}, 0x{4:X}, {5}, {6}, 0x{7:X}, 0x{8:X}, {9}, {10} );",
				_X, _Y, _NormalID, _PressedID, _ButtonID, type, _Param, _ItemID, _Hue, _Width, _Height );
		}

		public override string ToString()
		{
			return string.Format( "ImageTiledButton: \"X: \"{0}\", Y: \"{1}\", Id1: \"0x{2:X}\", Id2: \"0x{3:X}\", ButtonId: \"0x{4:X}\", Type: \"{5}\", Param: \"{6}\", ItemId: \"0x{7:X}\", Hue: \"0x{8:X}\", Width: \"{9}\", Height: \"{10}\"",
				_X, _Y, _NormalID, _PressedID, _ButtonID, _Type, _Param, _ItemID, _Hue, _Width, _Height );
		}
	}

	public class GumpItem : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _GumpId;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int GumpId { get { return _GumpId; } }

		public GumpItem( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_GumpId = GetInt32( 3 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddItem( {0}, {1}, 0x{2:X} );", _X, _Y, _GumpId );
		}

		public override string ToString()
		{
			return string.Format( "Item: \"X: \"{0}\", Y: \"{1}\", GumpId: \"0x{2:X}\"",
				_X, _Y, _GumpId );
		}
	}

	public class GumpItemColor : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _GumpId;
		private int _Color;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int GumpId { get { return _GumpId; } }
		public int Color { get { return _Color; } }

		public GumpItemColor( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_GumpId = GetInt32( 3 );
			_Color = GetInt32( 4 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddItem( {0}, {1}, 0x{2:X}, 0x{3:X} );",
				_X, _Y, _GumpId, _Color );
		}

		public override string ToString()
		{
			return string.Format( "ItemColor: \"X: \"{0}\", Y: \"{1}\", GumpId: \"0x{2:X}\", Color: \"0x{3:X}\"",
				_X, _Y, _GumpId, _Color );
		}
	}

	public class GumpLabel : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Color;
		private string _Text;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Color { get { return _Color; } }
		public string Text { get { return _Text; } }

		public GumpLabel( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Color = GetInt32( 3 );
			_Text = GetText( GetInt32( 4 ) );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddLabel( {0}, {1}, 0x{2:X}, \"{3}\" );",
				_X, _Y, _Color, Format( _Text ) );
		}

		public override string ToString()
		{
			return string.Format( "Label: \"X: \"{0}\", Y: \"{1}\", Color: \"0x{2:X}\", Text: \"{3}\"",
				_X, _Y, _Color, _Text );
		}
	}

	public class GumpLabelCropped : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private int _Color;
		private string _Text;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Color { get { return _Color; } }
		public string Text { get { return _Text; } }

		public GumpLabelCropped( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Color = GetInt32( 5 );
			_Text = GetText( GetInt32( 6 ) );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddLabelCropped( {0}, {1}, {2}, {3}, 0x{4:X}, \"{5}\" );",
				_X, _Y, _Width, _Height, _Color, Format( _Text ) );
		}

		public override string ToString()
		{
			return string.Format( "LabelCropped: \"X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Color: \"0x{4:X}\", Text: \"{5}\"",
				_X, _Y, _Width, _Height, _Color, _Text );
		}
	}

	public class GumpPage : GumpEntry
	{
		private int _Page;

		public int Page { get { return _Page; } }

		public GumpPage( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_Page = GetInt32( 1 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddPage( {0} );", _Page );
		}

		public override string ToString()
		{
			return string.Format( "Page: \"{0}\"", _Page );
		}
	}

	public class GumpRadio : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _InactiveId;
		private int _ActiveId;
		private bool _InitialState;
		private int _SwitchId;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int InactiveId { get { return _InactiveId; } }
		public int ActiveId { get { return _ActiveId; } }
		public bool InitialState { get { return _InitialState; } }
		public int SwitchId { get { return _SwitchId; } }

		public GumpRadio( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_InactiveId = GetInt32( 3 );
			_ActiveId = GetInt32( 4 );
			_InitialState = GetBoolean( 5 );
			_SwitchId = GetInt32( 6 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddRadio( {0}, {1}, 0x{2:X}, 0x{3:X}, {4}, {5} );",
				_X, _Y, _InactiveId, _ActiveId, Format( _InitialState ), _SwitchId );
		}

		public override string ToString()
		{
			return string.Format( "Radio: X: \"{0}\", Y: \"{1}\", InactiveId: \"0x{2:X}\", ActiveId: \"0x{3:X}\", InitialState: \"{4}\", SwitchId: \"{5}\"",
				_X, _Y, _InactiveId, _ActiveId, _InitialState, _SwitchId );
		}
	}

	public class GumpTextEntry : GumpEntry
	{
		private int _X;
		private int _Y;
		private int _Width;
		private int _Height;
		private int _Color;
		private int _EntryId;
		private string _InitialText;

		public int X { get { return _X; } }
		public int Y { get { return _X; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public int Color { get { return _Color; } }
		public int EntryId { get { return _EntryId; } }
		public string InitialText { get { return _InitialText; } }

		public GumpTextEntry( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_X = GetInt32( 1 );
			_Y = GetInt32( 2 );
			_Width = GetInt32( 3 );
			_Height = GetInt32( 4 );
			_Color = GetInt32( 5 );
			_EntryId = GetInt32( 6 );
			_InitialText = GetText( GetInt32( 7 ) );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddTextEntry( {0}, {1}, {2}, {3}, 0x{4:X}, {5}, \"{6}\" );",
				_X, _Y, _Width, _Height, _Color, _EntryId, Format( _InitialText ) );
		}

		public override string ToString()
		{
			return string.Format( "TextEntry: X: \"{0}\", Y: \"{1}\", Width: \"{2}\", Height: \"{3}\", Color: \"0x{4:X}\", EntryId: \"{5}\", Text: \"{6}\"",
				_X, _Y, _Width, _Height, _Color, _EntryId, _InitialText );
		}
	}

	public class GumpTooltip : GumpEntry
	{
		private int _Number;

		public int Number { get { return _Number; } }

		public GumpTooltip( string[] commands, GenericGumpPacket parent )
			: base( commands, parent )
		{
			_Number = GetInt32( 1 );
		}

		public override string GetRunUOLine()
		{
			return string.Format( "AddTooltip( {0} );", _Number );
		}

		public override string ToString()
		{
			return string.Format( "Tooltip: Number: \"{0}\"", _Number );
		}
	}
}
