using SbtMultiDB.Data.Repositories;

namespace SbtMultiDB;

/// <summary>
/// This class extends Session to allow type-safe access to values,
/// eliminating the use of typo-prone strings as keys.
/// </summary>
static public class SessionExtensions
{
    const string CurrentDatabaseKey = "CurrentDatabase";
    const string CurrentDatabaseIsUnavailableKey = "CurrentDatabaseIsUnavailable";

    public static void CurrentDatabase(this ISession session, IDivisionRepository.RepositoryType currentDatabase)
    {
        session.SetString(CurrentDatabaseKey, currentDatabase.ToString());
    }

    public static IDivisionRepository.RepositoryType CurrentDatabase(this ISession session, bool useDefault = true)
    {
        var currentDatabaseAsString = session.GetString(CurrentDatabaseKey);

        return IDivisionRepository.RepositoryTypeFromString(
            currentDatabaseAsString ?? "", useDefault);
    }

    public static void ClearCurrentDatabaseIsUnavailableFlag(this ISession session)
    {
        // The flag is present if and only if the current DB is not available.
        session.Remove(CurrentDatabaseIsUnavailableKey);
    }

    public static void SetCurrentDatabaseIsUnavailableFlag(this ISession session)
    {
        // The flag is present if and only if the current DB is not available.
        session.SetInt32(CurrentDatabaseIsUnavailableKey, 1);
    }

    public static bool IsCurrentDatabaseIsUnavailable(this ISession session)
    {
        var tmp = session.TryGetValue(CurrentDatabaseIsUnavailableKey, out var value2);

        // If the flag (key) exists, the current DB is not available.
        return session.TryGetValue(CurrentDatabaseIsUnavailableKey, out var value);
    }
}
