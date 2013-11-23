using System.Reflection;
using System;
using System.Reflection.Emit;

namespace Ultima.Spy
{
	/// <summary>
	/// Generic getter for ultima packet.
	/// </summary>
	/// <param name="target">Target packet</param>
	/// <returns></returns>
	public delegate object UltimaPacketPropertyGetter( object target );

	/// <summary>
	/// Describes ultima packet property.
	/// </summary>
	public class UltimaPacketPropertyDefinition
	{
		#region Properties
		private PropertyInfo _Info;

		/// <summary>
		/// Gets property info.
		/// </summary>
		public PropertyInfo Info
		{
			get { return _Info; }
		}

		private UltimaPacketPropertyAttribute _Attribute;

		/// <summary>
		/// Gets ultima attribute.
		/// </summary>
		public UltimaPacketPropertyAttribute Attribute
		{
			get { return _Attribute; }
		}

		private UltimaPacketPropertyGetter _Getter;

		/// <summary>
		/// Gets property getter.
		/// </summary>
		public UltimaPacketPropertyGetter Getter
		{
			get { return _Getter; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertyDefinition.
		/// </summary>
		/// <param name="info">Property info.</param>
		/// <param name="getter">Public getter for property.</param>
		/// <param name="attribute">Object property attribute.</param>
		public UltimaPacketPropertyDefinition( PropertyInfo info, UltimaPacketPropertyGetter getter, UltimaPacketPropertyAttribute attribute )
		{
			_Info = info;
			_Getter = getter;
			_Attribute = attribute;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Returns string representaion of this class.
		/// </summary>
		/// <returns>String representaion.</returns>
		public override string ToString()
		{
			return _Attribute.Name;
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima packet list property.
	/// </summary>
	public class UltimaPacketListPropertyDefinition : UltimaPacketPropertyDefinition
	{
		#region Properties
		private UltimaPacketClassDefinition _ChildDefinition;

		/// <summary>
		/// Gets type definition.
		/// </summary>
		public UltimaPacketClassDefinition ChildDefinition
		{
			get { return _ChildDefinition; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketListPropertyDefinition.
		/// </summary>
		/// <param name="info">Property info.</param>
		/// <param name="getter">Public getter for property.</param>
		/// <param name="attribute">Object property attribute.</param>
		/// <param name="childType">Property child type.</param>
		public UltimaPacketListPropertyDefinition( PropertyInfo info, UltimaPacketPropertyGetter getter, UltimaPacketPropertyAttribute attribute, Type childType ) : base( info, getter, attribute )
		{
			_ChildDefinition = new UltimaPacketClassDefinition( childType );
		}
		#endregion
	}
}
