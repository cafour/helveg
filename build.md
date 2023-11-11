# JSON Schemas

Use `eng/JsonSchemaGenerator` to generate the C# classes:

```bash
dotnet run --project ./eng/JsonSchemaGenerator/ -- ./schema/data.json ./src/Visualization/DataModel.cs Helveg.Visualization.DataModel
dotnet run --project ./eng/JsonSchemaGenerator/ -- ./schema/icon-set.json ./src/Visualization/IconSetModel.cs Helveg.Visualization.IconSetModel
```
