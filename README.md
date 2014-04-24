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
Whoaverse uses a SQL database to store messages, comments, votes etc. Database can be generated using Entity Framework Code First (models are in /Models folder). No other customizations or server-side installation is necessary.

### Why was this made?
This was just a hobby project to help me get a better understanding of C# and ASP.NET MVC and Entity Framework.

### How does it differ from related projects?
This is the only C# implementation of a Reddit®-like community as far as I know.

### What does the future hold?
Whoaverse aims to make a fully functional and scalable Reddit®-like community including all the kinks and features which today exist on reddit.com. 

### Contributing
There is a whole lot of work to be done, code contributions are welcome. A Contributor License Agreement (CLA) is required for all code contributions, configuration changes, documentation, or any other materials that you send to us.
CLA form can be signed and submitted at http://whoaverse.com/cla
