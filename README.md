whoaverse
=========

The code that powers whoaverse.com  http://www.whoaverse.com/v/whoaversedev

### Description
Whoaverse is a Reddit®-like community platform developed in C# ASP.NET MVC 5. 

### Installation/Dependencies
Needs MS SQL server and .NET framework 4.5 but other DB servers should work without much modification.

Whoaverse uses Markdown (https://code.google.com/p/markdownsharp/), Bootstrap (http://getbootstrap.com/), jQuery (http://jquery.com/). 

"Production" version of Whoaverse is running at whoaverse.com (early alpha) on Windows Server 2008 R2 with SQL Server Express.

### How can I run/use it?
Follow these steps to get up and running in 5 minutes:

#### step 1
- Create databases and database owners.
Whoaverse uses 2 SQL databases to store messages, comments, votes, users etc. 
Default database names are whoaverse and whoaverse_users.
You can use whoaverse.sql and whoaverse_user.sql to generate necessary tables for each respective database.

#### step 2
- Modify and place Web.config file
After cloning this repository, you will need to modify and place Web.config file in Whoaverse folder (the same folder where the file packages.config is located). You need to modify the following 2 connection strings in this file to reflect your SQL server address, port, database names and database usernames: 
DefaultConnection and whoaverseEntities

#### step 3
- Reinstall dependencies (binaries for NuGet packages) by issuing the following command in Package Manager Console (when asked to overwrite existing files, choose no for all:
Update-Package -Reinstall

#### step 4
- Remove file GrowthUtility.cs from the project by right-clicking on it (it can be found in Utilities folder) and selecting delete.

#### step 5
- Comment out the following line of code in the method "public async Task<ActionResult> Submit" in file HomeController.cs:
```c#
message.Name=GrowthUtility.GetRandomUsername();
```

### Why was this made?
This was just a hobby project to help me get a better understanding of C# and ASP.NET MVC and Entity Framework.

### How does it differ from related projects?
This is the only C# implementation of a Reddit®-like community as far as I know.

### What does the future hold?
Whoaverse aims to make a fully functional and scalable Reddit®-like community including all the kinks and features which today exist on reddit.com. 

### Contributing
There is a whole lot of work to be done, code contributions are welcome. A Contributor License Agreement (CLA) is required for all code contributions, configuration changes, documentation, or any other materials that you send to us.
CLA form can be signed and submitted at http://whoaverse.com/cla
