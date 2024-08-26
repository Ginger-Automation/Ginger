using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryLiteDB : ITelemetryDB<TelemetryLogRecord>, IDisposable
    {
        private readonly LiteDatabase _db;

        internal TelemetryLiteDB(string dbFilePath)
        {
            _db = new(new ConnectionString()
            {
                Filename = dbFilePath,
                Connection = ConnectionType.Shared,
            }, NewBsonMapper());
        }

        private BsonMapper NewBsonMapper()
        {
            BsonMapper bsonMapper = new();

            bsonMapper.RegisterType(
                serialize: dateTime => new BsonValue(dateTime.ToString("O")),
                deserialize: bsonValue => DateTime.Parse(bsonValue.AsString));

            bsonMapper.RegisterType(
                serialize: guid => new BsonValue(guid.ToString()),
                deserialize: bsonValue => Guid.Parse(bsonValue.AsString));

            return bsonMapper;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public Task AddAsync(TelemetryLogRecord log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            ILiteCollection<TelemetryLogRecord> collection = _db.GetCollection<TelemetryLogRecord>();
            collection.Insert(log);

            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(TelemetryLogRecord log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            ILiteCollection<TelemetryLogRecord> collection = _db.GetCollection<TelemetryLogRecord>();
            return Task.FromResult(collection.Delete(new BsonValue(log.Id)));
        }

        public Task<bool> MarkFailedToUpload(TelemetryLogRecord log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            ILiteCollection<TelemetryLogRecord> collection = _db.GetCollection<TelemetryLogRecord>();
            TelemetryLogRecord logInDB = collection.FindById(new BsonValue(log.Id));
            if (logInDB == null)
            {
                return Task.FromResult(false);
            }
            logInDB.FailedToUpload = true;
            logInDB.LastUpdateTimestamp = DateTime.UtcNow;
            collection.Update(logInDB);

            return Task.FromResult(true);
        }
    }
}
