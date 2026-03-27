# Alarm Management System

A web-based system designed to model and simulate fault detection, alarm handling, and impact analysis within a telecom service provider’s network.

## Overview

This project represents a hierarchical telecom topology and enables realistic simulation of:
- Power-related alarms
- Node failures
- Downstream network impact analysis

It demonstrates how modern Network Operations Centers (NOC) monitor distributed infrastructure, identify and classify faults, and assess service disruption impact using a centralized system.

> Note: This is a simulation framework and does not interact with real telecom hardware.

## Repository StructureS

- `Backend/` - ASP.NET solution and API, application services, domain models, and repositories
- `Frontend/` - Angular client UI and app logic

## Development

### Backend
1. Open `Backend/INMS.sln` in Visual Studio.
2. Restore NuGet packages and build the solution.
3. Run `INMS.API`.

### Frontend
1. Navigate to `Frontend/`.
2. Run `npm install` if needed.
3. Run `npm start`.

## Common Commands

From `Backend/`:
```bash
dotnet build
``` 

From `Frontend/`:
```bash
npm install
npm start
```

## Branching

Use feature branches like `YB/DTO/F2` to keep work isolated. Merge only after code review and tests pass.

