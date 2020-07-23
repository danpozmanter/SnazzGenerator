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
    let sql = SnazzGen<Photo>("Id").buildInsert()
    let expected = "INSERT INTO photo (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)
[<Fact>]
let ``Mapper generates correct SQL using bytea`` () =
    let sql = SnazzGen<Photo>("Id", SetByteA=true).buildInsert()
    let expected = "INSERT INTO photo (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)

[<Fact>]
let ``Mapper generates correct SQL with custom table name`` () =
    let sql = SnazzGen<Photo>("Id", "photographs").buildInsert()
    let expected = "INSERT INTO photographs (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)

[<Fact>]
let ``Mapper generates correct SQL with custom table name with bytea`` () =
    let sql = SnazzGen<Photo>("Id", "photographs", true).buildInsert()
    let expected = "INSERT INTO photographs (name, author, location, binary_data, comma_separated_tags, likes) VALUES (@Name, @Author, @Location, @BinaryData::bytea, @CommaSeparatedTags, @Likes)"
    Assert.Equal(expected, sql)