# JSON Schemas

Use `eng/JsonSchemaGenerator` to generate the C# classes:

```bash
dotnet run --project ./eng/JsonSchemaGenerator/ -- ./schema/data.json ./src/Visualization/DataSchema.cs Helveg.Visualization.DataSchema
```
