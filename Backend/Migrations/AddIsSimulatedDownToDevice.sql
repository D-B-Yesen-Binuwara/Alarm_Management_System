-- Migration: Add IsSimulatedDown column to Device table
-- Purpose: Enable controlled device failure simulation for testing

ALTER TABLE Device
ADD IsSimulatedDown BIT NOT NULL DEFAULT 0;

-- Update existing devices to have IsSimulatedDown = false
UPDATE Device
SET IsSimulatedDown = 0
WHERE IsSimulatedDown IS NULL;
