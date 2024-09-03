using LiteDB;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryLiteDB : ITelemetryDB<TelemetryLogRecord>, ITelemetryDB<TelemetryFeatureRecord>
    {
        private readonly string _dbFilePath;

        internal TelemetryLiteDB(string dbFilePath)
        {
            _dbFilePath = dbFilePath;
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

        private LiteDatabase NewLiteDb()
        {
            return new LiteDatabase(new ConnectionString()
            {
                Filename = _dbFilePath,
                Connection = ConnectionType.Shared,
            }, NewBsonMapper());
        }

        private readonly int size = 1_00_000;

        private void TruncateToSize()
        {
            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();

            List<TelemetryLogRecord> logs = [];
            for (int i = 0; i < 20_00_000; i++)
            {
                logs.Add(new TelemetryLogRecord()
                {
                    AppVersion = "",
                    CreationTimestamp = DateTime.UtcNow,
                    LastUpdateTimestamp = DateTime.UtcNow,
                    Level = "",
                    Message = "",
                    UserId = "",
                });
            }
            collection.InsertBulk(logs, batchSize: 5000);

            long start = DateTime.Now.Ticks;
            TelemetryLogRecord outOfSizeLog = collection
                .Find(Query.All(nameof(TelemetryBaseRecord.LastUpdateTimestamp), Query.Descending), 
                    skip: size, 
                    limit: 1)
                .FirstOrDefault();
            TimeSpan skipTime = TimeSpan.FromTicks(DateTime.Now.Ticks - start);

            start = DateTime.Now.Ticks;
            int count = collection.Count();
            TimeSpan countTime = TimeSpan.FromTicks(DateTime.Now.Ticks - start);

            collection.DeleteMany(log => true);
            db.Rebuild();

            if (count <= size)
            {
                return;
            }

            collection.DeleteMany(log => log.LastUpdateTimestamp <= outOfSizeLog.LastUpdateTimestamp);
        }

        public Task AddAsync(TelemetryLogRecord log)
        {
            TruncateToSize();
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
            collection.Insert(log);

            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(TelemetryLogRecord log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
            return Task.FromResult(collection.Delete(new BsonValue(log.Id)));
        }

        public Task<bool> MarkFailedToUpload(TelemetryLogRecord log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(paramName: nameof(log));
            }

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
            TelemetryLogRecord logInDB = collection.FindById(new BsonValue(log.Id));
            if (logInDB == null)
            {
                return Task.FromResult(false);
            }
            if (logInDB.FailedToUpload)
            {
                logInDB.RetryAttempt++;
            }
            else
            {
                logInDB.FailedToUpload = true;
            }
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

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
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

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();
            collection.Insert(feature);

            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(TelemetryFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(paramName: nameof(feature));
            }

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();
            return Task.FromResult(collection.Delete(new BsonValue(feature.Id)));
        }

        public Task<bool> MarkFailedToUpload(TelemetryFeatureRecord feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(paramName: nameof(feature));
            }

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();
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

            using LiteDatabase db = NewLiteDb();
            ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();
            return Task.FromResult<IEnumerable<TelemetryFeatureRecord>>(collection
                .Find(log => log.FailedToUpload, limit: size)
                .OrderBy(log => log.LastUpdateTimestamp));
        }
    }
}
