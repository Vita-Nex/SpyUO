using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Value getter delegate.
	/// </summary>
	/// <param name="key">Key type.</param>
	/// <param name="value">Return value type.</param>
	/// <returns>Value.</returns>
	public delegate Value SimpleCacheGetter<Key, Value>( Key key );

	/// <summary>
	/// Describes simple cache.
	/// </summary>
	/// <typeparam name="Key">Cache key type.</typeparam>
	/// <typeparam name="Value">Cache value type.</typeparam>
	public class SimpleCache<Key,Value>
	{
		#region Properties
		private Dictionary<Key, Value> _Cache;
		private Dictionary<Key, DateTime> _Usage;
		private int _CacheSize;
		#endregion

		#region Events

		/// <summary>
		/// Value getter for values not in cache.
		/// </summary>
		public event SimpleCacheGetter<Key,Value> Getter;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of SimpleCache.
		/// </summary>
		/// <param name="cacheSize">Maximum cache size.</param>
		public SimpleCache( int cacheSize )
		{
			_CacheSize = cacheSize;
			_Cache = new Dictionary<Key, Value>( cacheSize );
			_Usage = new Dictionary<Key, DateTime>( cacheSize );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets value from cache.
		/// </summary>
		/// <param name="key">Value from cache.</param>
		/// <returns>Value.</returns>
		public Value Get( Key key )
		{
			if ( _Cache.ContainsKey( key ) )
			{
				_Usage[ key ] = DateTime.Now;
				return _Cache[ key ];
			}

			if ( Getter == null )
				return default( Value );

			if ( _Usage.Count + 1 > _CacheSize )
			{
				Key minKey = default( Key );
				DateTime minTime = DateTime.Now;

				foreach ( KeyValuePair<Key, DateTime> kvp in _Usage )
				{
					if ( kvp.Value < minTime )
					{
						minKey = kvp.Key;
						minTime = kvp.Value;
					}
				}

				_Usage.Remove( minKey );
				_Cache.Remove( minKey );
			}

			Value value = Getter( key );

			_Usage.Add( key, DateTime.Now );
			_Cache.Add( key, value );

			return value;
		}

		/// <summary>
		/// Clears cache.
		/// </summary>
		public void Clear()
		{
			_Cache.Clear();
			_Usage.Clear();
		}
		#endregion
	}
}
