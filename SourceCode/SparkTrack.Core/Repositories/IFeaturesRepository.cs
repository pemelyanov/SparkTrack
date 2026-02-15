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
    /// <param name="projectId">Id проекта для поиска фич, если null - выборка будет по всем проектам</param>
    /// <param name="showCompleted">Попадут ли в выборку завершенные фичи</param>
    /// <param name="subTaskEmployeeId">Id сотрудника для фильтрации списка подзадач в фиче. Если null - будет выведен список всех подзадач</param>
    /// <param name="pageQuery">Параметры пагинации</param>
    ///     /// <param name="endDate">Максимальная дата создания</param>
    /// <param name="startDate">Минимальная дата создания</param>
    /// <returns></returns>
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, Guid? subTaskEmployeeId, DateTime? startDate,
                                                   DateTime? endDate, PageQuery pageQuery);

    /// <summary>
    /// Возфращает ифнормацию по фиче
    /// </summary>
    /// <param name="id">Id фичи</param>
    /// <param name="subTaskEmployeeId">Id сотрудника для фильтрации списка подзадач в фиче. Если null - будет выведен список всех подзадач</param>
    /// <returns></returns>
    Task<Feature?> GetAsync(int id, Guid? subTaskEmployeeId);

    Task<int> AddAsync(FeatureEdit feature);
    
    Task EditAsync(FeatureEdit feature);

    Task DeleteAsync(int id);

    Task SetArchiveStatus(int id, bool isArchived, EArchiveSource? archiveSource = null);
}