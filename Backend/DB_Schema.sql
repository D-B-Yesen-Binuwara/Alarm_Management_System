CREATE DATABASE INMS_SLT;
USE INMS_SLT;
DROP DATABASE INM_SLT;

/* AREA STRUCTURE -------------------------------------------------------- */
CREATE TABLE Region (
    RegionId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Province (
    ProvinceId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    RegionId INT NOT NULL,
    CONSTRAINT FK_Province_Region
        FOREIGN KEY (RegionId) REFERENCES Region(RegionId)
);

CREATE TABLE LEA (
    LEAId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ProvinceId INT NOT NULL,
    CONSTRAINT FK_LEA_Province
        FOREIGN KEY (ProvinceId) REFERENCES Province(ProvinceId)
);

/* USERS & ACCESS --------------------------------------------------------------- */

CREATE TABLE Role (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT FK_User_Role
        FOREIGN KEY (RoleId) REFERENCES Role(RoleId)
);

CREATE TABLE UserAreaAssignment (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    AreaType NVARCHAR(50) NOT NULL, -- Region | Province | LEA
    AreaId INT NOT NULL,
    CONSTRAINT FK_UserArea_User
        FOREIGN KEY (UserId) REFERENCES [User](UserId)
);

/* DEVICES ------------------------------------------------------ */
CREATE TABLE Device (
    DeviceId INT IDENTITY(1,1) PRIMARY KEY,
    DeviceName NVARCHAR(150) NOT NULL,
    DeviceType NVARCHAR(50) NOT NULL,  -- SLBN | CEAN | MSAN | CUSTOMER
    IP NVARCHAR(50),
    Status NVARCHAR(20) NOT NULL DEFAULT 'UP', -- UP | DOWN | UNREACHABLE
    PriorityLevel NVARCHAR(20) NOT NULL DEFAULT 'LOW', -- LOW | AVERAGE | HIGH | CRITICAL
    LEAId INT NOT NULL,
    AssignedUserId INT NULL,

    CONSTRAINT FK_Device_LEA
        FOREIGN KEY (LEAId) REFERENCES LEA(LEAId),

    CONSTRAINT FK_Device_User
        FOREIGN KEY (AssignedUserId) REFERENCES [User](UserId)
);

ALTER TABLE Device
ADD Latitude DECIMAL(9,6) NULL,
    Longitude DECIMAL(9,6) NULL;

/* TOPOLOGY LINKS --------------------------------------------------------------------- */
CREATE TABLE DeviceLink (
    LinkId INT IDENTITY(1,1) PRIMARY KEY,
    ParentDeviceId INT NOT NULL,
    ChildDeviceId INT NOT NULL,
    LinkStatus NVARCHAR(20) NOT NULL DEFAULT 'UP',

    CONSTRAINT FK_Link_Parent
        FOREIGN KEY (ParentDeviceId) REFERENCES Device(DeviceId),

    CONSTRAINT FK_Link_Child
        FOREIGN KEY (ChildDeviceId) REFERENCES Device(DeviceId)
);

/* ALARMS ---------------------------------------------------------------------------- */
CREATE TABLE Alarm (
    AlarmId INT IDENTITY(1,1) PRIMARY KEY,
    DeviceId INT NOT NULL,
    AlarmType NVARCHAR(50) NOT NULL, -- AC | BL | NODE_DOWN | LINK_DOWN
    RaisedTime DATETIME NOT NULL DEFAULT GETDATE(),
    ClearedTime DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Alarm_Device
        FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId)
);

/* ROOT CAUSE ---------------------------------------------------------------------- */
CREATE TABLE RootCause (
    RootCauseId INT IDENTITY(1,1) PRIMARY KEY,
    AlarmId INT NOT NULL,
    RootCauseDeviceId INT NOT NULL,
    RootCauseType NVARCHAR(50) NOT NULL, -- NODE_FAILURE | LINK_FAILURE
    DetectedTime DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_RootCause_Alarm
        FOREIGN KEY (AlarmId) REFERENCES Alarm(AlarmId),

    CONSTRAINT FK_RootCause_Device
        FOREIGN KEY (RootCauseDeviceId) REFERENCES Device(DeviceId)
);

