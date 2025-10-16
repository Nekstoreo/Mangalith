namespace Mangalith.Application.Common.Models;

/// <summary>
/// Resultado paginado genérico
/// </summary>
/// <typeparam name="T">Tipo de los elementos</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Número total de elementos
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Página actual
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Indica si hay una página anterior
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si hay una página siguiente
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Constructor vacío
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Constructor con parámetros
    /// </summary>
    /// <param name="items">Elementos de la página</param>
    /// <param name="totalCount">Total de elementos</param>
    /// <param name="page">Página actual</param>
    /// <param name="pageSize">Tamaño de página</param>
    public PagedResult(IEnumerable<T> items, long totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Crea un resultado paginado vacío
    /// </summary>
    /// <param name="page">Página actual</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Resultado paginado vacío</returns>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 50)
    {
        return new PagedResult<T>(Enumerable.Empty<T>(), 0, page, pageSize);
    }
}