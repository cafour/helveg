using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Roslyn;

internal class RoslynEntityTokenDocumentVisitor
{
    private readonly EntityTokenGenerator gen;

    public ConcurrentDictionary<Guid, EntityToken> Tokens { get; }
        = new();

    public ConcurrentDictionary<MetadataId, EntityToken> MetadataReferenceTokens { get; }
        = new();

    public RoslynEntityTokenDocumentVisitor(EntityTokenGenerator gen)
    {
        this.gen = gen;
    }

    public void VisitSolution(Solution solution)
    {
        Tokens.AddOrUpdate(solution.Id.Id, _ => gen.GetToken(EntityKind.Solution), (_, t) => t);
        foreach (var project in solution.Projects)
        {
            VisitProject(project);
        }
    }

    public void VisitProject(Project project)
    {
        Tokens.AddOrUpdate(project.Id.Id, _ => gen.GetToken(EntityKind.Project), (_, t) => t);
        
        // TODO: Visit documents

        foreach (var reference in project.MetadataReferences)
        {
            VisitMetadataReference(reference);
        }
    }

    public void VisitMetadataReference(MetadataReference reference)
    {
        switch (reference)
        {
            case PortableExecutableReference peReference:
                MetadataReferenceTokens.AddOrUpdate(
                    peReference.GetMetadataId(),
                    _ => gen.GetToken(EntityKind.ExternalDependency),
                    (_, t) => t);
                break;
            default:
                throw new NotSupportedException($"MetadataReferences of type {reference.GetType()} are not supported.");
        }
    }

    public EntityToken GetMetadataReferenceToken(MetadataReference reference)
    {
        if (reference is not PortableExecutableReference peReference)
        {
            return EntityToken.CreateError(EntityKind.ExternalDependency);
        }

        if (!MetadataReferenceTokens.TryGetValue(peReference.GetMetadataId(), out var token))
        {
            return EntityToken.CreateError(EntityKind.ExternalDependency);
        }

        return token;
    }

    public EntityToken RequireMetadataReferenceToken(MetadataReference reference)
    {
        var token = GetMetadataReferenceToken(reference);
        if (token.IsError)
        {
            throw new InvalidOperationException($"Metadata reference '{reference}' does not have a token even though " +
                $"it is required.");
        }

        return token;
    }

    public EntityToken GetProjectToken(ProjectId id)
    {
        return Tokens.TryGetValue(id.Id, out var token)
            ? token
            : EntityToken.CreateError(EntityKind.Project);
    }

    public EntityToken RequireProjectToken(ProjectId id)
    {
        return Tokens.TryGetValue(id.Id, out var token)
            ? token
            : throw new InvalidOperationException($"Project '{id}' does not have a token even though it is required.");
    }
}
