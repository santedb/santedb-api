using System;
using System.IO;
using System.Net.Mime;

namespace SanteDB.Core.Http
{
    public class PlaintextSerializer : IBodySerializer
    {
        /// <inheritdoc/>
        public string ContentType => "text/plain";

        /// <inheritdoc/>
        public object DeSerialize(Stream requestOrResponseStream, ContentType contentType, Type typeHint)
        {
            using(var sr = new StreamReader(requestOrResponseStream, System.Text.Encoding.GetEncoding(contentType.CharSet)))
            {
                return sr.ReadToEnd();
            }
        }

        /// <inheritdoc/>
        public object GetSerializer(Type typeHint) => null;

        /// <inheritdoc/>
        public void Serialize(Stream requestOrResponseStream, object objectToSerialize, out ContentType contentType)
        {
            using (var sw = new StreamWriter(requestOrResponseStream))
            {
                sw.Write(objectToSerialize);
                contentType = new ContentType($"text/plain; charset={sw.Encoding.WebName}");
            }
        }
    }
}
