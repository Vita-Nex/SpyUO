using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Ultima.Package
{
	/// <summary>
	/// Describes VLC player.
	/// <remarks>VLC player (1.1.0 or newer) must be installed for this to work.</remarks>
	/// </summary>
	public class VlcPlayer : IDisposable
	{
		#region Properties
		/// <summary>
		/// Gets VLC installation folder.
		/// </summary>
		public static string DefaultInstallationFolder
		{
			get
			{
				string folder = null;
				string keyPath;

				if ( SystemInfo.IsX64 )
					keyPath = "SOFTWARE\\Wow6432Node\\VideoLAN\\VLC";
				else
					keyPath = "SOFTWARE\\VideoLAN\\VLC";

				RegistryKey key = Registry.LocalMachine.OpenSubKey( keyPath );

				if ( key != null )
				{
					folder = key.GetValue( "InstallDir" ) as string;
					key.Close();
				}

				if ( String.IsNullOrEmpty( folder ) )
				{
					if ( SystemInfo.IsX64 )
						folder = @"C:\Program Files (x86)\VideoLAN\VLC\";
					else
						folder = @"C:\Program Files\VideoLAN\VLC\";
				}

				if ( Directory.Exists( folder ) )
					return folder;

				return null;
			}
		}

		/// <summary>
		/// Gets VLC version.
		/// </summary>
		public static Version DefaultVersion
		{
			get
			{
				string keyPath;

				if ( SystemInfo.IsX64 )
					keyPath = "SOFTWARE\\Wow6432Node\\VideoLAN\\VLC";
				else
					keyPath = "SOFTWARE\\VideoLAN\\VLC";

				RegistryKey key = Registry.LocalMachine.OpenSubKey( keyPath );

				if ( key != null )
				{
					string version = key.GetValue( "Version" ) as string;
					Version actual;

					key.Close();

					if ( Version.TryParse( version, out actual ) )
						return actual;
				}

				string folder = DefaultInstallationFolder;

				if ( folder != null )
				{
					string file = Path.Combine( folder, "vlc.exe" );

					if ( File.Exists( file ) )
					{
						FileVersionInfo info = FileVersionInfo.GetVersionInfo( file );

						if ( info == null )
							return null;

						Version actual;

						if ( Version.TryParse( info.FileVersion, out actual ) )
							return actual;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Gets required version (1.1.0).
		/// </summary>
		public static Version RequiredVersion
		{
			get { return new Version( 1, 1, 0 ); }
		}

		private static bool _Inititalized = false;

		/// <summary>
		/// Determines whether player is initialized.
		/// </summary>
		public static bool Inititalized
		{
			get { return _Inititalized; }
		}

		private static string _PluginsFolder;
		private static IntPtr _CoreLibraryAddress;
		private static IntPtr _LibraryAddress;

		// Delegates
		private static New _New;
		private static Release _Release;
		private static CreateMediaFromPath _CreateMediaFromPath;
		private static ReleaseMedia _ReleaseMedia;
		private static CreateMediaPlayerFromMedia _CreateMediaPlayerFromMedia;
		private static ReleasePlayer _ReleasePlayer;
		private static IsPlayingMedia _IsPlayingMedia;
		private static PlayMedia _PlayMedia;
		private static PauseMedia _PauseMedia;
		private static StopMedia _StopMedia;
		private static GetMediaLength _GetMediaLength;
		private static GetPlayerTime _GetPlayerTime;
		private static SetPlayerTime _SetPlayerTime;
		private static GetEventManager _GetEventManager;
		private static EventAttach _EventAttach;

		private bool _IsPlaying;

		/// <summary>
		/// Determines whether player is playing.
		/// </summary>
		public bool IsPlaying
		{
			get { return _IsPlaying; }
		}

		private long _Length;

		/// <summary>
		/// Gets media length in milliseconds.
		/// </summary>
		public long Length
		{
			get { return _Length; }
		}

		/// <summary>
		/// Gets or sets current time.
		/// </summary>
		public long Time
		{
			get
			{
				if ( _Player != IntPtr.Zero )
					return _GetPlayerTime( _Player );

				return 0;
			}
			set
			{
				if ( _Player != IntPtr.Zero )
					_SetPlayerTime( _Player, value );
			}
		}

		/// <summary>
		/// Determines whether media is loaded.
		/// </summary>
		public bool IsMediaLoaded
		{
			get { return _Media != IntPtr.Zero; }
		}

		// Player pointers
		private IntPtr _Instance;
		private IntPtr _Media;
		private IntPtr _Player;
		private IntPtr _EventManager;

		private VlcEventCallback _TimeChanged;
		private GCHandle _TimeChangedHandle;
		private VlcEventCallback _Stopped;
		private GCHandle _StoppedHandle;

		// Temporary file used to play stuff
		private string _FileStreamPath;
		private FileStream _FileStream;
		#endregion

		#region Events
		/// <summary>
		/// Occurs when player time changes when playing.
		/// </summary>
		public event Action TimeChanged;

		/// <summary>
		/// Occurs when player stops.
		/// </summary>
		public event Action Stopped;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of VlcPlayer.
		/// </summary>
		public VlcPlayer()
		{
			_FileStreamPath = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() );
			_Instance = _New( 4, new string[] { "-I", "dummy", "--ignore-config", _PluginsFolder } );

			if ( _Instance == IntPtr.Zero )
				throw new Exception( "Cannot create an instance of VLC player" );
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~VlcPlayer()
		{
			Dispose( true );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Loads media from memory.
		/// </summary>
		/// <param name="data">Data to load.</param>
		public void LoadFromMemory( byte[] data )
		{
			_Length = 0;

			if ( IsPlaying )
				Stop();

			if ( _Player != IntPtr.Zero )
			{
				_ReleasePlayer( _Player );
				_Player = IntPtr.Zero;
			}

			if ( _Media != IntPtr.Zero )
			{
				_ReleaseMedia( _Media );
				_Media = IntPtr.Zero;
			}

			if ( _FileStreamPath == null )
				_FileStreamPath = Path.Combine( Path.GetTempPath(), Path.GetRandomFileName() );

			// Fugly walkaround for imem (stupid callbacks)
			if ( _FileStream == null )
				_FileStream = File.Open( _FileStreamPath, FileMode.Create, FileAccess.Write, FileShare.Read );

			_FileStream.Seek( 0, SeekOrigin.Begin );
			_FileStream.Write( data, 0, data.Length );
			_FileStream.Flush();

			_Media = _CreateMediaFromPath( _Instance, _FileStreamPath );

			if ( _Media == IntPtr.Zero )
				throw new Exception( "Error opening media" );

			if ( _Player == IntPtr.Zero )
			{
				_Player = _CreateMediaPlayerFromMedia( _Media );

				if ( _Player == IntPtr.Zero )
					throw new Exception( "Cannot create media player from media" );

				_EventManager = _GetEventManager( _Player );

				if ( _EventManager == IntPtr.Zero )
					throw new Exception( "Cannot get instance event manager" );

				_TimeChanged = OnTimeChanged;
				_TimeChangedHandle = GCHandle.Alloc( _TimeChanged );
				_EventAttach( _EventManager, VlcEvent.MediaPlayerPositionChanged, _TimeChanged, IntPtr.Zero );
				_Stopped = OnStopped;
				_StoppedHandle = GCHandle.Alloc( _Stopped );
				_EventAttach( _EventManager, VlcEvent.MediaPlayerStopped, _Stopped, IntPtr.Zero );
			}
		}

		/// <summary>
		/// Loads media from file.
		/// </summary>
		/// <param name="filePath">File to load.</param>
		public void LoadFromFile( string filePath )
		{
			_Length = 0;

			if ( IsPlaying )
				Stop();

			if ( _Player != IntPtr.Zero )
			{
				_ReleasePlayer( _Player );
				_Player = IntPtr.Zero;
			}

			if ( _Media != IntPtr.Zero )
			{
				_ReleaseMedia( _Media );
				_Media = IntPtr.Zero;
			}

			_Media = _CreateMediaFromPath( _Instance, filePath );

			if ( _Media == IntPtr.Zero )
				throw new Exception( "Error opening media" );

			if ( _Player == IntPtr.Zero )
			{
				_Player = _CreateMediaPlayerFromMedia( _Media );

				if ( _Player == IntPtr.Zero )
					throw new Exception( "Cannot create media player from media" );

				_EventManager = _GetEventManager( _Player );

				if ( _EventManager == IntPtr.Zero )
					throw new Exception( "Cannot get instance event manager" );

				_TimeChanged = OnTimeChanged;
				_TimeChangedHandle = GCHandle.Alloc( _TimeChanged );
				_EventAttach( _EventManager, VlcEvent.MediaPlayerPositionChanged, _TimeChanged, IntPtr.Zero );
				_Stopped = OnStopped;
				_StoppedHandle = GCHandle.Alloc( _Stopped );
				_EventAttach( _EventManager, VlcEvent.MediaPlayerStopped, _Stopped, IntPtr.Zero );
			}
		}

		/// <summary>
		/// Plays file.
		/// </summary>
		public void Play()
		{
			if ( IsPlaying )
				return;

			if ( _Player == IntPtr.Zero )
				throw new Exception( "Media not loaded" );

			_PlayMedia( _Player );
			_IsPlaying = true;
		}

		/// <summary>
		/// Toggles pause.
		/// </summary>
		public void Pause()
		{
			if ( !IsPlaying )
				return;

			_PauseMedia( _Player );
			_IsPlaying = false;
		}

		/// <summary>
		/// Stops playing.
		/// </summary>
		public void Stop()
		{
			if ( !IsPlaying )
				return;

			_StopMedia( _Player );
			_IsPlaying = false;
		}

		private void OnTimeChanged( IntPtr eventData, IntPtr userData )
		{
			try
			{
				if ( _Length == 0 )
					_Length = _GetMediaLength( _Player );
			}
			catch
			{
			}

			if ( TimeChanged != null )
				TimeChanged();
		}

		private void OnStopped( IntPtr eventData, IntPtr userData )
		{
			_IsPlaying = false;

			if ( Stopped != null )
				Stopped();
		}

		/// <summary>
		/// Gets VLC version.
		/// </summary>
		/// <param name="vlcInstallationFolder">VLC installation folder.</param>
		public static Version GetVersion( string vlcInstallationFolder )
		{
			string folder = vlcInstallationFolder;

			if ( folder != null )
			{
				string file = Path.Combine( folder, "vlc.exe" );

				if ( File.Exists( file ) )
				{
					FileVersionInfo info = FileVersionInfo.GetVersionInfo( file );

					if ( info == null )
						return null;

					Version actual;

					if ( Version.TryParse( info.FileVersion, out actual ) )
						return actual;
				}
			}

			return null;
		}

		/// <summary>
		/// Initializes VLC.
		/// </summary>
		/// <param name="vlcInstallationFolder">VLC installation folder.</param>
		public static void Initialize( string vlcInstallationFolder )
		{
			if ( _Inititalized )
				return;

			// Load dynamic dll
			string dllPath = Path.Combine( vlcInstallationFolder, "libvlccore.dll" );

			if ( !File.Exists( dllPath ) )
				throw new Exception( "Cannot find '" + dllPath + "'" );

			_CoreLibraryAddress = LoadLibrary( dllPath );

			if ( _CoreLibraryAddress == IntPtr.Zero )
				throw new Exception( "Cannot load '" + dllPath + "'" );

			dllPath = Path.Combine( vlcInstallationFolder, "libvlc.dll" );

			if ( !File.Exists( dllPath ) )
				throw new Exception( "Cannot find '" + dllPath + "'" );

			_LibraryAddress = LoadLibrary( dllPath );

			if ( _LibraryAddress == IntPtr.Zero )
				throw new Exception( "Cannot load '" + dllPath + "'" );

			if ( GetVersion( vlcInstallationFolder ) < RequiredVersion )
				throw new Exception( String.Format( "Invalid VLC version. Minimum version required is {0}", RequiredVersion ) );

			// Get plugin folder
			_PluginsFolder = Path.Combine( vlcInstallationFolder, "plugins" );

			if ( !Directory.Exists( _PluginsFolder ) )
				throw new Exception( "Cannot find plugins folder '" + _PluginsFolder + "'" );

			_New = GetDelegate<New>( "libvlc_new" );
			_Release = GetDelegate<Release>( "libvlc_release" );
			_CreateMediaFromPath = GetDelegate<CreateMediaFromPath>( "libvlc_media_new_path" );
			_ReleaseMedia = GetDelegate<ReleaseMedia>( "libvlc_media_release" );
			_CreateMediaPlayerFromMedia = GetDelegate<CreateMediaPlayerFromMedia>( "libvlc_media_player_new_from_media" );
			_ReleasePlayer = GetDelegate<ReleasePlayer>( "libvlc_media_player_release" );
			_IsPlayingMedia = GetDelegate<IsPlayingMedia>( "libvlc_media_player_play" );
			_PlayMedia = GetDelegate<PlayMedia>( "libvlc_media_player_play" );
			_PauseMedia = GetDelegate<PauseMedia>( "libvlc_media_player_pause" );
			_StopMedia = GetDelegate<StopMedia>( "libvlc_media_player_play" );
			_GetMediaLength = GetDelegate<GetMediaLength>( "libvlc_media_player_get_length" );
			_GetPlayerTime = GetDelegate<GetPlayerTime>( "libvlc_media_player_get_time" );
			_SetPlayerTime = GetDelegate<SetPlayerTime>( "libvlc_media_player_set_time" );
			_GetEventManager = GetDelegate<GetEventManager>( "libvlc_media_player_event_manager" );
			_EventAttach = GetDelegate<EventAttach>( "libvlc_event_attach" );

			_Inititalized = true;
		}

		/// <summary>
		/// Frees VLC library.
		/// </summary>
		public static void Uninitialize()
		{
			if ( !_Inititalized )
				return;

			FreeLibrary( _CoreLibraryAddress );
			FreeLibrary( _LibraryAddress );
			_Inititalized = false;
		}

		private static T GetDelegate<T>( string functionName ) where T : class
		{
			IntPtr functionAddress = GetProcAddress( _LibraryAddress, functionName );

			if ( functionAddress == IntPtr.Zero )
				throw new Exception( "Cannot find function '" + functionName + "'" );

			T d = Marshal.GetDelegateForFunctionPointer( functionAddress, typeof( T ) ) as T;

			if ( d == null )
				throw new Exception( "Function '" + functionName + "' has invalid parameters" );

			return d;
		}

		#region Definitions
		private enum VlcEvent : uint
		{
			MediaMetaChanged = 0,
			MediaSubItemAdded,
			MediaDurationChanged,
			MediaParsedChanged,
			MediaFreed,
			MediaStateChanged,

			MediaPlayerMediaChanged = 0x100,
			MediaPlayerNothingSpecial,
			MediaPlayerOpening,
			MediaPlayerBuffering,
			MediaPlayerPlaying,
			MediaPlayerPaused,
			MediaPlayerStopped,
			MediaPlayerForward,
			MediaPlayerBackward,
			MediaPlayerEndReached,
			MediaPlayerEncounteredError,
			MediaPlayerTimeChanged,
			MediaPlayerPositionChanged,
			MediaPlayerSeekableChanged,
			MediaPlayerPausableChanged,
			MediaPlayerTitleChanged,
			MediaPlayerSnapshotTaken,
			MediaPlayerLengthChanged,
			MediaPlayerVideoOutChanged,

			MediaListItemAdded = 0x200,
			MediaListWillAddItem,
			MediaListItemDeleted,
			MediaListWillDeleteItem,

			MediaListViewItemAdded = 0x300,
			MediaListViewWillAddItem,
			MediaListViewItemDeleted,
			MediaListViewWillDeleteItem,

			MediaListPlayerPlayed = 0x400,
			MediaListPlayerNextItemSet,
			MediaListPlayerStopped,

			MediaDiscovererStarted = 0x500,
			MediaDiscovererEnded,

			VlmMediaAdded = 0x600,
			VlmMediaRemoved,
			VlmMediaChanged,
			VlmMediaInstanceStarted,
			VlmMediaInstanceStopped,
			VlmMediaInstanceStatusInit,
			VlmMediaInstanceStatusOpening,
			VlmMediaInstanceStatusPlaying,
			VlmMediaInstanceStatusPause,
			VlmMediaInstanceStatusEnd,
			VlmMediaInstanceStatusError
		}

		[DllImport( "kernel32" )]
		private static extern int GetLastError();

		[DllImport( "kernel32", CharSet = CharSet.Unicode )]
		private static extern IntPtr LoadLibrary( string dllToLoad );

		[DllImport( "kernel32", CharSet = CharSet.Ansi )]
		private static extern IntPtr GetProcAddress( IntPtr module, string procedureName );

		[DllImport( "kernel32" )]
		private static extern bool FreeLibrary( IntPtr module );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate IntPtr New( int argsCount, string[] args );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void Release( IntPtr instance );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate IntPtr CreateMediaFromPath( IntPtr instance, string path );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void ReleaseMedia( IntPtr media );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate IntPtr CreateMediaPlayerFromMedia( IntPtr media );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void ReleasePlayer( IntPtr player );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate int IsPlayingMedia( IntPtr mediaPlayer );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void PlayMedia( IntPtr mediaPlayer );
		
		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void PauseMedia( IntPtr mediaPlayer );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void StopMedia( IntPtr mediaPlayer );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate long GetMediaLength( IntPtr mediaPlayer );
		
		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate long GetPlayerTime( IntPtr mediaPlayer );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void SetPlayerTime( IntPtr mediaPlayer, long time );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate IntPtr GetEventManager( IntPtr player );

		[UnmanagedFunctionPointer( CallingConvention.Cdecl )]
		private delegate void EventAttach( IntPtr eventManager, VlcEvent type, VlcEventCallback handler, IntPtr data );

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void VlcEventCallback( IntPtr eventData, IntPtr userData );
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Closes file and deletes resources.
		/// </summary>
		public void Dispose()
		{
			Dispose( false );
			GC.SuppressFinalize( this );
		}

		private void Dispose( bool onlyUnmanaged )
		{
			if ( IsPlaying )
				Stop();

			if ( _Player != IntPtr.Zero )
			{
				_ReleasePlayer( _Player );
				_Player = IntPtr.Zero;
			}

			if ( _Media != IntPtr.Zero )
			{
				_ReleaseMedia( _Media );
				_Media = IntPtr.Zero;
			}

			if ( _Instance != IntPtr.Zero )
			{
				//_Release( _Instance );
				_Instance = IntPtr.Zero;
			}

			if ( !onlyUnmanaged )
			{
				if ( _FileStream != null )
				{
					_FileStream.Dispose();
					_FileStream = null;
				}

				if ( _FileStreamPath != null )
				{
					try
					{
						if ( File.Exists( _FileStreamPath ) )
							File.Delete( _FileStreamPath );
					}
					catch { }

					_FileStreamPath = null;
				}
			}
		}
		#endregion
		#endregion
	}
}
