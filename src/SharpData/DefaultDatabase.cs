﻿using System;
using System.Data;
using System.Text;
using Sharp.Data.Log;
using System.Data.Common;

namespace Sharp.Data {
    public class DefaultDatabase {
        private static readonly ISharpLogger Log = LogManager.GetLogger("Sharp.Data.Database");

        public IDataProvider Provider { get; protected set; }
        public string ConnectionString { get; protected set; }
        public int Timeout { get; set; }
        
        protected DbConnection Connection;
        protected DbTransaction Transaction;

        public DefaultDatabase(IDataProvider provider, string connectionString) {
            Provider = provider;
            ConnectionString = connectionString;
            LogDatabaseProviderName(provider.ToString());
        }

        protected static void LogDatabaseProviderName(string providerName) {
            Log.Debug("Provider: " + providerName);
        }

        protected void RetrieveOutParameters(object[] parameters, DbCommand cmd) {
            if (parameters == null) {
                return;
            }
            foreach (object parameter in parameters) {
                Out pout = parameter as Out;
                if (pout != null) {
                    pout.Value = ((DbParameter) cmd.Parameters[pout.Name]).Value;
                    continue;
                }
                InOut pinout = parameter as InOut;
                if (pinout != null) {
                    pinout.Value = ((DbParameter) cmd.Parameters[pinout.Name]).Value;
                    continue;
                }
            }
        }

        protected DbDataReader TryCreateReader(string call, object[] parameters, CommandType commandType) {
            DbCommand cmd = CreateCommand(call, parameters);
            cmd.CommandType = commandType;
            return cmd.ExecuteReader();
        }


        protected object TryQueryReader(string call, object[] parameters) {
            DbCommand cmd = CreateCommand(call, parameters);
            object obj = cmd.ExecuteScalar();
            return obj;
        }

        protected void TryExecuteStoredProcedure(string call, object[] parameters, bool isBulk = false) {
            DbCommand cmd = CreateCommand(call, parameters, isBulk);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
            RetrieveOutParameters(parameters, cmd);
        }

        protected object TryCallStoredFunction(DbType returnType, string call, object[] parameters) {
            DbParameter returnPar = GetReturnParameter(returnType);

            var cmd = CreateCommand(call, parameters);
            cmd.Parameters.Insert(0, returnPar);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();

            object returnObject = returnPar.Value;
            return returnObject;
        }
        
        protected void SetTimeoutForCommand(DbCommand cmd) {
            //Doesn't work for oracle!
            if (Timeout >= 0) {
                cmd.CommandTimeout = Timeout;
            }
        }

        protected DbCommand CreateIDbCommand(string call, object[] parameters) {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = call;
            cmd.Transaction = Transaction;
            return cmd;
        }

        protected static void LogCommandCall(string call, DbCommand cmd) {
            if (Log.IsDebugEnabled) {
                var sb = new StringBuilder();
                sb.Append("Call: ").AppendLine(call);
                foreach (DbParameter p in cmd.Parameters) {
                    sb.Append(p.Direction).Append("-> ").Append(p.ParameterName);
                    if (p.Value != null) {
                        sb.Append(": ").Append(p.Value);
                    }
                    sb.AppendLine();
                }
                Log.Debug(sb.ToString());
            }
        }

        protected void PopulateCommandParameters(DbCommand cmd, object[] parameters, bool isBulk) {
            if (parameters == null) {
                return;
            }
            foreach (object parameter in parameters) {
                DbParameter par;
                if (parameter is Out) {
                    par = GetOutParameter((Out) parameter);
                }
                else if (parameter is InOut) {
                    par = GetInOutParameter((InOut) parameter);
                }
                else if (parameter is In) {
                    par = GetInParameter((In) parameter, cmd, isBulk);
                }
                else {
                    par = GetInParameter(new In { Value = parameter }, cmd, isBulk);
                }
                //this is for when you have the cursor parameter, ignored by sql server
                if (par != null) {
                    cmd.Parameters.Add(par);
                }
            }
        }

        protected DbParameter GetInParameter(In p, DbCommand cmd, bool isBulk) {
            var par = Provider.GetParameter(p, isBulk);
            par.Direction = ParameterDirection.Input;
            par.Value = p.Value ?? DBNull.Value;
            par.ParameterName = p.Name;
            return par;
        }

