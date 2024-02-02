# SoftballTech: Multi-Database Version

This is the latest entry in my Learning Journey. Unlike previous versions, this one might be useful to anyone who wants to switch databases at runtime. Please see the ReadMe in the original repo for more info about the website itself.

For this project, my original goal was to use the repository layer I added earlier to access more than one database, using the appsettings file to choose the database. That proved to be too easy (thanks to EF Core) so I decided to allow the database to be set, and even changed, by the user at runtime. To learn how this was done, check out my 3-part series on LinkedIn. Part 1 is here (TBD). Other enhancements in this version are described in this LI post (tbd).

As with the others, this version runs on Azure, and can be found [here](https://sbt-multi-db.azurewebsites.net/).
