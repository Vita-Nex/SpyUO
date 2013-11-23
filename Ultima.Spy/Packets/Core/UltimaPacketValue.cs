using System.Collections.Generic;
using System.Text;
using System;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes collection of values.
	/// </summary>
	public class UltimaPacketValue
	{
		#region Properties
		private UltimaPacketClassDefinition _Definition;

		/// <summary>
		/// Gets owner definition.
		/// </summary>
		public UltimaPacketClassDefinition Definition
		{
			get { return _Definition; }
		}

		private object _Object;

		/// <summary>
		/// Gets property object.
		/// </summary>
		public object Object
		{
			get { return _Object; }
		}

		private List<UltimaPacketPropertyValue> _Properties;

		/// <summary>
		/// Gets list of properties.
		/// </summary>
		public List<UltimaPacketPropertyValue> Properties
		{
			get { return _Properties; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketValue.
		/// </summary>
		/// <param name="obj">Ultima packet.</param>
		public UltimaPacketValue( UltimaPacket obj )
		{
			_Object = obj;
			_Definition = obj.Definition;
			_Properties = new List<UltimaPacketPropertyValue>();

			foreach ( UltimaPacketPropertyDefinition d in _Definition.Properties )
				_Properties.Add( new UltimaPacketPropertyValue( d, this ) );
		}

		/// <summary>
		/// Constructs a new instance of UltimaPacketValue.
		/// </summary>
		/// <param name="definition">Class definition.</param>
		/// <param name="obj">Packet child.</param>
		public UltimaPacketValue( UltimaPacketClassDefinition definition, object obj )
		{
			_Object = obj;
			_Definition = definition;
			_Properties = new List<UltimaPacketPropertyValue>();

			foreach ( UltimaPacketPropertyDefinition d in _Definition.Properties )
				_Properties.Add( new UltimaPacketPropertyValue( d, this ) );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Builds string representation of packet value.
		/// </summary>
		/// <returns>String.</returns>
		public override string ToString()
		{
			return ToString( 0 );
		}

		/// <summary>
		/// Builds string representation of property value.
		/// </summary>
		/// <param name="indent">Number of tabs to indent</param>
		/// <returns>String.</returns>
		public string ToString( int indent )
		{
			StringBuilder builder = new StringBuilder( 0x1000 );
			UltimaPacket packet = _Object as UltimaPacket;

			if ( packet != null )
				AppendFormatLine( builder, indent, "{0} - {1}", packet.Ids, packet.Name );
			else if ( _Object != null )
				AppendFormatLine( builder, indent, "Child: {0}", _Object.ToString() );

			foreach ( UltimaPacketPropertyValue property in _Properties )
				builder.Append( property.ToString( indent + 1 ) );

			return builder.ToString();
		}

		private void AppendFormatLine( StringBuilder builder, int indent, string format, params object[] args )
		{
			for ( int i = 0; i < indent; i++ )
				builder.Append( '\t' );

			builder.AppendLine( String.Format( format, args ) );
		}
		#endregion
	}
}
