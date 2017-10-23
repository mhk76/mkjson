using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MkJSON;

namespace Test
{
	[TestClass]
	public class JSONTest
	{
		[TestMethod]
		public void JSON_Parse()
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
					if (JSON.Parse(json) != null)
					{
						Assert.Fail(file);
					}
				}
				catch (Exception e)
				{
					Assert.Fail(file + ": " + e.Message);
				}
			}
		}

		[TestMethod]
		public void JSON_Add_and_Get()
		{
			string testItem = "initializing";

			try
			{
				JSON json = new JSON();
				JSON array = new JSON(JSON.ValueType.Array);
				DateTime date = new DateTime(2004, 2, 29, 13, 15, 59);

				testItem = "adding to an array a long";
				array.Add(1, (long)10000000000);
				testItem = "adding to an array an int";
				array.Add(3, (int)2);
				testItem = "adding to an array a float";
				array.Add(5,(float)3.45);
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
				JSON json = new JSON();

				testItem = "pushing a string into undefined json object";
				json.Push("Hello World!");
				testItem = "pushing a null into the json array";
				json.Push(JSON.Null);
				testItem = "pushing an undefined into the json array";
				json.Push(JSON.Undefined);

				testItem = "checking property 'Count'";
				Assert.IsTrue(json.Count == 3);

				testItem = "checking property 'IsArray'";
				Assert.IsTrue(json.IsArray == true);
				Assert.IsTrue(json[0].IsArray == false);

				testItem = "checking property 'IsNull'";
				Assert.IsTrue(json[1].IsNull == true);
				Assert.IsTrue(json[0].IsNull == false);

				testItem = "checking property 'IsUndefined'";
				Assert.IsTrue(json[2].IsUndefined == true);
				Assert.IsTrue(json[1].IsUndefined == false);

				testItem = "checking property 'Type'";
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

				json.Stringify();

				testItem = "writing a class";
				Class toClass = new Class();
				toClass = json.To<Class>();

				json.Stringify();
				JSON.From(toClass).Stringify();


				testItem = "reading a struct";
				Struct fromStruct = new Struct(true);
				json = JSON.From(fromStruct);

				json.Stringify();

				testItem = "writing a struct";
				Struct toStruct = new Struct();
				toStruct = json.To<Struct>();
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message + " While " + testItem);
			}
		}

		[TestMethod]
		public void Error_Checking()
		{
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
			public bool Bool
			{
				get; set;
			}
			public float Float
			{
				get; set;
			}
			public double Double
			{
				get; set;
			}
			public int Int
			{
				get; set;
			}
			public long Long
			{
				get; set;
			}
			public string String
			{
				get; set;
			}
			public JSON Json
			{
				get; set;
			}
			public long?[] LongArray
			{
				get; set;
			}
			public Struct Struct
			{
				get; set;
			}
			public Class Self
			{
				get; set;
			}

			public Class()
			{
			}

			public Class(bool self)
			{
				Bool = true;
				Float = 1.1f;
				Double = 2.2d;
				Int = 3;
				Long = 4;
				String = "Hello, World";
				Json = new JSON("JSON Object");
				LongArray = new long?[] { 1, null, 3 };
				if (self)
				{
					Struct = new Struct(false);
					Self = new Class(false);
				}
			}
		}
	}
}
