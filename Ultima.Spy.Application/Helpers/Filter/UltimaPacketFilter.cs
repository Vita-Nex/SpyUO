using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Packet filter.
	/// </summary>
	public class UltimaPacketFilter : DependencyObject
	{
		#region Properties
		private const int FilterCode = 0x42524150; // CRAP
		private const int FilterVersion = 1;

		/// <summary>
		/// Represents Table property.
		/// </summary>
		public static readonly DependencyProperty TableProperty = DependencyProperty.Register(
			"Table", typeof( UltimaPacketFilterTable ), typeof( UltimaPacketFilter ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets filter table.
		/// </summary>
		public UltimaPacketFilterTable Table
		{
			get { return GetValue( TableProperty ) as UltimaPacketFilterTable; }
			set { SetValue( TableProperty, value ); }
		}

		/// <summary>
		/// Represents Active property.
		/// </summary>
		public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register(
			"Active", typeof( bool ), typeof( UltimaPacketFilter ), 
			new PropertyMetadata( true, new PropertyChangedCallback( Active_Changed ) ) );

		/// <summary>
		/// Gets or sets the filter state.
		/// </summary>
		public bool Active
		{
			get { return (bool) GetValue( ActiveProperty ); }
			set { SetValue( ActiveProperty, value ); }
		}

		/// <summary>
		/// Represents CommonVisible property.
		/// </summary>
		public static readonly DependencyProperty ShowAllProperty = DependencyProperty.Register(
			"ShowAll", typeof( bool ), typeof( UltimaPacketFilter ),
			new PropertyMetadata( false, new PropertyChangedCallback( ShowAll_Changed ) ) );

		/// <summary>
		/// Gets or sets the state of common packets.
		/// </summary>
		public bool ShowAll
		{
			get { return (bool) GetValue( ShowAllProperty ); }
			set { SetValue( ShowAllProperty, value ); }
		}

		/// <summary>
		/// Represents Filter property.
		/// </summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
			"Filter", typeof( string ), typeof( UltimaPacketFilter ),
			new PropertyMetadata( null, new PropertyChangedCallback( Filter_Changed ) ) );

		/// <summary>
		/// Gets or sets packet filter.
		/// </summary>
		public string Filter
		{
			get { return GetValue( FilterProperty ) as string; }
			set { SetValue( FilterProperty, value ); }
		}

		private bool _IsBusy;
		#endregion

		#region Events
		/// <summary>
		/// Occurs when filter changes.
		/// </summary>
		public event EventHandler Changed;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new UltimaPacketFilter.
		/// </summary>
		public UltimaPacketFilter()
		{
		}
		#endregion

		#region Methods
		private static void Active_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilter filter = d as UltimaPacketFilter;

			if ( filter != null )
				filter.OnChange();
		}

		private static void ShowAll_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilter filter = d as UltimaPacketFilter;

			if ( filter != null && !filter._IsBusy )
			{
				if ( filter.ShowAll )
					filter.Table.ShowAll();
				else
					filter.Table.HideUnknown();

				filter.OnChange();
			}
		}

		private static void Filter_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketFilter filter = d as UltimaPacketFilter;

			if ( filter != null )
				filter.Table.Filter( filter.Filter );
		}

		/// <summary>
		/// Initializes filter structure.
		/// </summary>
		public void Initialize()
		{
			UltimaPacketTable table = UltimaPacket.PacketTable;

			if ( table == null )
				throw new SpyException( "Y U NO initialize packet definitions" );

			Table = new UltimaPacketFilterTable( table, this );
		}

		/// <summary>
		/// Loads filter from file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public void Load( string filePath )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				Load( stream );
			}
		}

		/// <summary>
		/// Loads filter from stream.
		/// </summary>
		/// <param name="stream">Stream to load from.</param>
		public void Load( Stream stream )
		{
			try
			{
				_IsBusy = true;

				using ( BinaryReader reader = new BinaryReader( stream ) )
				{
					int format = reader.ReadInt32();

					if ( format != FilterCode )
						throw new SpyException( "Invalid file tag: 0x{0:X}", format );

					int version = reader.ReadInt32();

					if ( version > FilterVersion )
						throw new SpyException( "Unsupported version: {0}", version );

					ShowAll = reader.ReadBoolean();

					Table.Load( reader );
				}
			}
			finally
			{
				_IsBusy = false;
			}

			OnChange();
		}

		/// <summary>
		/// Saves filter to file.
		/// </summary>
		/// <param name="filePath">File path.</param>
		public void Save( string filePath )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Create, FileAccess.Write, FileShare.Write ) )
			{
				Save( stream );
			}
		}

		/// <summary>
		/// Saves filter to stream.
		/// </summary>
		/// <param name="stream">Stream to save to.</param>
		public void Save( Stream stream )
		{
			using ( BinaryWriter writer = new BinaryWriter( stream ) )
			{
				writer.Write( (int) FilterCode );
				writer.Write( (int) FilterVersion );
				writer.Write( (bool) ShowAll );

				Table.Save( writer );
			}
		}

		/// <summary>
		/// Checks if packet is displayed in packet list.
		/// </summary>
		/// <param name="packet">Packet to check.</param>
		/// <returns>True if visible, false otherwise.</returns>
		public bool IsDisplayed( UltimaPacket packet )
		{
			UltimaPacketFilterTable table = Table;
			UltimaPacketFilterTable childTable = null;
			IUltimaPacketFilterEntry item = null;
			int i = 0;

			do
			{
				item = table[ packet.Data[ i++ ] ];
				childTable = item as UltimaPacketFilterTable;

				if ( childTable != null )
				{
					if ( !childTable.IsChecked )
						return false;

					table = childTable;
				}
			}
			while ( childTable != null );

			UltimaPacketFilterEntry entry = item as UltimaPacketFilterEntry;

			if ( entry != null )
				return entry.IsVisible && entry.IsChecked && entry.IsDisplayed( packet );

			return false;
		}

		/// <summary>
		/// Triggers changed event.
		/// </summary>
		public void OnChange()
		{
			if ( Changed != null && !_IsBusy )
				Changed( this, new EventArgs() );
		}
		#endregion
	}
}
