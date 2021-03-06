﻿using System;
using System.Data;
using System.Xml;
using System.Xml.Linq;

namespace LinqToDB.DataProvider.Sybase
{
	using Mapping;
	using SqlProvider;

	public class SybaseDataProvider : DynamicDataProviderBase
	{
		#region Init

		public SybaseDataProvider()
			: this(ProviderName.Sybase, new SybaseMappingSchema())
		{
		}

		protected SybaseDataProvider(string name, MappingSchema mappingSchema)
			: base(name, mappingSchema)
		{
			SqlProviderFlags.AcceptsTakeAsParameter   = false;
			SqlProviderFlags.IsSkipSupported          = false;
			SqlProviderFlags.IsSubQueryTakeSupported  = false;
			SqlProviderFlags.IsCountSubQuerySupported = false;
			SqlProviderFlags.CanCombineParameters     = false;

			SetCharField("char",  (r,i) => r.GetString(i).TrimEnd());
			SetCharField("nchar", (r,i) => r.GetString(i).TrimEnd());

			SetProviderField<IDataReader,TimeSpan,DateTime>((r,i) => r.GetDateTime(i) - new DateTime(1900, 1, 1));
			SetProviderField<IDataReader,DateTime,DateTime>((r,i) => GetDateTime(r, i));

			SetTypes(
				"Sybase.Data.AseClient.AseConnection, " + SybaseFactory.AssemblyName,
				"Sybase.Data.AseClient.AseDataReader, " + SybaseFactory.AssemblyName);
		}

		static DateTime GetDateTime(IDataReader dr, int idx)
		{
			var value = dr.GetDateTime(idx);

			if (value.Year == 1900 && value.Month == 1 && value.Day == 1)
				return new DateTime(1, 1, 1, value.Hour, value.Minute, value.Second, value.Millisecond);

			return value;
		}

		protected override void OnConnectionTypeCreated()
		{
			_setUInt16        = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "UnsignedSmallInt");
			_setUInt32        = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "UnsignedInt");
			_setUInt64        = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "UnsignedBigInt");
			_setText          = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Text");
			_setNText         = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Unitext");
			_setBinary        = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Binary");
			_setVarBinary     = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "VarBinary");
			_setImage         = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Image");
			_setMoney         = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Money");
			_setSmallMoney    = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "SmallMoney");
			_setDate          = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Date");
			_setTime          = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "Time");
			_setSmallDateTime = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "SmallDateTime");
			_setTimestamp     = GetSetParameter("AseParameter", "AseDbType", "AseDbType", "TimeStamp");
		}

		static Action<IDbDataParameter> _setUInt16;//: ((AseParameter)parameter).AseDbType = AseDbType.UnsignedSmallInt; break;
		static Action<IDbDataParameter> _setUInt32        ;//: ((AseParameter)parameter).AseDbType = AseDbType.UnsignedInt;      break;
		static Action<IDbDataParameter> _setUInt64        ;//: ((AseParameter)parameter).AseDbType = AseDbType.UnsignedBigInt;   break;
		static Action<IDbDataParameter> _setText          ;//: ((AseParameter)parameter).AseDbType = AseDbType.Text;             break;
		static Action<IDbDataParameter> _setNText         ;//: ((AseParameter)parameter).AseDbType = AseDbType.Unitext;          break;
		static Action<IDbDataParameter> _setBinary        ;//: ((AseParameter)parameter).AseDbType = AseDbType.Binary;           break;
		static Action<IDbDataParameter> _setVarBinary     ;//: ((AseParameter)parameter).AseDbType = AseDbType.VarBinary;        break;
		static Action<IDbDataParameter> _setImage         ;//: ((AseParameter)parameter).AseDbType = AseDbType.Image;            break;
		static Action<IDbDataParameter> _setMoney         ;//: ((AseParameter)parameter).AseDbType = AseDbType.Money;            break;
		static Action<IDbDataParameter> _setSmallMoney    ;//: ((AseParameter)parameter).AseDbType = AseDbType.SmallMoney;       break;
		static Action<IDbDataParameter> _setDate          ;//: ((AseParameter)parameter).AseDbType = AseDbType.Date;             break;
		static Action<IDbDataParameter> _setTime          ;//: ((AseParameter)parameter).AseDbType = AseDbType.Time;             break;
		static Action<IDbDataParameter> _setSmallDateTime ;//: ((AseParameter)parameter).AseDbType = AseDbType.SmallDateTime;    break;
		static Action<IDbDataParameter> _setTimestamp     ;//: ((AseParameter)parameter).AseDbType = AseDbType.TimeStamp;        break;

		#endregion

		#region Overrides

		public override ISqlProvider CreateSqlProvider()
		{
			return new SybaseSqlProvider(SqlProviderFlags);
		}

		public override void SetParameter(IDbDataParameter parameter, string name, DataType dataType, object value)
		{
			switch (dataType)
			{
				case DataType.SByte      : 
					dataType = DataType.Int16;
					if (value is sbyte)
						value = (short)(sbyte)value;
					break;

				case DataType.Time       :
					if (value is TimeSpan) value = new DateTime(1900, 1, 1) + (TimeSpan)value;
					break;

				case DataType.Xml        :
					dataType = DataType.NVarChar;
					     if (value is XDocument)   value = value.ToString();
					else if (value is XmlDocument) value = ((XmlDocument)value).InnerXml;
					break;

				case DataType.Guid       :
					if (value != null)
						value = value.ToString();
					dataType = DataType.Char;
					parameter.Size = 36;
					break;

				case DataType.Undefined  :
					if (value == null)
						dataType = DataType.Char;
					break;
			}

			base.SetParameter(parameter, "@" + name, dataType, value);
		}

		protected override void SetParameterType(IDbDataParameter parameter, DataType dataType)
		{
			switch (dataType)
			{
				case DataType.VarNumeric    : parameter.DbType = DbType.Decimal;          break;
				case DataType.UInt16        : _setUInt16       (parameter);               break;
				case DataType.UInt32        : _setUInt32       (parameter);               break;
				case DataType.UInt64        : _setUInt64       (parameter);               break;
				case DataType.Text          : _setText         (parameter);               break;
				case DataType.NText         : _setNText        (parameter);               break;
				case DataType.Binary        : _setBinary       (parameter);               break;
				case DataType.VarBinary     : _setVarBinary    (parameter);               break;
				case DataType.Image         : _setImage        (parameter);               break;
				case DataType.Money         : _setMoney        (parameter);               break;
				case DataType.SmallMoney    : _setSmallMoney   (parameter);               break;
				case DataType.Date          : _setDate         (parameter);               break;
				case DataType.Time          : _setTime         (parameter);               break;
				case DataType.SmallDateTime : _setSmallDateTime(parameter);               break;
				case DataType.Timestamp     : _setTimestamp    (parameter);               break;
				default                     : base.SetParameterType(parameter, dataType); break;
			}
		}

		#endregion
	}
}
