module Tests

open System.Text
open Xunit
open SnazzGenerator

type Photo = {
    Id: int;
    Name: string;
    Author: string;
    Location: string;
    BinaryData: byte[];
    CommaSeparatedTags: string;
    Likes: int;
}

[<Fact>]
let ``Mapper generates correct SQL`` () =
    let sql = SnazzGen.buildInsert (typeof<Photo>) (SnazzGen.Meta("Id"))
    let expected = "INSERT INTO photo (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)
[<Fact>]
let ``Mapper generates correct SQL using bytea`` () =
    let sql = SnazzGen.buildInsert (typeof<Photo>) (SnazzGen.MetaSetBytea("Id"))
    let expected = "INSERT INTO photo (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)

[<Fact>]    
let ``Mapper generates correct SQL with custom table name`` () =
    let sql = SnazzGen.buildInsert (typeof<Photo>) (SnazzGen.MetaSetTable("Id", "photographs"))
    let expected = "INSERT INTO photographs (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)

[<Fact>]    
let ``Mapper generates correct SQL with custom table name with bytea`` () =
    let sql = SnazzGen.buildInsert (typeof<Photo>) (SnazzGen.MetaSetTableBytea("Id", "photographs"))
    let expected = "INSERT INTO photographs (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)

[<Fact>]
let ``Mapper fails on null type`` () =
    Assert.Throws<SnazzGen.SnazzGenTypeError>(fun() -> (SnazzGen.buildInsert null (SnazzGen.Meta("Id"))) |> ignore) |> ignore

[<Fact>]
let ``Mapper turns insert into bulk insert correctly`` () =
    let initialSql = SnazzGen.buildInsert (typeof<Photo>) (SnazzGen.MetaSetBytea("Id"))
    let sql = SnazzGen.makeInsertBulk initialSql 5
    let expected = StringBuilder("INSERT INTO photo (name, author, location, binary_data, comma_separated_tags, likes)")
    expected.Append(" VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)") |> ignore
    expected.Append(", (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)") |> ignore
    expected.Append(", (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)") |> ignore
    expected.Append(", (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)") |> ignore
    expected.Append(", (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes);") |> ignore
    Assert.Equal(expected.ToString(), sql)
