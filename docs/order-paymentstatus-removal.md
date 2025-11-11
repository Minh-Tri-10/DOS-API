# Removing PaymentStatus from Orders

This document records the refactor that removes the legacy PaymentStatus column from the DOSOrderDb.dbo.Orders table. Payment tracking now lives exclusively inside the Payment service.

## What changed
- Order entities, DTOs, repositories, stats, and MVC views no longer read or write PaymentStatus.
- Admin/customer experiences now display the order status only; payment insights live in the Payments pages.
- Order statistics compute revenue based on OrderStatus values (paid, completed, confirmed, processing, delivering, shipped).

## Database update
1. Back up DOSOrderDb.
2. Execute docs/sql/remove-order-paymentstatus.sql against the order database. The script safely drops any default constraint before removing the column.
3. No data backfill is required because the canonical payment state already exists in DOSPaymentDb.

## Deploy / config checklist
- Deploy OrderAPI, MVCApplication, and any other services that reference OrderDto so they pick up the new contracts.
- Clear stale OData metadata caches if you have long-lived API gateway instances.
- Redeploy reporting jobs that might have cached column projections.

## Validation
- Run dotnet build DOS.sln to ensure all projects compile.
- Smoke test the following flows:
  - Create an order (pending status) and ensure pages render.
  - Complete a payment and confirm the order status updates to paid (via the VNPay callback path if available).
  - Load Admin Orders list + filters, Admin Order details, and the customer Orders list.
  - Hit /odata/Orders and /api/order/{id} to ensure serialized payloads no longer contain PaymentStatus.

## Rollback
If you must roll back, restore the DB column from backups (or re-add it with ALTER TABLE dbo.Orders ADD PaymentStatus NVARCHAR(20) NULL) **before** redeploying older builds.
