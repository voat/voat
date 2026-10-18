-- Voat first-boot database initialization (Docker entrypoint).
--
-- The postgres image runs every file in /docker-entrypoint-initdb.d exactly
-- once, when the data volume is empty. This wrapper pulls in the ordered
-- migration set (schema -> identity -> functions -> base data -> seed) from
-- the read-only script mount at /voat-sql.
--
-- setup.sql is idempotent: to upgrade an existing database manually, run
--     docker compose exec db psql -U voat -d voat -f /voat-sql/setup.sql

\ir /voat-sql/setup.sql
