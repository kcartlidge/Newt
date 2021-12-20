# NEWT

Generate a DotNet (C#/EF Core) data access repository project from a Postgres database.

- [View the changelog](./CHANGELOG.md)

Note that whilst *this code repository* is AGPL there are specifically *no licensing constraints applied to generated code*.
You are free to apply any license you wish; generated code is entirely yours.

This is *beta*. It works, but there are issues with tables without primary keys, for example.

**Status:**

- Scans Postgres
- Creates a new data project
- Creates a SQL script for *emergency use*
  - Understands columns, primary/foreign keys, indexes
- Generates model classes for the tables
  - Includes data annotations, eg for Key, Required, and MaxLength
- Adds and registers an EF Core Postgres context
- Also adds an EF Core InMemory context

*Copyright K Cartlidge, 2021*

---

## Contents

- [Usage](#usage)
- [Command arguments](#command-arguments)
- [Database conventions](#database-conventions)
- [Output](#output)
- [Generating stand-alone builds](#generating-stand-alone-builds)
  - It's easy to generate for a choice of platforms
  - Builds are small enough to be checked into source control

## Usage

You need an environment variable containing a connection string.
That same environment variable name will be referenced in the generated EF context.

``` shell
export DB_CONNSTR="Server=127.0.0.1;Port=5432;Database=coregen;User Id=coregen;Password=coregen;"
```

*(The above is an example connection string, not a revealed secret.)*

Running Newt will use that to scan the database structure and generate code.
There are two commands shown. The second is for if you have a published binary in your path.

``` shell
dotnet run --project Newt -- -env DB_CONNSTR -schema public -folder ~/SampleAPI -namespace SampleAPI.Data --force
Newt -env DB_CONNSTR -schema public -folder ~/SampleAPI -namespace SampleAPI.Data --force
```

## Command arguments

The command arguments (which are always displayed at runtime) are as follows.

``` shell
Newt -env <environment_variable> -schema <schema> -folder <destination> -namespace <namespace> [--force]
```

| Name      | Purpose                               | Example                   |
|-----------|---------------------------------------|---------------------------|
| env       | Variable containing connection string | `DB_CONNSTR`              |
| schema    | Database schema to generate code for  | `public`                  |
| folder    | Where to write generated code         | `~/source/core/SampleAPI` |
| namespace | Namespace to place C# code in         | `SampleAPI.Data`          |
| --force   | Allow overwriting *(optional)*        | `--force`                 |

## Database conventions

Newt scans the specified Postgres database to generate code.
When it does so it needs to be able to understand, parse, and use what it finds.
This means certain conventions should be followed.

- *Postgres* naming conventions must be used, *not* C# ones
    - Table names are lower case snake (underscore delimited)
        - `customer_orders` not `CustomerOrders` or `customerOrders`
    - Columns names are *also* lower case snake (underscore delimited)
        - `display_name` not `DisplayName` or `displayName`
        - `id` not `ID` or `Id`
- Only one field in table primary keys

Some of these restrictions are expected to be removed in the near future.

## Output

The following represents generated output assuming a namespace of `SampleAPI.Data`.
The database has a `customers` and a `bookings` table.
The `customers` table has an `admin_email_address` column.

### Created project files

![created project](./created-project.png)

### Created context

![created context](./created-context.png)

### Created column

![created column](./created-column.png)

## Generating stand-alone builds

This can be done with a single command as shown below.
There are a selection, each targeting a different output system.

The commands should be run from within the `Newt/Newt` *project* folder, not the solution.

``` shell
# Mac, Apple Silicon (eg M1)
dotnet publish -r osx-arm64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true

# Mac, Intel
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true

# Linux, Intel
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true

# Windows, Intel
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Whichever command you choose to run, it will tell you in its response where it has placed the binary.
Copy that somewhere accessible via your system path and you can run it from anywhere.

*The binary is very small (about 20MB) so you can also check it into source control alongside your main project.*
