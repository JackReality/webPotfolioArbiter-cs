using System.Collections.Concurrent;
using Portfolio.Exceptions;

namespace Portfolio.Services;

public class CodeService
{
    private record CodeEntry(string Code, DateTime Expiry, int Attempts);
    private readonly ConcurrentDictionary<string, CodeEntry> _codes = new();

    public string GenerateCode(string email)
    {
        var code = Random.Shared.Next(100000, 999999).ToString("D6");
        _codes[email] = new CodeEntry(code, DateTime.UtcNow.AddMinutes(20), 0);
        return code;
    }

    public void CheckCode(string email, string input)
    {
        if (!_codes.TryGetValue(email, out var entry))
            throw new BusinessException("Code.NotFound");

        if (DateTime.UtcNow > entry.Expiry)
        {
            _codes.TryRemove(email, out _);
            throw new BusinessException("Code.Expired");
        }

        if (entry.Code != input.Trim())
        {
            var attempts = entry.Attempts + 1;
            if (attempts >= 5)
            {
                _codes.TryRemove(email, out _);
                throw new BusinessException("Code.MaxAttempts");
            }
            _codes[email] = entry with { Attempts = attempts };
            throw new BusinessException("Code.Invalid");
        }

        _codes.TryRemove(email, out _);
    }
}
