using System;
using System.IO;
using Ultima.Package;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima gump generators.
	/// </summary>
	public class UltimaGumpGenerator
	{
		#region Methods
		/// <summary>
		/// Generates class and saves it to stream.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="gump">Gump to generate.</param>
		public static void Generate( Stream stream, GenericGumpPacket gump )
		{
			UltimaStringCollection clilocs = Globals.Instance.Clilocs;

			using ( UltimaClassWriter writer = new UltimaClassWriter( stream ) )
			{
				writer.WriteUsing( "System" );
				writer.WriteUsing( "Server" );
				writer.WriteUsing( "Server.Gumps" );
				writer.WriteUsing( "Server.Network" );
				writer.WriteLine();
				writer.BeginNamespace( "Server.Gumps" );

				string className = "GenericGump";

				writer.BeginClass( className, "Gump" );
				writer.BeginConstructor( "public", className, null, String.Format( "{0}, {1}", gump.X, gump.Y ) );

				for ( int i = 0; i < gump.Entries.Count; i++ )
				{
					GumpEntry entry = gump.Entries[ i ];
					bool space = entry is GumpPage;

					if ( space && i != 0 )
						writer.WriteLine();

					writer.WriteWithIndent( entry.GetRunUOLine() );

					// Comment
					int cliloc = 0;

					if ( entry is GumpHtmlLocalized )
						cliloc = (int) ( (GumpHtmlLocalized) entry ).Number;
					else if ( entry is GumpHtmlLocalizedColor )
						cliloc = (int) ( (GumpHtmlLocalizedColor) entry ).Number;
					else if ( entry is GumpHtmlLocalizedArgs )
						cliloc = (int) ( (GumpHtmlLocalizedArgs) entry ).Number;

					if ( cliloc > 0 && clilocs != null )
					{
						string clilocText = clilocs.GetString( cliloc );

						if ( !String.IsNullOrEmpty( clilocText ) )
							writer.WriteLine( " // {0}", clilocText );
						else
							writer.WriteLine();
					}
					else
						writer.WriteLine();


					if ( space && i < gump.Entries.Count )
						writer.WriteLine();
				}

				writer.EndConstructor();
				writer.WriteLine();
				writer.BeginOverrideMethod( "public", "void", "OnResponse", "NetState sender, RelayInfo info" );
				writer.EndMethod();
				writer.EndClass();
				writer.EndNamespace();

				App.Window.ShowNotification( NotificationType.Info, "Gump generation complete" );
			}
		}
		#endregion
	}
}
