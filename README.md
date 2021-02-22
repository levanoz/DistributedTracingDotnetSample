# Distributed tracing in .NET Core services  
This sample project demonstrates distributed tracing implementation in .NET Core services using [OpenTelemetry](https://opentelemetry.io/).
  

## Test environment setup using Docker   
Use [Docker](https://www.docker.com/products/docker-desktop) and `./Docker/docker-compose.yml` file to setup required test environment for the sample project.  
`docker-compose.yml` file includes SQL Server, Redis, Elasticsearch, Kibana, Elastic APM and OpenTelemetry Collector images.  
Run following command: 
```
cd ./Docker
docker-compose up
```

In case of Elasticsearch error regarding max_map_count, please follow this instruction:  
https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html#_set_vm_max_map_count_to_at_least_262144  