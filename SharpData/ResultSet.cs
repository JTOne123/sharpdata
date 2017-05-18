using System;
using System.Collections.Generic;

namespace SharpData {
	public class ResultSet : List<TableRow> {
        private List<string> _originalColumnNames;
		private Dictionary<string, int> _cols;

        public ResultSet(params string[] cols) {
            _cols = new Dictionary<string, int>();
            _originalColumnNames = new List<string>();

            for (int i = 0; i < cols.Length; i++) {
                SetColumnName(cols[i], i);
            }
        }

	    private void SetColumnName(string col, int order) {
	        if (_cols.ContainsKey(col.ToUpper())) {
	            col+="_" + order;
	        }
	        _originalColumnNames.Add(col);
	        _cols.Add(col.ToUpper(), order);
	    }

	    public void AddRow(params object[] row) {
            if (row.Length != _cols.Keys.Count) {
                throw new ArgumentException("Wrong number of columns in row");
            }
            Add(new TableRow(this,row));
        }

        public int GetColumnIndex(string colName) {
            return _cols[colName.ToUpper()];
        }

        public string[] GetColumnNames() {
            return _originalColumnNames.ToArray();
        }

	    public bool IsEmpty {
	        get { return Count == 0; }
	    }
    }
}