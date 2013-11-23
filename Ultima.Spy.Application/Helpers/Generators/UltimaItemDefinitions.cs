using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes ultima item definitions.
	/// </summary>
	public class UltimaItemDefinitions
	{
		#region Propeties
		private const string DefaultFile = "Data\\ItemDefinitions.xml";
		private const string ElementRoot = "items";
		private const string ElementGroup = "group";

		private Dictionary<int,UltimaItemDefinition> _Items;

		/// <summary>
		/// Gets items by item ID.
		/// </summary>
		public Dictionary<int, UltimaItemDefinition> Items
		{
			get { return _Items; }
		}

		private List<UltimaItemDefinitionGroup> _Groups;

		/// <summary>
		/// Gets list of groups.
		/// </summary>
		public List<UltimaItemDefinitionGroup> Groups
		{
			get { return _Groups; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaItemDefinitions.
		/// </summary>
		public UltimaItemDefinitions() : this( DefaultFile )
		{
		}

		/// <summary>
		/// Constructs a new instance of UltimaItemDefinitions.
		/// </summary>
		/// <param name="filePath">Item definitions</param>
		public UltimaItemDefinitions( string filePath )
		{
			_Items = new Dictionary<int, UltimaItemDefinition>();
			_Groups = new List<UltimaItemDefinitionGroup>();

			// Parse
			XmlDocument doc = new XmlDocument();
			doc.Load( filePath );

			XmlElement root = doc[ ElementRoot ];

			if ( root == null )
				throw new XmlException( String.Format( "File '{0}' is not valid", filePath ) );

			foreach ( XmlNode node in root.ChildNodes )
			{
				XmlElement e = node as XmlElement;

				if ( e != null && String.Equals( e.Name, ElementGroup ) )
				{
					_Groups.Add( new UltimaItemDefinitionGroup( this, null, e ) );
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima definition group.
	/// </summary>
	public class UltimaItemDefinitionGroup
	{
		#region Properties
		private const string ElementGroup = "group";
		private const string ElementProperty = "property";
		private const string ElementItem = "item";

		private const string AttributeName = "name";
		private const string AttributeAnalyze = "analyze";
		private const string AttributeClass = "class";
		private const string AttributeBasePhysical = "basePhysical";

		private UltimaItemDefinitionGroup _Parent;

		/// <summary>
		/// Gets parent.
		/// </summary>
		public UltimaItemDefinitionGroup Parent
		{
			get { return _Parent; }
		}

		private string _Name;

		/// <summary>
		/// Gets group name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}

		private bool _Analyze;

		/// <summary>
		/// Determines whether to analyze this group.
		/// </summary>
		public bool Analyze
		{
			get { return _Analyze; }
		}

		private string _Class;

		/// <summary>
		/// Gets group class.
		/// </summary>
		public string Class
		{
			get { return _Class; }
		}

		private List<UltimaItemDefinitionGroup> _Groups;

		/// <summary>
		/// Gets list of groups.
		/// </summary>
		public List<UltimaItemDefinitionGroup> Groups
		{
			get { return _Groups; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of UltimaItemDefinitionGroup.
		/// </summary>
		/// <param name="root">Root parent.</param>
		/// <param name="parent">Group parent.</param>
		/// <param name="e">Xml element to construct from.</param>
		public UltimaItemDefinitionGroup( UltimaItemDefinitions root, UltimaItemDefinitionGroup parent, XmlElement e )
		{
			_Parent = parent;
			_Groups = new List<UltimaItemDefinitionGroup>();

			// Parse
			_Name = e.GetAttribute( AttributeName );

			if ( String.IsNullOrEmpty( _Name ) )
				throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeName ) );

			string analyze = e.GetAttribute( AttributeAnalyze );

			if ( !String.IsNullOrEmpty( analyze ) )
			{
				if ( !Boolean.TryParse( analyze, out _Analyze ) )
					throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be boolean", AttributeAnalyze, analyze ) );

				if ( _Analyze )
				{
					_Class = e.GetAttribute( AttributeClass );

					if ( String.IsNullOrEmpty( _Class ) )
						throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeClass ) );
				}
			}
			else
				_Analyze = false;

			foreach ( XmlNode node in e.ChildNodes )
			{
				XmlElement child = node as XmlElement;

				if ( child != null )
				{
					if ( String.Equals( child.Name, ElementItem ) )
					{
						UltimaItemDefinition item = null;

						if ( child.HasAttribute( AttributeBasePhysical ) )
							item = new UltimaArmorDefinition( this, child );
						else
							item = new UltimaItemDefinition( this, child );

						foreach ( int itemID in item.ItemIDs )
						{
							if ( root.Items.ContainsKey( itemID ) )
								throw new XmlException( String.Format( "Item '0x{0:X}' already defined", itemID ) );

							root.Items.Add( itemID, item );
						}
					}
					else if ( String.Equals( child.Name, ElementGroup )  )
						_Groups.Add( new UltimaItemDefinitionGroup( root, this, child ) );
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima item definition.
	/// </summary>
	public class UltimaItemDefinition
	{
		#region Properties
		private const string AttributeItemID = "itemID";
		private const string AttributeClass = "class";
		private static readonly char[] Separators = { ',' };

		private UltimaItemDefinitionGroup _Parent;

		/// <summary>
		/// Gets parent.
		/// </summary>
		public UltimaItemDefinitionGroup Parent
		{
			get { return _Parent; }
		}

		private List<int> _ItemIDs;

		/// <summary>
		/// Gets a list of item IDs for this item.
		/// </summary>
		public List<int> ItemIDs
		{
			get { return _ItemIDs; }
		}

		private string _Class;

		/// <summary>
		/// Gets class definition.
		/// </summary>
		public string Class
		{
			get { return _Class; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of UltimaItemDefinitionGroup.
		/// </summary>
		/// <param name="parent">Item parent.</param>
		/// <param name="e">Xml element to construct from.</param>
		public UltimaItemDefinition( UltimaItemDefinitionGroup parent, XmlElement e )
		{
			_Parent = parent;
			_ItemIDs = new List<int>();

			string itemIDs = e.GetAttribute( AttributeItemID );

			if ( String.IsNullOrEmpty( itemIDs ) )
				throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeItemID ) );

			string[] list = itemIDs.Split( Separators, StringSplitOptions.RemoveEmptyEntries );

			foreach ( string value in list )
			{
				string toParse = value;
				int integer = 0;
				NumberStyles style = NumberStyles.Integer;

				if ( toParse.StartsWith( "0x" ) )
				{
					toParse = toParse.Substring( 2, toParse.Length - 2 );
					style = NumberStyles.HexNumber;
				}

				if ( Int32.TryParse( toParse, style, null, out integer ) )
					_ItemIDs.Add( integer );
				else
					throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", AttributeItemID, value ) );
			}

			_Class = e.GetAttribute( AttributeClass );

			if ( String.IsNullOrEmpty( _Class ) )
				throw new XmlException( String.Format( "Attribute '{0}' is missing", AttributeClass ) );
		}
		#endregion
	}

	/// <summary>
	/// Describes ultima armor definition.
	/// </summary>
	public class UltimaArmorDefinition : UltimaItemDefinition
	{
		#region Properties
		private const string AttributeBasePhysical = "basePhysical";
		private const string AttributeBaseFire = "baseFire";
		private const string AttributeBaseCold = "baseCold";
		private const string AttributeBasePoison = "basePoison";
		private const string AttributeBaseEnergy = "baseEnergy";
		private const string AttributeBaseChaos = "baseChaos";
		private const string AttributeBaseDirect = "baseDirect";

		private int _BasePhysical;

		/// <summary>
		/// Gets base physical resistance
		/// </summary>
		public int BasePhysical
		{
			get { return _BasePhysical; }
		}

		private int _BaseFire;

		/// <summary>
		/// Gets base fire resistance.
		/// </summary>
		public int BaseFire
		{
			get { return _BaseFire; }
		}

		private int _BaseCold;

		/// <summary>
		/// Gets base cold resistance.
		/// </summary>
		public int BaseCold
		{
			get { return _BaseCold; }
		}

		private int _BasePoison;

		/// <summary>
		/// Gets base poison resistance.
		/// </summary>
		public int BasePoison
		{
			get { return _BasePoison; }
		}

		private int _BaseEnergy;

		/// <summary>
		/// Gets base energy resistance.
		/// </summary>
		public int BaseEnergy
		{
			get { return _BaseEnergy; }
		}

		private int _BaseChaos;

		/// <summary>
		/// Gets base chaos resistance.
		/// </summary>
		public int BaseChaos
		{
			get { return _BaseChaos; }
		}

		private int _BaseDirect;

		/// <summary>
		/// Gets base direct resistance.
		/// </summary>
		public int BaseDirect
		{
			get { return _BaseDirect; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaArmorDefinition.
		/// </summary>
		/// <param name="parent">Item parent.</param>
		/// <param name="e">Xml element to construct from.</param>
		public UltimaArmorDefinition( UltimaItemDefinitionGroup parent, XmlElement e ) : base( parent, e )
		{
			_BasePhysical = GetAttributeAsInteger( e, AttributeBasePhysical );
			_BaseFire = GetAttributeAsInteger( e, AttributeBaseFire );
			_BaseCold = GetAttributeAsInteger( e, AttributeBaseCold );
			_BasePoison = GetAttributeAsInteger( e, AttributeBasePoison );
			_BaseEnergy = GetAttributeAsInteger( e, AttributeBaseEnergy );
			_BaseChaos = GetAttributeAsInteger( e, AttributeBaseChaos, false );
			_BaseDirect = GetAttributeAsInteger( e, AttributeBaseDirect, false );
		}
		#endregion

		#region Methods
		private int GetAttributeAsInteger( XmlElement e, string attribute, bool mandatory = true )
		{
			string value = e.GetAttribute( attribute );

			if ( mandatory )
			{
				if ( String.IsNullOrEmpty( value ) )
					throw new XmlException( String.Format( "Attribute '{0}' is missing", attribute ) );
			}
			else if ( String.IsNullOrEmpty( value ) )
				return 0;

			int integer = 0;
			NumberStyles style = NumberStyles.Integer;

			if ( value.StartsWith( "0x" ) )
				style = NumberStyles.HexNumber;

			if ( !Int32.TryParse( value, style, null, out integer ) )
				throw new XmlException( String.Format( "Invalid '{0}' value '{1}'. Value must be integer", attribute, value ) );

			return integer;
		}
		#endregion
	}
}
