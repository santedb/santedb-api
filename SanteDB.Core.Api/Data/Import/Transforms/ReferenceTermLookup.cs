using SanteDB.Core.i18n;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import.Transforms
{
    /// <summary>
    /// A <see cref="IForeignDataElementTransform"/> implementation which can look up a concept based on its reference terminology
    /// </summary>
    public class ReferenceTermLookup : IForeignDataElementTransform
    {
        private readonly IConceptRepositoryService m_conceptRepositoryService;

        /// <summary>
        /// Lookup transform DI constructor
        /// </summary>
        public ReferenceTermLookup(IConceptRepositoryService concept)
        {
            this.m_conceptRepositoryService = concept;
        }

        /// <summary>
        /// Get the name of the transform
        /// </summary>
        public string Name => "ReferenceTermLookup";

        /// <inheritdoc/>
        public object Transform(object input, IForeignDataRecord sourceRecord, params object[] args)
        {
            if(args.Length != 1)
            {
                throw new ArgumentException("arg1", ErrorMessages.ARGUMENT_NULL);
            }
            return this.m_conceptRepositoryService.FindConceptsByReferenceTerm(input.ToString(), args[0].ToString()).Select(o=>o.SourceEntityKey).FirstOrDefault();
        }
    }
}
