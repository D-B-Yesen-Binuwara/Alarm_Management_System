-- Test script to verify vendor functionality
-- Insert test vendors

INSERT INTO Vendors (Name, Brand, DeviceType, Description, IsActive, CreatedAt)
VALUES 
('Huawei SLBN Vendor', 'Huawei', 0, 'Huawei vendor for SLBN devices', 1, GETDATE()),
('Nokia CEAN Vendor', 'Nokia', 1, 'Nokia vendor for CEAN devices', 1, GETDATE()),
('ZTE MSAN Vendor', 'ZTE', 2, 'ZTE vendor for MSAN devices', 1, GETDATE()),
('Huawei MSAN Vendor', 'Huawei', 2, 'Another Huawei vendor for MSAN devices', 1, GETDATE()),
('Ericsson SLBN Vendor', 'Ericsson', 0, 'Ericsson vendor for SLBN devices', 1, GETDATE());

-- Verify vendors were inserted
SELECT * FROM Vendors;

-- Test device type filtering
SELECT * FROM Vendors WHERE DeviceType = 0; -- SLBN vendors
SELECT * FROM Vendors WHERE DeviceType = 1; -- CEAN vendors  
SELECT * FROM Vendors WHERE DeviceType = 2; -- MSAN vendors

-- Test brand filtering
SELECT * FROM Vendors WHERE Brand = 'Huawei';
SELECT * FROM Vendors WHERE Brand = 'Nokia';
