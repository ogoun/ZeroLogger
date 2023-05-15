using APILogger.Models.DB;
using APILogger.Models.Requests;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using ZeroLevel;

namespace APILogger.Services.DB
{
    public class SourceName
    {
        public string Source { get; set; }
    }

    public class LogLevelName
    {
        public string LogLevel { get; set; }
    }

    public class LogRepository
        : BaseSqLiteDB<LogRecord>
    {
        private const string QUERY_DISTINCT_LOGLEVELS = @"SELECT DISTINCT LogLevel FROM LogRecord";
        private const string QUERY_DISTINCT_SOURCES = @"SELECT DISTINCT Source FROM LogRecord";

        private readonly ConcurrentQueue<LogRecord> _writeQueue = new ConcurrentQueue<LogRecord>();
        private long _updateTask = -1;
        public LogRepository(string dbPath)
            : base(dbPath)
        {
            CreateTable();
            _updateTask = Sheduller.RemindEvery(TimeSpan.FromSeconds(3), FlushToDb);
        }
        public void Write(LogRecord record)
        {
            _writeQueue.Enqueue(record);
        }

        private void FlushToDb()
        {
            if (_writeQueue.Count > 0)
            {
                do
                {
                    int batchSize = Math.Min(_writeQueue.Count, 5000);
                    var batch = new List<LogRecord>(batchSize);
                    while (batch.Count < batchSize && _writeQueue.Count > 0)
                    {
                        if (_writeQueue.TryDequeue(out var r))
                        {
                            batch.Add(r);
                        }
                    }
                    try
                    {
                        Append(batch);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[SqliteEventWriter.FlushToDb]");
                    }
                    batch = null;
                } while (_writeQueue.Count > 0);
            }
        }

        private Expression<Func<LogRecord, bool>> ExpressionReduceList(List<Expression> list, ParameterExpression argParam)
        {
            var indexesToRemove = new List<int>(list.Count);
            while (list.Count > 1)
            {
                for (int i = 0; i < list.Count; i += 2)
                {
                    list[i] = Expression.AndAlso(list[i], list[i + 1]);
                    indexesToRemove.Add(i + 1);
                }
                for (int i = indexesToRemove.Count - 1; i >= 0; i--)
                {
                    list.RemoveAt(indexesToRemove[i]);
                }
            }
            if (list.Count == 1)
            {
                return Expression.Lambda<Func<LogRecord, bool>>(list[0], argParam);
            }
            return null!;
        }

        public IEnumerable<LogRecord> Search(LogRecordSearchRequest filter)
        {
            var expressionList = new List<Expression>();
            var argParam = Expression.Parameter(typeof(LogRecord));

            if (filter.Id.HasValue)
            {
                var eventConst = Expression.Constant(filter.Id.Value);
                var eventProperty = Expression.Property(argParam, "Id");
                var eventExp = Expression.Equal(eventProperty, eventConst);
                expressionList.Add(eventExp);
            }

            if (string.IsNullOrWhiteSpace(filter.Tag) == false)
            {
                var tagConst = Expression.Constant(filter.Tag);
                var tagProperty = Expression.Property(argParam, "Tag");
                var tagExp = Expression.Equal(tagProperty, tagConst);
                expressionList.Add(tagExp);
            }

            if (string.IsNullOrWhiteSpace(filter.Source) == false)
            {
                var sourceConst = Expression.Constant(filter.Source);
                var sourceProperty = Expression.Property(argParam, "Source");
                var sourceExp = Expression.Equal(sourceProperty, sourceConst);
                expressionList.Add(sourceExp);
            }

            if (string.IsNullOrWhiteSpace(filter.LogLevel) == false)
            {
                var logLevelConst = Expression.Constant(filter.LogLevel);
                var logLevelProperty = Expression.Property(argParam, "LogLevel");
                var logLevelExp = Expression.Equal(logLevelProperty, logLevelConst);
                expressionList.Add(logLevelExp);
            }

            if (string.IsNullOrWhiteSpace(filter.Text) == false)
            {
                var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var textConst = Expression.Constant(filter.Text);
                var textProperty = Expression.Property(argParam, "Text");
                var containsMethodExp = Expression.Call(textProperty, method, textConst);
                expressionList.Add(containsMethodExp);
            }

            if (filter.Start.HasValue)
            {
                var ts = new DateTimeOffset(filter.Start.Value).ToUnixTimeMilliseconds();
                var startConst = Expression.Constant(ts);
                var startProperty = Expression.Property(argParam, "Timestamp");
                var startExp = Expression.GreaterThanOrEqual(startProperty, startConst);
                expressionList.Add(startExp);
            }

            if (filter.End.HasValue)
            {
                var ts = new DateTimeOffset(filter.End.Value).ToUnixTimeMilliseconds();
                var endConst = Expression.Constant(ts);
                var endProperty = Expression.Property(argParam, "Timestamp");
                var endExp = Expression.LessThanOrEqual(endProperty, endConst);
                expressionList.Add(endExp);
            }

            var expr = ExpressionReduceList(expressionList, argParam);
            if (expr != null)
            {
                return SelectBy(expr);
            }
            return Enumerable.Empty<LogRecord>();
        }

        public IEnumerable<string> GetSources()
        {
            return _db.Query<SourceName>(QUERY_DISTINCT_SOURCES).Select(r => r.Source);
        }

        public IEnumerable<string> GetLogLevels()
        {
            return _db.Query<LogLevelName>(QUERY_DISTINCT_LOGLEVELS).Select(r => r.LogLevel);
        }

        protected override void DisposeStorageData()
        {
            Sheduller.Remove(_updateTask);
            FlushToDb();
        }
    }
}
