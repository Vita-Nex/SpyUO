using System;
using System.Windows;
using System.Diagnostics;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for ErrorWindow.xaml
	/// </summary>
	public partial class ErrorWindow : Window
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of ErrorWindow.
		/// </summary>
		public ErrorWindow()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Shows error window.
		/// </summary>
		/// <param name="ex">Exception to show.</param>
		public static void Show( Exception ex )
		{
			ErrorWindow window = new ErrorWindow();
			window.ErrorTitle.Text = ex.Message;
			window.ErrorMessage.Text = ex.StackTrace;

			window.ShowDialog();
		}

		/// <summary>
		/// Shows error window.
		/// </summary>
		/// <param name="ex">Exception to show.</param>
		public static void Show( string title, string format, params string[] args )
		{
			ErrorWindow window = new ErrorWindow();
			window.ErrorTitle.Text = title;
			window.ErrorMessage.Text = String.Format( format, args );

			window.ShowDialog();
		}

		#region Event Handlers
		private void SubmitBug_Click( object sender, RoutedEventArgs e )
		{
			Process.Start( "www.google.com" );
		}

		private void CloseButton_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}
		#endregion
		#endregion
	}
}
