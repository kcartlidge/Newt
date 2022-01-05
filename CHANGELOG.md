# CHANGE LOG

## Unreleased

- Initial project created
- Standard repo files added
- Parse command arguments
- Scan Postgres
- Clear destination folder
- Create data project
- Create SQL script
  - Columns
  - Primary keys
  - Foreign keys
  - Indexes
- Create entities
  - Data annotations applied
    - Key, Required, MaxLength, DisplayName
- Create EF contexts
  - InMemory
  - Postgres
- Navigation properties
  - Derived from foreign keys
  - Currently one way only
    - Where an A has B's, A gets a List<B>
