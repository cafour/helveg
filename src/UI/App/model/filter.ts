export interface NodeFilter
{
    kind: string;
}

export interface PropertyValueFilter extends NodeFilter
{
    label: string;
    propertyName: string;
    propertyValue: string;
    comparison: ComparisonKind;
}

export enum ComparisonKind
{
    Invalid,
    Equals,
    Contains,
    Regex
}
