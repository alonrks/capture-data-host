# capture-data-host

This is an example of working concurrently with single SQLite database file using C# in .NET 7.0.

The example include a simple console application that runs in 3 modes: Consumer, Producer and Both.
SqlKata is used to read/write to the database file. SqlKata was chosen because it is a simple and
lightweight library that supports SQLite PRAGMA instructions via connection string.

NOTE: this is by no means a production ready example, nor is it necessarily the best way to do it,

## Development

### Prerequisites

- .NET 7.0 SDK
- SqlKata

### Build

To build the project, run the following command:
```bash
dotnet build
```

## Running

To run the example as Producer, run the following command:
```bash
dotnet run -- run --host Both --workload 10
```

Where `host` is either `Producer` or `Consumer` or  `Both`.
Where `workload` is the number of records to insert into the database.

- `Producer` - only creates and inserts records into the database.
- `Consumer` - gets records from the database and deletes.