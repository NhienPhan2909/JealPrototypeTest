using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace JealPrototype.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemaCheckController : ControllerBase
    {
        [HttpGet("easycars")]
        public async Task<IActionResult> CheckEasyCarsSchema()
        {
            var connString = "Host=localhost;Port=5432;Database=jeal_prototype;Username=postgres;Password=postgres";
            var results = new Dictionary<string, object>();
            
            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            
            // Check tables
            using (var cmd = new NpgsqlCommand(@"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name IN ('dealership_easycars_credentials', 'easycars_sync_logs', 'vehicles', 'leads')
                ORDER BY table_name", conn))
            {
                var tables = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                results["existingTables"] = tables;
            }
            
            // Check columns in vehicles
            using (var cmd = new NpgsqlCommand(@"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_name = 'vehicles' 
                AND column_name LIKE '%easycars%'
                ORDER BY column_name", conn))
            {
                var columns = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(0));
                }
                results["vehiclesEasyCarsColumns"] = columns;
            }
            
            // Check columns in leads
            using (var cmd = new NpgsqlCommand(@"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_name = 'leads' 
                AND column_name LIKE '%easycars%'
                ORDER BY column_name", conn))
            {
                var columns = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(0));
                }
                results["leadsEasyCarsColumns"] = columns;
            }
            
            return Ok(results);
        }
    }
}
