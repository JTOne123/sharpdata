using SharpData.Filters;

namespace SharpData.Fluent {
    public class Update : DataClientAction {

        public Filter Filter { get; set; }
        public string[] Columns { get; set; }
        public object[] Values { get; set; }
        public int AfectedRows { get; set; }

        public Update(IDataClient dataClient) : base(dataClient) { }

        protected override void ExecuteInternal() {
            AfectedRows = DataClient.UpdateSql(TableNames[0], Columns, Values, Filter);
        }
    }
}