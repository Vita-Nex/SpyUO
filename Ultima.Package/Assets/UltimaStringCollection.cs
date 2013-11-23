using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;

namespace Ultima.Package
{
	/// <summary>
	/// Describes string collection
	/// </summary>
	public class UltimaStringCollection
	{
		#region Properties
		private Dictionary<int, UltimaStringCollectionItem> _Dictionary;

		/// <summary>
		/// Gets or sets string collection as dictionary.
		/// </summary>
		public Dictionary<int, UltimaStringCollectionItem> Dictionary
		{
			get { return _Dictionary; }
			set { _Dictionary = value; }
		}

		private ObservableCollection<UltimaStringCollectionItem> _List;

		/// <summary>
		/// Gets or sets string collection as observable list.
		/// </summary>
		public ObservableCollection<UltimaStringCollectionItem> List
		{
			get { return _List; }
			set { _List = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaStringCollection.
		/// </summary>
		public UltimaStringCollection()
		{
			_Dictionary = new Dictionary<int, UltimaStringCollectionItem>();
			_List = new ObservableCollection<UltimaStringCollectionItem>();
		}

		/// <summary>
		/// Constructs a new instance of UltimaStringCollection.
		/// </summary>
		/// <param name="reader">Binary reader to construct from.</param>
		public UltimaStringCollection( BinaryReader reader )
		{
			_Dictionary = new Dictionary<int, UltimaStringCollectionItem>();
			_List = new ObservableCollection<UltimaStringCollectionItem>();

			AppendStrings( reader );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets string for specific number.
		/// </summary>
		/// <param name="number">String number.</param>
		/// <returns>String if exists, null otherwise.</returns>
		public string GetString( int number )
		{
			if ( _Dictionary.ContainsKey( number ) )
				return _Dictionary[ number ].Text;

			return null;
		}

		/// <summary>
		/// Adds strings from binary reader.
		/// </summary>
		/// <param name="reader">Reader to add strings from.</param>
		public void AppendStrings( BinaryReader reader )
		{
			byte[] buffer = new byte[ (int) Math.Pow( 2, 15 ) ];

			reader.ReadInt32();
			reader.ReadInt16();

			while ( reader.BaseStream.Position < reader.BaseStream.Length )
			{
				int number = reader.ReadInt32();
				reader.ReadByte();
				int length = reader.ReadInt16();

				reader.Read( buffer, 0, length );
				string text = Encoding.UTF8.GetString( buffer, 0, length );

				if ( !_Dictionary.ContainsKey( number ) )
				{
					UltimaStringCollectionItem item = new UltimaStringCollectionItem( number, text );

					_Dictionary.Add( number, item );
					_List.Add( item );
				}
			}
		}

		/// <summary>
		/// Constructs string collection from file.
		/// </summary>
		/// <param name="filePath">File to construct from.</param>
		/// <returns>String collection.</returns>
		public static UltimaStringCollection FromFile( string filePath )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				using ( BinaryReader reader = new BinaryReader( stream ) )
					return new UltimaStringCollection( reader );
			}
		}

		/// <summary>
		/// Constructs string collection from memory.
		/// </summary>
		/// <param name="data">Memory to construct from.</param>
		/// <returns>String collection.</returns>
		public static UltimaStringCollection FromMemory( byte[] data )
		{
			using ( MemoryStream stream = new MemoryStream( data ) )
				return FromStream( stream );
		}

		/// <summary>
		/// Constructs string collection from stream.
		/// </summary>
		/// <param name="stream">Stream to construct from.</param>
		/// <returns>String collection.</returns>
		public static UltimaStringCollection FromStream( Stream stream )
		{
			using ( BinaryReader reader = new BinaryReader( stream ) )
				return new UltimaStringCollection( reader );
		}


		/// <summary>
		/// Cosntructs string collection from UOP file.
		/// </summary>
		/// <param name="filePath">File to construct from.</param>
		/// <returns>String collection.</returns>
		public static UltimaStringCollection FromPackage( string filePath )
		{
			using ( UltimaPackage package = new UltimaPackage( filePath ) )
			{
				byte[] data = package.GetFile( "data/localizedstrings/001.cliloc" );

				if ( data != null )
				{
					using ( MemoryStream stream = new MemoryStream( data ) )
						return FromStream( stream );
				}
			}

			return null;
		}
		#endregion
	}

	/// <summary>
	/// Describes string collection item.
	/// </summary>
	public class UltimaStringCollectionItem
	{
		#region Properties
		private int _Number;

		/// <summary>
		/// Gets or sets number.
		/// </summary>
		public int Number
		{
			get { return _Number; }
			set { _Number = value; }
		}

		private string _Text;

		/// <summary>
		/// Gets or sets text.
		/// </summary>
		public string Text
		{
			get { return _Text; }
			set { _Text = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaStringCollectionItem.
		/// </summary>
		/// <param name="number">Item number.</param>
		/// <param name="text">Text</param>
		public UltimaStringCollectionItem( int number, string text )
		{
			_Number = number;
			_Text = text;
		}
		#endregion
	}
}
