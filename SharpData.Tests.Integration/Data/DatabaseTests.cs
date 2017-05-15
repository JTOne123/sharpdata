﻿using System;
using Xunit;
using Sharp.Data;
using Sharp.Data.Schema;
using SharpData.Tests.Integration;

namespace Sharp.Tests.Databases.Data {

    public abstract class DatabaseTests : IDisposable {
        protected IDataClient _dataClient;
        protected IDatabase _database;
        protected string TableFoo = "foo";

        protected virtual string GetParameterPrefix() {
            return ":";
        }

        [Fact]
        public void Can_query_with_string_filter() {
            _dataClient.AddTable(TableFoo,
                Column.String("colString"),
                Column.Int32("colInt"));

            _dataClient.Insert.Into(TableFoo).Columns("colString", "colInt").Values("foo", 1);

            ResultSet resultSet = _database.Query("select colString from foo where colString = :name",
                In.Named("name", "foo"));
            Assert.Equal("foo", resultSet[0][0].ToString());
        }

        [Fact]
        public void Can_query_with_string_and_int_filter() {
            _dataClient.AddTable(TableFoo,
                Column.String("colString"),
                Column.Int32("colInt"));

            _dataClient.Insert.Into(TableFoo).Columns("colString", "colInt").Values("foo", 1);

            ResultSet resultSet = _database.Query("select colString from foo where colString = :name and colInt = :id",
                In.Named("name", "foo"),
                In.Named("id", 1)
                );

            Assert.Equal("foo", resultSet[0][0].ToString());
        }

        [Fact]
        public void Can_query_with_string_and_int_by_name_filter() {
            _dataClient.AddTable(TableFoo,
                Column.String("colString"),
                Column.Int32("colInt"));

            _dataClient.Insert.Into(TableFoo).Columns("colString", "colInt").Values("foo", 1);

            ResultSet resultSet = _database.Query("select colString from foo where colString = :name and colInt = :id",
                In.Named("id", 1),
                In.Named("name", "foo")
                );

            Assert.Equal("foo", resultSet[0][0].ToString());
        }

        [Fact]
        public void Can_bulk_insert() {
            _dataClient.AddTable(TableFoo,
                Column.String("colString"),
                Column.Int32("colInt"));
            var v1s = new[] {"1", "2", "3", "4"};
            var v2s = new[] {1, 2, 3, 4};

            _database.ExecuteBulkSql("insert into " + TableFoo + " (colString, colInt) values (:v1,:v2)",
                In.Named("v1", v1s),
                In.Named("v2", v2s)
                );
            ResultSet res = _dataClient.Select
                .AllColumns()
                .From(TableFoo)
                .AllRows();

            Assert.Equal(4, res.Count);
        }

        [Fact]
        public void Can_bulk_insert_with_nullable() {
            _dataClient.AddTable(TableFoo, Column.Decimal("colDecimal"));
            var v1s = new decimal?[] {1, 2, 3, 4};
            _database.ExecuteBulkSql("insert into " + TableFoo + " (colDecimal) values (:v1)",
                In.Named("v1", v1s)
                );
            ResultSet res = _dataClient.Select
                .AllColumns()
                .From(TableFoo)
                .AllRows();
            Assert.Equal(4, res.Count);
        }

        [Fact]
        public void Can_insert_blob() {
            _dataClient.AddTable(TableFoo, Column.Binary("colBin"));
            var bytes = new byte[10];
            for (int i = 0; i < 10; i++) {
                bytes[i] = 66;
            }
            _database.ExecuteSql("insert into " + TableFoo + " (colBin) values (:v1)", In.Named("v1", bytes));
            var res = _dataClient.Select.AllColumns().From(TableFoo).AllRows();
            var bs = (byte[])res[0][0];
            CollectionAssert.AreEqual(bytes, bs);
        }

        [Fact]
        public void Can_insert_two_blobs() {
            _dataClient.AddTable(TableFoo, Column.Binary("colBin"), Column.Binary("colBin2"));
            var bytes = new byte[10];
            for (int i = 0; i < 10; i++) {
                bytes[i] = 66;
            }
            _database.ExecuteSql("insert into " + TableFoo + " (colBin,colBin2) values (:v1,:v2)", In.Named("v1", bytes), In.Named("v2", bytes));
            var res = _dataClient.Select.AllColumns().From(TableFoo).AllRows();
            var bs1 = (byte[])res[0][0];
            var bs2 = (byte[])res[0][1];
            CollectionAssert.AreEqual(bytes, bs1);
            CollectionAssert.AreEqual(bytes, bs2);
        }

        [Fact]
        public void Can_insert_big_blob() {
            _dataClient.AddTable(TableFoo, Column.Binary("colBin"));
            var bytes = new byte[1024 * 1024];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = 66;
            }
            _database.ExecuteSql("insert into " + TableFoo + " (colBin) values (:v1)", In.Named("v1", bytes));
            var res = _dataClient.Select.AllColumns().From(TableFoo).AllRows();
            var bs = (byte[])res[0][0];
            CollectionAssert.AreEqual(bytes, bs);
        }

        [Fact]
        public void Can_insert_two_big_blobs() {
            _dataClient.AddTable(TableFoo, Column.Binary("colBin"), Column.Binary("colBin2"), Column.Date("colDate"));
            var bin = new byte[1024*64];
            for (int i = 0; i < bin.Length; i++) {
                bin[i] = 66;
            }
            _database.ExecuteSql("insert into " + TableFoo + " (colBin,colBin2,colDate) values (:v1,:v2,:v3)", 
                In.Named("v1", bin),
                In.Named("v2", bin),
                In.Named("v3", DateTime.Now)
            );
            var res = _dataClient.Select.AllColumns().From(TableFoo).AllRows();
            var bs1 = (byte[])res[0][0];
            var bs2 = (byte[])res[0][1];
            CollectionAssert.AreEqual(bin, bs1);
            CollectionAssert.AreEqual(bin, bs2);
        }

        [Fact]
        public abstract void Can_bulk_insert_stored_procedure();

        [Fact]
        public abstract void Can_bulk_insert_stored_procedure_with_nullable();

        [Fact]
        public virtual void Can_bulk_insert_stored_procedure_with_first_item_null() { }

        [Fact]
        public abstract void Can_bulk_insert_stored_procedure_with_all_items_null();

        [Fact]
        public abstract void Can_bulk_insert_stored_procedure_with_nullable_and_dates();

        [Fact]
        public abstract void Can_call_stored_function_with_return_as_string();

        [Fact]
        public abstract void Can_call_stored_function_with_return_as_int();

        public void CleanTables() {
            if (_dataClient.TableExists(TableFoo)) {
                _dataClient.Remove.Table(TableFoo);
            }
        }

        public virtual void Dispose() {
            _database.RollBack();
            CleanTables();
            _database.Dispose();
            _dataClient.Dispose();
            _database = null;
            _dataClient = null;
            GC.Collect();
        }
    }
}