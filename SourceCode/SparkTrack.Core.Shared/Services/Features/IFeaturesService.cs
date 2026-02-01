namespace SparkTrack.Core.Shared.Services.Features;

using Data;
using Data.Edit;
using Data.Entities;

public interface IFeaturesService
{
    /// <summary>
    /// Возвращает список фич на странице
    /// </summary>
    /// <param name="projectId">Id проекта для поиска фич, если null - выборка будет по всем проектам</param>
    /// <param name="showCompleted">Попадут ли в выборку завершенные фичи</param>
    /// <param name="pageQuery">Параметры пагинации</param>
    /// <param name="endDate">Максимальная дата создания</param>
    /// <param name="startDate">Минимальная дата создания</param>
    /// <returns></returns>
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    );

    /// <summary>
    /// Возфращает ифнормацию по фиче
    /// </summary>
    /// <param name="id">Id фичи</param>
    /// <returns></returns>
    Task<Feature?> GetAsync(int id);

    Task<int> AddAsync(FeatureEdit feature);

    Task EditAsync(FeatureEdit feature);

    Task DeleteAsync(int id);
}