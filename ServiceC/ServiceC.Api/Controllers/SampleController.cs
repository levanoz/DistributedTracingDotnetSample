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

namespace ServiceC.Api.Controllers
{
    [Route("sample")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly SampleRepository _repository;

        public SampleController(SampleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get([FromRoute, Required] int id, CancellationToken cancellationToken)
        {
            var sampleData = await _repository.GetSampleData(id, cancellationToken);
            if (string.IsNullOrEmpty(sampleData))
                return NotFound();

            return Ok(sampleData);
        }
    }
}
