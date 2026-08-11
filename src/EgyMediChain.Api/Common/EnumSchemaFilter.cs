using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EgyMediChain.Api.Common;

// Marks a DTO property that's typed `string?` on the wire (via `SomeEnum.ToString()` in the
// controller) but whose actual values only ever come from a specific C# enum. Lets Swagger
// document the real allowed values without changing the property's C# type - changing 25+
// properties from string to the enum type directly would mean touching every controller call
// site that assigns them via `.ToString()`, which is a much larger and riskier change than just
// documenting the contract that's already true today.
[AttributeUsage(AttributeTargets.Property)]
public class DocumentedEnumAttribute : Attribute
{
    public Type EnumType { get; }
    public DocumentedEnumAttribute(Type enumType) => EnumType = enumType;
}

// Backend Action Report / Frontend Integration Audit - "Undocumented enums": batchStatus,
// shipmentStatus, inventoryStatus, entityType, registrationStatus, inspectionResult, etc. were
// all plain `string` in the Swagger with no `enum:` array, forcing the frontend to hardcode
// assumed values. This filter documents:
// 1) any property/parameter whose C# type is genuinely an enum (native support), and
// 2) any property marked [DocumentedEnum(typeof(X))] even though its wire type is string.
// Names emitted are exactly what JsonStringEnumConverter (registered globally in Program.cs)
// puts on the wire.
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var documented = context.MemberInfo?.GetCustomAttributes(typeof(DocumentedEnumAttribute), false)
            .OfType<DocumentedEnumAttribute>().FirstOrDefault();

        var enumType = documented?.EnumType ?? (Nullable.GetUnderlyingType(context.Type) ?? context.Type);
        if (!enumType.IsEnum) return;

        schema.Enum.Clear();
        schema.Type = "string";
        schema.Format = null;
        foreach (var name in Enum.GetNames(enumType))
            schema.Enum.Add(new OpenApiString(name));
    }
}

