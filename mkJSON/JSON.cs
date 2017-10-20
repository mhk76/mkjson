﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MkJSON
{
	public class JSON : IDictionary<string, JSON>
	{
		public enum JSONValueType
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

		private static string _errorMessage = null;

		private JSONValueType _type = JSONValueType.Undefined;
		private object _value = null;
		private int _maxIndex = -1;

		#region Properties
		public static string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
		}

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
				return _maxIndex + 1;
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
				return _type == JSONValueType.Null;
			}
		}

		public bool IsUndefined
		{
			get
			{
				return _type == JSONValueType.Undefined;
			}
		}

		public bool IsArray
		{
			get
			{
				return _type == JSONValueType.Array;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				if (_type != JSONValueType.Object)
				{
					return null;
				}

				return ((Dictionary<string, JSON>)_value).Keys;
			}
		}

		public JSONValueType Type
		{
			get
			{
				return _type;
			}
		}

		public ICollection<JSON> Values
		{
			get
			{
				if (_type != JSONValueType.Object)
				{
					return null;
				}

				return ((Dictionary<string, JSON>)_value).Values;
			}
		}
		#endregion

		#region Contructors
		public JSON()
		{
		}

		public JSON(JSONValueType type)
		{
			if (type == JSONValueType.Array)
			{
				_value = new SortedDictionary<int, JSON>();
			}
			else if (type == JSONValueType.Object)
			{
				_value = new Dictionary<string, JSON>();
			}
			else if (type != JSONValueType.Undefined)
			{
				throw new Exception("JSON object can be initialized only as an Array or an Object");
			}

			_type = type;
		}

		public JSON(JSON value)
		{
			_type = JSONValueType.Object;
			_value = value;
		}

		public JSON(string value)
		{
			if (value == null)
			{
				_type = JSONValueType.Null;
			}
			else
			{
				_type = JSONValueType.String;
				_value = value;
			}
		}

		public JSON(bool value)
		{
			_type = JSONValueType.Boolean;
			_value = value;
		}

		public JSON(int value)
		{
			_type = JSONValueType.Integer;
			_value = (long)value;
		}

		public JSON(long value)
		{
			_type = JSONValueType.Integer;
			_value = value;
		}

		public JSON(float value)
		{
			_type = JSONValueType.Float;
			_value = double.Parse(value.ToString("r"));
		}

		public JSON(double value)
		{
			_type = JSONValueType.Float;
			_value = value;
		}

		public JSON(DateTime value)
		{
			_type = JSONValueType.String;
			_value = value.ToString("yyyy-MM-ddTHH:mm:ss.fff");
		}
		#endregion

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

		#region Add(index, value)
		public void Add(int index, string value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, bool value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, int value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, long value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, float value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, double value)
		{
			Add(index, new JSON(value));
		}

		public void Add(int index, DateTime value)
		{
			Add(index, new JSON(value));
		}

		public void Add(KeyValuePair<int, JSON> item)
		{
			Add(item.Key, item.Value);
		}

		public void Add(int index, JSON value)
		{
			if (_type == JSONValueType.Undefined)
			{
				_value = new SortedDictionary<int, JSON>();
				_type = JSONValueType.Array;
			}
			else if (_type != JSONValueType.Array)
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
		public void Add(string name, string value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, bool value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, int value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, long value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, float value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, double value)
		{
			Add(name, new JSON(value));
		}

		public void Add(string name, DateTime value)
		{
			Add(name, new JSON(value));
		}

		public void Add(KeyValuePair<string, JSON> item)
		{
			Add(item.Key, item.Value);
		}

		public void Add(string name, JSON value)
		{
			if (_type == JSONValueType.Undefined)
			{
				_value = new Dictionary<string, JSON>();
				_type = JSONValueType.Object;
			}
			else if (_type != JSONValueType.Object)
			{
				throw new Exception("Can add named items only to an Object");
			}

			if (value == null)
			{
				value = new JSON(JSONValueType.Undefined);
			}

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
			if (_type == JSONValueType.Array)
			{
				_value = new SortedDictionary<int, JSON>();
				_maxIndex = -1;
			}
			if (_type == JSONValueType.Object)
			{
				_value = new Dictionary<string, JSON>();
			}
		}

		#region Contains()
		public bool Contains(KeyValuePair<int, JSON> item)
		{
			if (_value != null && _type == JSONValueType.Array)
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
			if (_value != null && _type == JSONValueType.Object)
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
			if (_value != null && _type == JSONValueType.Array)
			{
				return ((SortedDictionary<int, JSON>)_value).ContainsKey(index);
			}
			return false;
		}

		public bool ContainsKey(string name)
		{
			if (_value != null && _type == JSONValueType.Object)
			{
				return ((Dictionary<string, JSON>)_value).ContainsKey(name);
			}
			return false;
		}
		#endregion

		#region CopyTo()
		public void CopyTo(KeyValuePair<int, JSON>[] array, int index)
		{
			if (_type != JSONValueType.Array)
			{
				throw new Exception("Can access indices only with an Array");
			}
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException();
			}

			SortedDictionary<int, JSON> list = (SortedDictionary<int, JSON>)_value;

			if (array.Length - index > list.Count)
			{
				throw new ArgumentException();
			}

			for (int i = 0; i < list.Count; i++)
			{
				array[index] = list.ElementAt(i);
				++index;
			}
		}

		public void CopyTo(KeyValuePair<string, JSON>[] array, int index)
		{
			if (_type != JSONValueType.Array)
			{
				throw new Exception("Can access named elements only with an Object");
			}
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException();
			}

			Dictionary<string, JSON> list = (Dictionary<string, JSON>)_value;

			if (array.Length - index > list.Count)
			{
				throw new ArgumentException();
			}

			for (int i = 0; i < list.Count; i++)
			{
				array[index] = list.ElementAt(i);
				++index;
			}
		}
		#endregion

		#region GetEnumerator()
		public IEnumerator<KeyValuePair<string, JSON>> GetEnumerator()
		{
			if (_type == JSONValueType.Object)
			{
				return ((Dictionary<string, JSON>)_value).GetEnumerator();
			}

			return null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<int, JSON>> IndexEnumerator()
		{
			if (_type == JSONValueType.Array)
			{
				return ((SortedDictionary<int, JSON>)_value).GetEnumerator();
			}

			return null;
		}
		#endregion

		#region GetItem()
		public JSON GetItem(int index)
		{
			if (_type != JSONValueType.Array)
			{
				throw new Exception("Can access indices only with an Array");
			}
			if (index < 0)
			{
				throw new Exception("Cannot access negative index");
			}

			if (((SortedDictionary<int, JSON>)_value).ContainsKey(index))
			{
				return ((SortedDictionary<int, JSON>)_value)[index];
			}

			return new JSON(JSONValueType.Undefined);
		}

		public JSON GetItem(string name)
		{
			if (_type != JSONValueType.Object)
			{
				throw new Exception("Can access by named elements only with an Object");
			}

			if (((Dictionary<string, JSON>)_value).ContainsKey(name))
			{
				return ((Dictionary<string, JSON>)_value)[name];
			}

			return new JSON(JSONValueType.Undefined);
		}
		#endregion

		#region Type conversions
		public bool? ToBool()
		{
			if (TryGetValue(out bool? value))
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

		public double? ToDouble()
		{
			if (TryGetValue(out double? value))
			{
				return value;
			}
			return null;
		}

		public float? ToFloat()
		{
			if (TryGetValue(out float? value))
			{
				return value;
			}
			return null;
		}

		public int? ToInt()
		{
			if (TryGetValue(out int? value))
			{
				return value;
			}
			return null;
		}

		public long? ToLong()
		{
			if (TryGetValue(out long? value))
			{
				return value;
			}
			return null;
		}

		public override string ToString()
		{
			StringBuilder output = new StringBuilder();

			switch (_type)
			{
				case JSONValueType.Undefined:
				case JSONValueType.Null:
				{
					return null;
				}
				case JSONValueType.Boolean:
				{
					if ((bool)_value)
					{
						output.Append("true");
					}
					else
					{
						output.Append("false");
					}
					break;
				}
				case JSONValueType.String:
				{
					output.Append('"');
					output.Append(EncodeJSONString((string)_value));
					output.Append('"');
					break;
				}
				case JSONValueType.Integer:
				{
					output.Append((long)_value);
					break;
				}
				case JSONValueType.Float:
				{
					output.Append(((double)_value).ToString(CultureInfo.InvariantCulture).Replace("+", "").ToLower());
					break;
				}
				case JSONValueType.Object:
				{
					bool comma = false;

					output.Append('{');
					foreach (KeyValuePair<string, JSON> item in (Dictionary<string, JSON>)_value)
					{
						if (item.Value.Type == JSONValueType.Undefined)
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
						output.Append(item.Value.ToString());

						comma = true;
					}
					output.Append('}');
					break;
				}
				case JSONValueType.Array:
				{
					bool comma = false;
					int index = -1;

					output.Append('[');
					foreach (KeyValuePair<int, JSON> item in (SortedDictionary<int, JSON>)_value)
					{
						if (item.Value.Type == JSONValueType.Undefined)
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
						output.Append(item.Value.ToString());

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
					break;
				}
			}

			return output.ToString();
		}
		#endregion

		#region Push(value)
		public void Push(string value)
		{
			Push(new JSON(value));
		}

		public void Push(bool value)
		{
			Push(new JSON(value));
		}

		public void Push(int value)
		{
			Push(new JSON(value));
		}

		public void Push(long value)
		{
			Push(new JSON(value));
		}

		public void Push(float value)
		{
			Push(new JSON(value));
		}

		public void Push(double value)
		{
			Push(new JSON(value));
		}

		public void Push(DateTime value)
		{
			Push(new JSON(value));
		}

		public void Push(JSON value)
		{
			if (_type == JSONValueType.Undefined)
			{
				_value = new SortedDictionary<int, JSON>();
				_type = JSONValueType.Array;
			}
			else if (_type != JSONValueType.Array)
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
			if (_value != null && _type == JSONValueType.Array)
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
			if (_value != null && _type == JSONValueType.Object)
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
			if (_value != null && _type == JSONValueType.Array)
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
			if (_value != null && _type == JSONValueType.Object)
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

		#region TryGetValue()
		public bool TryGetValue(int index, out JSON output)
		{
			if (_type != JSONValueType.Array || index < 0 || index > _maxIndex)
			{
				output = null;
				return false;
			}

			if (((SortedDictionary<int, JSON>)_value).ContainsKey(index))
			{
				output = ((SortedDictionary<int, JSON>)_value)[index];
			}
			else
			{
				output = new JSON(JSONValueType.Undefined);
			}
			return true;
		}

		public bool TryGetValue(string name, out JSON output)
		{
			if (_type != JSONValueType.Object)
			{
				output = null;
				return false;
			}

			if (((Dictionary<string, JSON>)_value).ContainsKey(name))
			{
				output = ((Dictionary<string, JSON>)_value)[name];
				return true;
			}

			output = null;
			return false;
		}

		public bool TryGetValue(out string output)
		{
			switch (_type)
			{
				case JSONValueType.Boolean:
				{
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
				case JSONValueType.String:
				{
					output = (string)_value;
					return true;
				}
				case JSONValueType.Integer:
				{
					output = ((long)_value).ToString();
					return true;
				}
				case JSONValueType.Float:
				{
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

		public bool TryGetValue(out int? output)
		{
			if (TryGetValue(out long? longValue))
			{
				output = (int)longValue;
				return true;
			}
			output = 0;
			return false;
		}

		public bool TryGetValue(out bool? output)
		{
			switch (_type)
			{
				case JSONValueType.Boolean:
				{
					if ((bool)_value)
					{
						output = true;
					}
					else
					{
						output = false;
					}
					return true;
				}
				case JSONValueType.String:
				{
					if (bool.TryParse((string)_value, out bool value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case JSONValueType.Integer:
				{
					output = (long)_value != 0;
					return true;
				}
				case JSONValueType.Float:
				{
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

		public bool TryGetValue(out long? output)
		{
			switch (_type)
			{
				case JSONValueType.Boolean:
				{
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
				case JSONValueType.String:
				{
					if (long.TryParse((string)_value, out long value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case JSONValueType.Integer:
				{
					output = (long)_value;
					return true;
				}
				case JSONValueType.Float:
				{
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

		public bool TryGetValue(out float? output)
		{
			if (TryGetValue(out double? longValue))
			{
				output = (float)longValue;
				return true;
			}
			output = null;
			return false;
		}

		public bool TryGetValue(out double? output)
		{
			switch (_type)
			{
				case JSONValueType.Boolean:
				{
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
				case JSONValueType.String:
				{
					if (double.TryParse((string)_value, out double value))
					{
						output = value;
						return true;
					}
					output = null;
					return false;
				}
				case JSONValueType.Integer:
				{
					output = (double)(long)_value;
					return true;
				}
				case JSONValueType.Float:
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
			if (_type != JSONValueType.String)
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
		#endregion

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

		public static JSON Parse(string text)
		{
			if (text == null || text.Length == 0)
			{
				return null;
			}

			int index = 0;

			text = text.Trim(__whitespace);

			JSON json = ParsePart(text.ToCharArray(), ref index);

			if (json == null)
			{
				return null;
			}
			if (index < text.Length)
			{
				_errorMessage = "Invalid JSON at " + text.Substring(index);
				return null;
			}

			return json;
		}

		private static JSON ParsePart(char[] charArray, ref int index)
		{
			JSON json = new JSON(JSONValueType.Undefined);
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
										if (json.Type != JSONValueType.Array && json.Type != JSONValueType.Undefined)
										{
											_errorMessage = "Invalid character " + c + " at char " + index;
											return null;
										}
										json = new JSON(JSONValueType.Array);
										state = State.WaitValue;
									}
									else
									{
										if (json.Type != JSONValueType.Object && json.Type != JSONValueType.Undefined)
										{
											_errorMessage = "Invalid character " + c + " at char " + index;
											return null;
										}
										json = new JSON(JSONValueType.Object);
										state = State.WaitName;
									}
									break;
								}

								JSON value = ParsePart(charArray, ref index);

								if (value == null)
								{
									return null;
								}
								if (json.Type == JSONValueType.Array)
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
								JSON value = ParsePart(charArray, ref index);

								if (value == null)
								{
									return null;
								}
								if (json.Type == JSONValueType.Array)
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
								_errorMessage = "Invalid character " + c + " at char " + index;
								return null;
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
								_errorMessage = "Invalid character } at char " + index;
								return null;
							}
							case State.WaitNext:
							{
								if (json.Type != JSONValueType.Object)
								{
									_errorMessage = "Invalid character } at char " + index;
									return null;
								}
								++index;
								return json;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (json.Type == JSONValueType.Array)
								{
									_errorMessage = "Invalid character } at char " + index;
									return null;
								}
								json.Add(name, parseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
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
								_errorMessage = "Invalid character } at char " + index;
								return null;
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
								_errorMessage = "Invalid character ] at char " + index;
								return null;
							}
							case State.WaitNext:
							{
								if (json.Type != JSONValueType.Array)
								{
									_errorMessage = "Invalid character } at char " + index;
									return null;
								}
								++index;
								return json;
							}
							case State.Number:
							case State.Decimal:
							case State.Exponent:
							case State.WaitPeriod:
							{
								if (json.Type == JSONValueType.Array)
								{
									json.Push(parseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
									++index;
									return json;
								}
								_errorMessage = "Invalid character ] at char " + index;
								return null;
							}
							case State.Name:
							case State.Value:
							{
								builder.Append(']');
								break;
							}
							default:
							{
								_errorMessage = "Invalid character ] at char " + index;
								return null;
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
								if (json.Type == JSONValueType.Array)
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
								if (json.Type == JSONValueType.Array)
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
								_errorMessage = "Invalid character \" at char " + index;
								return null;
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
								_errorMessage = "Invalid character : at char " + index;
								return null;
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
								if (json.Type == JSONValueType.Array)
								{
									json.Push(parseNumberString(builder.ToString(), state == State.Number));
								}
								else
								{
									json.Add(name, parseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
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
								_errorMessage = "Invalid character , at char " + index;
								return null;
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
								_errorMessage = "Invalid character \\ at char " + index;
								return null;
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
								if (state == State.WaitStart && json.Type != JSONValueType.Array)
								{
									_errorMessage = "Invalid character " + c + " at char " + index;
									return null;
								}
								if (!checkStringLiteral(charArray, ref index, c))
								{
									_errorMessage = "Invalid character " + c + " at char " + index;
									return null;
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

								if (json.Type == JSONValueType.Array)
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
								_errorMessage = "Invalid character " + c + " at char " + index;
								return null;
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
								_errorMessage = "Invalid character / at char " + index;
								return null;
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
										_errorMessage = "Unicode character passed the end of data";
										return null;
									}
									if ("0123456789abcdefABCDEF".IndexOf(charArray[index]) == -1)
									{
										_errorMessage = "Invalid unicode literal at char " + index;
										return null;
									}
									hex.Append(charArray[index]);
								}
								builder.Append((char)int.Parse(hex.ToString(), NumberStyles.HexNumber));
								state = nextState.Pop();
								continue;
							}
							default:
							{
								_errorMessage = "Invalid character u at char " + index;
								return null;
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
								if (json.Type != JSONValueType.Array)
								{
									_errorMessage = "Invalid character " + c + " at char " + index;
									return null;
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
								_errorMessage = "Invalid character " + c + " at char " + index;
								return null;
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
								if (json.Type != JSONValueType.Array)
								{
									_errorMessage = "Invalid character - at char " + index;
									return null;
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
								_errorMessage = "Invalid character - at char " + index;
								return null;
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
								_errorMessage = "Invalid character + at char " + index;
								return null;
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
								_errorMessage = "Invalid character . at char " + index;
								return null;
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
								_errorMessage = "Invalid character " + c + " at char " + index;
								return null;
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
									_errorMessage = "Invalid character at char " + index;
									return null;
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
									_errorMessage = "Invalid character at char " + index;
									return null;
								}
								if (json.Type == JSONValueType.Array)
								{
									json.Push(parseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								}
								else
								{
									json.Add(name, parseNumberString(builder.ToString(), state == State.Number || state == State.WaitPeriod));
								}
								state = State.WaitNext;
								break;
							}
							case State.Name:
							case State.Value:
							{
								if (c != ' ' && __whitespace.Contains(c))
								{
									_errorMessage = "Invalid whitespace character at char " + index;
									return null;
								}
								builder.Append(c);
								break;
							}
							default:
							{
								_errorMessage = "Invalid character at char " + index;
								return null;
							}
						}
						break;
					}
				}

				++index;
			}

			if (state != State.End)
			{
				_errorMessage = "Invalid JSON";
				return null;
			}

			return json;
		}

		private static bool checkStringLiteral(char[] charArray, ref int index, char key)
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

		private static JSON parseNumberString(string number, bool isInteger)
		{
			if (isInteger)
			{
				long value;

				if (long.TryParse(number, out value))
				{
					return new JSON(value);
				}
				else
				{
					return new JSON(JSONValueType.Null);
				}
			}
			else
			{
				double value;

				if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				{
					return new JSON(value);
				}
				else
				{
					return new JSON(JSONValueType.Null);
				}
			}
		}
		#endregion
	}
}