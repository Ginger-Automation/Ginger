using Bogus;
using LiteDB;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            bsonMapper.RegisterType(
                serialize: timeSpan => new BsonValue(((int)timeSpan.TotalMilliseconds).ToString()),
                deserialize: bsonValue => TimeSpan.FromMilliseconds(int.Parse(bsonValue.AsString)));

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

        public Task AddAsync(IEnumerable<TelemetryLogRecord> logs)
        {
            if (logs == null)
            {
                throw new ArgumentNullException(paramName: nameof(logs));
            }
            if (!logs.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
    
                collection.InsertBulk(logs, batchSize: 5000);
            });
        }

        public Task DeleteAsync(IEnumerable<TelemetryLogRecord> logs)
        {
            if (logs == null)
            {
                throw new ArgumentNullException(paramName: nameof(logs));
            }
            if (!logs.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = logs.Select(log => log.Id);

                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();

                collection.DeleteMany(log => ids.Contains(log.Id));
            });
        }

        public Task IncrementUploadAttemptCount(IEnumerable<TelemetryLogRecord> logs)
        {
            if (logs == null)
            {
                throw new ArgumentNullException(paramName: nameof(logs));
            }
            if (!logs.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = logs.Select(l => l.Id);

                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();
                
                IEnumerable<TelemetryLogRecord> logsInDB = collection.Find(log => ids.Contains(log.Id));

                foreach (TelemetryLogRecord logInDB in logsInDB)
                {
                    logInDB.UploadAttempt++;
                    logInDB.LastUpdateTimestamp = DateTime.UtcNow;
                    collection.Update(logInDB);
                }
            });
        }

        public Task<IEnumerable<TelemetryLogRecord>> GetRecordsForRetry(IEnumerable<TelemetryLogRecord> exclude, int limit)
        {
            if (exclude == null)
            {
                throw new ArgumentNullException($"{nameof(exclude)} cannot be null");
            }
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(limit)}' cannot be less than or equal to 0");
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = exclude.Select(l => l.Id);
                
                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryLogRecord> collection = db.GetCollection<TelemetryLogRecord>();

                return collection
                    .Find(log => !ids.Contains(log.Id), limit: limit)
                    .OrderBy(log => log.LastUpdateTimestamp)
                    .ToList()
                    .AsEnumerable();
            });
        }

        public Task AddAsync(IEnumerable<TelemetryFeatureRecord> features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(paramName: nameof(features));
            }
            if (!features.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();

                collection.InsertBulk(features, batchSize: 5000);
            });
        }

        public Task DeleteAsync(IEnumerable<TelemetryFeatureRecord> features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(paramName: nameof(features));
            }
            if (!features.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = features.Select(feature => feature.Id);

                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();

                collection.DeleteMany(feature => ids.Contains(feature.Id));
            });
        }

        public Task IncrementUploadAttemptCount(IEnumerable<TelemetryFeatureRecord> features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(paramName: nameof(features));
            }
            if (!features.Any())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = features.Select(l => l.Id);

                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();

                IEnumerable<TelemetryFeatureRecord> featuresInDB = collection.Find(feature => ids.Contains(feature.Id));

                foreach (TelemetryFeatureRecord featureInDB in featuresInDB)
                {
                    featureInDB.UploadAttempt++;
                    featureInDB.LastUpdateTimestamp = DateTime.UtcNow;
                    collection.Update(featureInDB);
                }
            });
        }

        public Task<IEnumerable<TelemetryFeatureRecord>> GetRecordsForRetry(IEnumerable<TelemetryFeatureRecord> exclude, int limit)
        {
            if (exclude == null)
            {
                throw new ArgumentNullException($"{nameof(exclude)} cannot be null");
            }
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(limit)}' cannot be less than or equal to 0");
            }

            return Task.Run(() =>
            {
                IEnumerable<string> ids = exclude.Select(l => l.Id);

                using LiteDatabase db = NewLiteDb();
                ILiteCollection<TelemetryFeatureRecord> collection = db.GetCollection<TelemetryFeatureRecord>();

                return collection
                    .Find(log => !ids.Contains(log.Id), limit: limit)
                    .OrderBy(feature => feature.LastUpdateTimestamp)
                    .ToList()
                    .AsEnumerable();
            });
        }
    }
}
