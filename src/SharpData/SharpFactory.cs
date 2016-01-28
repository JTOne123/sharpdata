﻿using System;
using System.Collections.Generic;
using Sharp.Data.Databases;
using Sharp.Data.Databases.MySql;
using Sharp.Data.Databases.Oracle;
using Sharp.Data.Databases.PostgreSql;
using Sharp.Data.Databases.SqLite;
using Sharp.Data.Databases.SqlServer;

namespace Sharp.Data {
    public class SharpFactory : ISharpFactory {

        public string ConnectionString { get; set; }
        public string DataProviderName { get; set; }

        private Dictionary<string, Type> _dbFactoryTypes = new Dictionary<string, Type>();
        private Dictionary<string, DbFactory> _dbFactories = new Dictionary<string, DbFactory>();

        public SharpFactory() {
            _dbFactoryTypes.Add(DataProviderNames.OracleManaged, typeof(OracleManagedDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.OracleOdp, typeof(OracleOdpDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.MySql, typeof(MySqlDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.OleDb, typeof(OleDbDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.SqLite, typeof(SqLiteDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.SqlServer, typeof(SqlServerDbFactory));
            _dbFactoryTypes.Add(DataProviderNames.PostgreSql, typeof(PostgreDbFactory));
        }

        public SharpFactory(string connectionString, string databaseProviderName) {
            ConnectionString = connectionString;
            DataProviderName = databaseProviderName;
        }

        public IDataProvider CreateDataProvider(string databaseProviderName) {
            return GetConfig().CreateDataProvider();
        }

        public IDataProvider CreateDataProvider() {
            return CreateDataProvider(DataProviderName);
        }

        public IDatabase CreateDatabase(string connectionString, string databaseProviderName) {
            return GetConfig(databaseProviderName, connectionString).CreateDatabase();
        }

        public IDatabase CreateDatabase() {
            return CreateDatabase(ConnectionString, DataProviderName);
        }

        public IDataClient CreateDataClient(string connectionString, string databaseProviderName) {
            return GetConfig(databaseProviderName, connectionString).CreateDataClient();
        }

        public IDataClient CreateDataClient() {
            return CreateDataClient(ConnectionString, DataProviderName);
        }

        public Dialect CreateDialect(string databaseProviderName) {
            return GetConfig().CreateDialect();
        }

        public Dialect CreateDialect() {
            return CreateDialect(DataProviderName);
        }

        private DbFactory GetConfig() {
            return GetConfig(DataProviderName, ConnectionString);
        }

        private DbFactory GetConfig(string databaseProviderName, string connectionString) {
            EnsureProvider(databaseProviderName);
            EnsureProviderInstance(databaseProviderName, connectionString);
            return _dbFactories[databaseProviderName];
        }

        private void EnsureProvider(string databaseProviderName) {
            lock (_sync) {
                if (!_dbFactoryTypes.ContainsKey(databaseProviderName)) {
                    throw new ProviderNotFoundException("Could not find provider " + databaseProviderName);
                }
            }
        }

        private void EnsureProviderInstance(string databaseProviderName, string connectionString) {
            lock (_sync) {
                if (!_dbFactories.ContainsKey(databaseProviderName)) {
                    _dbFactories.Add(databaseProviderName, (DbFactory)Activator.CreateInstance(_dbFactoryTypes[databaseProviderName], databaseProviderName, connectionString));
                    return;
                }
                _dbFactories[databaseProviderName].ConnectionString = connectionString;
            }

        }

        private static ISharpFactory _default;
        public static ISharpFactory Default {
            get {
                lock (_sync) {
                    return _default ?? (_default = new SharpFactory());
                }
            }
            set { _default = value; }
        }

        private static object _sync = new object();
    }
}
