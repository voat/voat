DROP FUNCTION IF EXISTS "usp_CommentTree"(int, int, int);

CREATE OR REPLACE FUNCTION "usp_CommentTree"
	(
        SubmissionID INT,
		Depth INT,
		ParentID INT
    )
    
    /*
    
	SELECT * FROM "usp_CommentTree"(1, NULL, NULL)

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
        "UpCount" INT,
        "DownCount" INT,
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
        SELECT p."SubmissionID", p."UserName", p."ID", 0 FROM "Comment" AS p 
        WHERE 
            1 = 1
            AND p."ParentID" IS NULL
            AND p."SubmissionID" = SubmissionID
        UNION ALL
        SELECT c."SubmissionID", c."UserName", c."ParentID", c."ID" FROM "Comment" AS c
        WHERE
            1 = 1 
            AND c."ParentID" IS NOT NULL
            AND c."SubmissionID" = SubmissionID;
    ELSE
        INSERT INTO tree
        SELECT p."SubmissionID", p."UserName", p."ID", 0 FROM "Comment" AS p 
        WHERE 
            1 = 1
            AND p."ParentID" = ParentID
            AND p."SubmissionID" = SubmissionID
        UNION ALL
        SELECT c."SubmissionID", c."UserName", c."ParentID", c."ID" FROM "Comment" AS c
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
    INNER JOIN "Comment" AS c ON (c."ID" = CASE WHEN h."ChildID" IS NULL OR h."ChildID" = 0 THEN h."ParentID" ELSE h."ChildID" END)
    INNER JOIN "Submission" AS m ON (c."SubmissionID" = m."ID")
    WHERE 
        (h."Depth" <= Depth OR Depth IS NULL);
END;
$$ LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION "usp_Reports_UserVoteGivenStats"
(
	"BeginDate" TIMESTAMP,
	"EndDate" TIMESTAMP,
	"RecordCount" INT = 5
)
RETURNS TABLE
(
	"ContentType" INT,  
	"VoteType" INT,  
	"UserName" VARCHAR(50),
	"TotalCount" BIGINT
)
AS $$
BEGIN
	RETURN QUERY (
		(SELECT 1 AS "ContentType", 1 AS "VoteType", x."UserName", COUNT(*) AS "TotalCount" FROM "SubmissionVoteTracker" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND "VoteStatus" = 1
		GROUP BY x."UserName"
		ORDER BY COUNT(*) DESC
		LIMIT "RecordCount" OFFSET 0)

		UNION ALL

		(SELECT 1 AS "ContentType", -1 AS "VoteType", x."UserName", COUNT(*) AS "TotalCount" FROM "SubmissionVoteTracker" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND "VoteStatus" = -1
		GROUP BY x."UserName"
		ORDER BY COUNT(*) DESC
		LIMIT "RecordCount" OFFSET 0)

		UNION ALL

		(SELECT 2 AS "ContentType", 1 AS "VoteType", x."UserName", COUNT(*) AS "TotalCount" FROM "CommentVoteTracker" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND "VoteStatus" = 1
		GROUP BY x."UserName"
		ORDER BY COUNT(*) DESC
		LIMIT "RecordCount" OFFSET 0)

		UNION ALL

		(SELECT 2 AS "ContentType", -1 AS "VoteType", x."UserName", COUNT(*) AS "TotalCount" FROM "CommentVoteTracker" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND "VoteStatus" = -1
		GROUP BY x."UserName"
		ORDER BY COUNT(*) DESC
		LIMIT "RecordCount" OFFSET 0)
	);
END;
$$ LANGUAGE 'plpgsql';




CREATE OR REPLACE FUNCTION "usp_Reports_HighestVotedContent"
(
	"BeginDate" TIMESTAMP,
	"EndDate" TIMESTAMP,
	"RecordCount" INT = 5
)
RETURNS TABLE
(
	"ContentType" INT,
	"VoteType" INT,
	"ID" INT
)
AS $$
BEGIN
	RETURN QUERY (
		(SELECT 1 AS "ContentType", 1 AS "VoteType", x."ID"
		FROM "Submission" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False AND x."IsDeleted" = False
		ORDER BY x."UpCount" DESC
		LIMIT "RecordCount" OFFSET 0)
		
		UNION ALL
		
		(SELECT 2 AS "ContentType", 1 AS "VoteType", x."ID" 
		FROM "Comment" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False AND x."IsDeleted" = False
		ORDER BY x."UpCount" DESC
		LIMIT "RecordCount" OFFSET 0)
		
		UNION ALL
		
		(SELECT 1 AS "ContentType", -1 AS VoteType, x."ID"
		FROM "Submission" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False AND x."IsDeleted" = False
		ORDER BY x."DownCount" DESC
		LIMIT "RecordCount" OFFSET 0)
		
		UNION ALL
		
		(SELECT 2 AS "ContentType", -1 AS "VoteType", x."ID"
		FROM "Comment" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False AND x."IsDeleted" = False
		ORDER BY x."DownCount" DESC
		LIMIT "RecordCount" OFFSET 0)
	);
END;
$$ LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION "usp_Reports_UserVoteReceivedStats"
(
	"BeginDate" TIMESTAMP,
	"EndDate" TIMESTAMP,
	"RecordCount" INT = 5
)
RETURNS TABLE
(
	"ContentType" INT,
	"VoteType" INT,
	"UserName" VARCHAR(50),
	"AvgVotes" FLOAT,
	"TotalVotes" INT,
	"TotalCount" INT
)
AS $$
BEGIN
	RETURN QUERY (
		--- SUBMISSION AVG ---
		-- Top 
		(SELECT 1 AS "ContentType", 1 AS "VoteType", x."UserName", SUM(x."UpCount") / CAST(COUNT(x."ID") AS float) AS "AvgVotes", CAST(SUM(x."UpCount") AS INT) AS "TotalVotes", CAST(COUNT(x."ID") AS INT) AS "TotalCount"  FROM "Submission" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False
		GROUP BY x."UserName"
		ORDER BY SUM(x."UpCount") DESC
		LIMIT "RecordCount" OFFSET 0)

		--Bottom
		UNION ALL
		
		(SELECT 1 AS "ContentType", -1 AS "VoteType", x."UserName", SUM(x."DownCount") / CAST(COUNT(x."ID") AS float) AS "AvgVotes", CAST(SUM(x."DownCount") AS INT) AS "TotalVotes", CAST(COUNT(x."ID") AS INT) AS "TotalCount"  FROM "Submission" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False
		GROUP BY x."UserName"
		ORDER BY SUM(x."DownCount") DESC
		LIMIT "RecordCount" OFFSET 0)

		UNION ALL
		--- COMMENTS ---

		-- Top 
		(SELECT 2 AS "ContentType", 1 AS "VoteType", x."UserName", SUM(x."UpCount") / CAST(COUNT(x."ID") AS float) AS "AvgVotes", CAST(SUM(x."UpCount") AS INT) AS "TotalVotes", CAST(COUNT(x."ID") AS INT) AS "TotalCount"  FROM "Comment" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False
		GROUP BY x."UserName"
		ORDER BY SUM(x."UpCount") DESC
		LIMIT "RecordCount" OFFSET 0)

		UNION ALL
		
		(SELECT 2 AS "ContentType", -1 AS "VoteType", x."UserName", SUM(x."DownCount") / CAST(COUNT(x."ID") AS float) AS "AvgVotes", CAST(SUM(x."DownCount") AS INT) AS "TotalVotes", CAST(COUNT(x."ID") AS INT) AS "TotalCount"  FROM "Comment" x
		WHERE 
			x."CreationDate" > "BeginDate" AND x."CreationDate" < "EndDate"
			AND x."IsAnonymized" = False
		GROUP BY x."UserName"
		ORDER BY SUM(x."DownCount") DESC
		LIMIT "RecordCount" OFFSET 0)
	);
END;
$$ LANGUAGE 'plpgsql';




