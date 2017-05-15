using System;
using Sharp.Data.Databases;
using Sharp.Data.Databases.SqlServer;
using Sharp.Data.Providers;
using Sharp.Tests.Databases.Data;

namespace Sharp.Tests.Databases.SqlServer {
    public class SqlServerDataClientFactoryTests : DataClientFactoryTests {
        public override string GetDatabaseType() {
            return DataProviderNames.SqlServer;
        }

        public override Type GetDataProviderType() {
            return typeof (SqlProvider);
        }

        public override Type GetDataClientType() {
            return typeof (SqlServerDataClient);
        }

        public override Type GetDialectType() {
            return typeof (SqlDialect);
        }
    }
}