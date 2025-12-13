using System.Diagnostics.CodeAnalysis;
using Application.Contracts.ApiWrapper;

namespace Application.SubcutaneousTests.Extensions;

public class ErrorReasonComparer : IEqualityComparer<ErrorReason>
{
    public bool Equals(ErrorReason x, ErrorReason y)
    {
        return x.Code == y.Code && x.Description == y.Description;
    }

    public int GetHashCode([DisallowNull] ErrorReason obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (obj.Code?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.Description?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