        protected DbParameter GetOutParameter(Out outParameter) {
            DbParameter par;
            if (outParameter.IsCursor) {
                par = Provider.GetParameterCursor();
            }
            else {
                par = Provider.GetParameter();
            }
            //this "if != null" is for the cursor parameter, ignored by sql server
            if (par != null) {
                par.Direction = ParameterDirection.Output;
                par.ParameterName = outParameter.Name;
                par.Size = outParameter.Size;
                par.Value = outParameter.Value;
                par.DbType = outParameter.Type;
            }
            return par;
        }

        protected DbParameter GetInOutParameter(InOut p) {
            var par = Provider.GetParameter();

            par.Direction = ParameterDirection.InputOutput;
            par.ParameterName = p.Name;
            par.Size = p.Size;
            par.Value = p.Value ?? DBNull.Value;
            par.DbType = p.Type;
            return par;
        }

        protected DbParameter GetReturnParameter(DbType type) {
            var par = Provider.GetParameter(null, false);
            par.Direction = ParameterDirection.ReturnValue;
            par.Size = 4000;
            par.DbType = type;
            return par;
        }

        protected void CloseTransaction() {
            if (Transaction == null) {
                return;
            }
            try {
                Transaction.Dispose();
            }
            catch { }
            Transaction = null;
        }

        protected void CloseConnection() {
            if (Connection == null) {
                return;
            }
            try {
                Connection.Close();
                Connection.Dispose();
                Log.Debug("Connection closed");
            }
            catch { }
            Connection = null;
        }

        protected void CommitTransaction() {
            if (Transaction == null) {
                return;
            }
            Transaction.Commit();
            Log.Debug("Commit");
        }

        protected void RollBackTransaction() {
            if (Transaction == null) {
                return;
            }
            Transaction.Rollback();
            Log.Debug("Rollback");
        }

        protected int TryExecuteSql(string call, object[] parameters, bool isBulk = false) {
            DbCommand cmd = CreateCommand(call, parameters, isBulk);
            int modifiedRows = cmd.ExecuteNonQuery();
            RetrieveOutParameters(parameters, cmd);
            return modifiedRows;
        }

        protected DbCommand CreateCommand(string call, object[] parameters, bool isBulk = false) {
            OpenConnection();
            DbCommand cmd = CreateIDbCommand(call, parameters);
            Provider.ConfigCommand(cmd, parameters, isBulk);
            SetTimeoutForCommand(cmd);
            PopulateCommandParameters(cmd, parameters, isBulk);
            LogCommandCall(call, cmd);
            return cmd;
        }

        protected void OpenConnection() {
            if (Connection != null) {
                return;
            }
            Connection = Provider.GetConnection();
            Connection.ConnectionString = ConnectionString;
            Connection.Open();
            Transaction = Connection.BeginTransaction();

            Log.Debug("Connection open");
        }

        protected ResultSet ExecuteCatchingErrors(Func<DbDataReader> getReader, string call) {
            DbDataReader reader = null;
            try {
                BeforeActionVerifyIfExistisACommandToBeExecuted();
                reader = getReader();
                return DataReaderToResultSetMapper.Map(reader);
            }
            catch (Exception ex) {
                OnErrorVerifyIfExistisACommandToBeExecuted();
                throw Provider.CreateSpecificException(ex, call);
            }
            finally {
                if (reader != null) {
                    reader.Dispose();
                }
            }
        }

        protected T ExecuteCatchingErrors<T>(Func<T> action, string call) {
            try {
                BeforeActionVerifyIfExistisACommandToBeExecuted();
                return action();
            }
            catch (Exception ex) {
                OnErrorVerifyIfExistisACommandToBeExecuted();
                throw Provider.CreateSpecificException(ex, call);
            }
        }

        protected void ExecuteCatchingErrors(Action action, string call) {
            try {
                BeforeActionVerifyIfExistisACommandToBeExecuted();
                action();
            }
            catch (Exception ex) {
                OnErrorVerifyIfExistisACommandToBeExecuted();
                throw Provider.CreateSpecificException(ex, call);
            }
        }

        protected void OnErrorVerifyIfExistisACommandToBeExecuted() {
            if (!String.IsNullOrEmpty(Provider.CommandToBeExecutedAfterAnExceptionIsRaised())) {
                TryExecuteSql(Provider.CommandToBeExecutedAfterAnExceptionIsRaised(), new object[] { });
            }
        }

        protected void BeforeActionVerifyIfExistisACommandToBeExecuted() {
            if (!String.IsNullOrEmpty(Provider.CommandToBeExecutedBeforeEachOther())) {
                TryExecuteSql(Provider.CommandToBeExecutedBeforeEachOther(), new object[] { });
            }
        }
    }
}