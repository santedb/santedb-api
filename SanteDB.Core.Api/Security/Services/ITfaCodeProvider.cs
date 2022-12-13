using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    public interface ITfaCodeGenerator
    {
        string GenerateTfaCode(IIdentity identity);
    }

    public interface ITfaCodeVerifier
    {
        bool VerifyTfaCode(IIdentity identity, string code, DateTimeOffset? timeProvided = null);
    }

    public interface ITfaCodeProvider : ITfaCodeGenerator, ITfaCodeVerifier
    {
        //TfaSecret BeginTfaRegistration();
        //TfaSecret FinishTfaRegistration();


    }

    
}
