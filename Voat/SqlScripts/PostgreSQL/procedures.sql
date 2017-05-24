DROP FUNCTION IF EXISTS "dbo"."usp_CommentTree"(int, int, int);

CREATE OR REPLACE FUNCTION "dbo"."usp_CommentTree"
	(
        SubmissionID INT,
		Depth INT,
		ParentID INT
    )
    
    /*
    
	SELECT * FROM "dbo"."usp_CommentTree"(1, NULL, NULL)

    */
	RETURNS TABLE (
        "ChildCount" INT,
        "Depth" INT,
        "Path" TEXT, 
        "Subverse" VARCHAR, 
        "ID" INT,
        "UserName" VARCHAR,
        "Content" VARCHAR,
        "CreationDate" TIMESTAMP,
        "LastEditDate" TIMESTAMP,
        "SubmissionID" INT,
        "UpCount" BIGINT,
        "DownCount" BIGINT,
        "ParentID" INT,
        "IsAnonymized" BOOLEAN,
        "IsDistinguished" BOOLEAN,
        "FormattedContent" VARCHAR,
    	"IsDeleted" BOOLEAN
    )
AS $$

BEGIN

    CREATE TEMP TABLE tree (
       "SubmissionID" INT,
       "UserName" VARCHAR,
       "ParentID" INT,
       "ChildID" INT
    );

    IF ParentID IS NULL THEN
        INSERT INTO tree
        SELECT p."SubmissionID", p."UserName", p."ID", 0 FROM "dbo"."Comment" AS p 
        WHERE 
            1 = 1
            AND p."ParentID" IS NULL
            AND p."SubmissionID" = SubmissionID
        UNION ALL
        SELECT c."SubmissionID", c."UserName", c."ParentID", c."ID" FROM "dbo"."Comment" AS c
        WHERE
            1 = 1 
            AND c."ParentID" IS NOT NULL
            AND c."SubmissionID" = SubmissionID;
    ELSE
        INSERT INTO tree
        SELECT p."SubmissionID", p."UserName", p."ID", 0 FROM "dbo"."Comment" AS p 
        WHERE 
            1 = 1
            AND p."ParentID" = ParentID
            AND p."SubmissionID" = SubmissionID
        UNION ALL
        SELECT c."SubmissionID", c."UserName", c."ParentID", c."ID" FROM "dbo"."Comment" AS c
        WHERE
            1 = 1
            AND c."ParentID" IS NOT NULL 
            AND c."ParentID" > ParentID
            AND c."SubmissionID" = SubmissionID;
    END IF;

    RETURN QUERY WITH RECURSIVE comment_hierarchy
         AS (
            SELECT 
                t."SubmissionID",
                t."UserName",
                t."ParentID" AS "RootID", 
                CASE WHEN t."ChildID" = 0 THEN 0 ELSE 1 END AS "Depth", 
                CONCAT(CAST(t."ParentID" AS VARCHAR), CASE WHEN t."ChildID" != 0 THEN CONCAT('\\', CAST(t."ChildID" AS VARCHAR)) ELSE '' END) AS "Path",
                t."ChildID",
                t."ParentID" 
            FROM tree AS t
            WHERE NOT t."ParentID" IN (SELECT "ChildID" FROM tree)
            UNION ALL
                SELECT
                    P."SubmissionID", 
                    C."UserName",
                    P."RootID",
                    P."Depth" + 1,
                    CONCAT(P."Path", '\\', CAST(C."ChildID" AS VARCHAR)),
                    C."ChildID",
                    C."ParentID"
                FROM comment_hierarchy AS P
                INNER JOIN tree AS C ON P."ChildID" = C."ParentID"
           )
           
    SELECT 
        (
            SELECT CAST(COUNT(*) AS INT) FROM comment_hierarchy AS t
            WHERE
            1 = 1 
            AND c."ID" = t."ParentID" 
            AND t."ChildID" <> 0
        ) AS "ChildCount", 
        --h.*,
        h."Depth",
        h."Path",
        m."Subverse",
        c.*
    FROM comment_hierarchy AS h
    INNER JOIN "dbo"."Comment" AS c ON (c."ID" = CASE WHEN h."ChildID" IS NULL OR h."ChildID" = 0 THEN h."ParentID" ELSE h."ChildID" END)
    INNER JOIN "dbo"."Submission" AS m ON (c."SubmissionID" = m."ID")
    WHERE 
        (h."Depth" <= Depth OR Depth IS NULL);
END;

$$ LANGUAGE 'plpgsql'