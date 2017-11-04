using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MkJSON
{
	public class JSON : IDictionary<string, JSON>
	{
		public enum ValueType
		{
			Undefined,
			Null,
			Object,
			Array,
			String,
			Integer,
			Float,
			Boolean,
		}

		private static readonly char[] __whitespace = new char[] { ' ', '\n', '\r', '\t' };
		private static readonly Type __jsonType = new JSON().GetType();
		private static readonly Dictionary<Type, CallMethod> __tryGetValueMethods = GetTryGetValueMethods();
		private static readonly MethodInfo __getArrayMethod = __jsonType.GetMethod("GetArray", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo __toMethod = __jsonType.GetMethod("To");

		private ValueType _type = ValueType.Undefined;
		private object _value = null;
		private int _maxIndex = -1;

		#region Properties
		private static GlobalParameters _global = new GlobalParameters();
		public static GlobalParameters Global {
			get
			{
				return _global;
			}
			set
			{
				_global = value;
			}
		}

		public static JSON Undefined
		{
			get
			{
				return new JSON(ValueType.Undefined);
			}
		}

		public static JSON Null
		{
			get
			{
				return new JSON(ValueType.Null);
			}
		}

		public bool? Strict { get; set; }

		public JSON this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				Add(index, value);
			}
		}

		public JSON this[string name]
		{
			get
			{
				return GetItem(name);
			}
			set
			{
				Add(name, value);
			}
		}

		public int Count
		{
			get
			{
				if (_type == ValueType.Array)
				{
					return _maxIndex + 1;
				}
				if (_type == ValueType.Object)
				{
					return ((Dictionary<string, JSON>)_value).Count;
				}
				if (_type == ValueType.Undefined || _type == ValueType.Null)
				{
					return 0;
				}
				return 1;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsNull
		{
			get
			{
				return _type == ValueType.Null;
			}
		}

		public bool IsUndefined
		{
			get
			{
				return _type == ValueType.Undefined;
			}
		}

		public bool IsArray
		{
			get
			{
				return _type == ValueType.Array;
			}
		}

		public ValueType Type
		{
			get
			{
				return _type;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				if (_type != ValueType.Object)
				{
					return null;
				}

				return ((Dictionary<string, JSON>)_value).Keys;
			}
		}

		public ICollection<JSON> Values
		{
			get
			{
				if (_type != ValueType.Object)
				{
					return null;
				}

				return ((Dictionary<string, JSON>)_value).Values;
			}
		}
		#endregion

		#region Init workers
		private static Dictionary<Type, CallMethod> GetTryGetValueMethods()
		{
			Dictionary<Type, CallMethod> methods = new Dictionary<Type, CallMethod>();

			MethodInfo method;

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(string).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(string), new CallMethod(method, typeof(string)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(bool?).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(bool?), new CallMethod(method, typeof(bool?)));
			methods.Add(typeof(bool), new CallMethod(method, typeof(bool?)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(float?).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(float?), new CallMethod(method, typeof(float?)));
			methods.Add(typeof(float), new CallMethod(method, typeof(float?)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(double?).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(double?), new CallMethod(method, typeof(double?)));
			methods.Add(typeof(double), new CallMethod(method, typeof(double?)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(int?).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(int?), new CallMethod(method, typeof(int?)));
			methods.Add(typeof(int), new CallMethod(method, typeof(int?)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(long?).MakeByRefType(), typeof(bool) });
			methods.Add(typeof(long?), new CallMethod(method, typeof(long?)));
			methods.Add(typeof(long), new CallMethod(method, typeof(long?)));

			method = __jsonType.GetMethod("TryGetValue", new Type[] { typeof(DateTime?).MakeByRefType() });
			methods.Add(typeof(DateTime?), new CallMethod(method, typeof(DateTime?)));
			methods.Add(typeof(DateTime), new CallMethod(method, typeof(DateTime?)));

			return methods;
		}

		private struct CallMethod
		{
			public MethodInfo MethodInfo;
			public Type ParameterType;
			public bool WithStrict;

			public CallMethod(MethodInfo methodInfo, Type parameterType)
			{
				MethodInfo = methodInfo;
				ParameterType = parameterType;
				WithStrict = (methodInfo.GetParameters().Length == 2);
			}
		}
		#endregion

		#region Contructors
		public JSON()
		{
			_type = ValueType.Undefined;
		}

		public JSON(ValueType type, bool? caseSensitive = null)
		{
			if (type == ValueType.Array)
			{
				_value = new SortedDictionary<int, JSON>();
			}
			else if (type == ValueType.Object)
			{
				if (caseSensitive == null)
				{
					caseSensitive = Global.CaseSensitive;
				}

				if (caseSensitive != true)
				{
					_value = new Dictionary<string, JSON>(StringComparer.OrdinalIgnoreCase);
				}
				else
				{
					_value = new Dictionary<string, JSON>();
				}
			}
			else if (type != ValueType.Undefined && type != ValueType.Null)
			{
				throw new Exception("JSON object cannot be initialized as " + type.ToString());
			}

			_type = type;
		}

		public JSON(string value)
		{
			if (value == null)
			{
				_type = ValueType.Null;
			}
			else
			{
				_type = ValueType.String;
				_value = value;
			}
		}

		public JSON(bool value)
		{
			_type = ValueType.Boolean;
			_value = value;
		}

		public JSON(bool? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.Boolean;
				_value = (bool)value.Value;
				return;
			}
			_type = ValueType.Null;
		}

		public JSON(int value)
		{
			_type = ValueType.Integer;
			_value = (long)value;
		}

		public JSON(int? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.Integer;
				_value = (long)value.Value;
				return;
			}
			_type = ValueType.Null;
		}

		public JSON(long value)
		{
			_type = ValueType.Integer;
			_value = value;
		}

		public JSON(long? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.Integer;
				_value = (long)value.Value;
				return;
			}
			_type = ValueType.Null;
		}

		public JSON(float value)
		{
			_type = ValueType.Float;
			_value = double.Parse(value.ToString("r"));
		}

		public JSON(float? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.Float;
				_value = (double)value.Value;
				return;
			}
			_type = ValueType.Null;
		}

		public JSON(double value)
		{
			_type = ValueType.Float;
			_value = value;
		}

		public JSON(double? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.Float;
				_value = (double)value.Value;
				return;
			}
			_type = ValueType.Null;
		}

		public JSON(DateTime value)
		{
			_type = ValueType.String;
			_value = value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
		}

		public JSON(DateTime? value)
		{
			if (value.HasValue)
			{
				_type = ValueType.String;
				_value = value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
				return;
			}
			_type = ValueType.Null;
		}
		#endregion

		#region Add(index, value)
		public void Add(int index, string value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, bool value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, int value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, long value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, float value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, double value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(int index, DateTime value, bool? strict = null)
		{
			Add(index, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add<T>(int index, T value, bool? strict = null)
		{
			JSON json = JSON.From(value);

			json.Strict = strict;

			Add(index, json, strict);
		}

		public void Add(KeyValuePair<int, JSON> item)
		{
			Add(item.Key, item.Value, null);
		}

		public void Add(int index, JSON value, bool? strict = null)
		{
			if (_type == ValueType.Undefined && !IsStrict(strict))
			{
				_value = new SortedDictionary<int, JSON>();
				_type = ValueType.Array;
			}
			else if (_type != ValueType.Array)
			{
				throw new Exception("Can access indices only with an Array");
			}
			if (index < 0)
			{
				throw new Exception("Cannot add negative index");
			}

			if (index > _maxIndex)
			{
				_maxIndex = index;
			}

			value.Strict = Strict;

			if (((SortedDictionary<int, JSON>)_value).ContainsKey(index))
			{
				((SortedDictionary<int, JSON>)_value)[index] = value;
			}
			else
			{
				((SortedDictionary<int, JSON>)_value).Add(index, value);
			}
		}
		#endregion

		#region Add(name, value)
		public void Add(string name, string value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, bool value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, int value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, long value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, float value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, double value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add(string name, DateTime value, bool? strict = null)
		{
			Add(name, new JSON(value) { Strict = Strict }, strict);
		}

		public void Add<T>(string name, T value, bool? strict = null)
		{
			JSON json = JSON.From(value);

			json.Strict = strict;

			Add(name, json, strict);
		}

		public void Add(KeyValuePair<string, JSON> item)
		{
			Add(item.Key, item.Value, null);
		}

		public void Add(KeyValuePair<string, JSON> item, bool? strict = null)
		{
			Add(item.Key, item.Value, strict);
		}

		public void Add(string name, JSON value)
		{
			Add(name, value, null);
		}

		public void Add(string name, JSON value, bool? strict = null)
		{
			if (_type == ValueType.Undefined && !IsStrict(strict))
			{
				_value = new Dictionary<string, JSON>();
				_type = ValueType.Object;
			}
			else if (_type != ValueType.Object)
			{
				throw new Exception("Can add named items only to an Object");
			}

			if (value == null)
			{
				value = new JSON(ValueType.Undefined);
			}

			value.Strict = strict;

			if (((Dictionary<string, JSON>)_value).ContainsKey(name))
			{
				((Dictionary<string, JSON>)_value)[name] = value;
			}
			else
			{
				((Dictionary<string, JSON>)_value).Add(name, value);
			}
		}
		#endregion

		public void Clear()
		{
			if (_type == ValueType.Array)
			{
				_value = new SortedDictionary<int, JSON>();
				_maxIndex = -1;
			}
			if (_type == ValueType.Object)
			{
				_value = new Dictionary<string, JSON>();
			}
		}

		#region Contains()
		public bool Contains(KeyValuePair<int, JSON> item)
		{
			if (_value != null && _type == ValueType.Array)
			{
				SortedDictionary<int, JSON> list = (SortedDictionary<int, JSON>)_value;

				if (list.ContainsKey(item.Key))
				{
					return list[item.Key] == item.Value;
				}
			}
			return false;
		}

		public bool Contains(KeyValuePair<string, JSON> item)
		{
			if (_value != null && _type == ValueType.Object)
			{
				Dictionary<string, JSON> list = (Dictionary<string, JSON>)_value;

				if (list.ContainsKey(item.Key))
				{
					return list[item.Key] == item.Value;
				}
			}
			return false;
		}
		#endregion

		#region ContainsKey()
		public bool ContainsKey(int index)
		{
			if (_value != null && _type == ValueType.Array)
			{
				return ((SortedDictionary<int, JSON>)_value).ContainsKey(index);
			}
			return false;
		}

		public bool ContainsKey(string name)
		{
			if (_value != null && _type == ValueType.Object)
			{
				return ((Dictionary<string, JSON>)_value).ContainsKey(name);
			}
			return false;
		}
		#endregion

		#region CopyTo()
		public void CopyTo(KeyValuePair<int, JSON>[] array, int index)
		{
			if (_type != ValueType.Array)
			{
				throw new Exception("Can access indices only with an Array");
			}
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (index < 0)
			{
				throw new Exception("Index out of range");
			}
			if (array.Rank != 1)
			{
				throw new Exception("Not a one dimensional array");
			}

			SortedDictionary<int, JSON> list = (SortedDictionary<int, JSON>)_value;
			int count = list.Count - index;

			if (count > array.Length)
			{
				throw new Exception("The array is too small");
			}

			for (int i = 0; i < count; i++)
			{
				array[i] = list.ElementAt(index);
				++index;
			}
		}

		public void CopyTo(KeyValuePair<string, JSON>[] array, int index)
		{
			if (_type != ValueType.Object)
			{
				throw new Exception("Cant iterate key values only with an object");
			}
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (index < 0)
			{
				throw new Exception("Index out of range");
			}
			if (array.Rank != 1)
			{
				throw new Exception("Not a one dimensional array");
			}

			SortedDictionary<string, JSON> list = (SortedDictionary<string, JSON>)_value;

			if (list.Count - index > array.Length)
			{
				throw new Exception("The array is too small");
			}

			for (int i = 0; i < list.Count - index; i++)
			{
				array[i] = list.ElementAt(index);
				++index;
			}
		}
		#endregion

		#region Equals()
		public override bool Equals(object value)
		{
			return Equals(JSON.From(value));
		}

		public bool Equals<T>(T value, bool? strict)
		{
			return Equals(JSON.From(value, strict), strict);
		}

		public bool Equals(JSON value, bool? strict = null)
		{
			switch (_type)
			{
				case ValueType.Undefined:
				{
					return value.Type == ValueType.Undefined || (!IsStrict(strict) && value.Type == ValueType.Null);
				}
				case ValueType.Null:
				{
					return value.Type == ValueType.Null || (!IsStrict(strict) && value.Type == ValueType.Undefined);
				}
				case ValueType.Array:
				{
					if (value.Type != ValueType.Array || value.Count != Count)
					{
						return false;
					}

					SortedDictionary<int, JSON> array = (SortedDictionary<int, JSON>)_value;

					for (int i = 0; i < Count; i++)
					{
						if (!array[i].Equals(value[i], strict))
						{
							return false;
						}
					}
					return true;
				}
				case ValueType.Object:
				{
					if (value.Type != ValueType.Object)
					{
						return false;
					}

					Dictionary<string, JSON> array = (Dictionary<string, JSON>)_value;

					if (value.Count != array.Count)
					{
						return false;
					}

					foreach (KeyValuePair<string, JSON> item in array)
					{
						if (!value.Contains(item))
						{
							return false;
						}
						if (!value[item.Key].Equals(item.Value, strict))
						{
							return false;
						}
					}
					return true;
				}
				case ValueType.Boolean:
				{
					if (IsStrict(strict))
					{
						return value.Type == ValueType.Boolean && value.ToBool(true) == (bool)_value;
					}
					return value.ToBool(false) == (bool)_value;
				}
				case ValueType.Float:
				{
					if (IsStrict(strict))
					{
						return value.Type == ValueType.Float && value.ToDouble(true) == (double)_value;
					}
					return value.ToDouble(false) == (double)_value;
				}
				case ValueType.Integer:
				{
					if (IsStrict(strict))
					{
						return value.Type == ValueType.Integer && value.ToLong(true) == (long)_value;
					}
					return value.ToLong(false) == (long)_value;
				}
				case ValueType.String:
				{
					if (IsStrict(strict))
					{
						return value.Type == ValueType.String && value.ToString(true) == (string)_value;
					}
					return value.ToString(false) == (string)_value;
				}
				default:
				{
					throw new Exception("A bug: unhandled value type");
				}
			}
		}

		bool Equals(bool value, bool? strict = null)
		{
			if (IsStrict(strict))
			{
				return _type == ValueType.Boolean && value == (bool)_value;
			}
			return value == ToBool(false);
		}

		bool Equals(float value, bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.Float)
			{
				return false;
			}
			if (TryGetValue(out float? output, strict))
			{
				return value.Equals(output.Value);
			}
			return false;
		}

		bool Equals(double value, bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.Float)
			{
				return false;
			}
			if (TryGetValue(out double? output, strict))
			{
				return value.Equals(output.Value);
			}
			return false;
		}

		bool Equals(int value, bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.Integer)
			{
				return false;
			}
			if (TryGetValue(out int? output, strict))
			{
				return value == output.Value;
			}
			return false;
		}

		bool Equals(long value, bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.Integer)
			{
				return false;
			}
			if (TryGetValue(out long? output, strict))
			{
				return value == output.Value;
			}
			return false;
		}

		bool Equals(string value, bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.String)
			{
				return false;
			}
			if (TryGetValue(out string output, strict))
			{
				return value == output;
			}
			return false;
		}

		bool Equals(DateTime value)
		{
			if (_type != ValueType.String)
			{
				return false;
			}
			if (TryGetValue(out DateTime? output))
			{
				return value.CompareTo(output.Value) == 0;
			}
			return false;
		}
		#endregion

		#region From
		public static JSON From<T>(T input, bool? strict = null)
		{
			if (input == null)
			{
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}

			Type inputType = input.GetType();

			if (inputType == typeof(bool))
			{
				return new JSON((bool)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(bool?))
			{
				bool? value = (bool?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(float))
			{
				return new JSON((float)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(float?))
			{
				float? value = (float?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(double))
			{
				return new JSON((double)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(double?))
			{
				double? value = (double?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(int))
			{
				return new JSON((int)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(int?))
			{
				int? value = (int?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(long))
			{
				return new JSON((long)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(long?))
			{
				long? value = (long?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(string))
			{
				return new JSON((string)(object)input)
				{
					Strict = strict
				};
			}
			if (inputType == typeof(DateTime))
			{
				return new JSON(((DateTime)(object)input))
				{
					Strict = strict
				};
			}
			if (inputType == typeof(DateTime?))
			{
				DateTime? value = (DateTime?)(object)input;

				if (value.HasValue)
				{
					return new JSON(value.Value)
					{
						Strict = strict
					};
				}
				return new JSON(ValueType.Null)
				{
					Strict = strict
				};
			}
			if (inputType == __jsonType)
			{
				return (JSON)(object)input;
			}
			if (inputType.IsArray)
			{
				return GetArray((Array)(object)input, strict);
			}

			JSON output = new JSON(JSON.ValueType.Object)
			{
				Strict = strict
			};

			foreach (FieldInfo field in input.GetType().GetFields())
			{
				output.Add(field.Name, JSON.From((object)field.GetValue(input)));
			}

			foreach (PropertyInfo property in input.GetType().GetProperties())
			{
				if (property.CanRead)
				{
					output.Add(property.Name, JSON.From((object)property.GetValue(input)));
				}
			}

			return output;
		}

		private static JSON GetArray(Array input, bool? strict)
		{
			JSON output = new JSON(ValueType.Array)
			{
				Strict = strict
			};
			Type inputType = input.GetType().GetElementType();

			foreach (object item in input)
			{
				if (item == null)
				{
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(bool))
				{
					output.Push((bool)(object)item);
					continue;
				}
				if (inputType == typeof(bool?))
				{
					bool? value = (bool?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(float))
				{
					output.Push((float)(object)item);
					continue;
				}
				if (inputType == typeof(float?))
				{
					float? value = (float?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(double))
				{
					output.Push((double)(object)item);
					continue;
				}
				if (inputType == typeof(double?))
				{
					double? value = (double?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(int))
				{
					output.Push((int)(object)item);
					continue;
				}
				if (inputType == typeof(int?))
				{
					int? value = (int?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(long))
				{
					output.Push((long)(object)item);
					continue;
				}
				if (inputType == typeof(long?))
				{
					long? value = (long?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(string))
				{
					output.Push((string)(object)item);
					continue;
				}
				if (inputType == typeof(DateTime?))
				{
					DateTime? value = (DateTime?)(object)item;

					if (value.HasValue)
					{
						output.Push(value.Value);
						continue;
					}
					output.Push(JSON.Null);
					continue;
				}
				if (inputType == typeof(DateTime))
				{
					output.Push((DateTime)(object)item);
					continue;
				}
				if (inputType == __jsonType)
				{
					output.Push((JSON)(object)item);
					continue;
				}
				if (inputType.IsArray)
				{
					output.Push(GetArray((Array)(object)item, strict));
					continue;
				}
			}

			return output;
		}
		#endregion

		#region GetEnumerator()
		public IEnumerator<KeyValuePair<string, JSON>> GetEnumerator()
		{
			if (_type != JSON.ValueType.Object)
			{
				throw new Exception("Can iterate names only from an object");
			}

			return ((Dictionary<string, JSON>)_value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public SortedDictionary<int, JSON> GetIndexEnumerator()
		{
			if (_type != JSON.ValueType.Array)
			{
				throw new Exception("Can iterate indices only from an array");
			}

			return (SortedDictionary<int, JSON>)_value;
		}
		#endregion

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region GetItem()
		public JSON GetItem(int index)
		{
			return GetItem(index, null);
		}

		public JSON GetItem(int index, bool? strict = null)
		{
			if (_type != ValueType.Array)
			{
				if (IsStrict(strict))
				{
					throw new Exception("Can access indices only with an Array");
				}
				return new JSON(ValueType.Undefined) { Strict = Strict };
			}
			if (index < 0)
			{
				throw new Exception("Cannot access negative index");
			}

			if (((SortedDictionary<int, JSON>)_value).ContainsKey(index))
			{
				return ((SortedDictionary<int, JSON>)_value)[index];
			}

			return new JSON(ValueType.Undefined) { Strict = Strict };
		}

		public JSON GetItem(string name)
		{
			return GetItem(name, null, null);
		}

		public JSON GetItem(string name, bool? strict = null)
		{
			if (_type != ValueType.Object)
			{
				if (IsStrict(strict))
				{
					throw new Exception("Can access by named elements only with an Object");
				}
				return new JSON(ValueType.Undefined) { Strict = Strict };
			}

			if (((Dictionary<string, JSON>)_value).ContainsKey(name))
			{
				return ((Dictionary<string, JSON>)_value)[name];
			}

			return new JSON(ValueType.Undefined) { Strict = Strict };
		}
		#endregion

		private bool IsStrict(bool? strict = null)
		{
			if (strict.HasValue)
			{
				return strict.Value;
			}
			if (Strict.HasValue)
			{
				return Strict.Value;
			}
			return JSON.Global.Strict;
		}

		#region Parse
		private enum State
		{
			WaitStart,
			WaitName,
			WaitValue,
			Name,
			WaitColon,
			Value,
			StringEscape,
			Number,
			WaitNumber,
			WaitPeriod,
			WaitDecimal,
			Decimal,
			WaitExponentSign,
			WaitExponent,
			Exponent,
			WaitNext,
			End
		};

		public static JSON Parse(string text, bool? strict = null)
		{
			if (text == null || text.Length == 0)
			{
				return new JSON(ValueType.Undefined)
				{
					Strict = strict
				};
			}

			int index = 0;

			text = text.Trim(__whitespace);

			JSON json = ParsePart(text.ToCharArray(), ref index, strict);

			if (index < text.Length)
			{
				throw new Exception("Invalid JSON at " + text.Substring(index));
			}

			return json;
		}

		private static JSON ParsePart(char[] charArray, ref int index, bool? strict = null)
		{
			JSON json = new JSON(ValueType.Undefined)
			{
				Strict = strict
			};
			State state = State.WaitStart;
			StringBuilder builder = null;
			Stack<State> nextState = new Stack<State>();
			string name = null;
			char c;

			while (index < charArray.Length)
			{
				c = charArray[index];

				switch (c)
				{
					case '{':
					case '[':
					{
						switch (state)
						{
							case State.WaitStart:
							{
								if (json.Count == 0)
								{
									if (c == '[')
									{
										if (json.Type != ValueType.Array && json.Type != ValueType.Undefined)
										{
											throw new Exception("Invalid character " + c + " at char " + index);
										}
										json = new JSON(ValueType.Array);
										state = State.WaitValue;
									}
									else
									{
										if (json.Type != ValueType.Object && json.Type != ValueType.Undefined)
										{
											throw new Exception("Invalid character " + c + " at char " + index);
										}
										json = new JSON(ValueType.Object);
										state = State.WaitName;
									}
									break;
								}

								JSON value = ParsePart(charArray, ref index, strict);

								if (json.Type == ValueType.Array)
								{
									json.Push(value);
								}
								else
								{
									json.Add(name, value);
								}
								state = State.WaitNext;
								continue;
							}
							case State.WaitValue:
							{
								JSON value = ParsePart(charArray, ref index, strict);

								if (json.Type == ValueType.Array)
								{
									json.Push(value);
								}
								else
								{
									json.Add(name, value);
								}
								state = State.WaitNext;
								continue;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(c);
								break;
							}
							default:
							{
								throw new Exception("Invalid character " + c + " at char " + index);
							}
						}
						break;
					}
					case '}':
					{
						switch (state)
						{
							case State.WaitName:
							{
								if (json.Count == 0)
								{
									++index;
									return json;
								}
								throw new Exception("Invalid character } at char " + index);
							}
							case State.WaitNext:
							{
								if (json.Type != ValueType.Object)
								{
									throw new Exception("Invalid character } at char " + index);
								}
								++index;
								return json;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (json.Type == ValueType.Array)
								{
									throw new Exception("Invalid character } at char " + index);
								}
								json.Add(name, ParseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								++index;
								return json;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append('}');
								break;
							}
							default:
							{
								throw new Exception("Invalid character } at char " + index);
							}
						}
						break;
					}
					case ']':
					{
						switch (state)
						{
							case State.WaitValue:
							{
								if (json.Count == 0)
								{
									++index;
									return json;
								}
								throw new Exception("Invalid character ] at char " + index);
							}
							case State.WaitNext:
							{
								if (json.Type != ValueType.Array)
								{
									throw new Exception("Invalid character } at char " + index);
								}
								++index;
								return json;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (json.Type == ValueType.Array)
								{
									json.Push(ParseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
									++index;
									return json;
								}
								throw new Exception("Invalid character ] at char " + index);
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(']');
								break;
							}
							default:
							{
								throw new Exception("Invalid character ] at char " + index);
							}
						}
						break;
					}
					case '"':
					{
						switch (state)
						{
							case State.WaitStart:
							{
								builder = new StringBuilder();
								name = null;
								if (json.Type == ValueType.Array)
								{
									state = State.Value;
								}
								else
								{
									state = State.Name;
								}
								break;
							}
							case State.WaitName:
							{
								builder = new StringBuilder();
								name = null;
								state = State.Name;
								break;
							}
							case State.WaitValue:
							{
								builder = new StringBuilder();
								state = State.Value;
								break;
							}
							case State.Name:
							{
								name = builder.ToString();
								state = State.WaitColon;
								break;
							}
							case State.Value:
							{
								if (json.Type == ValueType.Array)
								{
									json.Push(new JSON(builder.ToString()));
								}
								else
								{
									json.Add(name, new JSON(builder.ToString()));
								}
								state = State.WaitNext;
								break;
							}
							case State.StringEscape:
							{
								builder.Append('"');
								state = nextState.Pop();
								break;
							}
							default:
							{
								throw new Exception("Invalid character \" at char " + index);
							}
						}
						break;
					}
					case ':':
					{
						switch (state)
						{
							case State.WaitColon:
							{
								state = State.WaitValue;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(':');
								break;
							}
							default:
							{
								throw new Exception("Invalid character : at char " + index);
							}
						}
						break;
					}
					case ',':
					{
						switch (state)
						{
							case State.WaitNext:
							{
								name = null;
								state = State.WaitStart;
								break;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (json.Type == ValueType.Array)
								{
									json.Push(ParseNumberString(builder.ToString(), state == State.Number));
								}
								else
								{
									json.Add(name, ParseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								}
								state = State.WaitStart;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(',');
								break;
							}
							default:
							{
								throw new Exception("Invalid character , at char " + index);
							}
						}
						break;
					}
					case '\\':
					{
						switch (state)
						{
							case State.Name:
							case State.Value:
							{
								nextState.Push(state);
								state = State.StringEscape;
								break;
							}
							case State.StringEscape:
							{
								builder.Append('\\');
								state = nextState.Pop();
								break;
							}
							default:
							{
								throw new Exception("Invalid character \\ at char " + index);
							}
						}
						break;
					}
					case 'b':
					case 'f':
					case 'n':
					case 'r':
					case 't':
					{
						switch (state)
						{
							case State.WaitStart:
							case State.WaitValue:
							{
								if (state == State.WaitStart && json.Type != ValueType.Array)
								{
									throw new Exception("Invalid character " + c + " at char " + index);
								}
								if (!CheckStringLiteral(charArray, ref index, c))
								{
									throw new Exception("Invalid character " + c + " at char " + index);
								}

								JSON value = null;

								switch (c)
								{
									case 't':
									{
										value = new JSON(true);
										break;
									}
									case 'f':
									{
										value = new JSON(false);
										break;
									}
									case 'n':
									{
										value = new JSON((string)null);
										break;
									}
								}

								if (json.Type == ValueType.Array)
								{
									json.Push(value);
								}
								else
								{
									json.Add(name, value);
								}

								state = State.WaitNext;
								continue;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(c);
								break;
							}
							case State.StringEscape:
							{
								switch (c)
								{
									case 'b':
									{
										builder.Append('\b');
										break;
									}
									case 'f':
									{
										builder.Append('\f');
										break;
									}
									case 'n':
									{
										builder.Append('\n');
										break;
									}
									case 'r':
									{
										builder.Append('\r');
										break;
									}
									case 't':
									{
										builder.Append('\t');
										break;
									}
								}
								state = nextState.Pop();
								break;
							}
							default:
							{
								throw new Exception("Invalid character " + c + " at char " + index);
							}
						}
						break;
					}
					case '/':
					{
						switch (state)
						{
							case State.Name:
							case State.Value:
							{
								builder.Append(c);
								break;
							}
							case State.StringEscape:
							{
								builder.Append('/');
								state = nextState.Pop();
								break;
							}
							default:
							{
								throw new Exception("Invalid character / at char " + index);
							}
						}
						break;
					}
					case 'u':
					{
						switch (state)
						{
							case State.Name:
							case State.Value:
							{
								builder.Append('u');
								break;
							}
							case State.StringEscape:
							{
								StringBuilder hex = new StringBuilder();

								for (int i = 0; i < 4; i++)
								{
									++index;
									if (index == charArray.Length)
									{
										throw new Exception("Unicode character passed the end of data");
									}
									if ("0123456789abcdefABCDEF".IndexOf(charArray[index]) == -1)
									{
										throw new Exception("Invalid unicode literal at char " + index);
									}
									hex.Append(charArray[index]);
								}
								builder.Append((char)int.Parse(hex.ToString(), NumberStyles.HexNumber));
								state = nextState.Pop();
								continue;
							}
							default:
							{
								throw new Exception("Invalid character u at char " + index);
							}
						}
						break;
					}
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					{
						switch (state)
						{
							case State.WaitStart:
							{
								if (json.Type != ValueType.Array)
								{
									throw new Exception("Invalid character " + c + " at char " + index);
								}
								builder = new StringBuilder();
								builder.Append(c);
								if (c == '0')
								{
									state = State.WaitPeriod;
								}
								else
								{
									state = State.Number;
								}
								break;
							}
							case State.WaitValue:
							{
								builder = new StringBuilder();
								builder.Append(c);
								if (c == '0')
								{
									state = State.WaitPeriod;
								}
								else
								{
									state = State.Number;
								}
								break;
							}
							case State.WaitNumber:
							{
								builder.Append(c);
								if (c == '0')
								{
									state = State.WaitPeriod;
								}
								else
								{
									state = State.Number;
								}
								break;
							}
							case State.WaitDecimal:
							{
								builder.Append(c);
								state = State.Decimal;
								break;
							}
							case State.WaitExponentSign:
							case State.WaitExponent:
							{
								builder.Append(c);
								state = State.Exponent;
								break;
							}
							case State.Name:
							case State.Value:
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							{
								builder.Append(c);
								break;
							}
							default:
							{
								throw new Exception("Invalid character " + c + " at char " + index);
							}
						}
						break;
					}
					case '-':
					{
						switch (state)
						{
							case State.WaitStart:
							{
								if (json.Type != ValueType.Array)
								{
									throw new Exception("Invalid character - at char " + index);
								}
								builder = new StringBuilder();
								builder.Append('-');
								state = State.WaitNumber;
								break;
							}
							case State.WaitValue:
							{
								builder = new StringBuilder();
								builder.Append('-');
								state = State.WaitNumber;
								break;
							}
							case State.WaitExponentSign:
							{
								builder.Append('-');
								state = State.WaitExponent;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append('-');
								break;
							}
							default:
							{
								throw new Exception("Invalid character - at char " + index);
							}
						}
						break;
					}
					case '+':
					{
						switch (state)
						{
							case State.WaitExponentSign:
							{
								state = State.WaitExponent;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append('+');
								break;
							}
							default:
							{
								throw new Exception("Invalid character + at char " + index);
							}
						}
						break;
					}
					case '.':
					{
						switch (state)
						{
							case State.WaitPeriod:
							case State.Number:
							{
								builder.Append('.');
								state = State.WaitDecimal;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append('.');
								break;
							}
							default:
							{
								throw new Exception("Invalid character . at char " + index);
							}
						}
						break;
					}
					case 'e':
					case 'E':
					{
						switch (state)
						{
							case State.Number:
							case State.Decimal:
							case State.WaitPeriod:
							{
								builder.Append('e');
								state = State.WaitExponentSign;
								break;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(c);
								break;
							}
							default:
							{
								throw new Exception("Invalid character " + c + " at char " + index);
							}
						}
						break;
					}
					default:
					{
						switch (state)
						{
							case State.WaitStart:
							case State.WaitName:
							case State.WaitColon:
							case State.WaitValue:
							case State.WaitNext:
							{
								if (!__whitespace.Contains(c))
								{
									throw new Exception("Invalid character at char " + index);
								}
								break;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (!__whitespace.Contains(c))
								{
									throw new Exception("Invalid character at char " + index);
								}
								if (json.Type == ValueType.Array)
								{
									json.Push(ParseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								}
								else
								{
									json.Add(name, ParseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								}
								state = State.WaitNext;
								break;
							}
							case State.Name:
							case State.Value:
							{
								if (c != ' ' && __whitespace.Contains(c))
								{
									throw new Exception("Invalid whitespace character at char " + index);
								}
								builder.Append(c);
								break;
							}
							default:
							{
								throw new Exception("Invalid character at char " + index);
							}
						}
						break;
					}
				}

				++index;
			}

			if (state != State.End)
			{
				throw new Exception("Invalid JSON");
			}

			return json;
		}

		private static bool CheckStringLiteral(char[] charArray, ref int index, char key)
		{
			char[] stringLiteral = getStringLiteral(key);

			if (stringLiteral == null)
			{
				return false;
			}

			for (int i = 0; i < stringLiteral.Length; i++)
			{
				if (index == charArray.Length)
				{
					return false;
				}
				if (stringLiteral[i] != charArray[index])
				{
					return false;
				}
				++index;
			}

			return true;
		}

		private static char[] getStringLiteral(char key)
		{
			switch (key)
			{
				case 't':
				{
					return "true".ToCharArray();
				}
				case 'f':
				{
					return "false".ToCharArray();
				}
				case 'n':
				{
					return "null".ToCharArray();
				}
				default:
				{
					return null;
				}
			}
		}

		private static JSON ParseNumberString(string number, bool isInteger)
		{
			if (isInteger)
			{
				if (long.TryParse(number, out long value))
				{
					return new JSON(value);
				}

				return new JSON(ValueType.Null);
			}
			else
			{
				if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
				{
					return new JSON(value);
				}

				return new JSON(ValueType.Null);
			}
		}
		#endregion

		#region Push(value)
		public void Push(string value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(bool value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(int value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(long value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(float value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(double value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(DateTime value, bool? strict = null)
		{
			Push(new JSON(value), strict);
		}

		public void Push(JSON value, bool? strict = null)
		{
			if (_type == ValueType.Undefined && !IsStrict(strict))
			{
				_value = new SortedDictionary<int, JSON>();
				_type = ValueType.Array;
			}
			else if (_type != ValueType.Array)
			{
				throw new Exception("Can push only to an Array");
			}

			++_maxIndex;
			((SortedDictionary<int, JSON>)_value).Add(_maxIndex, value);
		}
		#endregion

		#region Remove()
		public bool Remove(int index)
		{
			if (_value != null && _type == ValueType.Array)
			{
				SortedDictionary<int, JSON> list = (SortedDictionary<int, JSON>)_value;

				if (list.ContainsKey(index))
				{
					list.Remove(index);

					if (index == _maxIndex)
					{
						if (list.Count > 0)
						{
							_maxIndex = list.Last().Key;
						}
						else
						{
							_maxIndex = -1;
						}
					}
					return true;
				}
			}
			return false;
		}

		public bool Remove(string name)
		{
			if (_value != null && _type == ValueType.Object)
			{
				Dictionary<string, JSON> list = (Dictionary<string, JSON>)_value;

				if (list.ContainsKey(name))
				{
					list.Remove(name);
					return true;
				}
			}
			return false;
		}

		public bool Remove(KeyValuePair<int, JSON> item)
		{
			if (_value != null && _type == ValueType.Array)
			{
				SortedDictionary<int, JSON> list = (SortedDictionary<int, JSON>)_value;

				if (list.ContainsKey(item.Key) && list[item.Key] == item.Value)
				{
					list.Remove(item.Key);

					if (item.Key == _maxIndex)
					{
						if (list.Count > 0)
						{
							_maxIndex = list.Last().Key;
						}
						else
						{
							_maxIndex = -1;
						}
					}
					return true;
				}
			}
			return false;
		}

		public bool Remove(KeyValuePair<string, JSON> item)
		{
			if (_value != null && _type == ValueType.Object)
			{
				Dictionary<string, JSON> list = (Dictionary<string, JSON>)_value;

				if (list.ContainsKey(item.Key) && list[item.Key] == item.Value)
				{
					list.Remove(item.Key);
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Stringify()
		public string Stringify()
		{
			switch (_type)
			{
				case ValueType.Undefined:
				{
					return null;
				}
				case ValueType.Null:
				{
					return "null";
				}
				case ValueType.Boolean:
				{
					if ((bool)_value)
					{
						return "true";
					}
					return "false";
				}
				case ValueType.String:
				{
					StringBuilder output = new StringBuilder();

					output.Append('"');
					output.Append(EncodeJSONString((string)_value));
					output.Append('"');

					return output.ToString();
				}
				case ValueType.Integer:
				{
					return ((long)_value).ToString();
				}
				case ValueType.Float:
				{
					return ((double)_value).ToString(CultureInfo.InvariantCulture).Replace("+", "").ToLower();
				}
				case ValueType.Object:
				{
					StringBuilder output = new StringBuilder();
					bool comma = false;

					output.Append('{');

					foreach (KeyValuePair<string, JSON> item in (Dictionary<string, JSON>)_value)
					{
						if (item.Value.Type == ValueType.Undefined)
						{
							continue;
						}
						if (comma)
						{
							output.Append(',');
						}
						output.Append('"');
						output.Append(EncodeJSONString(item.Key));
						output.Append('"');
						output.Append(':');
						output.Append(item.Value.Stringify());

						comma = true;
					}

					output.Append('}');

					return output.ToString();
				}
				case ValueType.Array:
				{
					StringBuilder output = new StringBuilder();
					bool comma = false;
					int index = -1;

					output.Append('[');

					foreach (KeyValuePair<int, JSON> item in (SortedDictionary<int, JSON>)_value)
					{
						if (item.Value.Type == ValueType.Undefined)
						{
							continue;
						}
						for (int i = index + 1; i < item.Key; i++)
						{
							if (comma)
							{
								output.Append(',');
							}
							else
							{
								comma = true;
							}
							output.Append("null");
						}
						if (comma)
						{
							output.Append(',');
						}
						output.Append(item.Value.Stringify());

						comma = true;
						index = item.Key;
					}

					for (int i = index + 1; i < _maxIndex; i++)
					{
						if (comma)
						{
							output.Append(',');
						}
						output.Append("null");
					}

					output.Append(']');

					return output.ToString();
				}
				default:
				{
					throw new Exception("A bug: unhandled value type");
				}
			}
		}

		public static string EncodeJSONString(string input)
		{
			if (input == null || input.Length == 0)
			{
				return "";
			}

			char c;
			int i;
			string hex;
			StringBuilder output = new StringBuilder(input.Length + 4);

			for (i = 0; i < input.Length; i++)
			{
				c = input[i];

				switch (c)
				{
					case '\\':
					case '"':
					case '/':
					{
						output.Append('\\');
						output.Append(c);
						break;
					}
					case '\b':
					{
						output.Append('\\');
						output.Append('b');
						break;
					}
					case '\f':
					{
						output.Append('\\');
						output.Append('f');
						break;
					}
					case '\n':
					{
						output.Append('\\');
						output.Append('n');
						break;
					}
					case '\r':
					{
						output.Append('\\');
						output.Append('r');
						break;
					}
					case '\t':
					{
						output.Append('\\');
						output.Append('t');
						break;
					}
					default:
					{
						if (c < ' ' || c > 255)
						{
							hex = "000" + String.Format("X", c);
							output.Append('\\');
							output.Append('u');
							output.Append(hex.Substring(hex.Length - 4));
						}
						else
						{
							output.Append(c);
						}
						break;
					}
				}
			}

			return output.ToString();
		}
		#endregion

		#region Type conversions
		public bool? ToBool(bool? strict = null)
		{
			if (TryGetValue(out bool? value, strict))
			{
				return value;
			}
			return null;
		}

		public DateTime? ToDateTime()
		{
			if (TryGetValue(out DateTime? value))
			{
				return value;
			}
			return null;
		}

		public double? ToDouble(bool? strict = null)
		{
			if (TryGetValue(out double? value, strict))
			{
				return value;
			}
			return null;
		}

		public float? ToFloat(bool? strict = null)
		{
			if (TryGetValue(out float? value, strict))
			{
				return value;
			}
			return null;
		}

		public int? ToInt(bool? strict = null)
		{
			if (TryGetValue(out int? value, strict))
			{
				return value;
			}
			return null;
		}

		public long? ToLong(bool? strict = null)
		{
			if (TryGetValue(out long? value, strict))
			{
				return value;
			}
			return null;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public string ToString(bool? strict = null)
		{
			if (IsStrict(strict) && _type != ValueType.String)
			{
				return null;
			}

			switch (_type)
			{
				case ValueType.Undefined:
				case ValueType.Null:
				{
					return null;
				}
				case ValueType.Boolean:
				{
					if ((bool)_value)
					{
						return "true";
					}
					return "false";
				}
				case ValueType.String:
				{
					return (string)_value;
				}
				case ValueType.Integer:
				{
					return ((long)_value).ToString();
				}
				case ValueType.Float:
				{
					return ((double)_value).ToString(CultureInfo.InvariantCulture).Replace("+", "").ToLower();
				}
				case ValueType.Object:
				case ValueType.Array:
				{
					return Stringify();
				}
				default:
				{
					throw new Exception("A bug: Unhandled value type");
				}
			}
		}

		public T To<T>(bool? strict = null) where T : new()
		{
			if (TryGetValue(out T value, strict))
			{
				return value;
			}
			return default(T);
		}
		#endregion

		#region TryGetValue()
		public bool TryGetValue(int index, out JSON output)
		{
			return TryGetValue(index, out output, null);
		}

		public bool TryGetValue(int index, out JSON output, bool? strict = null)
		{
			if (_type != ValueType.Array || index < 0 || index > _maxIndex)
			{
				output = null;
				return false;
			}
			if (((SortedDictionary<int, JSON>)_value).ContainsKey(index))
			{
				output = ((SortedDictionary<int, JSON>)_value)[index];
				return true;
			}
			if (IsStrict(strict))
			{
				output = null;
				return false;
			}

			output = new JSON(ValueType.Undefined);
			return true;
		}

		public bool TryGetValue(string name, out JSON output)
		{
			return TryGetValue(name, out output, null);
		}

		public bool TryGetValue(string name, out JSON output, bool? strict = null)
		{
			if (_type != ValueType.Object)
			{
				output = null;
				return false;
			}
			if (((Dictionary<string, JSON>)_value).ContainsKey(name))
			{
				output = ((Dictionary<string, JSON>)_value)[name];
				return true;
			}
			if (IsStrict(strict))
			{
				output = null;
				return false;
			}

			output = new JSON(ValueType.Undefined);
			return true;
		}

		public bool TryGetValue(out string output, bool? strict = null)
		{
			switch (_type)
			{
				case ValueType.Boolean:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if ((bool)_value)
					{
						output = "true";
					}
					else
					{
						output = "false";
					}
					return true;
				}
				case ValueType.String:
				{
					output = (string)_value;
					return true;
				}
				case ValueType.Integer:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = ((long)_value).ToString();
					return true;
				}
				case ValueType.Float:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = ((double)_value).ToString(CultureInfo.InvariantCulture).Replace("+", "").ToLower();
					return true;
				}
				default:
				{
					output = null;
					return false;
				}
			}
		}

		public bool TryGetValue(out int? output, bool? strict = null)
		{
			if (TryGetValue(out long? longValue, strict))
			{
				output = (int)longValue;
				return true;
			}
			output = 0;
			return false;
		}

		public bool TryGetValue(out bool? output, bool? strict = null)
		{
			switch (_type)
			{
				case ValueType.Boolean:
				{
					output = (bool)_value;
					return true;
				}
				case ValueType.String:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if (bool.TryParse((string)_value, out bool value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case ValueType.Integer:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = (long)_value != 0;
					return true;
				}
				case ValueType.Float:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = (double)_value != 0d;
					return true;
				}
				default:
				{
					output = null;
					return false;
				}
			}
		}

		public bool TryGetValue(out long? output, bool? strict = null)
		{
			switch (_type)
			{
				case ValueType.Boolean:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if ((bool)_value)
					{
						output = 1;
					}
					else
					{
						output = 0;
					}
					return true;
				}
				case ValueType.String:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if (long.TryParse((string)_value, out long value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case ValueType.Integer:
				{
					output = (long)_value;
					return true;
				}
				case ValueType.Float:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = (long)(double)_value;
					return true;
				}
				default:
				{
					output = null;
					return false;
				}
			}
		}

		public bool TryGetValue(out float? output, bool? strict = null)
		{
			if (TryGetValue(out double? longValue))
			{
				output = (float)longValue;
				return true;
			}
			output = null;
			return false;
		}

		public bool TryGetValue(out double? output, bool? strict = null)
		{
			switch (_type)
			{
				case ValueType.Boolean:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if ((bool)_value)
					{
						output = 1;
					}
					else
					{
						output = 0;
					}
					return true;
				}
				case ValueType.String:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					if (double.TryParse((string)_value, out double value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case ValueType.Integer:
				{
					if (IsStrict(strict))
					{
						output = null;
						return false;
					}
					output = (double)(long)_value;
					return true;
				}
				case ValueType.Float:
				{
					output = (double)_value;
					return true;
				}
				default:
				{
					output = null;
					return false;
				}
			}
		}

		public bool TryGetValue(out DateTime? output)
		{
			if (_type != ValueType.String)
			{
				output = null;
				return false;
			}

			if (DateTime.TryParseExact(
					(string)_value,
					"yyyy-MM-ddTHH:mm:ss.fff",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out DateTime value
			))
			{
				output = value;
				return true;
			}

			output = null;
			return false;
		}

		public bool TryGetValue<T>(out T output, bool? strict = null) where T : new()
		{
			output = new T();

			TypedReference refOutput = __makeref(output);

			if (_type == ValueType.Undefined || _type == ValueType.Null)
			{
				output = default(T);
				return true;
			}

			if (output.GetType() == __jsonType)
			{
				output = (T)(object)this;
				return true;
			}

			foreach (FieldInfo field in output.GetType().GetFields())
			{
				if (GetValue(field.FieldType, field.Name, out object value, strict))
				{
					field.SetValueDirect(refOutput, value);
				}
			}

			foreach (PropertyInfo property in output.GetType().GetProperties())
			{
				if (property.CanWrite && GetValue(property.PropertyType, property.Name, out object value, strict))
				{
					property.SetValue(output, value);
				}
			}

			return true;
		}

		private bool GetValue(Type type, string name, out object output, bool? strict = null)
		{
			if (!ContainsKey(name))
			{
				output = null;
				return false;
			}

			object[] parameters;

			if (type == typeof(bool) || type == typeof(bool?))
			{
				output = GetItem(name).ToBool(strict);
				return true;
			}
			if (type == typeof(float) || type == typeof(float?))
			{
				output = GetItem(name).ToFloat(strict);
				return true;
			}
			if (type == typeof(double) || type == typeof(double?))
			{
				output = GetItem(name).ToDouble(strict);
				return true;
			}
			if (type == typeof(int) || type == typeof(int?))
			{
				output = GetItem(name).ToInt(strict);
				return true;
			}
			if (type == typeof(long) || type == typeof(long?))
			{
				output = GetItem(name).ToLong(strict);
				return true;
			}
			if (type == typeof(string))
			{
				output = GetItem(name).ToString(strict);
				return true;
			}
			if (type == typeof(DateTime) || type == typeof(DateTime?))
			{
				output = GetItem(name).ToDateTime();
				return true;
			}
			if (type.IsArray)
			{
				JSON value = GetItem(name);

				if (value.Type != ValueType.Array)
				{
					output = null;
					return false;
				}

				Type elementType = type.GetElementType();
				Type nullableType = Nullable.GetUnderlyingType(elementType);
				bool isNullable = (nullableType != null);

				if (isNullable)
				{
					elementType = nullableType;
				}

				if (elementType == typeof(bool))
				{
					if (isNullable)
					{
						output = GetArray<bool?>(value, elementType, strict);
						return true;
					}
					output = GetArray<bool>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(float))
				{
					if (isNullable)
					{
						output = GetArray<float?>(value, elementType, strict);
						return true;
					}
					output = GetArray<float>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(double))
				{
					if (isNullable)
					{
						output = GetArray<double?>(value, elementType, strict);
						return true;
					}
					output = GetArray<double>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(int))
				{
					if (isNullable)
					{
						output = GetArray<int?>(value, elementType, strict);
						return true;
					}
					output = GetArray<int>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(long))
				{
					if (isNullable)
					{
						output = GetArray<long?>(value, elementType, strict);
						return true;
					}
					output = GetArray<long>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(string))
				{
					output = GetArray<string>(value, elementType, strict);
					return true;
				}
				if (elementType == typeof(DateTime))
				{
					if (isNullable)
					{
						output = GetArray<DateTime?>(value, elementType, strict);
						return true;
					}
					output = GetArray<DateTime>(value, elementType, strict);
					return true;
				}

				parameters = new object[] { value, elementType, strict };

				output = __getArrayMethod.MakeGenericMethod(elementType).Invoke(value, parameters);
				return true;
			}

			parameters = new object[] { strict };

			output = __toMethod.MakeGenericMethod(type).Invoke(GetItem(name), parameters);

			if (output != null && output.GetType() == type)
			{
				return true;
			}

			output = null;
			return false;
		}

		private T[] GetArray<T>(JSON input, Type elementType, bool? strict = null)
		{
			List<T> output = new List<T>();
			int index = 0;

			foreach (KeyValuePair<int, JSON> item in input.GetIndexEnumerator())
			{
				for (int i = index; i < item.Key; i++)
				{
					output.Add(default(T));
				}

				if (__tryGetValueMethods.TryGetValue(elementType, out CallMethod callMethod))
				{
					object[] parameters;

					if (callMethod.WithStrict)
					{
						parameters = new object[] { null, strict };
					}
					else
					{
						parameters = new object[] { null };
					}

					if ((bool)callMethod.MethodInfo.Invoke(item.Value, parameters))
					{
						output.Add((T)parameters[0]);
					}
					else
					{
						output.Add(default(T));
					}

					++index;
					continue;
				}

				output.Add(default(T));
				++index;
			}

			return output.ToArray();
		}
		#endregion

		public class GlobalParameters
		{
			public bool Strict = false;
			public bool CaseSensitive = true;
		}
	}
}