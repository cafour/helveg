using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp.Symbols;

public abstract class SymbolVisitor : EntityVisitor
{
    public abstract void DefaultVisit(ISymbolDefinition symbol);

    public override void DefaultVisit(IEntity entity)
    {
        if (entity is not ISymbolDefinition symbol)
        {
            throw new NotSupportedException("An ISymbolVisitor cannot be used on entities that don't implement " +
                "ISymbolDefinition.");
        }

        DefaultVisit(symbol);
    }

    public virtual void VisitAssembly(AssemblyDefinition assembly)
    {
        DefaultVisit(assembly);
    }

    public virtual void VisitModule(ModuleDefinition module)
    {
        DefaultVisit(module);
    }

    public virtual void VisitNamespace(NamespaceDefinition @namespace)
    {
        DefaultVisit(@namespace);
    }

    public virtual void VisitType(TypeDefinition type)
    {
        DefaultVisit(type);
    }

    public virtual void VisitField(FieldDefinition field)
    {
        DefaultVisit(field);
    }

    public virtual void VisitEvent(EventDefinition @event)
    {
        DefaultVisit(@event);
    }

    public virtual void VisitProperty(PropertyDefinition property)
    {
        DefaultVisit(property);
    }

    public virtual void VisitMethod(MethodDefinition method)
    {
        DefaultVisit(method);
    }

    public virtual void VisitParameter(ParameterDefinition parameter)
    {
        DefaultVisit(parameter);
    }

    public virtual void VisitTypeParameter(TypeParameterDefinition typeParameter)
    {
        DefaultVisit(typeParameter);
    }
}
