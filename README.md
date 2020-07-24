# SnazzGenerator (SnazzGen)

[![Actions Status](https://github.com/danpozmanter/SnazzGenerator/workflows/dotnet/badge.svg)](https://github.com/danpozmanter/SnazzGenerator/actions)

[Dapper](https://github.com/StackExchange/Dapper) is a fast & user friendly light ORM.

SnazzGen is a snazzy generator for SQL to feed to Dapper.

In general writing raw SQL is the intuitive path forward.

There is one place where there's a bit of unwelcome boilerplate, and that's managing insert statements.

Let's take an example, a simple Photo object:

```fsharp
module Example

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

The sql you'd need to pass into Dapper:

```sql
INSERT INTO photo(name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)
```

You can see how a sufficiently large table becomes tedious to write out.

SnazzGenerator allows you to generate that SQL programmatically through reflection, in idiomatic F#:

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id").getInsertStatement()
// App code:
// Use the insert SQL
```

That's it. It will automatically remove the primary key field in this example: "Id". Optionally you can also set "::bytea" to be used for byte arrays.
It's still advisable not to do the sql generation in a "hot" path, but rather during initialization.

Custom table names:

```fsharp
// Initialization code:
let sql = SnazzGen("Id", "photographs").getInsertStatement()
// App code:
// Use the insert SQL
```

Automatically setting "::bytea" for byte[]:

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id", ByteA=true).getInsertStatement()
// App code:
// Use the insert SQL
```

```fsharp
// Initialization code:
let sql = SnazzGen<Photo>("Id", "photographs", true).getInsertStatement()
// App code:
// Use the insert SQL
```