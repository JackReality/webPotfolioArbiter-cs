namespace Portfolio.Exceptions;

public class BusinessException(string key) : Exception
{
    public string Key { get; } = key;
}
