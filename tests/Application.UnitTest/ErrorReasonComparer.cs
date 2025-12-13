using System.Collections;
using Application.Contracts.ApiWrapper;

namespace Application.UnitTest;

public class ErrorReasonComparer : IEqualityComparer
{
    public new bool Equals(object? x, object? y)
    {
        if (x is not ErrorReason a || y is not ErrorReason b)
        {
            return false;
        }

        return a.Code == b.Code && a.Description == b.Description;
    }

    public int GetHashCode(object obj)
    {
        if (obj is not ErrorReason e)
        {
            return 0;
        }
        unchecked
        {
            int hash = 17;
            hash = (hash * 23) + e.Code.GetHashCode();
            hash = (hash * 23) + (e.Description?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
