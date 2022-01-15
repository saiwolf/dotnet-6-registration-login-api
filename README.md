# dotnet-6-registration-login-api

(Forked from https://github.com/cornflourblue/dotnet-6-registration-login-api by @cornflourblue)

.NET 6.0 - User Registration and Login Tutorial with Example API

# Development Settings

This forked project uses the [.NET Secrets Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0) by default.

You'll need to run the following command, after cloning this repo. Be sure you're in the top-level
directory.

```shell
dotnet user-secrets init
```

This will update the `WebApi.csproj` file with a new User Secrets GUID.

From there, in Visual Studio, you can right click on the Project, then select "Manage User Secrets"
for a nice editing experience. From there, you can copy/paste the content from `appsettings.Production.json'
 and edit as needed.

If you're not using Visual Studio, then you can manage the secrets via your shell.
See the [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows#set-a-secret)
for more information on how to do this.

**Alternatively, you can skip the Secrets Manager, and just put your settings in a new file called
`appsettings.Development.json` in the root of this project.**