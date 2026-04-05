using CoolCardGames.Library.Core.GameEventTypes;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CoolCardGames.WebApi.OpenApi;

public class GameEventOpenApiDocumentTransformer(ILogger<GameEventOpenApiDocumentTransformer> logger) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // BUG: this won't serialize generic events
        // TODO: instead, have a GameEventDto w/ Summary and Payload?
        //      as-is, this is leaking way too much about internal representations, but we do also want everything to be visible
        //      prob want a CardDto
        //      idea: make GameEvents not generic, and then have type converter for Card?
        //          prob want a type converter for player account card too
        //      still don't love how easy it would be to leak things to the api
        //          might just have to flag it in GameEvent xmldocs
        var gameEventType = typeof(GameEvent);
        var gameEventTypes = gameEventType.Assembly.GetTypes();
        foreach (Type currGameEventType in gameEventTypes)
        {
            if (currGameEventType.IsAbstract)
                continue;
            if (!gameEventType.IsAssignableFrom(currGameEventType))
                continue;
            try
            {
                var baseTypeName = currGameEventType.BaseType?.Name ?? "";
                var schema = await context.GetOrCreateSchemaAsync(currGameEventType, cancellationToken: cancellationToken);
                document.Components?.Schemas?.Add($"{baseTypeName}{currGameEventType.Name}", schema);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Could not add {CurrGameEventType} to OpenAPI spec, likely because it is generic", currGameEventType);
            }
        }
    }
}