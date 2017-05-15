using System;
using Xunit;
using Sharp.Data.Databases.MySql;

namespace Sharp.Tests.Databases.Mysql {
   
    public class MySqlDialectDataTests : DialectDataTests {
    	
    	public MySqlDialectDataTests() {
    		_dialect = new MySqlDialect();
    	}

    	protected override string GetResultFor_Can_create_check_if_table_exists_sql() {
    		return "select count(TABLE_NAME) from INFORMATION_SCHEMA where TABLE_NAME = 'mytable'";
		}

    	protected override string GetResultFor_Can_generate_count_sql() {
    		return "SELECT COUNT(*) FROM myTable";
    	}

    	protected override string GetResultFor_Can_generate_select_sql_with_pagination(int skip, int to) {
            return "select * from mytable limit 10,20";
    	}
    }
}