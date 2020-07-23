namespace SnazzGenerator

module SnazzGen =
    open System
    open System.Collections.Generic
    open System.Text
    open System.Text.RegularExpressions
    open System.Reflection
    
    exception SnazzGenTypeError of string
    
    type SnazzMeta =
        | Meta of PrimaryKey:string
        | MetaSetBytea of PrimaryKey:string
        | MetaSetTable of PrimaryKey:string * TableName:string
        | MetaSetTableBytea of PrimaryKey:string * TableName:string
    
    let CamelCasePattern = Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+")
    let ValuesClausePattern = Regex(@"(?<= VALUES ).+")

    let transformDotnetNameToSQL (propertyName:string) =
        String.Join("_", CamelCasePattern.Matches(propertyName)).ToLower()

    let getValueFromProperty (property:PropertyInfo) (bytea:bool) = 
            if bytea && (property.PropertyType = Type.GetType("System.Byte[]")) then
                "@" + property.Name + "::bytea"
            else
                "@" + property.Name

    let setMeta (typeInstance:Type) (meta:SnazzMeta) =
        match meta with
            | MetaSetTable(key, table) -> (key, table, false)
            | MetaSetTableBytea(key, table) -> (key, table, true)
            | Meta(key) -> (key, (transformDotnetNameToSQL typeInstance.Name), false)
            | MetaSetBytea(key) -> (key, (transformDotnetNameToSQL typeInstance.Name), true)
    
    let buildInsert (typeInstance:Type) (meta:SnazzMeta) =
        if (typeInstance = null) then
            raise (SnazzGenTypeError "Error: Type provided to mapper is null.")
        let primaryKey, tableName, bytea = setMeta typeInstance meta
        let props = typeInstance.GetProperties()
        let statement = StringBuilder()
        let statement = statement.Append("INSERT INTO " + tableName)
        let fields = List<string>()
        let values = List<string>()
        for prop in props do
            if prop.Name <> primaryKey then
                fields.Add(transformDotnetNameToSQL prop.Name)
                values.Add(getValueFromProperty prop bytea)
        let statement = statement.Append(" (")
        let statement = statement.Append(String.Join(", ", fields))
        let statement = statement.Append(") VALUES (")
        let statement = statement.Append(String.Join(", ", values))
        let statement = statement.Append(")")
        statement.ToString()
    
    let makeInsertBulk insertStatement rows =
        let valuesClause = ValuesClausePattern.Match(insertStatement).Value
        insertStatement + ", " + String.Join(", ", List.replicate (rows - 1) valuesClause) + ";"
