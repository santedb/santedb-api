using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// 
    /// </summary>
    /// HACK - We need to make this better.
    [XmlType("RelationshipValidationRule", Namespace = "http://santedb.org/model"), JsonObject("RelationshipValidationRule")]
    public class RelationshipValidationRule : IdentifiedData, IRelationshipValidationRule
    {


        [JsonIgnore, XmlIgnore]
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        public Guid? SourceClassKey { get; set; }

        public Guid? TargetClassKey { get; set; }

        public Guid RelationshipTypeKey { get; set; }

        public string Description { get; set; }

        public override string ToDisplay()
        {
            if (null != Description)
            {
                return Description;
            }

            return $"{SourceClassKey?.ToString() ?? "*"} ==[{RelationshipTypeKey}]==> {TargetClassKey?.ToString() ?? "*"}";
        }
    }
}
