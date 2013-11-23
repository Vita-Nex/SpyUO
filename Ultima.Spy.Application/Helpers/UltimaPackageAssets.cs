using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ultima.Package;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Represents all UO assets.
	/// </summary>
	public class UltimaPackageAssets
	{
		#region Properties
		private const string SoundDefinitionsFileName = "data/audio/audio_sounds.csv";
		private const string MusicDefinitionsFileName = "data/audio/audio_music.csv";

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

		private Dictionary<int, string> _Sounds;
		private Dictionary<int, string> _Music;
		private Dictionary<int,UltimaAnimationDescriptor> _AnimationDescriptors;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPackageAssets.
		/// </summary>
		/// <param name="sourceFolder">Source folder to load .uop from.</param>
		/// <param name="ignoreList">List of packages to ignore.</param>
		public UltimaPackageAssets( string sourceFolder, List<string> ignoreList = null )
		{
			_SourceFolder = sourceFolder;
			_Files = new Dictionary<ulong, UltimaPackageFile>();

			LoadFile( "AnimationFrame1.uop" );
			LoadFile( "AnimationFrame2.uop" );
			LoadFile( "AnimationFrame3.uop" );
			LoadFile( "AnimationFrame4.uop" );
			LoadFile( "AnimationFrame5.uop" );
			LoadFile( "AnimationFrame6.uop" );
			LoadFile( "LocalizedStrings.uop" );
			LoadFile( "MainMisc.uop" );
			LoadFile( "Texture.uop" );
			LoadFile( "Audio.uop" );

			// Load sound definitions
			LoadHashes( SoundDefinitionsFileName, out _Sounds );
			LoadHashes( MusicDefinitionsFileName, out _Music );

			// Loads animation names
			byte[] mobart = GetFile( "data/gamedata/mobart.csv" );

			if ( mobart != null )
			{
				using ( MemoryStream stream = new MemoryStream( mobart ) )
				{
					_AnimationDescriptors = UltimaAnimationDescriptor.FromStream( stream );
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
			string fileName = String.Format( "build/worldart/{0:D8}.dds", textureID );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets sound file.
		/// </summary>
		/// <param name="soundID">Sound number to get.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetSound( int soundID )
		{
			string fileName = null;

			if ( _Sounds != null && _Sounds.TryGetValue( soundID, out fileName ) )
				return GetFile( fileName );

			return null;
		}

		/// <summary>
		/// Gets sound name.
		/// </summary>
		/// <param name="soundID">Sound number to get.</param>
		/// <returns>File name if exists, null otherwise.</returns>
		public string GetSoundName( int soundID )
		{
			string fileName = null;

			if ( _Music != null && _Sounds.TryGetValue( soundID, out fileName ) )
			{
				int index = fileName.LastIndexOf( '/' );

				if ( index >= 0 )
					return fileName.Substring( index + 1, fileName.Length - index - 1 );
			}

			return fileName;
		}

		/// <summary>
		/// Gets music file.
		/// </summary>
		/// <param name="musicID">Music number to get.</param>
		/// <returns>File data if exists, null otherwise.</returns>
		public byte[] GetMusic( int musicID )
		{
			string fileName = null;

			if ( _Music != null && _Music.TryGetValue( musicID, out fileName ) )
				return GetFile( fileName );

			return null;
		}

		/// <summary>
		/// Gets music name.
		/// </summary>
		/// <param name="musicID">Music number to get.</param>
		/// <returns>File name if exists, null otherwise.</returns>
		public string GetMusicName( int musicID )
		{
			string fileName = null;

			if ( _Music != null && _Music.TryGetValue( musicID, out fileName ) )
			{
				int index = fileName.LastIndexOf( '/' );

				if ( index >= 0 )
					return fileName.Substring( index + 1, fileName.Length - index - 1 );
			}

			return fileName;
		}

		/// <summary>
		/// Gets collection of strings.
		/// </summary>
		/// <returns>String collection.</returns>
		public UltimaStringCollection GetClilocs()
		{
			byte[] data = GetFile( "data/localizedstrings/001.cliloc" );

			if ( data != null )
				return UltimaStringCollection.FromMemory( data );

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

			string fileName = String.Format( "build/animationframe/{0:D6}/{1:D2}.bin", bodyID, index );
			return GetFile( fileName );
		}

		/// <summary>
		/// Gets animation name.
		/// </summary>
		/// <param name="bodyID">Body ID.</param>
		/// <returns>Body name.</returns>
		public string GetAnimationName( int bodyID )
		{
			if ( _AnimationDescriptors != null && _AnimationDescriptors.ContainsKey( bodyID ) )
				return _AnimationDescriptors[ bodyID ].Name;

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

		private void LoadHashes( string fileName, out Dictionary<int, string> list )
		{
			list = null;

			byte[] data = GetFile( fileName );
			char[] separators = new char[] { ',' };

			if ( data != null )
			{
				list = new Dictionary<int, string>();

				using ( MemoryStream stream = new MemoryStream( data ) )
				{
					using ( StreamReader reader = new StreamReader( stream ) )
					{
						string line;

						while ( ( line = reader.ReadLine() ) != null )
						{
							// Skip comments
							if ( line.StartsWith( "#" ) )
								continue;

							// Omg its a line
							string[] parts = line.Split( separators, StringSplitOptions.RemoveEmptyEntries );

							if ( parts.Length >= 2 )
							{
								int id = 0;

								if ( Int32.TryParse( parts[ 0 ], out id ) )
								{
									string audioFileName = Path.Combine( "data/audio/", parts[ 1 ].Replace( '\\', '/' ) );

									if ( !list.ContainsKey( id ) )
										list.Add( id, audioFileName );
								}
							}
						}
					}
				}
			}
		}
		#endregion
	}
}
