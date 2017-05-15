﻿using Xunit;
using Sharp.Data.Databases;
using Sharp.Tests.Databases.Data;

namespace Sharp.Tests.Databases.PostgreSql {
    public class PostgreSqlDataTests : DataClientDataTests {
        public PostgreSqlDataTests() {
            _dataClient = DBBuilder.GetDataClient(DataProviderNames.PostgreSql);
        }
    }
}
