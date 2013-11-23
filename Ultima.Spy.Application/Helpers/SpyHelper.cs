using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Spy helper.
	/// </summary>
	public class SpyHelper : DependencyObject
	{
		#region Properties
		/// <summary>
		/// Gets enhanced client folder.
		/// </summary>
		public static string ClassicClientFolder
		{
			get
			{
				string folder = null;
				string keyPath;

				if ( SystemInfo.IsX64 )
					keyPath = "SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Ultima Online Classic";
				else
					keyPath = "SOFTWARE\\Electronic Arts\\EA Games\\Ultima Online Classic";

				RegistryKey key = Registry.LocalMachine.OpenSubKey( keyPath );

				if ( key != null )
					folder = key.GetValue( "InstallDir" ) as string;

				if ( String.IsNullOrEmpty( folder ) )
					folder = @"C:\Program Files\Electronic Arts\Ultima Online Classic\";

				if ( Directory.Exists( folder ) )
					return folder;

				return null;
			}
		}

		/// <summary>
		/// Gets enhanced client folder.
		/// </summary>
		public static string EnhancedClientFolder
		{
			get
			{
				string folder = null;
				string keyPath;

				if ( SystemInfo.IsX64 )
					keyPath = "SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Ultima Online Enhanced";
				else
					keyPath = "SOFTWARE\\Electronic Arts\\EA Games\\Ultima Online Enhanced";

				RegistryKey key = Registry.LocalMachine.OpenSubKey( keyPath );

				if ( key != null )
					folder = key.GetValue( "InstallDir" ) as string;

				if ( String.IsNullOrEmpty( folder ) )
					folder = @"C:\Program Files\Electronic Arts\Ultima Online Enhanced\";

				if ( Directory.Exists( folder ) )
					return folder;

				return null;
			}
		}

		/// <summary>
		/// Represents Active property.
		/// </summary>
		public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register(
			"Active", typeof( bool ), typeof( SpyHelper ), new PropertyMetadata( false ) );

		/// <summary>
		/// Determines whether spy is active or not.
		/// </summary>
		public bool Active
		{
			get { return (bool) GetValue( ActiveProperty ); }
			set { SetValue( ActiveProperty, value ); }
		}

		/// <summary>
		/// Represents CanStart property.
		/// </summary>
		public static readonly DependencyProperty CanStartProperty = DependencyProperty.Register(
			"CanStart", typeof( bool ), typeof( SpyHelper ), new PropertyMetadata( true ) );

		/// <summary>
		/// Determines whether spy can be started.
		/// </summary>
		public bool CanStart
		{
			get { return (bool) GetValue( CanStartProperty ); }
			set { SetValue( CanStartProperty, value ); }
		}

		/// <summary>
		/// Represents CanAttach property.
		/// </summary>
		public static readonly DependencyProperty CanAttachProperty = DependencyProperty.Register(
			"CanAttach", typeof( bool ), typeof( SpyHelper ), new PropertyMetadata( true ) );

		/// <summary>
		/// Determines whether spy can be attached.
		/// </summary>
		public bool CanAttach
		{
			get { return (bool) GetValue( CanAttachProperty ); }
			set { SetValue( CanAttachProperty, value ); }
		}

		/// <summary>
		/// Represents CanPause property.
		/// </summary>
		public static readonly DependencyProperty CanPauseProperty = DependencyProperty.Register(
			"CanPause", typeof( bool ), typeof( SpyHelper ), new PropertyMetadata( false ) );

		/// <summary>
		/// Determines whether spy can be paused.
		/// </summary>
		public bool CanPause
		{
			get { return (bool) GetValue( CanPauseProperty ); }
			set { SetValue( CanPauseProperty, value ); }
		}

		/// <summary>
		/// Represents CanStop property.
		/// </summary>
		public static readonly DependencyProperty CanStopProperty = DependencyProperty.Register(
			"CanStop", typeof( bool ), typeof( SpyHelper ), new PropertyMetadata( false ) );

		/// <summary>
		/// Determines whether spy can be stopped.
		/// </summary>
		public bool CanStop
		{
			get { return (bool) GetValue( CanStopProperty ); }
			set { SetValue( CanStopProperty, value ); }
		}

		/// <summary>
		/// Represents Path property.
		/// </summary>
		public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
			"Path", typeof( string ), typeof( SpyHelper ), new PropertyMetadata( null ) );

		/// <summary>
		/// Determines whether spy can be stopped.
		/// </summary>
		public string Path
		{
			get { return GetValue( PathProperty ) as string; }
			set { SetValue( PathProperty, value ); }
		}

		/// <summary>
		/// Represents Count property.
		/// </summary>
		public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
			"Count", typeof( int ), typeof( SpyHelper ), new PropertyMetadata( 0 ) );

		/// <summary>
		/// Gets or sets number of packets.
		/// </summary>
		public int Count
		{
			get { return (int) GetValue( CountProperty ); }
			set { SetValue( CountProperty, value ); }
		}

		private ClientSpy _Spy;

		/// <summary>
		/// Gets reference to client spy.
		/// </summary>
		public ClientSpy Spy
		{
			get { return _Spy; }
		}

		private Process _Client;

		/// <summary>
		/// Gets reference to client process.
		/// </summary>
		public Process Client
		{
			get { return _Client; }
		}

		/// <summary>
		/// Determines whether spy is enhanced.
		/// </summary>
		public bool IsEnhancedClient
		{
			get { return _Spy is EnhancedClientSpy; }
		}

		private SmartObservableCollection<UltimaPacket> _Packets;

		/// <summary>
		/// Gets a list of all packets intercepted.
		/// </summary>
		public SmartObservableCollection<UltimaPacket> Packets
		{
			get { return _Packets; }
		}

		private ICollectionView _PacketsView;

		/// <summary>
		/// Gets default packet view.
		/// </summary>
		public ICollectionView PacketsView
		{
			get { return _PacketsView; }
		}

		/// <summary>
		/// Determines whether spy is paused;
		/// </summary>
		public bool IsPaused
		{
			get { return !Active && _Client != null; }
		}
		#endregion

		#region Events
		public event Action<UltimaPacket> OnPacket;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of SpyHelper.
		/// </summary>
		public SpyHelper()
		{
			_Packets = new SmartObservableCollection<UltimaPacket>();
			_PacketsView = CollectionViewSource.GetDefaultView( _Packets );
		}
		#endregion

		#region Members
		/// <summary>
		/// Attaches spy to process.
		/// </summary>
		/// <param name="process">Process to attach.</param>
		public void Attach( Process process )
		{
			_Spy = ClientSpyStarter.Initialize( process );
			_Spy.OnPacket += new Action<byte[],bool>( Spy_OnPacket );
			_Spy.OnStopped += new Action<SpyStoppedArgs>( Spy_OnStopped );
			_Spy.AttachAsync( process );

			Active = true;
			CanStart = false;
			CanAttach = false;
			CanPause = true;
			CanStop = true;
			Path = process.MainModule.FileName;
		}

		/// <summary>
		/// Starts executable and attaches spy to it.
		/// </summary>
		/// <param name="filePath">Path to executable.</param>
		public void Start( string filePath )
		{
			_Spy = ClientSpyStarter.Initialize( filePath, out _Client );
			_Spy.OnPacket += new Action<byte[],bool>( Spy_OnPacket );
			_Spy.OnStopped += new Action<SpyStoppedArgs>( Spy_OnStopped );
			_Spy.AttachAsync( _Client );

			Active = true;
			CanStart = false;
			CanAttach = false;
			CanPause = true;
			CanStop = true;
			Path = filePath;
		}

		/// <summary>
		/// Pauses spy.
		/// </summary>
		public void Pause()
		{
			if ( CanPause )
			{
				_Spy.Stop();

				CanStart = true;
				CanAttach = false;
				CanPause = false;
				CanStop = true;
				Path = null;
			}
		}

		/// <summary>
		/// Resumes spy.
		/// </summary>
		public void Resume()
		{
			if ( IsPaused )
				Attach( _Client );
		}

		/// <summary>
		/// Stops spy.
		/// </summary>
		public void Stop()
		{
			if ( CanStop )
			{
				if ( Active )
					_Spy.Stop();

				_Client = null;
			}
		}

		/// <summary>
		/// Clears packets.
		/// </summary>
		public void ClearPackets()
		{
			Count = 0;
			_Packets.Clear();
		}

		/// <summary>
		/// Saves to binary file.
		/// </summary>
		/// <param name="filePath">File to save to.</param>
		public void SaveBinary( string filePath )
		{
			using ( FileStream stream = File.Create( filePath ) )
			{
				using ( BinaryWriter writer = new BinaryWriter( stream ) )
				{
					writer.Write( (int) 0 ); // version
					writer.Write( (int) _Packets.Count );

					foreach ( UltimaPacket packet in _Packets )
					{
						packet.Save( writer );
					}
				}
			}
		}

		/// <summary>
		/// Reads packets from binary file.
		/// </summary>
		/// <param name="filePath">File to read from.</param>
		public void LoadBinary( string filePath )
		{
			List<UltimaPacket> packets = new List<UltimaPacket>();

			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				using ( BinaryReader reader = new BinaryReader( stream ) )
				{
					int version = reader.ReadInt32();

					if ( version > 0 )
						throw new SpyException( "Unsupported file version" );

					int count = reader.ReadInt32();

					for ( int i = 0; i < count; i++ )
					{
						try
						{
							UltimaPacket packet = UltimaPacket.ConstructPacket( reader );

							if ( packet != null )
								packets.Add( packet );
						}
						catch ( Exception ex )
						{
							App.Window.ShowNotification( NotificationType.Error, ex );
						}
					}
				}
			}

			App.Current.Dispatcher.BeginInvoke( new Action<List<UltimaPacket>>( LoadBinary_Completed ), packets );
		}

		private void LoadBinary_Completed( List<UltimaPacket> items )
		{
			_Packets.AddRange( items );
		}

		/// <summary>
		/// Adds packet to list.
		/// </summary>
		/// <param name="packet">Packet to add.</param>
		public void AddPacket( UltimaPacket packet )
		{
			try
			{
				Count++;
				_Packets.Add( packet );

				if ( OnPacket != null )
					OnPacket( packet );
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		/// <summary>
		/// Finds all entities with specific serial.
		/// </summary>
		/// <param name="serial">Serial to find by.</param>
		/// <returns>Collection of entities.</returns>
		public ObservableCollection<UltimaPacket> FindEntities( uint serial )
		{
			ObservableCollection<UltimaPacket> packets = new ObservableCollection<UltimaPacket>();

			foreach ( UltimaPacket packet in _Packets )
			{
				IUltimaEntity entity = packet as IUltimaEntity;

				if ( entity != null && entity.Serial == serial )
					packets.Add( packet );
			}

			return packets;
		}

		/// <summary>
		/// Finds first packet by type.
		/// </summary>
		/// <param name="serial">Serial to find by.</param>
		/// <param name="type">Type to find by.</param>
		/// <returns>Ultima packet.</returns>
		public UltimaPacket FindFirstPacket( uint serial, Type type = null )
		{
			if ( type != null )
			{
				foreach ( UltimaPacket packet in _Packets )
				{
					if ( !packet.Definition.IsDefault )
					{
						IUltimaEntity entity = packet as IUltimaEntity;

						if ( entity != null && entity.Serial == serial && packet.GetType() == type )
							return packet;
					}
				}
			}
			else
			{
				foreach ( UltimaPacket packet in _Packets )
				{
					if ( !packet.Definition.IsDefault )
					{
						IUltimaEntity entity = packet as IUltimaEntity;

						if ( entity != null && entity.Serial == serial )
							return packet;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Finds container item.
		/// </summary>
		/// <param name="serial">Item serial.</param>
		/// <returns>Container item.</returns>
		public ContainerItem FindContainerItem( uint serial )
		{
			foreach ( UltimaPacket packet in _Packets )
			{
				ContainerContentPacket container = packet as ContainerContentPacket;

				if ( container != null )
				{
					foreach ( ContainerItem item in container.Items )
						if ( item.Serial == serial )
							return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Finds packets by type.
		/// </summary>
		/// <param name="serial">Serial to find by.</param>
		/// <param name="type">Type to find by.</param>
		/// <returns>List of ultima packets.</returns>
		public List<UltimaPacket> FindPackets( uint serial, Type type = null )
		{
			List<UltimaPacket> list = new List<UltimaPacket>();

			if ( type != null )
			{
				foreach ( UltimaPacket packet in _Packets )
				{
					if ( packet.Definition != null && !packet.Definition.IsDefault )
					{
						IUltimaEntity entity = packet as IUltimaEntity;

						if ( entity.Serial == serial && packet.GetType() == type )
							list.Add( packet );
					}
				}
			}
			else
			{
				foreach ( UltimaPacket packet in _Packets )
				{
					if ( packet.Definition != null && !packet.Definition.IsDefault )
					{
						IUltimaEntity entity = packet as IUltimaEntity;

						if ( entity.Serial == serial )
							list.Add( packet );
					}
				}
			}

			return list;
		}

		#region Event Handlers
		private void Spy_OnPacket( byte[] data, bool fromClient )
		{
			App.Current.Dispatcher.BeginInvoke( new Action<byte[], bool, DateTime>( Spy_OnPacketSafe ), data, fromClient, DateTime.Now );
		}

		private void Spy_OnPacketSafe( byte[] data, bool fromClient, DateTime time )
		{
			UltimaPacket packet = null;

			try
			{
				packet = UltimaPacket.ConstructPacket( data, fromClient, time );
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}

			if ( packet != null )
				AddPacket( packet );
		}

		private void Spy_OnStopped( SpyStoppedArgs args )
		{
			App.Current.Dispatcher.BeginInvoke( new Action<SpyStoppedArgs>( Spy_OnStoppedSafe ), args );
		}

		private void Spy_OnStoppedSafe( SpyStoppedArgs args )
		{
			Active = false;
			CanStart = true;
			CanAttach = true;
			CanPause = false;
			CanStop = false;
			Path = null;

			if ( args.StopType == SpyStopType.Closed )
			{
				_Client = null;
			}
			else if ( args.StopType == SpyStopType.Error )
			{
				App.Window.ShowNotification( NotificationType.Error, args.Error );
			}
		}
		#endregion
		#endregion
	}
}
