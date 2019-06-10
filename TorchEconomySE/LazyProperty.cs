using System;

namespace TorchEconomySE
{
	public class LazyProperty<T> where T : class, new()
	{
		private readonly Func<T> _constructor;
		private T _cachedValue;
		private TimeSpan _cacheLifetime;
		private DateTime _lastRefresh;

		public LazyProperty(Func<T> constructor)
		{
			_constructor = constructor;
			_cacheLifetime = TimeSpan.FromMinutes(5);
		}
        
		public LazyProperty(Func<T> constructor, int lifetimeSeconds)
		{
			_constructor = constructor;
			_cacheLifetime = TimeSpan.FromMinutes(lifetimeSeconds * 60);
		}
        
		public LazyProperty(Func<T> constructor, TimeSpan cacheLifetime)
		{
			_constructor = constructor;
			_cacheLifetime = cacheLifetime;
		}
        
		public T Get()
		{
			if (_cachedValue == null)
				Refresh();
			if ((DateTime.UtcNow - _lastRefresh) > _cacheLifetime)
				Refresh();
            
			return _cachedValue;
		}

		public void Set(T value)
		{
			_cachedValue = value;
			_lastRefresh = DateTime.UtcNow;
		}

		public void Refresh()
		{
			Set(_constructor());
		}
	}
}