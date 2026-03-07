namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

public interface IFeaturesRepository
{
    /// <summary>
    /// Возвращает список фич на странице
    /// </summary>
    /// <param name="subTaskEmployeeId">Id сотрудника для фильтрации списка подзадач в фиче. Если null - будет выведен список всех подзадач</param>
    /// <param name="authorId">Id автора фичи</param>
    /// <param name="featureFilterQuery">Набор фильтров</param>
    /// <param name="sortQuery">Параметры сортировки</param>
    /// <param name="pageQuery">Параметры пагинации</param>
    /// <returns></returns>
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? subTaskEmployeeId = null,
        Guid? authorId = null,
        FeatureFilterQuery? featureFilterQuery = null,
        SortQuery? sortQuery = null,
        PageQuery? pageQuery = null
    );

    /// <summary>
    /// Возфращает ифнормацию по фиче
    /// </summary>
    /// <param name="id">Id фичи</param>
    /// <param name="subTaskEmployeeId">Id сотрудника для фильтрации списка подзадач в фиче. Если null - будет выведен список всех подзадач</param>
    /// <returns></returns>
    Task<Feature?> GetAsync(int id, Guid? subTaskEmployeeId);

    Task<Feature> AddAsync(FeatureEdit feature);

    Task<Feature> EditAsync(FeatureEdit feature);

    Task DeleteAsync(int id);

    Task SetArchiveStatus(int id, bool isArchived, EArchiveSource? archiveSource = null);
}