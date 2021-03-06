# CHANGE LOG

## 2022-06-24

- Scan table comments in Postgres
    - Include table comments in SQL script
    - Include table comments in entity XmlSummary

## 2022-05-31

- Move `.dot` file down a folder
- Include `default` in SQL scripts
    - Except `nextval()` functions

## 2022-05-29

- Write Graphiz `.dot` source

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
