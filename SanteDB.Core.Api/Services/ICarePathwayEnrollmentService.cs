using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Service which manages the enrolment of patients into care pathways
    /// </summary>
    public interface ICarePathwayEnrollmentService : IServiceImplementation
    {

        /// <summary>
        /// Gets the carepaths that the <paramref name="patient"/> meets eligibility criteria for 
        /// </summary>
        /// <param name="patient">The for which eligibility should be determined</param>
        /// <returns>The care pathways that the patient is eligible to enrol in</returns>
        IEnumerable<CarePathwayDefinition> GetEligibleCarePaths(Patient patient);

        /// <summary>
        /// Get all the care pathways in which the patient is enrolled (i.e. has an active care plan)
        /// </summary>
        /// <param name="patient">The patient for which the enrolled care plans should be fetched</param>
        /// <returns>The care plans for the enrolled carepaths</returns>
        IEnumerable<CarePlan> GetEnrolledCarePaths(Patient patient);

        /// <summary>
        /// Enrol the patient in the specified <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient to be enrolled</param>
        /// <param name="carePathway">The care pathway which is to be enrolled in</param>
        /// <returns>The care plan representing the registration into the care pathway</returns>
        CarePlan Enroll(Patient patient, CarePathwayDefinition carePathway);

        /// <summary>
        /// Un-enrols the patient from the <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient which is to be un-enroled in the care pathway</param>
        /// <param name="carePathway">The care pathway from which the patient is to be un-enroled</param>
        /// <returns>The care plan that was removed (marked obsolete)</returns>
        CarePlan UnEnroll(Patient patient, CarePathwayDefinition carePathway);

        /// <summary>
        /// Determines if <paramref name="patient"/> is enrolled in <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient to check for enrolment</param>
        /// <param name="carePathway">The care pathway which enrolment is to be checked</param>
        /// <returns>True if <paramref name="patient"/> is enrolled in <paramref name="carePathway"/></returns>
        bool IsEnrolled(Patient patient, CarePathwayDefinition carePathway);
    }
}
