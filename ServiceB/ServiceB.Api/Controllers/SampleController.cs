using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ServiceB.Api.Controllers
{
    [Route("sample")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private const string SampleDateCacheKey = "ServiceB.SampleData";

        private readonly IConfiguration _configuration;
        private readonly IDatabase _cache;

        public SampleController(IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _cache = redis.GetDatabase();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get([FromRoute, Required] int id, CancellationToken cancellationToken)
        {
            var sampleData = await GetSampleData(id, cancellationToken);
            if (string.IsNullOrEmpty(sampleData))
                return NotFound();

            return Ok(sampleData);
        }

        private async Task<string> GetSampleData(int id, CancellationToken cancellationToken)
        {
            //Get data from redis cache or SQL database.
            //In a real project, these operations and methods should be placed in Repository.

            string sampleData = await _cache.StringGetAsync(SampleDateCacheKey);

            if (string.IsNullOrEmpty(sampleData))
            {
                sampleData = await GetSampleDataFromDatabase(id, cancellationToken);
                if (sampleData == null)
                    return null;

                await _cache.StringSetAsync(SampleDateCacheKey, sampleData, TimeSpan.FromSeconds(30));
            }

            return sampleData;
        }

        private async Task<string> GetSampleDataFromDatabase(int id, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Database"));
            await connection.OpenAsync(cancellationToken);

            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT @id as SampleDataId, 'SampleData from Service B' as SampleData";
            cmd.Parameters.Add(new SqlParameter("id", SqlDbType.Int) {Value = id});

            var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetString("SampleData");
            }

            return null;
        }
    }
}
