-- Connect to database and verify Story 2.3 tables
SELECT table_name, column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'easycar_sync_logs' 
ORDER BY ordinal_position;
