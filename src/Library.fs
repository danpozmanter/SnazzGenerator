namespace SnazzGenerator

open System
open System.Collections.Generic
open System.Reflection
open System.Text
open System.Text.RegularExpressions

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

    member this.buildUpdate (?fields:string[]) =
        let fields = defaultArg fields [||]
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("UPDATE " + TableName)
        let fields = List<string>(fields)
        let propSet = List<string>()
        let mutable pkVal = ""
        for prop in props do
            if (prop.Name <> Key) &&
               ((fields.Count = 0) || (fields.Contains prop.Name)) then
                let field = (transformDotnetNameToSQL prop.Name)
                let value = (getValueFromProperty prop ByteA)
                propSet.Add(field + " = " + value)
            elif (prop.Name = Key) then 
                pkVal <- (getValueFromProperty prop ByteA)

        statement
            .Append(" SET ")
            .Append(String.Join(", ", propSet))
            .Append(" WHERE ")
            .Append((transformDotnetNameToSQL Key))
            .Append(" = ")
            .Append(pkVal)
        |> string
