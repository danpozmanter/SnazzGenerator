# SnazzGenerator (SnazzGen)

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
let sql = getInsertStatement typeof<Photo> (SnazzGen.Meta("Id"))
// App code:
// Use the insert SQL
```

That's it. It will automatically remove the primary key field ("Id" by default). And if the database is Postgres, ensure "::bytea" is used for binary data.
While this is fairly performant, it's still advisable not to do the sql generation in a "hot" path, but rather during initialization.

Supports bulk inserts:

```fsharp
// Initialization code:
let sql = getInsertStatement typeof<Photo> "Id"
// App code:
// Something that gets you 100 objects
let bulkSql = makeInsertBulk sql 100
// Do the insert using bulkSql instead.
```

Custom table names:

```fsharp
// Initialization code:
let sql = getInsertStatement typeof<Photo> (SnazzGen.MetaSetTable(PrimaryKey="Id", TableName="photographs"))
// App code:
// Use the insert SQL
```

Automatically setting "::bytea" for byte[]:

```fsharp
// Initialization code:
let sql = getInsertStatement typeof<Photo> (SnazzGen.MetaSetBytea(PrimaryKey="Id", TableName="photographs"))
// App code:
// Use the insert SQL
```

```fsharp
// Initialization code:
let sql = getInsertStatement typeof<Photo> (SnazzGen.MetaSetTableBytea(PrimaryKey="Id", TableName="photographs"))
// App code:
// Use the insert SQL
```