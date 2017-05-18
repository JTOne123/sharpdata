﻿using System;

namespace SharpData.Databases.SqlServer {
	public class SqlServerDataClient : DataClient {
        public SqlServerDataClient(IDatabase database, Dialect dialect)
            : base(database, dialect) {
	    }

	    public override void RemoveColumn(string tableName, string columnName) {
			string[] sqls = Dialect.GetDropColumnSql(tableName, columnName);
			object defaultConstraintName = Database.QueryScalar(sqls[0]);
			if (defaultConstraintName != null) {
				Database.ExecuteSql(String.Format(sqls[1], defaultConstraintName));
			}
			Database.ExecuteSql(sqls[2]);
		}
	}
}