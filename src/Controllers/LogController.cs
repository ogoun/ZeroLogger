using APILogger.Models;
using APILogger.Models.DB;
using APILogger.Models.Requests;
using APILogger.Services;
using APILogger.Services.DB;
using Microsoft.AspNetCore.Mvc;

namespace APILogger.Controllers
{
    [ApiController]
    public class LogController : ControllerBase
    {
        const int MAX_LAST_SECONDS = 60 * 60 * 24 * 7; // WEEK
        private readonly LogRepository _logger;

        public LogController(LogRepository logger)
        {
            _logger = logger;
        }

        [HttpPost("/api/log")]
        public IActionResult Save([FromBody] LogRecordRequest request)
        {
            var id = (long)HttpContext.Items["UserId"]!;
            _logger.Write(new LogRecord
            {
                LogLevel = request.LogLevel,
                Tag = request.Tag,
                Text = request.Text,
                Source = request.Source,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                RemoteAddress = HttpContext.Items["remoteAddress"]?.ToString() ?? string.Empty,
                UserId = id
            });
            return Ok();
        }

        [HttpGet("/api/log/{id}")]
        public ActionResult<LogRecord> Get([FromRoute] long id)
        {
            var userId = (long)HttpContext.Items["UserId"]!;
            if (userId == -1)
            {
                return Ok(_logger.Single(x => x.Id == id));
            }
            var user = AuthProvider.GetUserThoughCachee(userId);
            if (user != null)
            {
                var record = _logger.Single(x => x.Id == id);
                if (user.Sources.Contains(record.Source))
                {
                    return Ok(record);
                }
            }
            return NoContent();
        }

        [HttpGet("/api/last")]
        public ActionResult<LogRecord> GetLast([FromQuery] int? seconds)
        {
            long ts = 60 * 15;
            if (seconds.HasValue && seconds > 0 && seconds <= MAX_LAST_SECONDS)
            {
                ts = DateTimeOffset.UtcNow.AddSeconds(-seconds.Value).ToUnixTimeMilliseconds();
            }
            var userId = (long)HttpContext.Items["UserId"]!;
            if (userId == -1)
            {
                return Ok(_logger.SelectBy(x => x.Timestamp > ts));
            }
            var user = AuthProvider.GetUserThoughCachee(userId);
            if (user != null)
            {
                var records = _logger
                    .SelectBy(x => x.Timestamp > ts)
                    .ToArray()
                    .Where(r => user.Sources.Contains(r.Source));
                return Ok(records);
            }
            return NoContent();
        }

        [HttpPost("/api/search")]
        public ActionResult<IEnumerable<LogRecord>> Search([FromBody] LogRecordSearchRequest request)
        {
            var userId = (long)HttpContext.Items["UserId"]!;
            if (userId == -1)
            {
                return Ok(_logger.Search(request));
            }
            var user = AuthProvider.GetUserThoughCachee(userId);
            if (user != null)
            {
                var records = _logger.Search(request)
                    .ToArray()
                    .Where(r => user.Sources.Contains(r.Source));
                return Ok(records);
            }
            return Ok(Enumerable.Empty<string>());
        }

        [HttpGet("/api/sources")]
        public ActionResult<IEnumerable<LogRecord>> GetSources()
        {
            var userId = (long)HttpContext.Items["UserId"]!;
            if (userId == -1)
            {
                return Ok(_logger.GetSources());
            }
            var user = AuthProvider.GetUserThoughCachee(userId);
            if (user != null)
            {
                var sources = _logger.GetSources().ToArray().Where(s => user.Sources.Contains(s));
                return Ok(sources);
            }
            return Ok(Enumerable.Empty<string>());
        }

        [HttpGet("/api/loglevels")]
        public ActionResult<IEnumerable<string>> GetLogLevels()
        {
            return Ok(_logger.GetLogLevels());
        }
    }
}