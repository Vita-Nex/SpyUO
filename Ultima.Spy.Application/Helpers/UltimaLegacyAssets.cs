using System;
using System.Collections.Generic;
using System.IO;
using Ultima.Package;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes legacy assets.
	/// </summary>
	public class UltimaLegacyAssets
	{
		#region Properties
		private string _SourceFolder;

		/// <summary>
		/// Gets or source folder (location of the .uop files).
		/// </summary>
		public string SourceFolder
		{
			get { return _SourceFolder; }
		}

		private Dictionary<ulong, UltimaPackageFile> _Files;

		/// <summary>
		/// Gets all files.
		/// </summary>
		public Dictionary<ulong, UltimaPackageFile> Files
		{
			get { return _Files; }
		}

		private Dictionary<int, string> _Music;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaLegacyAssets.
		/// </summary>
		/// <param name="sourceFolder">Source folder to load resources from.</param>
		public UltimaLegacyAssets( string sourceFolder )
		{
			_SourceFolder = sourceFolder;
			_Files = new Dictionary<ulong, UltimaPackageFile>();

			LoadFile( "artLegacyMUL.uop" );
			LoadFile( "AnimationFrame1.uop" );
			LoadFile( "AnimationFrame2.uop" );
			LoadFile( "AnimationFrame3.uop" );
			LoadFile( "AnimationFrame4.uop" );
			LoadFile( "AnimationFrame5.uop" );
			LoadFile( "AnimationFrame6.uop" );
			LoadFile( "soundLegacyMUL.uop" );
			LoadFile( "string_dictionary.uop" );

			// Load music
			_Music = new Dictionary<int, string>();
			string musicDef = Path.Combine( sourceFolder, "Music", "Digital", "Config.txt" );

			if ( File.Exists( musicDef ) )
			{
				using ( FileStream stream = File.Open( musicDef, FileMode.Open, FileAccess.Read, FileShare.Read ) )
				{
					using ( StreamReader reader = new StreamReader( stream ) )
					{
						string line;
						char[] separators = { ',', ' ' };

						while ( ( line = reader.ReadLine() ) != null )
						{
							string[] split = line.Split( separators, StringSplitOptions.RemoveEmptyEntries );

							if ( split.Length > 1 )
							{
								int id = 0;

								if ( Int32.TryParse( split[ 0 ], out id ) && !_Music.ContainsKey( id ) )
								{
									string file = split[ 1 ];

									if ( !file.EndsWith( ".mp3" ) )
										file += ".mp3";

									_Music.Add( id, split[ 1 ] );
								}
							}
						}
					}
				}
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets world art files.
		/// </summary>
		/// <param name="textureID">Texture ID.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetTexture( int textureID )
		{
			string fileName = String.Format( "build/artlegacymul/{0:D8}.tga", 0x4000 + textureID );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets land texture.
		/// </summary>
		/// <param name="landID">Land ID.</param>
		/// <returns>Land texture if exists, null otherwise.</returns>
		public byte[] GetLand( int landID )
		{
			string fileName = String.Format( "build/artlegacymul/{0:D8}.tga", landID );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets sound file.
		/// </summary>
		/// <param name="soundID">Sound number to get.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetSound( int soundID )
		{
			string fileName = String.Format( "build/soundlegacymul/{0:D8}.dat", soundID );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets music file.
		/// </summary>
		/// <param name="musicID">Music number to get.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetMusic( int musicID )
		{
			string fileName = null;

			if ( _Music.TryGetValue( musicID, out fileName ) )
			{
				string filePath = Path.Combine( _SourceFolder, "Music", "Digital", fileName + ".mp3" );

				if ( File.Exists( filePath ) )
				{
					using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
					{
						byte[] data = new byte[ stream.Length ];
						stream.Read( data, 0, data.Length );
						return data;
					}
				}
			}

			return GetFile( fileName );
		}

		/// <summary>
		/// Gets music name.
		/// </summary>
		/// <param name="musicID">Music number to get.</param>
		/// <returns>File name if exists, null otherwise.</returns>
		public string GetMusicName( int musicID )
		{
			string fileName = null;

			if ( _Music.TryGetValue( musicID, out fileName ) )
				return fileName + ".mp3";

			return null;
		}

		/// <summary>
		/// Gets animation frame.
		/// </summary>
		/// <param name="bodyID">Body ID.</param>
		/// <param name="action">Action ID.</param>
		/// <param name="direction">Direction ID.</param>
		/// <returns>Animation frame.</returns>
		public byte[] GetAnimation( int bodyID, int action, int direction )
		{
			int index = action * 5;

			if ( direction <= 4 )
				index += direction;
			else
				index += direction - ( direction - 4 ) * 2;

			string fileName = String.Format( "build/animationlegacyframe/{0:D6}/{1:D2}.bin", bodyID, index );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets collection of strings.
		/// </summary>
		/// <returns>String collection.</returns>
		public UltimaStringCollection GetClilocs()
		{
			string filePath = Path.Combine( _SourceFolder, "Cliloc.enu" );

			if ( File.Exists( filePath ) )
				return UltimaStringCollection.FromFile( filePath );

			return null;
		}

		/// <summary>
		/// Gets file based on file name in package.
		/// </summary>
		/// <param name="fileName">File name to get.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetFile( string fileName )
		{
			ulong fileNameHash = UltimaPackage.HashFileName( fileName.ToLowerInvariant() );
			UltimaPackageFile file = null;

			if ( _Files != null && _Files.TryGetValue( fileNameHash, out file ) )
				return file.Package.GetFile( fileNameHash );

			return null;
		}

		private void LoadFile( string fileName )
		{
			string filePath = Path.Combine( _SourceFolder, fileName );

			if ( !File.Exists( filePath ) )
				return;

			UltimaPackage package = new UltimaPackage( filePath );

			foreach ( KeyValuePair<ulong, UltimaPackageFile> kvp in package.Files )
			{
				if ( !_Files.ContainsKey( kvp.Key ) )
					_Files.Add( kvp.Key, kvp.Value );
			}
		}
		#endregion
	}
}
