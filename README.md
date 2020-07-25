# SnazzGenerator (SnazzGen)

[![Actions Status](https://github.com/danpozmanter/SnazzGenerator/workflows/dotnet/badge.svg)](https://github.com/danpozmanter/SnazzGenerator/actions) [![SnazzGenerator](https://img.shields.io/badge/nuget-SnazzGenerator-blue)](https://www.nuget.org/packages/SnazzGenerator/)

[Dapper](https://github.com/StackExchange/Dapper) is a fast & user friendly light ORM.

SnazzGenerator is a snazzy generator for SQL to feed to Dapper.

In general writing raw SQL is the intuitive path forward - especially for queries. But for inserts (and to a lesser degree updates), there's a bit of unwelcome boilerplate. Yet using runtime reflection is expensive. That's where SnazzGenerator comes in.

## Installation

```
dotnet add package SnazzGenerator
```

## Usage Guide

```fsharp
open SnazzGenerator

type Example { ... }

// Initialization code
let insertSql = SnazzGen<{Type}>("{PrimaryKeyFieldName}", table="{tableName}", setByteA={Bool: Use ::bytea notation}).buildInsert()
let updateSql = SnazzGen<{Type}>("{PrimaryKeyFieldName}", table="{tableName}", setByteA={Bool: Use ::bytea notation}).buildUpdate([|string array of propery names|])
let updateSqlAllFields = SnazzGen<{Type}>("{PrimaryKeyFieldName}", table="{tableName}", setByteA={Bool: Use ::bytea notation}).buildUpdate()

// Application code (examples with dapper)
// Insert:
let example = { ... }
connection.ExecuteAsync(insertSql, example).Result |> ignore

// Bulk Insert
let examples = // Generate a list of examples to insert
connection.ExecuteAsync(insertSql, examples).Result |> ignore

// Update all :
let updatedExample = { ... }
// Update just the fields specified above:
connection.ExecuteAsync(updateSql, updateExample).Result |> ignore
// Update all the fields
connection.ExecuteAsync(updateSqlAllFields, updateExample).Result |> ignore
```

## Examples

Let's start with a simple Photo object:

```fsharp
type Photo = {
    Id: int;
    Name: string;
    Author: string;
    Location: string;
    BinaryData: byte[];
    CommaSeparatedTags: string;
    Likes: int;
}
```

Here's the sql you'd need to pass into Dapper for an insert:

```sql
INSERT INTO photo(name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)
```

You can imagine how a sufficiently large table becomes tedious to write out for every type/table.

SnazzGenerator allows you to generate that SQL programmatically through reflection:

### Insert

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id").buildInsert()
// App code:
// Use the insert SQL
```

That's it. It will automatically remove the primary key field in this example: "Id". Optionally you can also set "::bytea" to be used for byte arrays.
It's still advisable not to do the sql generation in a "hot" path, but rather during initialization.

Custom table names:

```fsharp
// Initialization code:
let sql = SnazzGen("Id", "photographs").buildInsert()
// App code:
// Use the insert SQL
```

Automatically setting "::bytea" for byte[]:

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id", setByteA=true).buildInsert()
// App code:
// Use the insert SQL
```

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id", "photographs", true).buildInsert()
// App code:
// Use the insert SQL
```

### Update

You can also generate UPDATE statements ahead of time

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id").buildUpdate([|"Name", "Author", "Likes"|])
let sqlAllFields = SnazzGen<Photo>("Id").buildUpdate()
// App code:
// Use the insert SQL
```

## License

SnazzGenerator is made available through the Apache License Version 2.0