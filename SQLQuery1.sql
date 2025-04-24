WITH t1 AS (
    SELECT DISTINCT FileCryptContainerId
    FROM FileCryptScrapedLinks
    WHERE FileName IS NULL and Status = 'Offline'
)
SELECT 
    *
FROM 
    FileCryptScrapedLinks f
INNER JOIN 
    t1 ON f.FileCryptContainerId = t1.FileCryptContainerId;
