## 📌 Project Goal

Developed a REST API for receiving vendor invoices, managing the review and approval workflow, generating barcodes, and tracking the physical invoice delivery process. Built with .NET 9, Entity Framework Core, AutoMapper, and Scalar for API documentation.

---

# ✅ Phase 1 - Project Setup

## Project

- [x]  Create ASP.NET Core Web API
- [x]  Setup Git Repository
- [x]  Install EF Core
- [x]  Install SQLite Provider
- [x]  Configure DbContext
- [x]  Create Initial Migration
- [x]  Update Database
- [x]  Create appsettings.json configuration

## Verify

- [x]  API can run
- [x]  Database created successfully

---

# ✅ Phase 2 - Vendor Module

## Database

- [x]  Create Vendor Entity
- [x]  Migration

## DTO

- [x]  Create VendorRequestDto
- [x]  Create VendorResponseDto

## API

- [x]  GET Vendors
- [x]  GET Vendor By Id
- [x]  POST Vendor
- [x]  PUT Vendor
- [x]  DELETE Vendor

## Validation

- [x]  Vendor Name required

---

# ✅ Phase 3 - Authentication (JWT)

## User Entity

- [x]  Create User Entity
- [x]  Password Hash
- [x]  User Role
- [x]  VendorId (nullable)

## JWT

- [x]  Install JWT Package
- [x]  Configure JWT Authentication
- [x]  Configure Authorization

## API

- [x]  Login
- [x]  Register (Optional)

## Authorization

- [x]  Admin Role
- [x]  Vendor Role

## Testing

- [x]  Login Success
- [x]  Access Protected Endpoint
- [x]  Invalid Token
- [x]  Role Authorization

---

# ✅ Phase 4 - Invoice Module

## Database

- [x]  Create Invoice Entity
- [x]  Configure Vendor Relationship
- [x]  Migration

## Status

- [x]  Pending
- [x]  Approved
- [x]  Rejected
- [x]  ReadyToSend
- [x]  Completed

## API

- [x]  GET Invoices
- [x]  GET Invoice Detail
- [x]  POST Invoice
- [x]  PUT Invoice
- [x]  DELETE Invoice

## Business Rules

- [x]  Default Status = Pending

## Testing

- [x]  CRUD Invoice

---

# ✅ Phase 5 - Upload Invoice

## Upload

- [x]  Create Upload Folder
- [x]  Configure Static Files
- [x]  Upload PDF
- [x]  Save File Path

## Validation

- [x]  Only PDF
- [x]  Maximum File Size
- [x]  File Exists

## API

- [x]  Upload Invoice Endpoint

## Testing

- [x]  Upload Success
- [x]  Invalid File

---

# ✅ Phase 6 - Invoice Review

## Database

- [x]  Create InvoiceReview Entity
- [x]  Migration

## API

- [x]  Approve Invoice
- [x]  Reject Invoice

## Business Rules

- [x]  Only Pending Invoice can be Reviewed
- [x]  Save Reviewer
- [x]  Save Review Date
- [x]  Save Remark

## Testing

- [x]  Approve Flow
- [x]  Reject Flow

---

# ✅ Phase 7 - Barcode

## Barcode

- [x]  Create Barcode Service
- [x]  Generate Barcode Number
- [x]  Save Barcode

## API

- [x]  Generate Barcode Endpoint

## Business Rules

- [x]  Only Approved Invoice
- [x]  Barcode Generated Once

## Bonus

- [x]  Barcode Image (ZXing)

---

# ✅ Phase 8 - Send Invoice

## API

- [x]  Mark Ready To Send
- [x]  Mark Completed

## Business Rules

- [x]  Approved → ReadyToSend
- [x]  ReadyToSend → Completed
- [x]  Reject Invalid Status Transition

---

# ✅ Phase 9 - Search & Filter

## Search

- [x]  Invoice Number
- [x]  Vendor
- [x]  Status
- [x]  Date Range

## Pagination

- [x]  Page
- [x]  Page Size

## Sorting

- [x]  Invoice Date
- [x]  Created Date

---

# ✅ Phase 10 - Dashboard

## Statistics

- [ ]  Pending Count
- [ ]  Approved Count
- [ ]  Rejected Count
- [ ]  Completed Count

## Optional

- [ ]  Total Invoice Amount
- [ ]  Monthly Summary

---

# ✅ Phase 11 - Polish

## Validation

- [ ]  FluentValidation (Optional)

## Exception Handling

- [ ]  Global Exception Middleware

## Logging

- [ ]  Request Logging

## Seed Data

- [x]  Admin User
- [x]  Sample Vendors

## Documentation

- [x]  README
- [ ]  API Documentation

---

# 🚀 Bonus Features

- [ ]  Refresh Token
- [ ]  Role Based Authorization
- [ ]  Audit Trail
- [ ]  Soft Delete
- [ ]  Export PDF
- [ ]  Docker
- [ ]  Unit Test
- [ ]  Integration Test
