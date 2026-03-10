-- Add missing columns to Device table
-- This SQL adds the topology and simulation support columns

-- Add IsSimulatedDown column
ALTER TABLE [Device]
ADD [IsSimulatedDown] BIT NOT NULL DEFAULT 0;


