using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

/// <summary>
/// A visitor of the <see cref="SymbolDefinition"/> tree.
/// </summary>
public interface ISymbolVisitor : IEntityVisitor
{
    void DefaultVisit(ISymbolDefinition symbol);
    void VisitAssembly(AssemblyDefinition assembly);
    void VisitModule(ModuleDefinition module);
    void VisitNamespace(NamespaceDefinition @namespace);
    void VisitType(TypeDefinition type);
    void VisitField(FieldDefinition field);
    void VisitEvent(EventDefinition @event);
    void VisitProperty(PropertyDefinition property);
    void VisitMethod(MethodDefinition method);
    void VisitParameter(ParameterDefinition parameter);
    void VisitTypeParameter(TypeParameterDefinition typeParameter);
}
