namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data;
using DTO;

public static class EntitiesMappingExtensions
{
    public static EditableEntityIdentity ToDomain(this EditableEntityIdentityDTO data) => new(data.Id, data.Version);
}