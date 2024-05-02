using System;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class AssociatedModelAttribute(Models model) : Attribute
{
    public Models Model { get; } = model;
}