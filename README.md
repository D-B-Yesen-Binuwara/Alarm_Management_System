<<<<<<< HEAD
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

=======
# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.
>>>>>>> thulasi/login-register
