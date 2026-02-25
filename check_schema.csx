using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var connString = "Host=localhost;Port=5432;Database=jeal_prototype;Username=postgres;Password=postgres";

using var conn = new NpgsqlConnection(connString);
conn.Open();

// Check if dealership_easycars_credentials table exists
using (var cmd = new NpgsqlCommand(@"
    SELECT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'dealership_easycars_credentials'
    )", conn))
{
    var exists = (bool)cmd.ExecuteScalar();
    Console.WriteLine($"dealership_easycars_credentials exists: {exists}");
}

// Check if easycars_sync_logs table exists
using (var cmd = new NpgsqlCommand(@"
    SELECT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'easycars_sync_logs'
    )", conn))
{
    var exists = (bool)cmd.ExecuteScalar();
    Console.WriteLine($"easycars_sync_logs exists: {exists}");
}

// Check if vehicles table has easycars columns
using (var cmd = new NpgsqlCommand(@"
    SELECT column_name 
    FROM information_schema.columns 
    WHERE table_name = 'vehicles' 
    AND column_name LIKE 'easycars%'
    ORDER BY column_name", conn))
{
    using var reader = cmd.ExecuteReader();
    Console.WriteLine("\nEasyCars columns in vehicles table:");
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader.GetString(0)}");
    }
}

// Check if leads table has easycars columns
using (var cmd = new NpgsqlCommand(@"
    SELECT column_name 
    FROM information_schema.columns 
    WHERE table_name = 'leads' 
    AND column_name LIKE 'easycars%'
    ORDER BY column_name", conn))
{
    using var reader = cmd.ExecuteReader();
    Console.WriteLine("\nEasyCars columns in leads table:");
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader.GetString(0)}");
    }
}
