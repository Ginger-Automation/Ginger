using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryLiteDB : ITelemetryDB<TelemetryLogRecord>, ITelemetryDB<TelemetryFeatureRecord>, IDisposable
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
                serialize: dateTime =>
                {
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
                    }
                    return new BsonValue(dateTime.ToString("O"));
                },
                deserialize: bsonValue =>
                {
                    DateTime dateTime = DateTime.Parse(bsonValue.AsString);
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                });

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

        public Task<IEnumerable<TelemetryLogRecord>> GetFailedToUploadRecords(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(size)}' cannot be less than or equal to 0");
            }

            ILiteCollection<TelemetryLogRecord> collection = _db.GetCollection<TelemetryLogRecord>();
            return Task.FromResult<IEnumerable<TelemetryLogRecord>>(collection
                .Find(log => log.FailedToUpload, limit: size)
                .OrderBy(log => log.LastUpdateTimestamp));
        }

        public Task AddAsync(TelemetryFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(paramName: nameof(feature));
            }

            ILiteCollection<TelemetryFeatureRecord> collection = _db.GetCollection<TelemetryFeatureRecord>();
            collection.Insert(feature);

            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(TelemetryFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(paramName: nameof(feature));
            }

            ILiteCollection<TelemetryFeatureRecord> collection = _db.GetCollection<TelemetryFeatureRecord>();
            return Task.FromResult(collection.Delete(new BsonValue(feature.Id)));
        }

        public Task<bool> MarkFailedToUpload(TelemetryFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(paramName: nameof(feature));
            }

            ILiteCollection<TelemetryFeatureRecord> collection = _db.GetCollection<TelemetryFeatureRecord>();
            TelemetryFeatureRecord featureInDB = collection.FindById(new BsonValue(feature.Id));
            if (featureInDB == null)
            {
                return Task.FromResult(false);
            }
            featureInDB.FailedToUpload = true;
            featureInDB.LastUpdateTimestamp = DateTime.UtcNow;
            collection.Update(featureInDB);

            return Task.FromResult(true);
        }

        Task<IEnumerable<TelemetryFeatureRecord>> ITelemetryDB<TelemetryFeatureRecord>.GetFailedToUploadRecords(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(size)}' cannot be less than or equal to 0");
            }

            ILiteCollection<TelemetryFeatureRecord> collection = _db.GetCollection<TelemetryFeatureRecord>();
            return Task.FromResult<IEnumerable<TelemetryFeatureRecord>>(collection
                .Find(log => log.FailedToUpload, limit: size)
                .OrderBy(log => log.LastUpdateTimestamp));
        }
    }
}
