namespace SnazzGenerator

//module Snazz =
open System
open System.Collections.Generic
open System.Text
open System.Text.RegularExpressions
open System.Reflection

type SnazzGen<'Type>(PrimaryKey:string, ?Table:string, ?SetByteA:bool) =
    let CamelCasePattern = Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+")

    let transformDotnetNameToSQL (propertyName:string) =
        String.Join("_", CamelCasePattern.Matches(propertyName)).ToLower()
    let typeInstance = typeof<'Type>
    let Key = PrimaryKey
    let TableName = defaultArg Table (transformDotnetNameToSQL typeInstance.Name)
    let ByteA = defaultArg SetByteA false

    let getValueFromProperty (property:PropertyInfo) (bytea:bool) =
            if bytea && (property.PropertyType = typeof<Byte[]>) then
                "@" + property.Name + "::bytea"
            else
                "@" + property.Name

    member this.buildInsert () =
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("INSERT INTO " + TableName)
        let fields = List<string>()
        let values = List<string>()
        for prop in props do
            if prop.Name <> Key then
                fields.Add(transformDotnetNameToSQL prop.Name)
                values.Add(getValueFromProperty prop ByteA)

        statement
            .Append(" (")
            .Append(String.Join(", ", fields))
            .Append(") VALUES (")
            .Append(String.Join(", ", values))
            .Append(")")
        |> string
