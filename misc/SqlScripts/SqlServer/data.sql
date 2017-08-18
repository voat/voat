
SET IDENTITY_INSERT "dbo"."RuleSet" ON

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 1)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (1, 1, NULL, NULL, -1000, 'Spam', 'Content violates spam guidelines', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 2)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (2, 1, NULL, NULL, -900, 'Dox', 'Content contains personal information that relates to a users real world or online identity', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 3)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (3, 1, NULL, NULL, -800, 'Illegal', 'Content contains or links to content that is illegal', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 4)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (4, 0, NULL, NULL, -700, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 5)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (5, 0, NULL, NULL, -600, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 6)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (6, 0, NULL, NULL, -500, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 7)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (7, 0, NULL, NULL, -400, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 8)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (8, 0, NULL, NULL, -300, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 9)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (9, 0, NULL, NULL, -200, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

IF NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 10)
	INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
	VALUES (10, 0, NULL, NULL, -100, 'Placeholder', 'Placeholder', 'Voat', GETUTCDATE())

SET IDENTITY_INSERT "dbo"."RuleSet" OFF