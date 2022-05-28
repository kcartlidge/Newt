# CHANGE LOG

## 2022-05-28

- Write JSON schema dump

## 2022-05-27

- Tidied up the code
- Added extra `XmlSummary` comments
- Updated the README file
- Added column comments

## 2022-01-05

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
