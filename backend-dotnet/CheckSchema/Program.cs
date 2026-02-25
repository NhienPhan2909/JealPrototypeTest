using System;
using Npgsql;

var connString = "Host=localhost;Port=5432;Database=jeal_prototype;Username=postgres;Password=postgres";

try
{
    using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();
    Console.WriteLine("Connected to database: jeal_prototype\n");
    
    // Check if dealership_easycars_credentials table exists
    using (var cmd = new NpgsqlCommand(@"
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'dealership_easycars_credentials'
        )", conn))
    {
        var exists = (bool)(await cmd.ExecuteScalarAsync() ?? false);
        Console.WriteLine($"dealership_easycars_credentials table exists: {exists}");
    }
    
    // Check if easycars_sync_logs table exists
    using (var cmd = new NpgsqlCommand(@"
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'easycars_sync_logs'
        )", conn))
    {
        var exists = (bool)(await cmd.ExecuteScalarAsync() ?? false);
        Console.WriteLine($"easycars_sync_logs table exists: {exists}");
    }
    
    // Check if vehicles table has easycars columns
    using (var cmd = new NpgsqlCommand(@"
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'vehicles' 
        AND column_name LIKE 'easycars%'
        ORDER BY column_name", conn))
    {
        using var reader = await cmd.ExecuteReaderAsync();
        Console.WriteLine("\nEasyCars columns in vehicles table:");
        var count = 0;
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  - {reader.GetString(0)}");
            count++;
        }
        if (count == 0) Console.WriteLine("  (none found)");
    }
    
    // Check if leads table has easycars columns
    using (var cmd = new NpgsqlCommand(@"
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'leads' 
        AND column_name LIKE 'easycars%'
        ORDER BY column_name", conn))
    {
        using var reader = await cmd.ExecuteReaderAsync();
        Console.WriteLine("\nEasyCars columns in leads table:");
        var count = 0;
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"  - {reader.GetString(0)}");
            count++;
        }
        if (count == 0) Console.WriteLine("  (none found)");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

return 0;
