using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for FilterPropertiesEditor.xaml
	/// </summary>
	public partial class FilterPropertiesEditor : Window
	{
		#region Properties
		/// <summary>
		/// Represents Entry property.
		/// </summary>
		public static readonly DependencyProperty EntryProperty = DependencyProperty.Register(
			"Entry", typeof( UltimaPacketFilterEntry ), typeof( FilterPropertiesEditor ), 
			new PropertyMetadata( null, new PropertyChangedCallback( Entry_Changed ) ) );

		/// <summary>
		/// Gets or sets entry bound to this thing.
		/// </summary>
		public UltimaPacketFilterEntry Entry
		{
			get { return GetValue( EntryProperty ) as UltimaPacketFilterEntry; }
			set { SetValue( EntryProperty, value ); }
		}

		/// <summary>
		/// Represents Properties property.
		/// </summary>
		public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(
			"Properties", typeof( List<UltimaPacketFilterProperty> ), typeof( FilterPropertiesEditor ),
			new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets list of properties.
		/// </summary>
		public List<UltimaPacketFilterProperty> Properties
		{
			get { return GetValue( PropertiesProperty ) as List<UltimaPacketFilterProperty>; }
			set { SetValue( PropertiesProperty, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of FilterPropertiesEditor.
		/// </summary>
		public FilterPropertiesEditor()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private static void Entry_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			FilterPropertiesEditor editor = d as FilterPropertiesEditor;

			if ( editor != null  )
				editor.UpdateEntry();
		}

		private void UpdateEntry()
		{
			UltimaPacketFilterEntry entry = Entry;

			if ( entry == null )
			{
				DataContext = null;
				Filters.DataContext = null;
				return;
			}

			DataContext = entry;

			// Clone filter properties
			Properties = new List<UltimaPacketFilterProperty>();

			if ( entry.Properties != null )
			{
				foreach ( UltimaPacketFilterProperty property in entry.Properties )
					Properties.Add( property.Clone() );
			}

			Filters.ItemsSource = null;
			Filters.ItemsSource = Properties;

			if ( Properties.Count > 0 )
				Filters.SelectedIndex = 0;

			// Get available properties
			List<UltimaPacketPropertyDefinition> availableProperties = new List<UltimaPacketPropertyDefinition>();

			foreach ( UltimaPacketPropertyDefinition definition in Entry.Definition.Properties )
			{
				if ( UltimaPacketFilterParser.GetTypeOperations( definition ) != null )
					availableProperties.Add( definition );
			}

			Definition.ItemsSource = availableProperties;
		}

		private void UpdateSample()
		{
			UltimaPacketFilterProperty filter = Filters.SelectedValue as UltimaPacketFilterProperty;
			UltimaPacketPropertyDefinition property = Definition.SelectedValue as UltimaPacketPropertyDefinition;
			UltimaPacketFilterTypeOperation? operation = Operation.SelectedValue as UltimaPacketFilterTypeOperation?;

			if ( filter != null && property != null && operation != null )
			{
				SampleValue.Text = UltimaPacketFilterParser.GetSample( property, (UltimaPacketFilterTypeOperation) operation, false );
				Sample.Text = UltimaPacketFilterParser.GetComposedSample( property, (UltimaPacketFilterTypeOperation) operation, false, false );
			}
		}

		private void RemoveFilter()
		{
			if ( Properties == null )
				return;

			UltimaPacketFilterProperty filter = Filters.SelectedValue as UltimaPacketFilterProperty;
			int index = Filters.SelectedIndex;

			if ( filter != null )
			{
				Properties.Remove( filter );

				if ( index >= Properties.Count )
					index--;

				Filters.ItemsSource = null;
				Filters.ItemsSource = Properties;
				Filters.SelectedIndex = index;
			}
		}

		private void AddButton_Click( object sender, RoutedEventArgs e )
		{
			if ( Properties == null || Entry == null )
				return;

			UltimaPacketFilterProperty filter = new UltimaPacketFilterProperty( Entry );
			filter.IsChecked = true;
			filter.Text = filter.ToString();
			Properties.Add( filter );
			Filters.ItemsSource = null;
			Filters.ItemsSource = Properties;
		}

		private void RemoveButton_Click( object sender, RoutedEventArgs e )
		{
			RemoveFilter();
		}

		private void ConfirmButton_Click( object sender, RoutedEventArgs e )
		{
			if ( Properties == null || Entry == null )
				return;

			bool isValid = true;

			if ( Properties.Count > 0 )
			{
				foreach ( UltimaPacketFilterProperty filter in Properties )
				{
					if ( !filter.IsValid )
					{
						isValid = false;
						break;
					}
				}
			}
			else
				isValid = false;

			if ( isValid )
			{
				Entry.Properties = Properties;
				DialogResult = true;
			}
			else
			{
				Filters.BorderThickness = new Thickness( 1 );
				Filters.BorderBrush = new SolidColorBrush( Colors.Red );
			}
		}

		private void Filters_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Delete )
				RemoveFilter();
		}

		private void Filters_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			UltimaPacketFilterProperty filter = Filters.SelectedValue as UltimaPacketFilterProperty;
			bool isValid = filter != null;

			Definition.IsEnabled = isValid;
			Operation.IsEnabled = isValid;
			Value.IsEnabled = isValid;

			if ( isValid )
			{
				Sample.Visibility = Visibility.Visible;
				SampleValue.Visibility = Visibility.Visible;
			}
			else
			{
				Sample.Visibility = Visibility.Collapsed;
				SampleValue.Visibility = Visibility.Collapsed;
			}

			UpdateSample();
		}

		private void Definition_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			UltimaPacketFilterProperty filter = Filters.SelectedValue as UltimaPacketFilterProperty;
			UltimaPacketPropertyDefinition property = Definition.SelectedValue as UltimaPacketPropertyDefinition;

			if ( property != null )
				Operation.ItemsSource = UltimaPacketFilterParser.GetTypeOperations( property );

			if ( filter != null )
			{
				Operation.SelectedValue = null;
				Operation.SelectedValue = filter.Operation;
			}

			UpdateSample();
		}

		private void Operation_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			UpdateSample();
		}
		#endregion
	}
}
