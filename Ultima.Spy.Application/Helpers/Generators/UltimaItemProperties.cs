using System;
using System.Collections.Generic;
using System.Xml;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima item property definitions.
	/// </summary>
	public class UltimaItemProperties
	{
		#region Propeties
		private const string DefaultFile = "Data\\ItemPropertyDefinitions.xml";
		private const string ElementRoot = "properties";
		private const string ElementProperty = "property";
		private const string ElementMisc = "misc";
		private const string ElementRunic = "runic";
		private const string ElementSwitch = "switch";
		private const string ElementCase = "case";

		private const string AttributeID = "id";
		private const string AttributeConstant = "constant";
		private const string AttributeValue = "value";

		private Dictionary<int,UltimaItemProperty> _Properties;

		/// <summary>
		/// Gets list of properties by cliloc.
		/// </summary>
		public Dictionary<int, UltimaItemProperty> Properties
		{
			get { return _Properties; }
		}

		private Dictionary<string, Dictionary<int, UltimaItemProperty>> _GroupProperties;

		/// <summary>
		/// Gets list of properties by group
		/// </summary>
		public Dictionary<string, Dictionary<int, UltimaItemProperty>> GroupProperties
		{
			get { return _GroupProperties; }
		}

		private Dictionary<string, Dictionary<int, string>> _Switches;

		/// <summary>
		/// Gets list of switches.
		/// </summary>
		public Dictionary<string, Dictionary<int, string>> Switches
		{
			get { return _Switches; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaItemProperties.
		/// </summary>
		public UltimaItemProperties() : this( DefaultFile )
		{
		}

		/// <summary>
		/// Constructs a new instance of UltimaItemProperties.
		/// </summary>
		/// <param name="filePath">Item definitions</param>
		public UltimaItemProperties( string filePath )
		{
			_Properties = new Dictionary<int, UltimaItemProperty>();
			_GroupProperties = new Dictionary<string, Dictionary<int, UltimaItemProperty>>();
			_Switches = new Dictionary<string, Dictionary<int, string>>();

			// Parse
			XmlDocument doc = new XmlDocument();
			doc.Load( filePath );

			XmlElement root = doc[ ElementRoot ];

			if ( root == null )
				throw new XmlException( String.Format( "File '{0}' is not valid", filePath ) );

			foreach ( XmlNode node in root.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null && String.Equals( child.Name, ElementSwitch, StringComparison.InvariantCultureIgnoreCase ) )
					ParseSwitch( child );
			}

			foreach ( XmlNode node in root.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null )
				{
					if ( String.Equals( child.Name, ElementMisc, StringComparison.InvariantCultureIgnoreCase ) )
						ParseProperties( child, false );
					else if ( String.Equals( child.Name, ElementRunic, StringComparison.InvariantCultureIgnoreCase ) )
						ParseProperties( child, true );
				}
			}
		}

		private void ParseProperties( XmlElement e, bool isRunic )
		{
			foreach ( XmlNode node in e.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null && String.Equals( child.Name, ElementProperty ) )
				{
					UltimaItemProperty property = new UltimaItemProperty( this, child, isRunic );
					Dictionary<int, UltimaItemProperty> properties = null;

					if ( !String.IsNullOrEmpty( property.Group ) )
					{
						if ( !_GroupProperties.TryGetValue( property.Group, out properties ) )
						{
							properties = new Dictionary<int, UltimaItemProperty>();
							_GroupProperties.Add( property.Group, properties );
						}
					}
					else
						properties = _Properties;

					if ( property.Switch != null )
					{
						foreach ( KeyValuePair<int, string> kvp in property.Switch )
						{
							if ( properties.ContainsKey( kvp.Key ) )
								throw new XmlException( String.Format( "Property with cliloc '{0}' already exists", kvp.Key ) );

							properties.Add( kvp.Key, property );
						}
					}
					else
					{
						foreach ( int cliloc in property.Clilocs )
						{
							if ( properties.ContainsKey( cliloc ) )
								throw new XmlException( String.Format( "Property with cliloc '{0}' already exists", cliloc ) );

							properties.Add( cliloc, property );
						}
					}
				}
			}
		}

		private void ParseSwitch( XmlElement e )
		{
			string id = e.GetAttribute( AttributeID );

			if ( String.IsNullOrEmpty( id ) )
				throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeID ) );

			if ( _Switches.ContainsKey( id ) )
				throw new XmlException( String.Format( "Switch '{0}' already exists", id ) );

			Dictionary<int, string> collection = new Dictionary<int, string>();

			foreach ( XmlNode node in e.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null && String.Equals( child.Name, ElementCase, StringComparison.InvariantCultureIgnoreCase ) )
				{
					string constant = child.GetAttribute( AttributeConstant );

					if ( String.IsNullOrEmpty( constant ) )
						throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeConstant ) );

					int integer = 0;

					if ( !Int32.TryParse( constant, out integer ) )
						throw new XmlException( String.Format( "Constant '{0}' must be integer", constant ) );

					if ( collection.ContainsKey( integer ) )
						throw new XmlException( String.Format( "Switch '{0}' already contains case '{1}'", id, constant ) );

					string value = child.GetAttribute( AttributeValue );

					if ( String.IsNullOrEmpty( value ) )
						throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeValue ) );

					collection.Add( integer, value );
				}
			}

			_Switches.Add( id, collection );
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima item property.
	/// </summary>
	public class UltimaItemProperty
	{
		#region Properties
		private const string ElementSetter = "setter";
		private const string AttributeCliloc = "cliloc";
		private const string AttributeGroup = "group";
		private const string AttributeFormat = "format";
		private const string AttributeSwitch = "switch";
		private const string AttributeMin = "min";
		private const string AttributeMax = "max";
		private static readonly char[] Separators = { ',' };

		private bool _IsRunic;

		/// <summary>
		/// Determines whether property is runic.
		/// </summary>
		public bool IsRunic
		{
			get { return _IsRunic; }
		}

		private List<int> _Clilocs;

		/// <summary>
		/// Gets cliloc for this property.
		/// </summary>
		public List<int> Clilocs
		{
			get { return _Clilocs; }
		}

		private string _Group;

		/// <summary>
		/// Gets property group.
		/// </summary>
		public string Group
		{
			get { return _Group; }
		}

		#region Resists
		/// <summary>
		/// Determines whether property is base physical.
		/// </summary>
		public bool IsBasePhysical
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1060448;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base fire.
		/// </summary>
		public bool IsBaseFire
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1060447;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base cold.
		/// </summary>
		public bool IsBaseCold
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1060445;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base poison.
		/// </summary>
		public bool IsBasePoison
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1060449;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base energy.
		/// </summary>
		public bool IsBaseEnergy
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1060446;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base chaos.
		/// </summary>
		public bool IsBaseChaos
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1072846;

				return false;
			}
		}

		/// <summary>
		/// Determines whether property is base direct.
		/// </summary>
		public bool IsBaseDirect
		{
			get
			{
				if ( _Clilocs.Count > 0 )
					return _Clilocs[ 0 ] == 1079978;

				return false;
			}
		}
		#endregion

		private int _Min;

		/// <summary>
		/// Gets minimum value (loot generator).
		/// </summary>
		public int Min
		{
			get { return _Min; }
		}

		private int _Max;

		/// <summary>
		/// Gets maximum value (loot generator)
		/// .
		/// </summary>
		public int Max
		{
			get { return _Max; }
		}

		private List<UltimaItemPropertySetter> _Setters;

		/// <summary>
		/// Gets list of property setters.
		/// </summary>
		public List<UltimaItemPropertySetter> Setters
		{
			get { return _Setters; }
		}

		private Dictionary<int, string> _Switch;

		/// <summary>
		/// Gets property switch.
		/// </summary>
		public Dictionary<int, string> Switch
		{
			get { return _Switch; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of UltimaItemProperty.
		/// </summary>
		/// <param name="root">Root property collection.</param>
		/// <param name="e">XML element to construct from.</param>
		/// <param name="isRunic">Runic properties.</param>
		public UltimaItemProperty( UltimaItemProperties root, XmlElement e, bool isRunic )
		{
			_IsRunic = isRunic;
			_Clilocs = new List<int>();
			_Setters = new List<UltimaItemPropertySetter>();

			// Parse
			string sw = e.GetAttribute( AttributeSwitch );

			if ( String.IsNullOrEmpty( sw ) )
			{
				string cliloc = e.GetAttribute( AttributeCliloc );

				if ( String.IsNullOrEmpty( cliloc ) )
					throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeCliloc ) );

				string[] values = cliloc.Split( Separators, StringSplitOptions.RemoveEmptyEntries );

				foreach ( string value in values )
				{
					int integer = 0;

					if ( !Int32.TryParse( value, out integer ) )
						throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", AttributeCliloc, value ) );

					_Clilocs.Add( integer );
				}
			}
			else
			{
				if ( !root.Switches.TryGetValue( sw, out _Switch ) )
					throw new XmlException( String.Format( "Cannot find switch '{0}'", sw ) );
			}

			_Group = e.GetAttribute( AttributeGroup );

			string min = e.GetAttribute( AttributeMin );
			string max = e.GetAttribute( AttributeMax );

			if ( !String.IsNullOrEmpty( min ) || !String.IsNullOrEmpty( max ) )
			{
				if ( String.IsNullOrEmpty( min ) )
					throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeMin ) );

				if ( String.IsNullOrEmpty( max ) )
					throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeMax ) );

				if ( !Int32.TryParse( min, out _Min ) )
					throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", AttributeMin, min ) );

				if ( !Int32.TryParse( max, out _Max ) )
					throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", AttributeMax, max ) );

				if ( _Min >= _Max )
					throw new XmlException( String.Format( "Min property value must be greater than max" ) );
			}

			foreach ( XmlNode node in e.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null && String.Equals( child.Name, ElementSetter ) )
					_Setters.Add( new UltimaItemPropertySetter( root, child ) );
			}
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima item property setter.
	/// </summary>
	public class UltimaItemPropertySetter
	{
		#region Properties
		private const string AttributeType = "type";
		private const string AttributeIndex = "index";
		private const string AttributeReturnType = "returnType";
		private const string AttributeName = "name";
		private const string AttributeFormat = "format";
		private const string AttributeSwitch = "switch";

		private bool _Overrides;

		/// <summary>
		/// Determine whether to override parent setter.
		/// </summary>
		public bool Overrides
		{
			get { return _Overrides; }
		}

		private string _ReturnType;

		/// <summary>
		/// Gets return type for overridable properties.
		/// </summary>
		public string ReturnType
		{
			get { return _ReturnType; }
		}

		private int _Index;

		/// <summary>
		/// Gets cliloc argument index.
		/// </summary>
		public int Index
		{
			get { return _Index; }
		}

		private string _Name;

		/// <summary>
		/// Gets setter name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}

		/// <summary>
		/// Determiens whether settter is slayer.
		/// </summary>
		public bool IsSlayer
		{
			get
			{
				return String.Equals( _Name, "Slayer", StringComparison.InvariantCultureIgnoreCase );
			}
		}

		private string _Format;

		/// <summary>
		/// Gets setter format.
		/// </summary>
		public string Format
		{
			get { return _Format; }
		}

		private Dictionary<int, string> _Switch;

		/// <summary>
		/// Gets setter switch.
		/// </summary>
		public Dictionary<int, string> Switch
		{
			get { return _Switch; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of UltimaItemPropertySetter.
		/// </summary>
		/// <param name="root">Root property collection.</param>
		/// <param name="e">XML element to cosntruct from.</param>
		public UltimaItemPropertySetter( UltimaItemProperties root, XmlElement e )
		{
			string type = e.GetAttribute( AttributeType );

			if ( String.Equals( type, "override", StringComparison.InvariantCultureIgnoreCase ) )
			{
				_Overrides = true;
				_ReturnType = e.GetAttribute( AttributeReturnType );

				if ( String.IsNullOrEmpty( _ReturnType ) )
					throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeReturnType ) );
			}

			string index = e.GetAttribute( AttributeIndex );

			if ( !String.IsNullOrEmpty( index ) )
			{
				if ( !Int32.TryParse( index, out _Index ) )
					throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", AttributeIndex, index ) );
			}
			else
				_Index = 0;

			_Name = e.GetAttribute( AttributeName );

			if ( String.IsNullOrEmpty( _Name ) )
				throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeName ) );

			_Format = e.GetAttribute( AttributeFormat );

			if ( String.IsNullOrEmpty( _Format ) )
				_Format = "{0}";

			string sw = e.GetAttribute( AttributeSwitch );

			if ( !String.IsNullOrEmpty( sw ) )
			{
				if ( !root.Switches.TryGetValue( sw, out _Switch ) )
					throw new XmlException( String.Format( "Cannot find switch '{0}'", sw ) );
			}
			else
				_Switch = null;
		}
		#endregion
	}
}
