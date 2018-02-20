﻿using System.Collections.Generic;
using SharpData.Filters;
using SharpData.Schema;

namespace SharpData.Databases {
	public class SelectBuilder {
		private Dialect _dialect;
		private string[] _tables;
		private string[] _columns;

		public Filter Filter { get; set; }
		public OrderBy[] OrderBys { get; set; }
		public int Skip { get; set; }
		public int Take { get; set; }

		public In[] Parameters { get; set; }

		public bool HasFilter { get; protected set; }

		private string _select;

		public SelectBuilder(Dialect dialect, string[] tables, string[] columns) {
			_dialect = dialect;
			_tables = tables;
			_columns = columns;

			Parameters = new In[0];
		}

		public string Build() {
			_select = _dialect.GetSelectSql(_tables, _columns);
			ApplyFilter();
			ApplyOrderBy();
			ApplySkipTakeToSql();
			return _select;
		}

		private void ApplyFilter() {
			if (Filter != null) {
				HasFilter = true;

				var whereSql = _dialect.GetWhereSql(Filter, 0);
				var pars = Filter.GetAllValueParameters();
				Parameters = _dialect.ConvertToNamedParameters(0, pars);
				_select += " " + whereSql;
			}
		}

		private void ApplyOrderBy() {
			if (OrderBys != null) {
				_select += " " + _dialect.GetOrderBySql(OrderBys);
			}
		}

		protected string ApplySkipTakeToSql() {
			if (Skip > 0 || Take > 0) {
				_select = _dialect.WrapSelectSqlWithPagination(_select, Skip, Take);
			}
			return _select;
		}
	}
}