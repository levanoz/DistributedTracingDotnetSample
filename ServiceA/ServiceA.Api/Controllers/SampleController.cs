using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ServiceA.Api.Controllers
{
    [Route("sample")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public SampleController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get([FromRoute, Required] int id, CancellationToken cancellationToken)
        {
            var sampleData = await GetSampleDataFromDatabase(id, cancellationToken);
            if (string.IsNullOrEmpty(sampleData))
                return NotFound();

            //Some fake logic
            var sampleDataFromOtherService = id % 2 == 0
                ? await GetSampleDataFromServiceB(id, cancellationToken)
                : await GetSampleDataFromServiceC(id, cancellationToken);

            return Ok($"{sampleData}, {sampleDataFromOtherService}");
        }

        private async Task<string> GetSampleDataFromDatabase(int id, CancellationToken cancellationToken)
        {
            //Get data from SQL database.
            //In a real project, these method should be placed in Repository.

            await using var connection = new SqlConnection(_configuration.GetConnectionString("Database"));
            await connection.OpenAsync(cancellationToken);

            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT @id as SampleDataId, 'SampleData from Service A' as SampleData";
            cmd.Parameters.Add(new SqlParameter("id", SqlDbType.Int) { Value = id });

            var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetString("SampleData");
            }

            return null;
        }

        private async Task<string> GetSampleDataFromServiceB(int id, CancellationToken cancellationToken)
        {
            var response = await _httpClientFactory.CreateClient("ServiceB").GetAsync($"/sample/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetSampleDataFromServiceC(int id, CancellationToken cancellationToken)
        {
            var response = await _httpClientFactory.CreateClient("ServiceC").GetAsync($"/sample/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
    }
}
