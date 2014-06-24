# whoaverse

<img height="200" width="200" src="http://whoaverse.com/Graphics/whoaverse-mascot.png"
 alt="Whoaverse mascot" title="Whoaverse" align="right" />

The code that powers http://www.whoaverse.com

### Description
Whoaverse is a Reddit®-like community platform developed in C# ASP.NET MVC 5. 
Early alpha version of Whoaverse is running at http://whoaverse.com on Windows Server 2008 R2 with SQL Server Express.

### Dependencies and attributions
Whoaverse uses MS SQL server and .NET framework 4.5 but other DB servers should work without much modification.

The following 3rd party libraries/extensions are used:

- Markdowndeep (http://www.toptensoftware.com/markdowndeep/)
- Bootstrap (http://getbootstrap.com/)
- jQuery (http://jquery.com/)
- Recaptcha for .NET library (http://recaptchanet.codeplex.com/)
- Entity Framework (https://entityframework.codeplex.com/)

### Installation instructions
Follow these steps to get up and running in 5 minutes:

#### step 1
- Create databases and database owners.
Whoaverse uses 2 SQL databases to store messages, comments, votes, users etc. 
Default database names are whoaverse and whoaverse_users.
You can use whoaverse.sql and whoaverse_user.sql to generate necessary tables for each respective database.

#### step 2
- After cloning this repository, you will need to modify and place Web.config file in Whoaverse folder (the same folder where the file packages.config is located). You need to modify the following 2 connection strings in this file to reflect your SQL server address, port, database names and database usernames: 
DefaultConnection and whoaverseEntities

#### step 3
- Reinstall dependencies (binaries for NuGet packages) by issuing the following command in Package Manager Console (when asked to overwrite existing files, choose no for all:
Update-Package -Reinstall

### After installation
Start by creating your user account. The frontpage will be empty, so you should start by creating a subverse.
After creating your subverse, you can visit it (localhost/v/yourtestsubverse) and start posting stories or links. You can now comment on the new stories and vote on them.

### Why was this made?
This was just a hobby project to help me get a better understanding of C# and ASP.NET MVC and Entity Framework.

### How does it differ from related projects?
Whoaverse has increased focus on users privacy. It enables users to delete their account by automatically overwriting every comment and every submission the user has made with a string "deleted", before proceeding to remove the user account from user credentials database. Furthermore, whoaverse is the only active C# implementation of a Reddit®-like community as far as I know.

### What does the future hold?
Whoaverse aims to make a fully functional and scalable Reddit®-like community including all the kinks and features which exist on reddit.com today. 

### Contributing
There is a whole lot of work to be done, code contributions are welcome. A Contributor License Agreement (CLA) is required for all code contributions, configuration changes, documentation, or any other materials that you send to us.
CLA form can be signed and submitted at http://whoaverse.com/cla
