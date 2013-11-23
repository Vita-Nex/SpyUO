using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes property value.
	/// </summary>
	public class UltimaPacketPropertyValue
	{
		#region Properties
		private UltimaPacketPropertyDefinition _Definition;

		/// <summary>
		/// Gets ultima packet property definition.
		/// </summary>
		public UltimaPacketPropertyDefinition Definition
		{
			get { return _Definition; }
		}

		private UltimaPacketValue _Owner;

		/// <summary>
		/// Gets property object.
		/// </summary>
		public UltimaPacketValue Owner
		{
			get { return _Owner; }
		}

		private object _Value;

		/// <summary>
		/// Gets property value.
		/// </summary>
		public object Value
		{
			get { return _Value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertyValue and
		/// gets property value.
		/// </summary>
		/// <param name="definition">Property defintion.</param>
		/// <param name="owner">Proeprty owner.</param>
		public UltimaPacketPropertyValue( UltimaPacketPropertyDefinition definition, UltimaPacketValue owner )
		{
			_Definition = definition;
			_Owner = owner;

			// Get value
			object value = definition.Getter( owner.Object );

			if ( definition is UltimaPacketListPropertyDefinition )
			{
				List<UltimaPacketValue> objects = new List<UltimaPacketValue>();
				UltimaPacketListPropertyDefinition list = (UltimaPacketListPropertyDefinition) definition;
				IEnumerable e = value as IEnumerable;

				if ( e != null )
				{
					foreach ( object o in e )
						objects.Add( new UltimaPacketValue( list.ChildDefinition, o ) );
				}

				_Value = objects;
			}
			else
				_Value = value;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Builds string representation of property value.
		/// </summary>
		/// <param name="indent">Number of tabs to indent</param>
		/// <returns>String.</returns>
		public string ToString( int indent )
		{
			StringBuilder builder = new StringBuilder( 0x1000 );

			if ( _Definition is UltimaPacketListPropertyDefinition )
			{
				List<UltimaPacketValue> objects = _Value as List<UltimaPacketValue>;

				if ( objects != null )
				{
					AppendFormatLine( builder, indent, "{0}:", _Definition.Attribute.Name );

					foreach ( UltimaPacketValue o in objects )
						AppendFormatLine( builder, indent - 1, "{0}", o.ToString( indent + 1 ) );
				}

				return builder.ToString();
			}

			if ( _Definition.Attribute.Format != null )
				AppendFormatLine( builder, indent,"{0}: {1}", _Definition.Attribute.Name, String.Format( _Definition.Attribute.Format, _Value ) );
			else
				AppendFormatLine( builder, indent,"{0}: {1}", _Definition.Attribute.Name, _Value );

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
