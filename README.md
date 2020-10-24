# CMS-Skeleton
A control management skeleton to be used as a basic outline and as a testing environment
## Services Currently Implemented
* Data Service
  * If database is empty then populate with default users else ignore
  * Uses Microsoft Entity Framework to encrypt passwords given
  * Asychronous programming is used to control the database initialize task from multiple requests
  * Stores the DbSet locally so the backend can access member in the database
* Functional Service
  * Responsible for creating users and adding them to the correct roles when supplemented with what the CMS requests
  * Asychronous programming is used to control the database user creation task from multiple requests
* Logging Service
  * Used for logging data that has been entered and to catch any thrown exceptions/warnings
  * Currently just sends to a .txt maybe in future I will add it to a console window in Visual Studio
* Model Service
  * Outlines properties for User profiles, their addresses, and account security requirements
## Tools
* Angular 10
* MSSQL Server
* Docker- used as MSSQL Server container
## Libraries
* jQuery
* popper.js
* twitter-boostrap V4.5.0
## Packages
* Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
* Microsoft.AspNetCore.SpaServices.Extensions
* Microsoft.EntityFrameworkCore.Design
* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.Extensions.DependencyInjection
* Microsoft.TypeScript.MSBuild
* Microsoft.AspNetCore.DataProtection.EntityFrameworkCore
* Microsoft.AspNetCore.Identity.EntityFrameworkCore
* Microsoft.EntityFrameworkCore.Tools
* Microsoft.AspNetCore.Identity
* Serilog.AspNetCore
* Serilog.Settings.Configuration
* Serilog.Sinks.Async
* Serilog.Sinks.Console
* Serilog.Sinks.File
