using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Describes smart collection.
	/// </summary>
	/// <typeparam name="T">Type of objects in collection.</typeparam>
	public class SmartObservableCollection<T> : ObservableCollection<T>
	{
		#region Methods
		/// <summary>
		/// Add range of items to collection.
		/// </summary>
		/// <param name="items">Items to add.</param>
		public void AddRange( IEnumerable<T> items )
		{
			CheckReentrancy();

			foreach ( T o in items )
				Items.Add( o );

			OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}
		#endregion
	}
}
