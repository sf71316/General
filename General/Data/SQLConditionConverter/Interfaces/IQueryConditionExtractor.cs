namespace General.Data.SQLConditionConverter.Interfaces
{
    public interface IQueryConditionExtractor
    {
        QueryParameters Parameters { get; }

        string Translate();
    }
}