/* IMPACT ANALYSIS ------------------------------------------------------------------- */
CREATE TABLE ImpactedDevice (
    ImpactId INT IDENTITY(1,1) PRIMARY KEY,
    RootCauseId INT NOT NULL,
    DeviceId INT NOT NULL,
    ImpactType NVARCHAR(50) NOT NULL, -- UPSTREAM | DOWNSTREAM

    CONSTRAINT FK_Impact_RootCause
        FOREIGN KEY (RootCauseId) REFERENCES RootCause(RootCauseId),

    CONSTRAINT FK_Impact_Device
        FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId)
);

/* HEARTBEAT ------------------------------------------------------------------------ */
CREATE TABLE Heartbeat (
    HeartbeatId INT IDENTITY(1,1) PRIMARY KEY,
    DeviceId INT NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL, -- UP | DOWN

    CONSTRAINT FK_Heartbeat_Device
        FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId)
);

/* SIMULATION EVENTS -------------------------------------------------------------- */
CREATE TABLE SimulationEvent (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    DeviceId INT NOT NULL,
    EventType NVARCHAR(50) NOT NULL, -- AC_FAIL | BATTERY_DRAIN | LINK_FAIL
    EventTime DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Simulation_Device
        FOREIGN KEY (DeviceId) REFERENCES Device(DeviceId)
);

/* --------------------------------------------------- */

CREATE INDEX IX_Device_LEA ON Device(LEAId);
CREATE INDEX IX_Alarm_Device ON Alarm(DeviceId);
CREATE INDEX IX_RootCause_Alarm ON RootCause(AlarmId);
CREATE INDEX IX_Impact_Device ON ImpactedDevice(DeviceId);
CREATE INDEX IX_DeviceLink_Parent ON DeviceLink(ParentDeviceId);
CREATE INDEX IX_DeviceLink_Child ON DeviceLink(ChildDeviceId);


-- Add missing columns
ALTER TABLE Role ADD Description NVARCHAR(255) NULL;
ALTER TABLE Region ADD Description NVARCHAR(255) NULL;

-- Rename Role column
EXEC sp_rename 'Role.RoleName', 'Name', 'COLUMN';

/*------------------------------------------------*/
USE INMS_SLT;

/* SAMPLE DATA - HIERARCHICAL STRUCTURE */

-- Regions
INSERT INTO Region (Name) VALUES 
('Western Region'),
('Central Region'),
('Southern Region');

-- Provinces
INSERT INTO Province (Name, RegionId) VALUES 
('Colombo', 1),
('Gampaha', 1),
('Kandy', 2),
('Galle', 3),
('Matara', 3);

-- LEAs
INSERT INTO LEA (Name, ProvinceId) VALUES 
('Colombo Central', 1),
('Colombo North', 1),
('Gampaha Town', 2),
('Kandy City', 3),
('Galle Fort', 4);

-- Roles
INSERT INTO Role (Name) VALUES 
('Admin'),
('Region Officer'),
('Province Officer'),
('LEA Officer');

USE INMS_SLT;
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Role';


-- Users
INSERT INTO [User] (Username, PasswordHash, FullName, RoleId) VALUES 
('admin', 'hash123', 'System Admin', 1),
('officer1', 'hash456', 'John Silva', 4),
('officer2', 'hash789', 'Mary Fernando', 4);

-- Devices (SLBN → CEAN → MSAN hierarchy)
INSERT INTO Device (DeviceName, DeviceType, IP, Status, PriorityLevel, LEAId, AssignedUserId) VALUES
('SLBN-Colombo-01', 'SLBN', '10.0.1.1', 'UP', 'Critical', 1, NULL),
('SLBN-Gampaha-01', 'SLBN', '10.0.2.1', 'UP', 'High', 3, NULL),
('CEAN-Colombo-Central-01', 'CEAN', '10.1.1.1', 'UP', 'High', 1, 2),
('CEAN-Colombo-North-01', 'CEAN', '10.1.2.1', 'UP', 'High', 2, 2),
('CEAN-Gampaha-01', 'CEAN', '10.2.1.1', 'UP', 'Avg', 3, 3),
('MSAN-Colombo-A1', 'MSAN', '10.10.1.1', 'UP', 'Avg', 1, 2),
('MSAN-Colombo-A2', 'MSAN', '10.10.1.2', 'UP', 'Low', 1, 2),
('MSAN-Gampaha-A1', 'MSAN', '10.20.1.1', 'UP', 'Avg', 3, 3);

