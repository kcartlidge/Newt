# NEWT

Generate a DotNet (C#/EF Core) data access repository project from a Postgres database.

- [View the changelog](./CHANGELOG.md)

Note that whilst this repository is AGPL there is specifically no licensing constraints applied to generated code.
You are free to apply any license you wish; generated code is entirely yours.

*Copyright K Cartlidge, 2021*

---

## Usage

You need an environment variable containing a connection string.
Running Newt will use that to scan the database structure and generate code.
That same environment variable will be referenced in the generated EF context.

``` sh
export DB_CONNSTR="Server=127.0.0.1;Port=5432;Database=coregen;User Id=coregen;Password=coregen;"
dotnet run --project Newt DB_CONNSTR /Users/<username>/source/core/SampleAPI SampleAPI.Data --force
```

*The above is a sample connection string, not a revealed secret.*

## Command arguments

The command arguments (which are always displayed at runtime) are as follows.
Positions *must* be maintained - these are *ordered arguments*, not parameters.

```
Newt <environment_variable> <destination> <namespace> [--force]
```

| Name                 | Purpose                               | Example                   |
| -------------------- | ------------------------------------- | ------------------------- |
| environment_variable | Variable containing connection string | `DB_CONNSTR`              |
| destination          | Where to write generated code         | `~/source/core/SampleAPI` |
| namespace            | Namespace to place C# code in         | `SampleAPI`               |
| --force              | (Optional) flag to allow overwriting  | `--force`                 |

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
