using System.Collections.Generic;

namespace GingerCore.Environments
{
    public interface IDatabaseOperations
    {
        string ConnectionStringCalculated { get; }
        string PassCalculated { get; }
        string TNSCalculated { get; }
        string UserCalculated { get; }

        bool CheckUserCredentialsInTNS();
        void CloseConnection();
        bool Connect(bool displayErrorPopup = false);
        string CreateConnectionString();
        List<object> FreeSQL(string SQL, int? timeout = null);
        string fTableColWhere(string Table, string Column, string Where);
        string fUpdateDB(string updateCmd, bool commit);
        string GetCalculatedWithDecryptTrue(string value);
        string GetConnectionString();
        List<string> GetTablesColumns(string table);
        List<string> GetTablesList(string Keyspace = null);
        string GetRecordCount(string SQL);
        bool MakeSureConnectionIsOpen();
        void SplitUserIdPassFromTNS();
    }
}
