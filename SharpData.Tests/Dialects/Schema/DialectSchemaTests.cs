using System;
using Xunit;
using Sharp.Data.Schema;
using Sharp.Tests.Data.Databases;

namespace Sharp.Tests.Databases {
    public abstract class DialectSchemaTests : DialectTests {

        protected abstract string[] GetResultFor_Can_create_table_sql();
        protected abstract string[] GetResultFor_Can_drop_table();
        protected abstract string GetResultFor_Can_convert_column_to_sql__with_not_null();
        protected abstract string GetResultFor_Can_convert_column_to_sql__with_primary_key();
        protected abstract string GetResultFor_Can_convert_column_to_sql__autoIncrement();
        protected abstract string GetResultFor_Can_convert_column_to_sql__autoIncrement_and_primary_key();
        protected abstract string GetResultFor_Can_convert_column_to_sql__default_value();
        protected abstract string[] GetResultFor_Can_convert_column_to_values();
        protected abstract string GetResultFor_Can_add_comment_to_column();

        protected virtual string GetResutFor_Can_drop_index_sql() {
            return "drop index indexName on foo";
        }

        [Fact]
        public void Can_create_table_sql() {
            Table table = new Table("myTable");
            table.Columns.Add(Column.AutoIncrement("id").AsPrimaryKey().Object);
            table.Columns.Add(Column.String("name").NotNull().Object);

            string[] sqls = _dialect.GetCreateTableSqls(table);
            AssertSql.AreEqual(GetResultFor_Can_create_table_sql(), sqls);
        }

        [Fact]
        public void Can_drop_table() {
            string[] sqls = _dialect.GetDropTableSqls("myTable");
            AssertSql.AreEqual(GetResultFor_Can_drop_table(), sqls);
        }

        [Fact]
        public void Can_convert_column_to_sql__with_not_null() {
            Column column = Column.String("col")
                .Size(255)
                .NotNull().Object;

            string sql = _dialect.GetColumnToSqlWhenCreate(column);

            AssertSql.AreEqual(GetResultFor_Can_convert_column_to_sql__with_not_null(), sql);
        }

        [Fact]
        public void Can_convert_column_to_sql__with_primary_key() {
            Column column = Column.String("col")
                .Size(255)
                .AsPrimaryKey()
                .Object;

            string sql = _dialect.GetColumnToSqlWhenCreate(column);

            AssertSql.AreEqual(GetResultFor_Can_convert_column_to_sql__with_primary_key(), sql);
        }

        [Fact]
        public virtual void Can_convert_column_to_sql__autoIncrement() {
            Column column = Column.AutoIncrement("col").Object;

            string sql = _dialect.GetColumnToSqlWhenCreate(column);

            AssertSql.AreEqual(GetResultFor_Can_convert_column_to_sql__autoIncrement(), sql);
        }

        [Fact]
        public virtual void Can_convert_column_to_sql__default_value() {
            Column column = Column.String("col")
                                  .DefaultValue("some string").Object;

            string sql = _dialect.GetColumnToSqlWhenCreate(column);

            AssertSql.AreEqual(GetResultFor_Can_convert_column_to_sql__default_value(), sql.ToUpper());
        }

        [Fact]
        public virtual void Can_convert_column_to_sql__autoIncrement_and_primary_key() {
            Column column = Column.AutoIncrement("col").AsPrimaryKey().Object;

            string sql = _dialect.GetColumnToSqlWhenCreate(column);

            AssertSql.AreEqual(GetResultFor_Can_convert_column_to_sql__autoIncrement_and_primary_key(), sql);
        }

        [Fact]
        public virtual void Can_convert_column_to_values() {
            Assert.Equal(GetResultFor_Can_convert_column_to_values()[0], _dialect.GetColumnValueToSql("foo"));
            Assert.Equal(GetResultFor_Can_convert_column_to_values()[1], _dialect.GetColumnValueToSql(1));
            Assert.Equal(GetResultFor_Can_convert_column_to_values()[2], _dialect.GetColumnValueToSql(true));
            Assert.Equal(GetResultFor_Can_convert_column_to_values()[3], _dialect.GetColumnValueToSql(24.33));
            Assert.Equal(GetResultFor_Can_convert_column_to_values()[4],
                            _dialect.GetColumnValueToSql(new DateTime(2009, 1, 20, 12, 30, 0, 567)));
        }

        [Fact]
        public void Can_create_index_sql() {
            string sql = _dialect.GetCreateIndexSql("index", TABLE_NAME, "col1");
            AssertSql.AreEqual("create index index on myTable (col1)", sql);
        }

        [Fact]
        public void Can_create_index_with_multiple_columns_sql() {
            string sql = _dialect.GetCreateIndexSql("indexName", TABLE_NAME, "col1", "col2", "col3");
            AssertSql.AreEqual("create index indexName on myTable (col1, col2, col3)", sql);
        }

        [Fact]
        public void Can_drop_index_sql() {
            string sql = _dialect.GetDropIndexSql("indexName", "foo");
            AssertSql.AreEqual(GetResutFor_Can_drop_index_sql(), sql);
        }

        [Fact]
        public virtual void Can_add_comment_to_column() {
            var sql = _dialect.GetAddCommentToColumnSql(TABLE_NAME, "col1", "this is a comment");
            AssertSql.AreEqual(GetResultFor_Can_add_comment_to_column(), sql);
        }
    }
}