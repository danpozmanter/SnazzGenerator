namespace SnazzGenerator

module SnazzGen =
    open System
    open System.Collections.Generic
    open System.Text
    open System.Text.RegularExpressions
    open System.Reflection

    type SnazzMeta =
        | Meta of PrimaryKey:string
        | MetaSetBytea of PrimaryKey:string
        | MetaSetTable of PrimaryKey:string * Table:string
        | MetaSetTableBytea of PrimaryKey:string * Table:string

    let private CamelCasePattern = Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+")
    let private ValuesClausePattern = Regex(@"(?<= VALUES ).+")

    let private transformDotnetNameToSQL (propertyName:string) =
        String.Join("_", CamelCasePattern.Matches(propertyName)).ToLower()

    let private getValueFromProperty (property:PropertyInfo) (bytea:bool) =
            if bytea && (property.PropertyType = typeof<Byte[]>) then
                "@" + property.Name + "::bytea"
            else
                "@" + property.Name

    let private setMeta<'Type> (meta:SnazzMeta) =
        let typeInstance = typeof<'Type>
        match meta with
            | MetaSetTable(key, table) -> (key, table, false)
            | MetaSetTableBytea(key, table) -> (key, table, true)
            | Meta(key) -> (key, (transformDotnetNameToSQL typeInstance.Name), false)
            | MetaSetBytea(key) -> (key, (transformDotnetNameToSQL typeInstance.Name), true)

    let buildInsert<'Type> (meta:SnazzMeta) =
        let typeInstance = typeof<'Type>

        let primaryKey, table, bytea = setMeta<'Type> meta
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("INSERT INTO " + table)
        let fields = List<string>()
        let values = List<string>()
        for prop in props do
            if prop.Name <> primaryKey then
                fields.Add(transformDotnetNameToSQL prop.Name)
                values.Add(getValueFromProperty prop bytea)

        statement
            .Append(" (")
            .Append(String.Join(", ", fields))
            .Append(") VALUES (")
            .Append(String.Join(", ", values))
            .Append(")")
        |> string
