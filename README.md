# NEWT

Generate a DotNet (C#/EF Core) data access repository project from a Postgres database.

- [View the changelog](./CHANGELOG.md)

Note that whilst this repository is AGPL there is specifically no licensing constraints applied to generated code.
You are free to apply any license you wish; generated code is entirely yours.

*Copyright K Cartlidge, 2021*

---

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
