# mkJSON
> A C# JSON component

## JSON types
`JSON.ValueType`
* Undefined
* Null
* Object
* Array
* Boolean
* Interer
* Float
* String

## Create
`<json> = new JSON(<json-type>, <case-sensitive>)`
> Creates a JSON object of certain type.
> The JSON type can be _Undefined_, _Null_, _Object_ or _Array_.
> The _Object_ type of JSON can be initialized as case-insensitive with case-sensitive parameter. With other types of JSON the parameter is ignored.

`<json> = new JSON(<data-type>)`
> Creates a JSON object based on certain data type. 
> The data type can be _bool_, _int_, _long_, _float_, _double_, _string_ and _DateTime_, or their nullable version.
> A JSON object based on a generic data type can be created with [JSON.From](#writing-data) method.

## To and from JSON string
`<json> = JSON.Parse(<text>, <strict>)`
> Parses a block of text and returns a JSON object.

`<data> = json.Stringify()`
> Returns the data as a JSON string.

## Strict
> A JSON object can behave strictly, when data types must match exactly: There's no automatic data conversion. Doing unallowed action in strict mode will throw an error.
> Or an object can be behave loosely: Data types are automatically converted, when possible, and traversing object tree is more laxed. Failed operations return _Undefined_ object.
> For instance trying to read a property from an _Undefined_ does not throw an error when in loose mode.

> Strict mode can be set with static [JSON.Global.Strict](#global-properties) parameter, with JSON object's Strict parameter or when calling certain methods. **The default value for the global setting is _false_**. A JSON object does not have strict value set by default, so it uses the global value. If an object has a value, it will override the global value and **will be inherited** to all child objects **when they are added**. Methods' parameter value will override all. Methods' _strict_ parameters are **always optional**.

`JSON.Global.Strict = <bool>`

`<json> = new JSON() { Strict = <bool> }`

`json.Add(<name>, <value>, <strict>)`

## Writing data
`json[<index>] = <data>`

`json.Add(<index>, <data>, <strict>)`

`json.Push(<data>, <strict>)`
> Adds data into an array. If specified index is already occupied, it will be overriden with the new data.
> _Push_ method adds data at the end of the list, the next index after previous last.
> Having _strict_ set to _false_, allows pushing to _Undefined_ type JSON object, which is then converted to an array.

`json[<name>] = <data>`

`json.Add(<name>, <data>, <strict>)`
> Adds data into an object. If the named slot is already occupied, it will be overriden with the new data.
> Having _strict_ set to _false_, allows adding to _Undefined_ type JSON object, which is then converted to an object.

`<data> = JSON.From(<data>, <strict>)`
> Read data from classes and structs into a new JSON object. The strict parameter value will be set as the Strict parameter value for the returned JSON object. Note: static method. 

## Reading data
`<data> = json[<index>]`

`<data> = json[<name>]`

`<data> = json.GetItem(<index>)`

`<data> = json.GetItem(<name>)`

`<bool> = json.TryGetValue(<index>, out <json>)`

`<bool> = json.TryGetValue(<name>, out <json>)`

`<bool> = json.TryGetValue(out <variable>, <strict>)`

`<data> = json.ToBool(<strict>)`

`<data> = json.ToInt(<strict>)`

`<data> = json.ToLong(<strict>)`

`<data> = json.ToFloat(<strict>)`

`<data> = json.ToDouble(<strict>)`

`<data> = json.ToString(<strict>)`

`<data> = json.ToDateTime(<strict>)`

`<data> = json.To<data type>(<strict>)`
> NOTE: All methods return nullable version of the data type.

> NOTE: ToString() returns only selected object's data as a "normal" string. It is **not** formated as JSON string. Both _Undefined_ and _Null_ JSON types are returned as null.

`<data> = json.Pop(<strict>)`
> Returns the last item in an array and removes it from the array. 

`<data> = json.Shift(<strict>)`
> Returns the first item in an array and removes it from the array. NOTE: All the indexes will be shifted by one.

## Enumeration
`foreach (KeyValuePair<string, JSON> item in json)`

`foreach (KeyValuePair<int, JSON> item in json.GetIndexEnumerator())`
> NOTE: Currenly arrays need to be enumerated using _GetIndexEnumerator()_ method.

## Helper functions
> NOTE: These can be applied only to an _Array_ type of JSON

`json.Each(<delegate>)`
> Iterates all the elements in the array and calling: void delegate(JSON item, int index)

`<bool> = json.Every(<delegate>)`
> Iterates all the elements in the array and calling: bool delegate(JSON item, int index)
> If *all* the delegate calls return _true_, the method will return _true_ and otherwise _false_.

`<data> = json.Filter(<delegate>)`
> Iterates all the elements in the array and calling: bool delegate(JSON item, int index)
> Returns a new copy of the JSON array, where all items for which delegate call returns _false_ are dropped.

`<data> = json.Find(<delegate>)`
> Finds an item from the array based on calling: bool delegate(JSON item, int index)

`<int> = json.FindIndex(<delegate>)`
> Finds an item index from the array based on calling: bool delegate(JSON item, int index)

`<bool> = json.some(<delegate>)`
> Iterates the elements in the array and calling: bool delegate(JSON item, int index)
> If *any* of the delegate calls return _true_, the method will return _true_ and otherwise _false_.

`json.Sort(<delegate>)`
> Sorts the array based on: void delegate(JSON a, JSON b)
> Delegate should return _true_ if _a > b_.

## Properties
`json.Count`
> Returns the number of items in an array or an object.

`json.IsNull`
> Is type of the JSON object _Null_.

`json.IsUndefined`
> Is type of the JSON object _Undefined_.

`json.IsArray`
> Is type of the JSON object _Array_.

`json.Type`
> The type of the JSON object.

`json.Keys`
> Return the list of keys in an _Object_ type JSON object.

`json.Value`
> Return the list of values in an _Object_ type JSON object.

`json.Strict`
> Gets and sets the object to strict mode.

`json.IsReadOnly`
> Always false

## Global properties
`JSON.Global.Strict`
> Gets and set global default value for strict mode. The default value is _false_.
`JSON.Global.CaseSensitive`
> Gets and sets global default value for object name case sensitivity. The default value is _true_.

`JSON.Null`
> Returns a new _Null_ type JSON object.

`JSON.Undefined`
> Returns a new `Undefined` type JSON object.

## Other
`json.Clear()`
> Clears an array or an object.

`<bool> = json.Contains(<target>)`
> Checks if a KeyValuePair is in an array or an object. _KeyValuePair<int, JSON>_ for an array and _KeyValuePair<string, JSON>_ for an object.

`<bool> = json.ContainsKey(<target>)`
> Checks if an index of an array or a item of an object has a value. 

`json.CopyTo(<key-value-pair-array>, <index>)`
> Copies the elements starting from the specified index onwards into the specified _KeyEventPair<int, JSON>_ array.

`<bool> = json.Equals(<object>, <strict>)`
> Checks if the JSON object matches with an another object.

`json.GetHashCode()`

`<bool> = json.Remove(<target>)`
> Removes an index from an array or a item from an object.
