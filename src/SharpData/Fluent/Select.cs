using System;
using Sharp.Data.Filters;
using Sharp.Data.Schema;

namespace Sharp.Data.Fluent {
    public class Select : DataClientAction {

        public string[] Columns { get; set; }
        public Filter Filter { get; set; }
		public OrderBy[] OrderBy { get; set; } 
        public ResultSet ResultSet { get; set; }

    	public int Skip { get; set; }
    	public int Take { get; set; }

    	public Select(IDataClient dataClient) : base(dataClient) { }

        protected override void ExecuteInternal() {
            ResultSet = DataClient.SelectSql(TableNames, Columns, Filter, OrderBy, Skip, Take);
        }
    }
}