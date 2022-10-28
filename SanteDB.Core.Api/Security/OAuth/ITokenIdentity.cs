using SanteDB.Core.Security.Claims;

namespace SanteDB.Core.Security.OAuth
{
    /// <summary>
    /// Represents an identity that is based on a token.
    /// </summary>
    public interface ITokenIdentity : IClaimsIdentity
    {
    }
}
