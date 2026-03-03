namespace SparkTrack.Core.Shared.Services.Features;

using Data;
using Data.Edit;
using Data.Entities;

public interface IFeaturesService
{
    /// <summary>
    /// Возвращает список фич на странице
    /// </summary>\
    /// <param name="pageQuery">Параметры пагинации</param>
    /// <param name="featureFilterQuery">Набор фильтров</param>
    /// <param name="sortQuery">Параметры сортировки</param>
    /// <param name="showOnlyMine">Показывать только фичи текущего пользователя (используется только для администратора)</param>
    /// <returns></returns>
    Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        bool showOnlyMine = true,
        FeatureFilterQuery? featureFilterQuery = null,
        SortQuery? sortQuery = null,
        PageQuery? pageQuery = null
    );

    /// <summary>
    /// Возфращает ифнормацию по фиче
    /// </summary>
    /// <param name="id">Id фичи</param>
    /// <returns></returns>
    Task<Feature?> GetAsync(int id);

    Task<int> AddAsync(FeatureEdit feature);

    Task EditAsync(FeatureEdit feature);

    Task DeleteAsync(int id, bool force);
}