-- Voat PostgreSQL setup — runs the whole ordered migration set with one command:
--
--     psql -h localhost -U voat -d voat -f setup.sql
--
-- (Any PostgreSQL client works; psql is used here because \ir includes the
--  sibling scripts relative to this file's location.)
--
-- Order matters: schema (voat.sql) -> identity schema (voat_users.sql) ->
-- functions (procedures.sql) -> base data (data.sql) -> seed (seed.sql).
--
-- The set is idempotent: re-running it against an existing database is safe.
--
-- Note: voat.sql runs "CREATE EXTENSION IF NOT EXISTS citext". citext is a
-- trusted extension on PostgreSQL 13+, so the database owner can create it;
-- otherwise run setup as a superuser.

\ir voat.sql
\ir voat_users.sql
\ir procedures.sql
\ir data.sql
\ir seed.sql
