namespace Chromium.Embedded
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using Xilium.CefGlue;

	internal sealed class JSValue
	{
		public readonly Object Value;

		public JSValue( Object value )
		{
			if( value == null ) return;

			this.Value = value;
		}

		public JSValue( CefV8Value value )
		{
			if( value == null ) return;

			if( value.IsArray )
			{
				var result = new List<Object>();
				for( var i=0; i < value.GetArrayLength(); i++ )
				{
					result.Add( new JSValue( value.GetValue( i ) ).Value );
				}
				this.Value = result.ToArray();
			}
			else if( value.IsObject )
			{
				var result = new Dictionary<String, Object>();
				var keys = value.GetKeys();
				for( var i=0; i < keys.Length; i++ )
				{
					try
					{
						result.Add( keys[i], new JSValue( value.GetValue( keys[i] ) ).Value );
					}
					catch( ArgumentException )
					{
					}
				}
				this.Value = result;
			}
			else if( value.IsInt )
			{
				this.Value = value.GetIntValue();
			}
			else if( value.IsUInt )
			{
				this.Value = value.GetUIntValue();
			}
			else if( value.IsDouble )
			{
				this.Value = value.GetDoubleValue();
			}
			else if( value.IsBool )
			{
				this.Value = value.GetBoolValue();
			}
			else if( value.IsDate )
			{
				this.Value = value.GetDateValue();
			}
			else if( value.IsString )
			{
				this.Value = value.GetStringValue();
			}
			else if( value.IsNull )
			{
				this.Value = null;
			}
			else if( value.IsUndefined )
			{
				this.Value = null;
			}
		}

		public JSValue( CefBinaryValue value )
		{
			if( value == null ) return;

			this.Value = value.ToArray();
		}

		public JSValue( CefDictionaryValue value )
		{
			if( value == null ) return;

			var result = new Dictionary<String,Object>();
			var keys = value.GetKeys();

			for( var i=0; i < value.Count; i++ )
			{
				try
				{
					switch( value.GetKeyType( keys[i] ) )
					{
						case CefValueType.List:
							result.Add( keys[i], new JSValue( value.GetList( keys[i] ) ).Value );
							break;
						case CefValueType.Binary:
							result.Add( keys[i], new JSValue( value.GetBinary( keys[i] ) ).Value );
							break;
						case CefValueType.String:
							result.Add( keys[i], value.GetString( keys[i] ) );
							break;
						case CefValueType.Double:
							result.Add( keys[i], value.GetDouble( keys[i] ) );
							break;
						case CefValueType.Int:
							result.Add( keys[i], value.GetInt( keys[i] ) );
							break;
						case CefValueType.Bool:
							result.Add( keys[i], value.GetBool( keys[i] ) );
							break;
						case CefValueType.Dictionary:
							result.Add( keys[i], new JSValue( value.GetDictionary( keys[i] ) ).Value );
							break;
						default:
							result.Add( keys[i], null );
							break;
					}
				}
				catch( ArgumentException )
				{
				}
			}

			this.Value = result;
		}

		public JSValue( CefListValue value )
		{
			if( value == null ) return;

			var result = new List<Object>();

			for( var i=0; i < value.Count; i++ )
			{
				switch( value.GetValueType( i ) )
				{
					case CefValueType.List:
						result.Add( new JSValue( value.GetList( i ) ).Value );
						break;
					case CefValueType.Binary:
						result.Add( new JSValue( value.GetBinary( i ) ).Value );
						break;
					case CefValueType.String:
						result.Add( value.GetString( i ) );
						break;
					case CefValueType.Double:
						result.Add( value.GetDouble( i ) );
						break;
					case CefValueType.Int:
						result.Add( value.GetInt( i ) );
						break;
					case CefValueType.Bool:
						result.Add( value.GetBool( i ) );
						break;
					case CefValueType.Dictionary:
						result.Add( new JSValue( value.GetDictionary( i ) ).Value );
						break;
					default:
						result.Add( null );
						break;
				}
			}

			this.Value = result.ToArray();
		}

		public CefV8Value AsV8Value()
		{
			CefV8Value result = null;

			if( this.Value is IList )
			{
				var v = (IList)this.Value;
				
				result = CefV8Value.CreateArray( v.Count );

				for( var i=0; i < v.Count; i++ )
				{
					result.SetValue( i, new JSValue( v[i] ).AsV8Value() );
				}
			}
			else if( this.Value is Boolean )
			{
				result = CefV8Value.CreateBool( (Boolean)this.Value );
			}
			else if( this.Value is DateTime )
			{
				result = CefV8Value.CreateDate( (DateTime)this.Value );
			}
			else if( this.Value is Single || this.Value is Double || this.Value is Decimal || this.Value is UInt64 || this.Value is Int64 )
			{
				result = CefV8Value.CreateDouble( (Double)this.Value );
			}
			else if( this.Value is CefV8Handler )
			{
				result = CefV8Value.CreateFunction( null, (CefV8Handler)this.Value );
			}
			else if( this.Value is SByte || this.Value is Int16 || this.Value is Int32 )
			{
				result = CefV8Value.CreateInt( (Int32)this.Value );
			}
			else if( this.Value == null )
			{
				result = CefV8Value.CreateNull();
			}
			else if( this.Value is IDictionary )
			{
				var v = (IDictionary)this.Value;

				Debug.Assert( v.Keys.Count == v.Values.Count );

				var vKeys = new String[v.Keys.Count];
				var vValues = new CefV8Value[v.Values.Count];

				result = CefV8Value.CreateObject( null );

				for( var i=0; i < vKeys.Length; i++ )
				{
					result.SetValue( vKeys[i], new JSValue( vValues[i] ).AsV8Value(), CefV8PropertyAttribute.None );
				}
			}
			else if( this.Value is String )
			{
				result = CefV8Value.CreateString( (String)this.Value );
			}
			else if( this.Value is Byte || this.Value is UInt16 || this.Value is UInt32 )
			{
				result = CefV8Value.CreateUInt( (UInt32)this.Value );
			}

			if( result == null ) result = CefV8Value.CreateUndefined();

			return result;
		}

		public CefBinaryValue AsCefBinaryValue()
		{
			if( this.Value is Byte[] )
			{
				return CefBinaryValue.Create( (Byte[])this.Value );
			}

			return CefBinaryValue.Create( new Byte[0] );
		}

		public CefDictionaryValue AsCefDictionaryValue()
		{
			var result = CefDictionaryValue.Create();

			if( this.Value is IDictionary )
			{
				var v = (IDictionary)this.Value;

				Debug.Assert( v.Keys.Count == v.Values.Count );

				var vKeys = new String[v.Keys.Count];
				var vValues = new Object[v.Values.Count];

				v.Keys.CopyTo( vKeys, 0 );
				v.Values.CopyTo( vValues, 0 );

				for( var i=0; i < vKeys.Length; i++ )
				{
					if( vValues[i] is Byte[] )
					{
						result.SetBinary( vKeys[i], new JSValue( vValues[i] ).AsCefBinaryValue() );
					}
					else if( vValues[i] is Boolean )
					{
						result.SetBool( vKeys[i], (Boolean)vValues[i] );
					}
					else if( vValues[i] is IDictionary )
					{
						result.SetDictionary( vKeys[i], new JSValue( vValues[i] ).AsCefDictionaryValue() );
					}
					else if( vValues[i] is Single || vValues[i] is Double || vValues[i] is Decimal || vValues[i] is UInt32 || vValues[i] is UInt64 || vValues[i] is Int64 )
					{
						result.SetDouble( vKeys[i], (Double)vValues[i] );
					}
					else if( vValues[i] is Byte || vValues[i] is SByte || vValues[i] is UInt16 || vValues[i] is Int16 || vValues[i] is Int32 )
					{
						result.SetInt( vKeys[i], (Int32)vValues[i] );
					}
					else if( vValues[i] is IList )
					{
						result.SetList( vKeys[i], new JSValue( vValues[i] ).AsCefListValue() );
					}
					else if( vValues[i] is String )
					{
						result.SetString( vKeys[i], (String)vValues[i] );
					}
					else
					{
						result.SetNull( vKeys[i] );
					}
				}
			}

			return result;
		}

		public CefListValue AsCefListValue()
		{
			var result = CefListValue.Create();

			if( this.Value is IList )
			{
				var v = (IList)this.Value;

				var vList = new Object[v.Count];

				v.CopyTo( vList, 0 );

				for( var i=0; i < vList.Length; i++ )
				{
					if( vList[i] is Byte[] )
					{
						result.SetBinary( i, new JSValue( vList[i] ).AsCefBinaryValue() );
					}
					else if( vList[i] is Boolean )
					{
						result.SetBool( i, (Boolean)vList[i] );
					}
					else if( vList[i] is IDictionary )
					{
						result.SetDictionary( i, new JSValue( vList[i] ).AsCefDictionaryValue() );
					}
					else if( vList[i] is Single || vList[i] is Double || vList[i] is Decimal || vList[i] is UInt32 || vList[i] is UInt64 || vList[i] is Int64 )
					{
						result.SetDouble( i, (Double)vList[i] );
					}
					else if( vList[i] is Byte || vList[i] is SByte || vList[i] is UInt16 || vList[i] is Int16 || vList[i] is Int32 )
					{
						result.SetInt( i, (Int32)vList[i] );
					}
					else if( vList[i] is IList )
					{
						result.SetList( i, new JSValue( vList[i] ).AsCefListValue() );
					}
					else if( vList[i] is String )
					{
						result.SetString( i, (String)vList[i] );
					}
					else
					{
						result.SetNull( i );
					}
				}
			}

			return result;
		}
	}
}
