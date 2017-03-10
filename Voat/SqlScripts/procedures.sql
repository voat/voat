-- VOAT PROCS 
-- This file contains procs that Voat uses

-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
IF NOT EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_CommentTree]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN

	EXEC(N'CREATE PROC [dbo].[usp_CommentTree]
	AS 
	SELECT 1')	
END

GO

ALTER PROCEDURE [dbo].[usp_CommentTree]
	@SubmissionID INT,
	@Depth INT = NULL,
	@ParentID INT = NULL
AS

/*
I spent 14 hours working on this. Not. Even. Joking. 
Man, <**censored**> SQL. 
For at *least* two hours I was just randomly changing 
things in the hopes that I would accidently fix it.
*/

DECLARE @tree TABLE (
   SubmissionID INT,
   UserName NVARCHAR(30),
   ParentID INT,
   ChildID INT
);

IF @ParentID IS NULL 
	INSERT INTO @tree
	SELECT p.SubmissionID, p.UserName, ID, 0 FROM Comment p 
	WHERE 
		1 = 1
		AND p.ParentID IS NULL
		AND p.SubmissionID = @SubmissionID
	UNION ALL
	SELECT c.SubmissionID, c.UserName, c.ParentID, c.ID FROM Comment c
	WHERE
		1 = 1 
		AND c.ParentID IS NOT NULL
		AND c.SubmissionID = @SubmissionID
ELSE 
	INSERT INTO @tree
	SELECT p.SubmissionID, p.UserName, ID, 0 FROM Comment p 
	WHERE 
		1 = 1
		AND p.ParentID = @ParentID
		AND p.SubmissionID = @SubmissionID
	UNION ALL
	SELECT c.SubmissionID, c.UserName, c.ParentID, c.ID FROM Comment c
	WHERE
		1 = 1
		AND c.ParentID IS NOT NULL 
		AND c.ParentID > @ParentID
		AND c.SubmissionID = @SubmissionID

;WITH CommentHierarchy
     AS (
		SELECT 
			SubmissionID,
			UserName,
			RootID = ParentID, 
			[Depth] = CASE WHEN ChildID = 0 THEN 0 ELSE 1 END, 
			[Path] = CAST(ParentID AS VARCHAR(MAX)) + CASE WHEN ChildID != 0 THEN '\' + CAST(ChildID AS VARCHAR(MAX)) ELSE '' END,
			ChildID = ChildID ,
			ParentID = ParentID 
        FROM @tree
        WHERE NOT ParentID IN (SELECT ChildID FROM @tree)
        UNION ALL
			SELECT
				P.SubmissionID, 
				C.UserName,
				P.RootID,
				P.[Depth] + 1 ,
				P.[Path] + '\' + CAST(C.ChildID AS VARCHAR(MAX)) ,
				C.ChildID,
				C.ParentID
			FROM CommentHierarchy P
			INNER JOIN @tree C ON P.ChildID = C.ParentID
       )
SELECT 
	ChildCount = (
			SELECT COUNT(*)  FROM CommentHierarchy 
			WHERE
				1 = 1 
				AND c.ID = ParentID 
				AND ChildID != 0
		), 
	--h.*,
	h.Depth,
	h.Path,
	m.Subverse,
	c.*
FROM CommentHierarchy h
INNER JOIN Comment c ON (c.ID = CASE WHEN ChildID IS NULL OR ChildID = 0 THEN h.ParentID ELSE ChildID END)
INNER JOIN Submission m ON (c.SubmissionID = m.ID)
WHERE 
	([Depth] <= @Depth OR @Depth IS NULL)
OPTION (MAXRECURSION 1000)

--ORDER BY ID, Depth, ParentID
	--AND (h.RootID = @ParentID OR @ParentID IS NULL)

/*
	LOAD ALL COMMENTS FOR SUBMISSION
	usp_CommentTree 2441
	, NULL, 6116
	usp_CommentTree 2510, 1, 7407
	usp_CommentTree 2510, 1, 7408
	usp_CommentTree 2510, 1, 7409
		usp_CommentTree 2441, NULL, 6177
	usp_CommentTree 2441, 1, 6116
	usp_CommentTree 2441, NULL, 6113
	usp_CommentTree 2441, NULL, 6287
	SELECT p.MessageID, ID, 0 FROM Comments p 
	WHERE 
		p.ID = 6177
	UNION ALL

	SELECT c.MessageID, c.ParentID, c.ID FROM Comments c
	WHERE
		c.MessageID = 2441
		AND
		c.ParentID != 6177
*/

GO

-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
IF NOT EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_Reports_UserVoteGivenStats]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN

	EXEC (N'CREATE PROC [dbo].[usp_Reports_UserVoteGivenStats]
	AS 
	SELECT 1')
END 

GO

ALTER PROC [dbo].[usp_Reports_UserVoteGivenStats]

	@BeginDate DATETIME,
	@EndDate DATETIME,
	@RecordCount INT = 5
AS


/*

 usp_Reports_UserVoteGivenStats '1/1/2017', '1/8/2017'

*/

DECLARE @Output TABLE (  
    ContentType INT NOT NULL,  
    VoteType INT,  
    UserName NVARCHAR(50),
	TotalCount INT);  

INSERT @Output
SELECT ContentType = 1, VoteType = 1, x.UserName, TotalCount = COUNT(*) FROM SubmissionVoteTracker x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND VoteStatus = 1
GROUP BY x.UserName
ORDER BY COUNT(*) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 1, VoteType = -1, x.UserName, TotalCount = COUNT(*) FROM SubmissionVoteTracker x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND VoteStatus = -1
GROUP BY x.UserName
ORDER BY COUNT(*) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 2, VoteType = 1, x.UserName, TotalCount = COUNT(*) FROM CommentVoteTracker x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND VoteStatus = 1
GROUP BY x.UserName
ORDER BY COUNT(*) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 2, VoteType = -1, x.UserName, TotalCount = COUNT(*) FROM CommentVoteTracker x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND VoteStatus = -1
GROUP BY x.UserName
ORDER BY COUNT(*) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 


SELECT * FROM @Output

GO

-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
IF NOT EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_Reports_HighestVotedContent]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    
	EXEC (N'CREATE PROC [dbo].[usp_Reports_HighestVotedContent]
	AS 
	SELECT 1')
	
END
GO

ALTER PROCEDURE [dbo].[usp_Reports_HighestVotedContent]

	@BeginDate DATETIME,
	@EndDate DATETIME,
	@RecordCount INT = 5
AS


/*

 usp_Reports_HighestVotedContent '1/1/2017', '1/8/2017'

*/

DECLARE @Output TABLE (  
    ContentType INT NOT NULL,  
    VoteType INT,  
    ID INT);  

INSERT @Output
SELECT ContentType = 1, VoteType = 1, x.ID 
 -- , * 
FROM Submission x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 AND x.IsDeleted = 0
ORDER BY x.UpCount DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 2, VoteType = 1, x.ID
-- , * 
FROM Comment x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 AND x.IsDeleted = 0
ORDER BY x.UpCount DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 1, VoteType = -1, x.ID
-- , * 
FROM Submission x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 AND x.IsDeleted = 0
ORDER BY x.DownCount DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

INSERT @Output
SELECT ContentType = 2, VoteType = -1, x.ID
-- , * 
FROM Comment x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 AND x.IsDeleted = 0
ORDER BY x.DownCount DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

SELECT * FROM @Output

GO


-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
IF NOT EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[usp_Reports_UserVoteReceivedStats]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    EXEC(N'CREATE PROC [dbo].[usp_Reports_UserVoteReceivedStats]
	AS 
	SELECT 1')	
END

GO

ALTER PROCEDURE [dbo].[usp_Reports_UserVoteReceivedStats]

	@BeginDate DATETIME,
	@EndDate DATETIME,
	@RecordCount INT = 5
AS

/*

 usp_Reports_UserVoteReceivedStats '1/1/2017', '1/8/2017'

*/

DECLARE @Output TABLE (  
    ContentType INT NOT NULL,  
    VoteType INT,  
    UserName NVARCHAR(50),
	AvgVotes FLOAT,
	TotalVotes INT,
	TotalCount INT);  


--- SUBMISSION AVG ---

-- Top 
INSERT @Output
SELECT ContentType = 1, VoteType = 1, x.UserName, AvgVotes = SUM(x.UpCount) / CAST(COUNT(x.ID) AS float), TotalVotes = SUM(x.UpCount), TotalCount = COUNT(x.ID) FROM Submission x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 
GROUP BY x.UserName
ORDER BY SUM(x.UpCount) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

--Bottom
INSERT @Output
SELECT ContentType = 1, VoteType = -1, x.UserName, AvgVotes = SUM(x.DownCount) / CAST(COUNT(x.ID) AS float), TotalVotes = SUM(x.DownCount), TotalCount = COUNT(x.ID)  FROM Submission x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 
GROUP BY x.UserName
ORDER BY SUM(x.DownCount) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 


--- COMMENTS ---

-- Top 
INSERT @Output
SELECT ContentType = 2, VoteType = 1, x.UserName, AvgVotes = SUM(x.UpCount) / CAST(COUNT(x.ID) AS float), TotalVotes = SUM(x.UpCount), TotalCount = COUNT(x.ID)  FROM Comment x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 
GROUP BY x.UserName
ORDER BY SUM(x.UpCount) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY

INSERT @Output
SELECT ContentType = 2, VoteType = -1, x.UserName, AvgVotes = SUM(x.DownCount) / CAST(COUNT(x.ID) AS float), TotalVotes = SUM(x.DownCount), TotalCount = COUNT(x.ID)  FROM Comment x
WHERE 
	x.CreationDate > @BeginDate AND x.CreationDate < @EndDate
	AND x.IsAnonymized = 0 
GROUP BY x.UserName
ORDER BY SUM(x.DownCount) DESC
OFFSET 0 ROWS 
FETCH NEXT @RecordCount ROWS ONLY 

SELECT * FROM @Output

