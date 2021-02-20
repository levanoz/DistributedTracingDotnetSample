using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ServiceC.Api
{
    public class SampleRepository
    {
        private const string SampleDateCacheKey = "ServiceC.SampleData";

        private readonly IConfiguration _configuration;
        private readonly IDatabase _cache;

        public static readonly ActivitySource ActivitySource = new ActivitySource(typeof(SampleRepository).FullName);

        public SampleRepository(IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _cache = redis.GetDatabase();
        }

        public async Task<string> GetSampleData(int id, CancellationToken cancellationToken)
        {
            using var activity = ActivitySource.StartActivity($"{nameof(SampleRepository)}.{nameof(GetSampleData)}");
            activity?.SetTag("ObjectId", id);

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
            cmd.CommandText = "SELECT @id as SampleDataId, 'SampleData from Service C' as SampleData";
            cmd.Parameters.Add(new SqlParameter("id", SqlDbType.Int) { Value = id });

            var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetString("SampleData");
            }

            return null;
        }
    }
}
