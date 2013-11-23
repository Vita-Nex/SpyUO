using System;

namespace Ultima.Spy
{
	public enum UltimaPacketPropertyType
	{
		/// <summary>
		/// Non resource related property.
		/// </summary>
		None,

		/// <summary>
		/// Displays direction.
		/// </summary>
		Direction,

		/// <summary>
		/// Music.
		/// </summary>
		Music,

		/// <summary>
		/// Sound.
		/// </summary>
		Sound,

		/// <summary>
		/// Texture.
		/// </summary>
		Texture,

		/// <summary>
		/// Cliloc string number.
		/// </summary>
		Cliloc,

		/// <summary>
		/// Body.
		/// </summary>
		Body,
	}

	/// <summary>
	/// Ultima packet attribute.
	/// </summary>
	[AttributeUsage( AttributeTargets.Property )]
	public class UltimaPacketPropertyAttribute : Attribute
	{
		#region Properties
		private string _Name;

		/// <summary>
		/// Gets or sets property display name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		private string _Format;

		/// <summary>
		/// Gets or sets property display format (only for properties without type).
		/// </summary>
		public string Format
		{
			get { return _Format; }
			set { _Format = value; }
		}

		private UltimaPacketPropertyType _Type;

		/// <summary>
		/// Gets property display type.
		/// </summary>
		public UltimaPacketPropertyType Type
		{
			get { return _Type; }
		}
		#endregion

		#region Construtors
		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertyAttribute.
		/// </summary>
		/// <param name="type">Property type.</param>
		public UltimaPacketPropertyAttribute()
		{
			_Name = null;
			_Format = null;
			_Type = UltimaPacketPropertyType.None;
		}

		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertyAttribute.
		/// </summary>
		/// <param name="type">Property type.</param>
		public UltimaPacketPropertyAttribute( UltimaPacketPropertyType type = UltimaPacketPropertyType.None )
		{
			_Name = null;
			_Format = null;
			_Type = type;
		}

		/// <summary>
		/// Cosntructs a new instance of UltimaPacketPropertyAttribute.
		/// </summary>
		/// <param name="name">Property display name.</param>
		/// <param name="format">Property display format.</param>
		public UltimaPacketPropertyAttribute( string name, string format )
		{
			_Name = name;
			_Format = format;
			_Type = UltimaPacketPropertyType.None;
		}

		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertyAttribute.
		/// </summary>
		/// <param name="name">Property display name.</param>
		/// <param name="type">Property type.</param>
		public UltimaPacketPropertyAttribute( string name, UltimaPacketPropertyType type = UltimaPacketPropertyType.None )
		{
			_Name = name;
			_Type = type;
			_Format = null;
		}
		#endregion
	}
}
