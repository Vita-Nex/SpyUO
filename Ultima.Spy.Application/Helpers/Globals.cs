using System;
using System.IO;
using Ultima.Package;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes global variables.
	/// </summary>
	public class Globals
	{
		#region Propetie
		/// <summary>
		/// Global instance of globals.
		/// </summary>
		public static Globals Instance;

		private string _LegacyClientFolder;

		/// <summary>
		/// Gets legacy client folder.
		/// </summary>
		public string LegacyClientFolder
		{
			get { return _LegacyClientFolder; }
		}

		private UltimaLegacyAssets _LegacyAssets;

		/// <summary>
		/// Gets legacy client assets.
		/// </summary>
		public UltimaLegacyAssets LegacyAssets
		{
			get { return _LegacyAssets; }
		}

		private string _EnhancedClientFolder;

		/// <summary>
		/// Gets enhanced client folder.
		/// </summary>
		public string EnhancedClientFolder
		{
			get { return _EnhancedClientFolder; }
		}

		private UltimaPackageAssets _EnhancedAssets;

		/// <summary>
		/// Gets enhanced client assets.
		/// </summary>
		public UltimaPackageAssets EnhancedAssets
		{
			get { return _EnhancedAssets; }
		}

		private UltimaStringCollection _Clilocs;

		/// <summary>
		/// Gets cliloc collection.
		/// </summary>
		public UltimaStringCollection Clilocs
		{
			get { return _Clilocs; }
		}

		private string _VlcInstallationFolder;

		/// <summary>
		/// Gets VLC installation folder.
		/// </summary>
		public string VlcInstallationFolder
		{
			get { return _VlcInstallationFolder; }
		}

		private VlcPlayer _VlcPlayer;

		/// <summary>
		/// Gets VLC player.
		/// </summary>
		public VlcPlayer VlcPlayer
		{
			get { return _VlcPlayer; }
		}

		private UltimaItemDefinitions _ItemDefinitions;

		/// <summary>
		/// Gets item definitions.
		/// </summary>
		public UltimaItemDefinitions ItemDefinitions
		{
			get { return _ItemDefinitions; }
		}

		private UltimaItemProperties _ItemProperties;

		/// <summary>
		/// Gets item properties.
		/// </summary>
		public UltimaItemProperties ItemProperties
		{
			get { return _ItemProperties; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of Globals.
		/// </summary>
		public Globals()
		{
			// Register packets
			UltimaPacket.RegisterPackets();

			// Generators
			_ItemDefinitions = new UltimaItemDefinitions();
			_ItemProperties = new UltimaItemProperties();

			// Get enhanced client folder
			_LegacyClientFolder = SpyHelper.ClassicClientFolder;

			if ( _LegacyClientFolder != null )
			{
				string clilocFilePath = Path.Combine( _LegacyClientFolder, "Cliloc.enu" );

				if ( File.Exists( clilocFilePath ) )
					_Clilocs = UltimaStringCollection.FromFile( clilocFilePath );

				InitializeLegacyAssets( _LegacyClientFolder );
			}

			_EnhancedClientFolder = SpyHelper.EnhancedClientFolder;

			if ( _EnhancedClientFolder != null )
			{
				string clilocPackage = Path.Combine( _EnhancedClientFolder, "string_collection.uop" );

				if ( File.Exists( clilocPackage ) )
					_Clilocs = UltimaStringCollection.FromPackage( clilocPackage );

				if ( _LegacyAssets == null )
					InitializeEnhancedAssets( _EnhancedClientFolder );
			}

			// Initialize VLC player
			_VlcInstallationFolder = VlcPlayer.DefaultInstallationFolder;

			if ( !String.IsNullOrEmpty( _VlcInstallationFolder ) )
			{
				try
				{
					VlcPlayer.Initialize( _VlcInstallationFolder );

					_VlcPlayer = new VlcPlayer();
				}
				catch
				{
					_VlcInstallationFolder = null;
				}
			}
		}

		/// <summary>
		/// Initializes legacy client assets.
		/// </summary>
		/// <param name="sourceFolder">Source folder.</param>
		public void InitializeLegacyAssets( string sourceFolder )
		{
			if ( _LegacyAssets == null )
				_LegacyAssets = new UltimaLegacyAssets( sourceFolder );

			if ( _Clilocs == null )
				_Clilocs = _LegacyAssets.GetClilocs();

			_EnhancedAssets = null;
		}

		/// <summary>
		/// Initializes enhanced client assets.
		/// </summary>
		/// <param name="sourceFolder">Source folder.</param>
		public void InitializeEnhancedAssets( string sourceFolder )
		{
			if ( _EnhancedAssets == null )
				_EnhancedAssets = new UltimaPackageAssets( sourceFolder );

			if ( _Clilocs == null )
				_Clilocs = _EnhancedAssets.GetClilocs();

			_LegacyAssets = null;
		}
		#endregion
	}
}
