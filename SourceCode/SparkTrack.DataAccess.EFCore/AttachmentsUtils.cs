namespace SparkTrack.DataAccess.EFCore;

using Core.Shared.Data.Entities;
using Data.Entities;

internal static class AttachmentsUtils
{
    public static AttachmentData ToAttachmentData(Attachment a) => new()
    {
        Name = a.Name,
        Extension = a.Extension,
        Size = a.Size,
        FileId = a.FileId,
        IsImage = a.IsImage,
        Checksum = a.Checksum
    };

    public static void HandleAttachments(
        SparkTrackDbContext dbContext,
        IEnumerable<Attachment> newAttachments,
        IAttachmentsOwner attachmentsOwner
    )
    {
        var existingAttachments = attachmentsOwner.AttachmentsList
            .ToDictionary(t => t.Id);

        foreach (var attachment in newAttachments)
        {
            if (attachment.Id == Guid.Empty)
            {
                attachmentsOwner.AttachmentsList.Add(
                    ToAttachmentData(attachment)
                );

                continue;
            }

            existingAttachments.Remove(attachment.Id, out AttachmentData _);
        }

        if (existingAttachments.Count > 0)
        {
            dbContext.Attachments.RemoveRange(existingAttachments.Values);
        }
    }
}