-- Device Links (Topology: SLBN → CEAN → MSAN)
INSERT INTO DeviceLink (ParentDeviceId, ChildDeviceId, LinkStatus) VALUES 
(1, 3, 'UP'),
(1, 4, 'UP'),
(2, 5, 'UP'),
(3, 6, 'UP'),
(3, 7, 'UP'),
(5, 8, 'UP');

-- Verify data
SELECT * FROM Region;
SELECT * FROM Province;
SELECT * FROM LEA;
SELECT * FROM Device;
SELECT * FROM DeviceLink;

ALTER TABLE Device
ADD Latitude DECIMAL(9,6) NULL,
    Longitude DECIMAL(9,6) NULL;


-- SLBN - Colombo
UPDATE Device SET Latitude = 6.9271, Longitude = 79.8612 WHERE DeviceId = 1;

-- SLBN - Gampaha
UPDATE Device SET Latitude = 7.0917, Longitude = 79.9992 WHERE DeviceId = 2;

-- CEAN - Colombo Central (slight offset)
UPDATE Device SET Latitude = 6.9300, Longitude = 79.8650 WHERE DeviceId = 3;

-- CEAN - Colombo North
UPDATE Device SET Latitude = 6.9500, Longitude = 79.8700 WHERE DeviceId = 4;

-- CEAN - Gampaha
UPDATE Device SET Latitude = 7.0950, Longitude = 80.0020 WHERE DeviceId = 5;

-- MSAN - Colombo A1 
UPDATE Device SET Latitude = 6.9285, Longitude = 79.8625 WHERE DeviceId = 6;

-- MSAN - Colombo A2
UPDATE Device SET Latitude = 6.9260, Longitude = 79.8595 WHERE DeviceId = 7;

-- MSAN - Gampaha A1
UPDATE Device SET Latitude = 7.0925, Longitude = 79.9975 WHERE DeviceId = 8;



---- || TEST SCENARIO FOR MAP || ----

-- 🔁 Reset
DELETE FROM ImpactedDevice;
DELETE FROM RootCause;
DELETE FROM Alarm;

UPDATE Device
SET Status = 'UP';


-- 🔴 Step 1: Root failure
UPDATE Device
SET Status = 'DOWN'
WHERE DeviceName = 'SLBN-Colombo-01';


-- 🚨 Step 2: Insert Alarm (FIXED)
INSERT INTO Alarm (DeviceId, AlarmType)
VALUES (
    (SELECT DeviceId FROM Device WHERE DeviceName = 'SLBN-Colombo-01'),
    'SIMULATION'
);


-- 🧠 Step 3: Insert RootCause (NOW AlarmId will exist)
INSERT INTO RootCause (AlarmId, RootCauseDeviceId, RootCauseType, DetectedTime)
VALUES (
    (SELECT TOP 1 AlarmId FROM Alarm ORDER BY AlarmId DESC),
    (SELECT DeviceId FROM Device WHERE DeviceName = 'SLBN-Colombo-01'),
    'SIMULATION',
    GETDATE()
);


-- 🟡 Step 4: Insert Impacted Devices
INSERT INTO ImpactedDevice (DeviceId, RootCauseId, ImpactType)
SELECT DeviceId,
       (SELECT TOP 1 RootCauseId FROM RootCause ORDER BY RootCauseId DESC),
       'DOWNSTREAM'  -- or 'IMPACTED'
FROM Device
WHERE DeviceName IN (
    'CEAN-Colombo-Central-01',
    'CEAN-Colombo-North-01',
    'MSAN-Colombo-A1',
    'MSAN-Colombo-A2'
);

