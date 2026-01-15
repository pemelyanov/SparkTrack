namespace SparkTrack.DataAccess.EFCore.Data.Entities;

public interface IAttachmentsOwner
{
    public ICollection<AttachmentData> AttachmentsList { get; }
}