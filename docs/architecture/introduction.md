# 1. Introduction

## Starter Template or Existing Project

**N/A - Greenfield Project**

This is a greenfield project with no existing starter template or codebase. The PRD explicitly defines a **custom monorepo structure** (`/backend` and `/frontend` folders) with Node.js/Express backend and React frontend, deployed as a unified full-stack application.

**Rationale for Not Using a Starter:**
- The PRD specifies custom requirements (multi-tenancy via `dealershipId`, Cloudinary integration, specific monorepo structure) that don't align well with opinionated starters
- The 48-hour timeline benefits from minimal boilerplate - using Create React App/Vite for frontend and vanilla Express for backend is faster than learning/configuring a fullstack starter
- Free tier constraints (Railway/Render) work best with simple, standard deployments rather than framework-specific configurations

**Alternative Starters Considered (but not recommended):**
- **T3 Stack** (Next.js + tRPC + Tailwind + Prisma): Great for type-safety but adds learning curve for tRPC/Prisma; PRD already specifies REST API
- **MERN Starters**: Most assume separate frontend/backend hosting; PRD requires unified deployment
- **Turborepo/Nx Monorepo Templates**: Overkill for 2-package monorepo; adds complexity without benefit

## Introduction Content

This document outlines the complete fullstack architecture for **JealPrototypeTest - Multi-Dealership Car Website + CMS Platform**, including backend systems, frontend implementation, and their integration. It serves as the single source of truth for AI-driven development, ensuring consistency across the entire technology stack.

This unified approach combines what would traditionally be separate backend and frontend architecture documents, streamlining the development process for a modern fullstack application where these concerns are tightly integrated. The architecture is optimized for a **48-hour development timeline** while maintaining production viability for small-to-medium dealership deployments.

**Key Architectural Principles:**
- **Multi-tenancy from Day 1:** `dealershipId`-based data isolation enabling multiple independent dealerships
- **Zero-cost deployment:** Entire stack runs on free tiers (Railway/Render + PostgreSQL + Cloudinary)
- **Monolith simplicity:** Single codebase, single deployment, unified backend serving frontend
- **Rapid iteration:** Technology choices prioritize speed of implementation and proven reliability

## Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-11-19 | 1.0 | Initial architecture document | Winston (Architect Agent) |

---
