namespace SnazzGenerator

open System
open System.Collections.Generic
open System.Reflection
open System.Text
open System.Text.RegularExpressions

type SnazzGen<'Type>(?primaryKey:string, ?table:string, ?setByteA:bool) =
    // DotNetCasing -> sql_casing:
    let camelCasePattern = Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+")
    let transformDotnetNameToSQL (propertyName:string) =
        String.Join("_", camelCasePattern.Matches(propertyName)).ToLower()
    // Type and primary data points
    let typeInstance = typeof<'Type> // required
    let primaryKey = defaultArg primaryKey "Id"
    let tableName = defaultArg table (transformDotnetNameToSQL typeInstance.Name)
    // Use ::bytea notation, eg "@BinaryField::bytea":
    let setByteA = defaultArg setByteA false

    let getValueFromProperty (property:PropertyInfo) =
            // Using Dapper compatible syntax:
            if setByteA && (property.PropertyType = typeof<Byte[]>) then
                "@" + property.Name + "::bytea"
            else
                "@" + property.Name

    member this.BuildInsert () =
        // Build an insert statement from type properties
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("INSERT INTO " + tableName)
        let fields = List<string>()
        let values = List<string>()
        for prop in props do
            if prop.Name <> primaryKey then
                fields.Add(transformDotnetNameToSQL prop.Name)
                values.Add(getValueFromProperty prop)
        statement
            .Append(" (")
            .Append(String.Join(", ", fields))
            .Append(") VALUES (")
            .Append(String.Join(", ", values))
            .Append(")")
        |> string

    member this.BuildUpdate (?propertyNames:string[]) =
        // Build an update statement from type properties
        // Optionally specify particular fields to apply 
        let propertyNames = defaultArg propertyNames [||]
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("UPDATE " + tableName)
        let propertyNames = List<string>(propertyNames)
        let propSet = List<string>()
        let mutable pkVal = ""
        for prop in props do
            if (prop.Name <> primaryKey) &&
               ((propertyNames.Count = 0) || (propertyNames.Contains prop.Name)) then
                let field = (transformDotnetNameToSQL prop.Name)
                let value = (getValueFromProperty prop)
                propSet.Add(field + " = " + value)
            elif (prop.Name = primaryKey) then 
                pkVal <- (getValueFromProperty prop) // set the primary key
        statement
            .Append(" SET ")
            .Append(String.Join(", ", propSet))
            .Append(" WHERE ")
            .Append((transformDotnetNameToSQL primaryKey))
            .Append(" = ")
            .Append(pkVal)
        |> string
