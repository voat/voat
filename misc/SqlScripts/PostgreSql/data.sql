INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 1, TRUE, NULL, NULL, -1000, 'Spam', 'Content violates spam guidelines', 'Voat', now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 1);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 2, TRUE, NULL, NULL, -900, 'Dox', 'Content contains personal information that relates to a users real world or online identity', 'Voat', now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 2);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 3, TRUE, NULL, NULL, -800, 'Illegal', 'Content contains or links to content that is illegal', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 3);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 4, FALSE, NULL, NULL, -700, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 4);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 5, FALSE, NULL, NULL, -600, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 5);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 6, FALSE, NULL, NULL, -500, 'Placeholder', 'Placeholder', 'Voat', now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 6);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 7, FALSE, NULL, NULL, -400, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 7);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 8, FALSE, NULL, NULL, -300, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 8);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 9, FALSE, NULL, NULL, -200, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 9);

INSERT INTO "dbo"."RuleSet" ("ID", "IsActive", "Subverse", "ContentType", "SortOrder", "Name", "Description", "CreatedBy", "CreationDate")
SELECT 10, FALSE, NULL, NULL, -100, 'Placeholder', 'Placeholder', 'Voat',  now()
WHERE NOT EXISTS (SELECT * FROM "dbo"."RuleSet" WHERE "ID" = 10);

select setval('"dbo"."ruleset_id_seq"',(select max("ID") from "dbo"."RuleSet")::bigint);