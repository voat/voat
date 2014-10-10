# WhoaVerse

<img height="200" width="200" src="http://whoaverse.com/Graphics/whoaverse-mascot.png"
 alt="WhoaVerse mascot" title="Whoaverse" align="right" />

This is the code behind http://www.whoaverse.com.

### Description
WhoaVerse is a media aggregator and community platform developed in C# ASP.NET MVC 5.

### Dependencies and attributions
Whoaverse uses SQL server and .NET framework 4.5.

The following 3rd party libraries/extensions are used:

- Markdowndeep (http://www.toptensoftware.com/markdowndeep/)
- Bootstrap (http://getbootstrap.com/)
- jQuery (http://jquery.com/)
- Entity Framework (https://entityframework.codeplex.com/)
- WebApiThrottle (https://github.com/stefanprodan/WebApiThrottle)

### Installation instructions
Follow these steps to get up and running:

#### step 1
- Create databases and database owners.
WhoaVerse uses 2 SQL databases to store messages, comments, votes, users etc. 
Default database names are whoaverse and whoaverse_users.
You can use whoaverse.sql and whoaverse_user.sql to generate necessary tables for each respective database.

#### step 2
- After cloning this repository, you will need to modify and place Web.config file in WhoaVerse folder (the same folder where the file packages.config is located). You need to modify the following 2 connection strings in this file to reflect your SQL server address, port, database names and database usernames: 
DefaultConnection and whoaverseEntities

#### step 3
- Reinstall dependencies (binaries for NuGet packages) by issuing the following command in Package Manager Console (when asked to overwrite existing files, choose no for all:
Update-Package -Reinstall

### After installation
Start by creating your user account. The frontpage will be empty, so you should start by creating a subverse.
After creating your subverse, you can visit it (localhost/v/yourtestsubverse) and start posting stories or links. You can then comment on these stories and vote on them.

### Why was this made?
This was just a hobby project to help me get a better understanding of C# and ASP.NET MVC and Entity Framework.

### How does it differ from related projects?
- WhoaVerse has increased focus on users privacy. It enables users to delete their account by automatically overwriting every comment and every submission the user has made with a string "deleted", before proceeding to remove the user account from user credentials database. 
- built-in night mode
- responsive design which works great on mobile out of the box
- limited voting (new users need to gain a certain amount of points before they are able to vote without restrictions)
- limited number of owned subs per user
- a score bar which graphically shows percentage of upvotes/downvotes
- user profiles which show statistics about user activity, for example, submission distribution and highest-lowest rated submissions
- YouTube-like revenue sharing model (in development, we will disclose more details soon) where community is rewarded with real money
- based in Switzerland, no censorship policy as long as content is legal in Switzerland

### What does the future hold?
WhoaVerse aims to make a media aggregator platform with new ideas and unique features that set WhoaVerse apart from similar platforms.

### Contributing
There is a whole lot of work to be done, code contributions are welcome. A Contributor License Agreement (CLA) is required for all code contributions, configuration changes, documentation, or any other materials that you send to us.
CLA form can be signed and submitted at http://whoaverse.com/cla
