using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LRUCacheNameSpace
{
	class Program
	{
		static void Main(string[] args)
		{
			LRUCache<Employee> oLRUCache = LRUCache<Employee>.GetInstance();

			Employee oEmp = null;
			CacheObject<Employee> cacheItem = null;

			Task task1 = Task.Run(() =>
			{
				oEmp = new Employee { Name = "Lokesh Sharma", SSN = "1182", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User1", SSN = "1180", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User2", SSN = "1184", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User2", SSN = "1183", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);
			});

			Task task2 = Task.Run(() =>
			{
				oEmp = new Employee { Name = "Test User3", SSN = "1185", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User1", SSN = "1180", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User4", SSN = "1186", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User5", SSN = "1187", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);

				oEmp = new Employee { Name = "Test User4", SSN = "1186", Company = "Winshuttle" };
				cacheItem = new CacheObject<Employee>(oEmp.SSN, oEmp);
				oLRUCache.Put(cacheItem);
			});

			Task.WaitAll(task1, task2);

			Print(oLRUCache);
		}

		static void Print(LRUCache<Employee> cache)
		{
			IEnumerator<CacheObject<Employee>> enumerator = cache.CacheList.GetEnumerator();

			while (enumerator.MoveNext())
			{
				Console.WriteLine("Name: {0}, SSN: {1}, Company: {2}", enumerator.Current.Value.Name, enumerator.Current.Value.SSN, enumerator.Current.Value.Company);
			}
		}
	}

	public class Employee
	{
		public string Name;
		public string SSN;
		public string Company;
	}

	public class CacheObject<T> where T : class
	{
		string _key;
		T _value;

		public CacheObject(string key, T value)
		{
			this._key = key;
			this._value = value;
		}

		public string Key
		{
			get { return this._key; }
		}

		public T Value
		{
			get { return this._value; }
		}
	}

	public abstract class Cache<T> where T : class
	{
		public abstract CacheObject<T> Get(string key);
		public abstract void Put(CacheObject<T> cache);
	}

	public class LRUCache<T> : Cache<T> where T : class
	{
		const int LIST_THRASHOLD_SIZE = 5;
		static LRUCache<T> _instance;
		static Dictionary<string, LinkedListNode<CacheObject<T>>> _map = new Dictionary<string, LinkedListNode<CacheObject<T>>>();
		static LinkedList<CacheObject<T>> _list = new LinkedList<CacheObject<T>>();

		private LRUCache()
		{ 
			// 
		}

		static object _lockObj = new object();
		public static LRUCache<T> GetInstance()
		{
			lock (_lockObj)
			{
				if (_instance == null)
				{
					lock (_lockObj)
					{
						_instance = new LRUCache<T>();
					}
				}
			}

			return _instance;
		}

		public LinkedList<CacheObject<T>> CacheList
		{
			get { return _list; }
		}

		public override CacheObject<T> Get(string key)
		{
			lock (_lockObj)
			{
				// if exists, then get it.
				if (_map.ContainsKey(key))
				{
					CacheObject<T> cachedItem = null;
					cachedItem = _map[key].Value;

					// re-arrange the linked list for frequently accessed item.
					RearrangeList(cachedItem);

					return cachedItem;
				}
			}

			// return the cache object
			return default(CacheObject<T>);
		}

		public override void Put(CacheObject<T> cacheItem)
		{
			lock (_lockObj)
			{
				if (_map.ContainsKey(cacheItem.Key))
				{
					// delete and insert again.
					_list.Remove(_map[cacheItem.Key]);
					_map.Remove(cacheItem.Key);
				}

				if (_list.Count < LIST_THRASHOLD_SIZE)
				{
					var node = _list.AddFirst(cacheItem);
					_map.Add(cacheItem.Key, node);
				}
				else
				{
					// while putting check for the size availability. If size exceeds the threashhold value, then remove the least recently used cache and put in the new cache.
					Evict();

					var node = _list.AddFirst(cacheItem);
					_map.Add(cacheItem.Key, node);
				}
			}
		}

		private void Evict()
		{
			lock (_lockObj)
			{
				_map.Remove(_list.Last.Value.Key);
				_list.RemoveLast();
			}
		}

		private void RearrangeList(CacheObject<T> cacheItem)
		{
			lock (_lockObj)
			{
				_list.Remove(cacheItem);
				_map.Remove(cacheItem.Key);

				LinkedListNode<CacheObject<T>> node = _list.AddFirst(cacheItem);
				_map.Add(cacheItem.Key, node);
			}
		}
	}
}
