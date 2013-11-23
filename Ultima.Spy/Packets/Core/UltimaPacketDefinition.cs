using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes ultima entity.
	/// </summary>
	public interface IUltimaEntity
	{
		/// <summary>
		/// Gets entity serial.
		/// </summary>
		uint Serial { get; }
	}

	/// <summary>
	/// Generic packet constructor.
	/// </summary>
	/// <returns>Ultima packet.</returns>
	public delegate UltimaPacket UltimaPacketConstructor();

	/// <summary>
	/// Describes packet table entry.
	/// </summary>
	public class UltimaPacketDefinition : UltimaPacketClassDefinition
	{
		#region Properties
		private UltimaPacketAttribute _Attribute;

		/// <summary>
		/// Gets ultima packet attribute.
		/// </summary>
		public UltimaPacketAttribute Attribute
		{
			get { return _Attribute; }
		}

		private bool _IsDefault;

		/// <summary>
		/// Determines whether this is default definition.
		/// </summary>
		public bool IsDefault
		{
			get { return _IsDefault; }
		}

		private UltimaPacketConstructor _Constructor;

		/// <summary>
		/// Gets packet constructor.
		/// </summary>
		public UltimaPacketConstructor Constructor
		{
			get { return _Constructor; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketDefinition.
		/// </summary>
		/// <param name="type">Packet type.</param>
		/// <param name="attribute">Ultima packet attribute.</param>
		public UltimaPacketDefinition( Type type, UltimaPacketAttribute attribute ) : base( type )
		{
			_Attribute = attribute;
			_IsDefault = attribute == null;

			// Construct constructor delegate
			ConstructorInfo constructor = type.GetConstructor( new Type[] { } );

			if ( constructor == null )
				throw new SpyException( "Type '{0}' does not have a constructor with no parameters", type );

			DynamicMethod dynamicMethod = new DynamicMethod( "CreateInstance", type, null );
			ILGenerator generator = dynamicMethod.GetILGenerator();
			generator.Emit( OpCodes.Newobj, constructor );
			generator.Emit( OpCodes.Ret );

			_Constructor = (UltimaPacketConstructor) dynamicMethod.CreateDelegate( typeof( UltimaPacketConstructor ) );
		}
		#endregion
	}
}
