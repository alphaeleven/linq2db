﻿using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace LinqToDB.DataProvider.SqlServer
{
	using Data;

	public class SqlServerFactory : IDataProviderFactory
	{
		#region Init

		static readonly SqlServerDataProvider _sqlServerDataProvider2000 = new SqlServerDataProvider(ProviderName.SqlServer2000, SqlServerVersion.v2000);
		static readonly SqlServerDataProvider _sqlServerDataProvider2005 = new SqlServerDataProvider(ProviderName.SqlServer2005, SqlServerVersion.v2005);
		static readonly SqlServerDataProvider _sqlServerDataProvider2008 = new SqlServerDataProvider(ProviderName.SqlServer2008, SqlServerVersion.v2008);
		static readonly SqlServerDataProvider _sqlServerDataProvider2012 = new SqlServerDataProvider(ProviderName.SqlServer2012, SqlServerVersion.v2012);

		static SqlServerFactory()
		{
			DataConnection.AddDataProvider(ProviderName.SqlServer, _sqlServerDataProvider2008);
			DataConnection.AddDataProvider(_sqlServerDataProvider2012);
			DataConnection.AddDataProvider(_sqlServerDataProvider2008);
			DataConnection.AddDataProvider(_sqlServerDataProvider2005);
			DataConnection.AddDataProvider(_sqlServerDataProvider2000);
		}

		#endregion

		#region IDataProviderFactory Implementation

		IDataProvider IDataProviderFactory.GetDataProvider(NameValueCollection attributes)
		{
			for (var i = 0; i < attributes.Count; i++)
			{
				if (attributes.GetKey(i) == "version")
				{
					switch (attributes.Get(i))
					{
						case "2000" : return _sqlServerDataProvider2000;
						case "2005" : return _sqlServerDataProvider2005;
						case "2012" : return _sqlServerDataProvider2012;
					}
				}
			}

			return _sqlServerDataProvider2008;
		}

		#endregion

		#region Public Members

		public static IDataProvider GetDataProvider(SqlServerVersion version = SqlServerVersion.v2008)
		{
			switch (version)
			{
				case SqlServerVersion.v2000 : return _sqlServerDataProvider2000;
				case SqlServerVersion.v2005 : return _sqlServerDataProvider2005;
				case SqlServerVersion.v2012 : return _sqlServerDataProvider2012;
			}

			return _sqlServerDataProvider2008;
		}

		public static void AddUdtType(Type type, string udtName)
		{
			_sqlServerDataProvider2000.AddUdtType(type, udtName);
			_sqlServerDataProvider2005.AddUdtType(type, udtName);
			_sqlServerDataProvider2008.AddUdtType(type, udtName);
			_sqlServerDataProvider2012.AddUdtType(type, udtName);
		}

		public static void AddUdtType<T>(string udtName, T nullValue, DataType dataType = DataType.Undefined)
		{
			_sqlServerDataProvider2000.AddUdtType(udtName, nullValue, dataType);
			_sqlServerDataProvider2005.AddUdtType(udtName, nullValue, dataType);
			_sqlServerDataProvider2008.AddUdtType(udtName, nullValue, dataType);
			_sqlServerDataProvider2012.AddUdtType(udtName, nullValue, dataType);
		}

		#endregion

		#region AssemblyResolver

		class AssemblyResolver
		{
			public string Path;

			public Assembly Resolver(object sender, ResolveEventArgs args)
			{
				if (args.Name == "Microsoft.SqlServer.Types")
				{
					if (System.IO.File.Exists(Path))
						return Assembly.LoadFile(Path);
					return Assembly.LoadFile(System.IO.Path.Combine(Path, args.Name, ".dll"));
				}

				return null;
			}
		}

		public static void ResolveSqlTypesPath([NotNull] string path)
		{
			if (path == null) throw new ArgumentNullException("path");

			if (path.StartsWith("file:///"))
				path = path.Substring("file:///".Length);

			ResolveEventHandler resolver = new AssemblyResolver { Path = path }.Resolver;

#if FW4

			var l = Expression.Lambda<Action>(Expression.Call(
				Expression.Constant(AppDomain.CurrentDomain),
				typeof(AppDomain).GetEvent("AssemblyResolve").GetAddMethod(),
				Expression.Constant(resolver)));

			l.Compile()();
#else
			AppDomain.CurrentDomain.AssemblyResolve += resolver;
#endif
		}

		#endregion

		#region CreateDataConnection

		public static DataConnection CreateDataConnection(string connectionString, SqlServerVersion version = SqlServerVersion.v2008)
		{
			switch (version)
			{
				case SqlServerVersion.v2000 : return new DataConnection(_sqlServerDataProvider2000, connectionString);
				case SqlServerVersion.v2005 : return new DataConnection(_sqlServerDataProvider2005, connectionString);
				case SqlServerVersion.v2012 : return new DataConnection(_sqlServerDataProvider2012, connectionString);
			}

			return new DataConnection(_sqlServerDataProvider2008, connectionString);
		}

		public static DataConnection CreateDataConnection(IDbConnection connection, SqlServerVersion version = SqlServerVersion.v2008)
		{
			switch (version)
			{
				case SqlServerVersion.v2000 : return new DataConnection(_sqlServerDataProvider2000, connection);
				case SqlServerVersion.v2005 : return new DataConnection(_sqlServerDataProvider2005, connection);
				case SqlServerVersion.v2012 : return new DataConnection(_sqlServerDataProvider2012, connection);
			}

			return new DataConnection(_sqlServerDataProvider2008, connection);
		}

		public static DataConnection CreateDataConnection(IDbTransaction transaction, SqlServerVersion version = SqlServerVersion.v2008)
		{
			switch (version)
			{
				case SqlServerVersion.v2000 : return new DataConnection(_sqlServerDataProvider2000, transaction);
				case SqlServerVersion.v2005 : return new DataConnection(_sqlServerDataProvider2005, transaction);
				case SqlServerVersion.v2012 : return new DataConnection(_sqlServerDataProvider2012, transaction);
			}

			return new DataConnection(_sqlServerDataProvider2008, transaction);
		}

		#endregion
	}
}
