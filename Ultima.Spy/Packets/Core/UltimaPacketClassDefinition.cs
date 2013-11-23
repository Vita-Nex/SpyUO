using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Ultima.Spy
{
	/// <summary>
	/// Defines ultima packet class.
	/// </summary>
	public class UltimaPacketClassDefinition
	{
		#region Properties
		private Type _Type;

		/// <summary>
		/// Gets class type.
		/// </summary>
		public Type Type
		{
			get { return _Type; }
		}

		private List<UltimaPacketPropertyDefinition> _Properties;

		/// <summary>
		/// Gets class properties.
		/// </summary>
		public List<UltimaPacketPropertyDefinition> Properties
		{
			get { return _Properties; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketClassDefinition.
		/// </summary>
		/// <param name="type">Class type.</param>
		public UltimaPacketClassDefinition( Type type )
		{
			_Type = type;
			_Properties = new List<UltimaPacketPropertyDefinition>();

			foreach ( PropertyInfo info in type.GetProperties() )
			{
				UltimaPacketPropertyAttribute[] attributes = info.GetCustomAttributes( typeof( UltimaPacketPropertyAttribute ), false ) as UltimaPacketPropertyAttribute[];

				if ( attributes != null && attributes.Length == 1 )
				{
					UltimaPacketPropertyAttribute attribute = attributes[ 0 ];
					MethodInfo getterInfo = info.GetGetMethod();
					Type propertyType = info.PropertyType;
					UltimaPacketPropertyGetter getter;

					if ( attribute.Name == null )
						attribute.Name = info.Name;

					if ( getterInfo == null )
						throw new SpyException( "Property '{0}' in type '{1}' does not have a public GET accessor", info.Name, type.Name );

					DynamicMethod dynamicMethod = new DynamicMethod( "DynamicGet", typeof( object ), new Type[] { typeof( object ) }, type, true );
					ILGenerator generator = dynamicMethod.GetILGenerator();
					generator.Emit( OpCodes.Ldarg_0 );
					generator.Emit( OpCodes.Call, getterInfo );

					if ( getterInfo.ReturnType.IsValueType )
						generator.Emit( OpCodes.Box, getterInfo.ReturnType );

					generator.Emit( OpCodes.Ret );
					getter = (UltimaPacketPropertyGetter) dynamicMethod.CreateDelegate( typeof( UltimaPacketPropertyGetter ) );

					if ( typeof( IEnumerable ).IsAssignableFrom( propertyType ) && !typeof( String ).IsAssignableFrom( propertyType ) )
					{
						if ( propertyType.IsGenericType )
						{
							Type[] genericTypes = propertyType.GetGenericArguments();

							if ( genericTypes != null && genericTypes.Length == 1 )
							{
								Type child = genericTypes[ 0 ];
								UltimaPacketListPropertyDefinition listProperty = new UltimaPacketListPropertyDefinition( info, getter, attribute, child );

								_Properties.Add( listProperty );
							}
							else
								throw new SpyException( "Property '{0}' in type '{1}' must have exactly one generic argument", info.Name, type.Name );
						}
						else if ( propertyType.IsArray )
						{
							Type child = propertyType.GetElementType();

							if ( child != null )
							{
								UltimaPacketListPropertyDefinition listProperty = new UltimaPacketListPropertyDefinition( info, getter, attribute, child );

								_Properties.Add( listProperty );
							}
						}
						else
							throw new SpyException( "Property '{0}' in type '{1}' must be either array or generic list", info.Name, type.Name );
					}
					else
					{
						_Properties.Add( new UltimaPacketPropertyDefinition( info, getter, attribute ) );
					}
				}
				else if ( attributes != null && attributes.Length > 1 )
					throw new SpyException( "Property '{0}' in type '{1}' has too many ultima packet attributes", info.Name, type.Name );
			}
		}
		#endregion
	}
}
