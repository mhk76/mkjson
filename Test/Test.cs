using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mkJSON;

namespace Test
{
	[TestClass]
	public class JSONTest
	{
		[TestMethod]
		public void Parse()
		{
			string[] passFiles = Directory.GetFiles(@"..\..\test", "pass*.json");
			string[] failFiles = Directory.GetFiles(@"..\..\test", "fail*.json");

			foreach (string file in passFiles)
			{
				string json = File.ReadAllText(file);

				try
				{
					JSON.Parse(json);
				}
				catch (Exception e)
				{
					Assert.Fail(file + ": " + e.Message);
				}
			}

			foreach (string file in failFiles)
			{
				string json = File.ReadAllText(file);

				try
				{
					JSON.Parse(json);
					Assert.Fail(file + ": should have failed");
				}
				catch (Exception e)
				{
					e.ToString();
				}
			}
		}

		[TestMethod]
		public void Add_and_Get()
		{
			string testItem = "initializing";

			try
			{
				JSON json = new JSON(JSON.ValueType.Object);
				JSON array = new JSON(JSON.ValueType.Array);
				DateTime date = new DateTime(2004, 2, 29, 13, 15, 59);

				testItem = "adding to an array a long";
				array.Add(1, (long)10000000000);
				testItem = "adding to an array an int";
				array.Add(3, (int)2);
				testItem = "adding to an array a float";
				array.Add(5, (float)3.45);
				testItem = "adding to an array a double";
				array.Add(7, (double)4.56789);
				testItem = "adding to an array a bool";
				array.Add(9, true);
				testItem = "adding to an array a date";
				array.Add(11, date);
				testItem = "adding to an array an string";
				array.Add(13, "Hello");
				testItem = "adding to an array a string json object";
				array.Add(15, new JSON("World"));
				testItem = "adding to an array index an empty json object";
				array[17] = new JSON(JSON.ValueType.Object);

				testItem = "adding to an object a long";
				json.Add("long", (long)10000000000);
				testItem = "adding to an object an int";
				json.Add("int", (int)2);
				testItem = "adding to an object a float";
				json.Add("float", (float)3.456789);
				testItem = "adding to an object a double";
				json.Add("double", (double)4.56);
				testItem = "adding to an object a bool";
				json.Add("bool", true);
				testItem = "adding to an object a date";
				json.Add("date", date);
				testItem = "adding to an object a string";
				json.Add("string", "Hello World");
				testItem = "adding to an object an array json object";
				json.Add("array", array);
				testItem = "adding to an object a string json object";
				json.Add("Hello", new JSON("World"));
				testItem = "adding to an object's named property an empty json object";
				json["object"] = new JSON(JSON.ValueType.Object);


				testItem = "comparing the value of an element 'long' to a long";
				Assert.IsTrue(json.GetItem("long").ToLong() == (long)10000000000);
				testItem = "comparing the value of an element 'long' to a bool";
				Assert.IsTrue(json.GetItem("int").ToBool(false) == true);

				testItem = "comparing the value of an element 'int' to an int";
				Assert.IsTrue(json.GetItem("int").ToInt() == (int)2);
				testItem = "comparing the value of an element 'int' to a long";
				Assert.IsTrue(json.GetItem("int").ToLong(false) == (long)2);
				testItem = "comparing the value of an element 'int' to a bool";
				Assert.IsTrue(json.GetItem("int").ToBool(false) == true);

				testItem = "comparing the value of an element 'float' to a float";
				Assert.IsTrue(json.GetItem("float").ToFloat() == (float)3.456789);
				testItem = "comparing the value of an element 'float' to an int";
				Assert.IsTrue(json.GetItem("float").ToInt(false) == (int)3);
				testItem = "comparing the value of an element 'float' to a long";
				Assert.IsTrue(json.GetItem("float").ToLong(false) == (long)3);
				testItem = "comparing the value of an element 'float' to a double";
				Assert.IsTrue(json.GetItem("float").ToDouble() == (double)3.456789);
				testItem = "comparing the value of an element 'float' to a bool";
				Assert.IsTrue(json.GetItem("float").ToBool(false) == true);

				testItem = "comparing the value of an element 'double' to an int";
				Assert.IsTrue(json.GetItem("double").ToInt(false) == (int)4);
				testItem = "comparing the value of an element 'double' to a long";
				Assert.IsTrue(json.GetItem("double").ToLong(false) == (long)4);
				testItem = "comparing the value of an element 'double' to a double";
				Assert.IsTrue(json.GetItem("double").ToDouble(false) == (double)4.56);
				testItem = "comparing the value of an element 'double' to a bool";
				Assert.IsTrue(json.GetItem("double").ToBool(false) == true);

				testItem = "comparing the value of an element 'bool' to a bool";
				Assert.IsTrue(json.GetItem("bool").ToBool() == true);

				testItem = "comparing the value of an element 'string' to a string";
				Assert.IsTrue(json.GetItem("string").ToString() == "Hello World");

				testItem = "comparing the value of an element 'date' to a date";
				Assert.IsTrue(date.Equals(json.GetItem("date").ToDateTime()));


				testItem = "retrieving an element 'array' by named property";
				JSON arrayJson = json["array"];


				testItem = "comparing Stringify() output";
				Assert.AreEqual(
					json.Stringify(),
					"{\"long\":10000000000,\"int\":2,\"float\":3.456789,\"double\":4.56,\"bool\":true,\"date\":\"2004-02-29T13:15:59.000\""
					+ ",\"string\":\"Hello World\",\"array\":[null,10000000000,null,2,null,3.45,null,4.56789,null,true,null,\"2004-02-29T13:15:59.000\","
					+ "null,\"Hello\",null,\"World\",null,{}],\"Hello\":\"World\",\"object\":{}}"
				);


				testItem = "reading a dictionary with int key type";
				Dictionary<int, string> intDictionary = new Dictionary<int, string>
				{
					{ 1, "A" },
					{ 2, "B" }
				};

				Assert.AreEqual(
					JSON.From(intDictionary).Stringify(),
					"[null,\"A\",\"B\"]"
				);


				testItem = "reading a dictionary with string key type";
				Dictionary<string, long> stringDictionary = new Dictionary<string, long>
				{
					{ "A", 1 },
					{ "B", 2 }
				};

				Assert.AreEqual(
					JSON.From(stringDictionary).Stringify(),
					"{\"A\":1,\"B\":2}"
				);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void Manipulation_And_Properties()
		{
			string testItem = "initializing";

			try
			{
				JSON json = new JSON(JSON.ValueType.Array);

				testItem = "pushing a string into a json array";
				json.Push("Hello World!");
				testItem = "pushing a null into the json array";
				json.Push(JSON.Null);
				testItem = "pushing an undefined into the json array";
				json.Push(JSON.Undefined);


				testItem = "testing property 'Count'";
				Assert.IsTrue(json.Count == 3);

				testItem = "testing property 'IsArray'";
				Assert.IsTrue(json.IsArray == true);
				Assert.IsTrue(json[0].IsArray == false);

				testItem = "testing property 'IsNull'";
				Assert.IsTrue(json[1].IsNull == true);
				Assert.IsTrue(json[0].IsNull == false);

				testItem = "testing property 'IsUndefined'";
				Assert.IsTrue(json[2].IsUndefined == true);
				Assert.IsTrue(json[1].IsUndefined == false);

				testItem = "testing property 'Type'";
				Assert.IsTrue(json.Type == JSON.ValueType.Array);


				testItem = "enumerating the json array items";
				int counter = 0;
				foreach (KeyValuePair<int, JSON> item in json.GetIndexEnumerator())
				{
					++counter;
				}
				Assert.IsTrue(counter == 3);


				testItem = "testing 'CopyTo' method";
				KeyValuePair<int, JSON>[] itemArray = new KeyValuePair<int, JSON>[2];
				json.CopyTo(itemArray, 1);
				Assert.IsTrue(itemArray[0].Value.Equals(json[1]));
				Assert.IsTrue(itemArray[1].Value.Equals(json[2]));


				testItem = "clearing the array";
				json.Clear();
				Assert.IsTrue(json.Count == 0);


				testItem = "initalizing json";
				json = new JSON(JSON.ValueType.Object)
				{
					{
						"first",
						new JSON(JSON.ValueType.Object)
						{
							{ "second", new JSON("third") }
						}
					},
					{ "array", new JSON(JSON.ValueType.Array) }
				};

				testItem = "traversing existing objects (strict)";
				Assert.IsTrue(json["first"]["second"].ToString() == "third");


				json = new JSON("1");

				testItem = "converting string to long (method)";
				Assert.IsTrue(json.ToLong(false) == 1);
				testItem = "converting string to double (method)";
				Assert.IsTrue(json.ToDouble(false) == 1d);


				json = new JSON("true");

				testItem = "converting string to bool (method)";
				Assert.IsTrue(json.ToBool(false) == true);


				json = new JSON() { Strict = false };

				testItem = "traversing non-existing objects (object)";
				Assert.IsTrue(json["notExisting"].Type == JSON.ValueType.Undefined);
				testItem = "traversing non-existing array index (object)";
				Assert.IsTrue(json[10].Type == JSON.ValueType.Undefined);


				json = new JSON();

				JSON.Global.Strict = false;

				testItem = "traversing non-existing objects (global)";
				Assert.IsTrue(json["notExisting"].Type == JSON.ValueType.Undefined);
				testItem = "traversing non-existing array index (global)";
				Assert.IsTrue(json[10].Type == JSON.ValueType.Undefined);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void Generic_Types()
		{
			string testItem = "initializing";

			try
			{
				testItem = "reading a class";
				Class fromClass = new Class(true);
				JSON json = JSON.From(fromClass);

				testItem = "writing a class";
				Class toClass = new Class();
				toClass = json.To<Class>();

				Assert.IsTrue(json.Stringify() == JSON.From(toClass).Stringify());


				testItem = "reading a struct";
				Struct fromStruct = new Struct(true);
				json = JSON.From(fromStruct);

				testItem = "writing a struct";
				Struct toStruct = new Struct();
				toStruct = json.To<Struct>();

				Assert.IsTrue(json.Stringify() == JSON.From(toStruct).Stringify());
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void Error_testing()
		{
			string testItem = "initializing";

			try
			{
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void Helper_Functions()
		{
			string testItem = "initializing";

			try
			{
				JSON json = new JSON
				{
					{ 0, "B" },
					{ 1, "E" },
					{ 2, "A" },
					{ 5, "C" }
				};
				int count = 0;


				testItem = "testing Each()";
				json.Each(delegate (JSON item, int index)
				{
					++count;
				});

				Assert.IsTrue(count == 4);


				testItem = "testing Every()";
				Assert.IsTrue(json.Every(delegate (JSON item, int index)
				{
					return true;
				}));
				Assert.IsFalse(json.Every(delegate (JSON item, int index)
				{
					return item.ToString() == "B";
				}));


				testItem = "testing Filter()";
				Assert.IsTrue(json.Filter(delegate (JSON item, int index)
				{
					return item.ToString() != "E";
				}).Stringify() == "[\"B\",\"A\",\"C\"]");


				testItem = "testing Find()";
				Assert.IsTrue(json.Find(delegate (JSON item, int index)
				{
					return item.ToString() == "C";
				}).Stringify() == "\"C\"");


				testItem = "testing FindIndex()";
				Assert.IsTrue(json.FindIndex(delegate (JSON item, int index)
				{
					return item.ToString() == "A";
				}) == 2);


				testItem = "testing Some()";
				Assert.IsTrue(json.Some(delegate (JSON item, int index)
				{
					return item.ToString() == "E";
				}));
				Assert.IsFalse(json.Some(delegate (JSON item, int index)
				{
					return item.ToString() == "D";
				}));


				testItem = "testing Pop()";
				json.Pop();
				Assert.IsTrue(json.Stringify() == "[\"B\",\"E\",\"A\",null]");


				testItem = "testing Shift()";
				json.Shift();
				Assert.IsTrue(json.Stringify() == "[\"E\",\"A\",null]");


				testItem = "testing Sort()";
				json.Sort(delegate (JSON a, JSON b)
				{
					return a.ToString().CompareTo(b.ToString()) != -1;
				});
				Assert.IsTrue(json.Stringify() == "[\"A\",\"E\",null]");
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void From_DataTables()
		{
			string testItem = "initializing";

			try
			{
				DataSet dataSet = new DataSet();

				dataSet.Tables.Add(new DataTable());
				dataSet.Tables.Add(new DataTable());

				dataSet.Tables[0].Columns.Add("first_table_column_1", typeof(int));
				dataSet.Tables[0].Columns.Add("first-Table-Column-2", typeof(string));
				dataSet.Tables[0].Rows.Add((int)0, "0");
				dataSet.Tables[0].Rows.Add((int)1, "1");
				dataSet.Tables[1].Columns.Add("Second.Table columN 1", typeof(string));
				dataSet.Tables[1].Rows.Add("2");


				JSON.Global.NameFormat = JSON.NameFormat.AsIs;

				testItem = "reading dataset as is";
				Assert.IsTrue(JSON.From(dataSet).Stringify() == "[[{\"first_table_column_1\":0,\"first-Table-Column-2\":\"0\"},{\"first_table_column_1\":1,\"first-Table-Column-2\":\"1\"}],[{\"Second.Table columN 1\":\"2\"}]]");

				testItem = "reading datatable as is";
				Assert.IsTrue(JSON.From(dataSet.Tables[0]).Stringify() == "[{\"first_table_column_1\":0,\"first-Table-Column-2\":\"0\"},{\"first_table_column_1\":1,\"first-Table-Column-2\":\"1\"}]");

				testItem = "reading datarow as is";
				Assert.IsTrue(JSON.From(dataSet.Tables[0].Rows[0]).Stringify() == "{\"first_table_column_1\":0,\"first-Table-Column-2\":\"0\"}");


				JSON.Global.NameFormat = JSON.NameFormat.LowerCamelCase;

				testItem = "reading dataset as lower camelcase";
				Assert.IsTrue(JSON.From(dataSet).Stringify() == "[[{\"firstTableColumn1\":0,\"firstTableColumn2\":\"0\"},{\"firstTableColumn1\":1,\"firstTableColumn2\":\"1\"}],[{\"secondTableColumn1\":\"2\"}]]");

				testItem = "reading datatable as lower camelcase";
				Assert.IsTrue(JSON.From(dataSet.Tables[0]).Stringify() == "[{\"firstTableColumn1\":0,\"firstTableColumn2\":\"0\"},{\"firstTableColumn1\":1,\"firstTableColumn2\":\"1\"}]");

				testItem = "reading datarow as lower camelcase";
				Assert.IsTrue(JSON.From(dataSet.Tables[0].Rows[0]).Stringify() == "{\"firstTableColumn1\":0,\"firstTableColumn2\":\"0\"}");


				JSON.Global.NameFormat = JSON.NameFormat.UpperCamelCase;

				testItem = "reading dataset as upper camelcase";
				Assert.IsTrue(JSON.From(dataSet).Stringify() == "[[{\"FirstTableColumn1\":0,\"FirstTableColumn2\":\"0\"},{\"FirstTableColumn1\":1,\"FirstTableColumn2\":\"1\"}],[{\"SecondTableColumn1\":\"2\"}]]");

				testItem = "reading datatable as upper camelcase";
				Assert.IsTrue(JSON.From(dataSet.Tables[0]).Stringify() == "[{\"FirstTableColumn1\":0,\"FirstTableColumn2\":\"0\"},{\"FirstTableColumn1\":1,\"FirstTableColumn2\":\"1\"}]");

				testItem = "reading datarow as upper camelcase";
				Assert.IsTrue(JSON.From(dataSet.Tables[0].Rows[0]).Stringify() == "{\"FirstTableColumn1\":0,\"FirstTableColumn2\":\"0\"}");
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		struct Struct
		{
			public bool? Bool;
			public float? Float;
			public double? Double;
			public int? Int;
			public long? Long;
			public string String;
			public DateTime? Date;
			public JSON Json;
			public Class[] ClassArray;
		
			public Struct(bool self)
			{
				Bool = false;
				Float = 11.1f;
				Double = 22.2d;
				Int = 33;
				Long = 44;
				String = "Well, hello";
				Date = new DateTime(2000, 1, 1);
				Json = new JSON("JSON object");
				if (self)
				{
					ClassArray = new Class[] { new Class(false) };
				}
				else
				{
					ClassArray = new Class[] { };
				}
			}
		}

		class Class
		{
			public bool Bool { get; set; }
			public float Float;
			public double Double { get; set; }
			public int Int;
			public long Long { get; set; }
			public string String;
			public JSON Json { get; set; }
			public Struct[] StructArray { get; set; }
			public Class Self;

			public Class() { }

			public Class(bool self)
			{
				Bool = true;
				Float = 1.1f;
				Double = 2.2d;
				Int = 3;
				Long = 4;
				String = "Hello, World";
				Json = new JSON("JSON Object");
				if (self)
				{
					StructArray = new Struct[] { new Struct(false) };
					Self = new Class(false);
				}
			}
		}
	}
}
