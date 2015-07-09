# Voat

<img height="252" width="345" src="http://voat.co/Graphics/voat-goat.png"
 alt="Voat mascot" title="Voat" align="right" />

This is the code behind http://www.voat.co.

### Description
Voat is a media aggregator and community platform developed in C# ASP.NET MVC 5.

### Dependencies and attributions
Voat uses SQL server and .NET framework 4.5.

The following 3rd party libraries/extensions are used:

- Markdowndeep (http://www.toptensoftware.com/markdowndeep)
- Entity Framework (https://entityframework.codeplex.com)
- WebApiThrottle (https://github.com/stefanprodan/WebApiThrottle)
- SignalR (https://github.com/SignalR/SignalR)
- OpenGraph-Net (https://github.com/ghorsey/OpenGraph-Net)
- .NET Image Library (https://www.nuget.org/packages/ImageLibrary)
- HtmlAgilityPack (http://www.nuget.org/packages/HtmlAgilityPack)
- Bootstrap (http://getbootstrap.com)
- jQuery (http://jquery.com)

### Installation instructions
Follow these steps to get up and running:

#### Step 1: Install software

You'll need the following software installed to get up and running

- Visual Studio 2015 Community Edition (https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)
- SQL Server 2015 Express Edition with Advanced Services (https://www.microsoft.com/en-us/server-cloud/products/sql-server-editions/sql-server-express.aspx)

#### Step 1: Setup databases

- Run SQL Server 2014 Management Studio
- Click "File" -> "Open" and select createDB.sql in the root of the project. When it has loaded, click "Execute"
- Do the same with whoaverse.sql and whoaverse_users.sql
- Select the "Databases" item under the root of the "Object Explorer" pane and press F5 to refresh it.
- Expand the "Databases" item under the root of the "Object Explorer" pane. Verify that you see both a "voat" and "voatUsers" item. If so, for each item
  - Expand "Security" -> "Users" and verify that you see "voat" or "voatUsers," respectively
  - Expand "Security" -> "Roles" -> "Database Roles." Double click "db_owner" and verify that "voat" or "voatUsers" in the "Role Members" box.
- Right click your server in the "Object Explorer" pane (highest level item) and select "Properties." Select "Security" on the left and verify that "SQL Server and Windows Authentication mode" is selected. Press "OK."
- Right click the server again and select "Restart" and accept the admin prompt. Select "Yes" in the next dialog to restart the server.
- Right click the server again and select "New Query." Enter **SELECT @@SERVERNAME**, click "Execute" and copy the result to use in Step 2.

#### Step 2: Setup project

- After cloning this repository, copy the **Web.config** file from the root of the project folder into the **/Whoaverse/Whoaverse** folder (the same folder where the file **packages.config** is located). You need to modify the following 2 connection strings in this file to reflect your SQL server address, port, database names and database usernames: 
whoaverseUsers and whoaverseEntities
```
<add name="whoaverseUsers" connectionString="Data Source=yourdomain.com, 1433;Initial Catalog=whoaverse_users;Persist Security Info=True;User ID=yourusername;Password=yourpassword" providerName="System.Data.SqlClient" />
<add name="whoaverseEntities" connectionString="metadata=res://*/Models.WhoaverseEntityDataModel.csdl|res://*/Models.WhoaverseEntityDataModel.ssdl|res://*/Models.WhoaverseEntityDataModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=yourdomain.com;initial catalog=whoaverse;persist security info=True;user id=yourusername;password=yourpassword;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />
```
- (Following this guide, you'll have the following values for these attributes in the two entries above:
  - Data Source: Replace "yourdomain.com" with the value you copied at the end of step 1. Should end up reading something like "NAME-PC\LOCALHOST, 1433".
  - Initial Catalog: voatUsers/voat
  - User ID: voatUsers/voat
  - Password: secretpwd

- You need to sign up for recaptcha service at https://www.google.com/recaptcha/admin#whyrecaptcha to get your public and private recaptcha keys. Use "localhost" for the domain, and your e-mail for the e-mail.
- Once you have your recaptcha keys, you need to modify the Web.config file and in section `<appSettings>`, you need to add the following for your keys:
```
<add key="recaptchaPublicKey" value="your public key goes here" />
<add key="recaptchaPrivateKey" value="your private key goes here" />
```

- Delete the folder **/Whoaverse/Whoaverse/packages/WebActivator.1.5.0** if it exists

#### Step 3: Visual Studio Setup

- Open **/Whoaverse/Voat.sln**
- Go to Tools -> NuGet Package Manager -> Package Manager Console
- If a yellow bar with a "Restore" button appears, click "Restore"
- Reinstall dependencies (binaries for NuGet packages) by issuing the following command in Package Manager Console: "Update-Package -Reinstall" (when asked to overwrite existing files, choose "No To All")
- Select Debug -> Start Debugging

### After installation
Start by creating your user account. The frontpage will be empty, so you should start by creating a subverse.
After creating your subverse, you can visit it (localhost/v/yourtestsubverse) and start posting stories or links. You can then comment on these stories and vote on them.

### Why was this made?
This was just a hobby project to help me get a better understanding of C# and ASP.NET MVC and Entity Framework.

### How does Voat differ from related projects?
- based in Switzerland, no censorship policy as long as content is legal in Switzerland
- ad revenue sharing model (in development, we will disclose more details soon) where community is rewarded with real money for quality original content
- deterministically scaling voting quota
- limited voting (new users need to gain a certain amount of points before they are able to vote without restrictions)
- limited number of owned/moderated subs per user (10)
- voat has increased focus on privacy. It enables users to delete their account by automatically overwriting every comment and every submission the user has made with a string "deleted", before proceeding to remove the user account from user credentials database. 
- built-in night mode
- subverse set system similar to the one used on Google News
- realtime notifications for user mentions, post and comment replies
- realtime chat for every subverse
- markdown toolbar for user friendly text editing
- automatic expando creation for many popular services
- anonymized mode: subverse owners can irreversibly convert their subverse to anonymized mode which hides all usernames and disables all voting actions within that subverse
- responsive design which works great on mobile out of the box
- user profiles show statistics about user activity, for example, submission distribution and highest-lowest rated submissions and a short biography with avatar

### What does the future hold?
Voat aims to make a media aggregator platform with new ideas and unique features that set Voat apart from similar platforms.

### Contributing
There is a whole lot of work to be done, code contributions are more than welcome. By submitting a pull request, you are agreeing for your contribution to be distributed under GPL V3 license (the same license voat uses for the rest of voat project).
