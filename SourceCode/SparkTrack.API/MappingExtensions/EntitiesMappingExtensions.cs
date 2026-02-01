namespace SparkTrack.API.MappingExtensions;

using API;
using SparkTrack.Core.Shared.Data;

public static class EntitiesMappingExtensions
{
    public static EditableEntityIdentityDTO ToDTO(this EditableEntityIdentity data) => new()
    {
        Id = data.Id, 
        Version = data.Version
    };
}