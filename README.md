# Quick Status Updates

This branch is fully building on .NET Core 2 Preview and a majority of unit tests and UI are ported and working.

Right now we are concentrating on getting 100% unit test coverage. 

# Voat Branch for .NET Core Port (With the PostgreSQL Branch Merged)

Currently using .NET Core 2 Preview and Visual Studio 2017 Preview

To run the tests, run this on the `template1` database first:
```PLpgSQL
CREATE EXTENSION IF NOT EXISTS citext;
```

If you use a SQL tool to navigate to this database, you may have to turn on "Show system objects."
