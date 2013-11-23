using System;
using System.Windows;
using System.Windows.Controls;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Selector for filter entry template.
	/// </summary>
	public class FilterEntryTemplateSelector : DataTemplateSelector
	{
		#region Properties
		private DataTemplate _PropertyTemplate;

		/// <summary>
		/// Gets or sets property template.
		/// </summary>
		public DataTemplate PropertyTemplate
		{
			get { return _PropertyTemplate; }
			set { _PropertyTemplate = value; }
		}

		private HierarchicalDataTemplate _EntryTemplate;

		/// <summary>
		/// Gets or sets table template.
		/// </summary>
		public HierarchicalDataTemplate EntryTemplate
		{
			get { return _EntryTemplate; }
			set { _EntryTemplate = value; }
		}

		private HierarchicalDataTemplate _TableTemplate;

		/// <summary>
		/// Gets or sets table template.
		/// </summary>
		public HierarchicalDataTemplate TableTemplate
		{
			get { return _TableTemplate; }
			set { _TableTemplate = value; }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Selects template based on item type.
		/// </summary>
		/// <param name="item">Item to select template for.</param>
		/// <param name="container">Container that holds property.</param>
		/// <returns>Data template.</returns>
		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if ( item is UltimaPacketFilterProperty )
				return _PropertyTemplate;
			else if ( item is UltimaPacketFilterEntry )
				return _EntryTemplate;
			else if ( item is UltimaPacketFilterTable )
				return _TableTemplate;

			return null;
		}
		#endregion
	}

	/// <summary>
	/// Selector for ultima packet properties.
	/// </summary>
	public class PropertyTemplateSelector : DataTemplateSelector
	{
		#region Properties
		private HierarchicalDataTemplate _ObjectTemplate;

		/// <summary>
		/// Gets or sets template, which represents object.
		/// </summary>
		public HierarchicalDataTemplate ObjectTemplate
		{
			get { return _ObjectTemplate; }
			set { _ObjectTemplate = value; }
		}

		private HierarchicalDataTemplate _ListPropertyTemplate;

		/// <summary>
		/// Gets or sets property which represents list of objects.
		/// </summary>
		public HierarchicalDataTemplate ListPropertyTemplate
		{
			get { return _ListPropertyTemplate; }
			set { _ListPropertyTemplate = value; }
		}

		private DataTemplate _DirectionTemplate;

		/// <summary>
		/// Gets or sets direction template.
		/// </summary>
		public DataTemplate DirectionTemplate
		{
			get { return _DirectionTemplate; }
			set { _DirectionTemplate = value; }
		}

		private DataTemplate _MusicTemplate;

		/// <summary>
		/// Gets or sets music template
		/// </summary>
		public DataTemplate MusicTemplate
		{
			get { return _MusicTemplate; }
			set { _MusicTemplate = value; }
		}

		private DataTemplate _SoundTemplate;

		/// <summary>
		/// Gets or sets sound template
		/// </summary>
		public DataTemplate SoundTemplate
		{
			get { return _SoundTemplate; }
			set { _SoundTemplate = value; }
		}

		private DataTemplate _TextureTemplate;

		/// <summary>
		/// Gets or sets texture template.
		/// </summary>
		public DataTemplate TextureTemplate
		{
			get { return _TextureTemplate; }
			set { _TextureTemplate = value; }
		}

		private DataTemplate _ClilocTemplate;

		/// <summary>
		/// Gets or sets cliloc template.
		/// </summary>
		public DataTemplate ClilocTemplate
		{
			get { return _ClilocTemplate; }
			set { _ClilocTemplate = value; }
		}

		private DataTemplate _BodyTemplate;

		/// <summary>
		/// Gets or sets body template.
		/// </summary>
		public DataTemplate BodyTemplate
		{
			get { return _BodyTemplate; }
			set { _BodyTemplate = value; }
		}

		private DataTemplate _DefaultPropertyTemplate;

		/// <summary>
		/// Gets or sets template, which represents simple property (title and value).
		/// </summary>
		public DataTemplate DefaultPropertyTemplate
		{
			get { return _DefaultPropertyTemplate; }
			set { _DefaultPropertyTemplate = value; }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Selects template based on item type.
		/// </summary>
		/// <param name="item">Item to select template for.</param>
		/// <param name="container">Container that holds property.</param>
		/// <returns>Data template.</returns>
		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if ( item is UltimaPacketPropertyValue )
			{
				UltimaPacketPropertyValue property = (UltimaPacketPropertyValue) item;

				if ( property.Definition is UltimaPacketListPropertyDefinition )
					return _ListPropertyTemplate;

				switch ( property.Definition.Attribute.Type )
				{
					case UltimaPacketPropertyType.Direction: return _DirectionTemplate;
					case UltimaPacketPropertyType.Music:
					{
						if ( Globals.Instance.EnhancedAssets == null || Globals.Instance.VlcPlayer == null )
							return _DefaultPropertyTemplate;

						return _MusicTemplate;
					}
					case UltimaPacketPropertyType.Sound:
					{
						if ( ( Globals.Instance.EnhancedAssets == null && Globals.Instance.LegacyAssets == null ) ||  Globals.Instance.VlcPlayer == null )
							return _DefaultPropertyTemplate;

						return _SoundTemplate;
					}
					case UltimaPacketPropertyType.Texture:
					{
						if ( Globals.Instance.EnhancedAssets == null && Globals.Instance.LegacyAssets == null )
							return _DefaultPropertyTemplate;

						return _TextureTemplate;
					}
					case UltimaPacketPropertyType.Cliloc:
					{
						if ( Globals.Instance.Clilocs == null )
							return _DefaultPropertyTemplate;

						return _ClilocTemplate;
					}
					case UltimaPacketPropertyType.Body:
					{
						if ( Globals.Instance.EnhancedAssets == null && Globals.Instance.LegacyAssets == null )
							return _DefaultPropertyTemplate;

						return _BodyTemplate;
					}
				}

				return _DefaultPropertyTemplate;
			}
			else if ( item is UltimaPacketValue )
				return _ObjectTemplate;

			return null;
		}
		#endregion
	}

	/// <summary>
	/// Selector for ultima packet properties.
	/// </summary>
	public class PropertyTemplateContainerSelector : StyleSelector
	{
		#region Properties
		private Style _DefaultPropertyStyle;

		/// <summary>
		/// Gets or sets style, which represents simple property (title and value).
		/// </summary>
		public Style DefaultPropertyStyle
		{
			get { return _DefaultPropertyStyle; }
			set { _DefaultPropertyStyle = value; }
		}

		private Style _ListPropertyStyle;

		/// <summary>
		/// Gets or sets property which represents list of objects.
		/// </summary>
		public Style ListPropertyStyle
		{
			get { return _ListPropertyStyle; }
			set { _ListPropertyStyle = value; }
		}
		#endregion

		#region Methods
		/// <summary>
		/// Selects template based on item type.
		/// </summary>
		/// <param name="item">Item to select template for.</param>
		/// <param name="container">Container that holds property.</param>
		/// <returns>Data template.</returns>
		public override Style SelectStyle(object item, DependencyObject container)
		{
			if ( item is UltimaPacketPropertyValue )
			{
				UltimaPacketPropertyValue property = (UltimaPacketPropertyValue) item;

				if ( property.Definition is UltimaPacketListPropertyDefinition )
					return _ListPropertyStyle;
			}

			return _DefaultPropertyStyle;
		}
		#endregion
	}
}
