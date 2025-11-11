USE DOSOrderDb;
GO

DECLARE @constraintName sysname;
SELECT @constraintName = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON c.default_object_id = dc.object_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Orders')
  AND c.name = 'PaymentStatus';

IF @constraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Orders DROP CONSTRAINT ' + QUOTENAME(@constraintName));
END;

IF COL_LENGTH('dbo.Orders','PaymentStatus') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Orders DROP COLUMN PaymentStatus;
END;
GO
