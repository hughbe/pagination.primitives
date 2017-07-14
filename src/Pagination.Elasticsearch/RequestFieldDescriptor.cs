namespace Pagination.Elasticsearch
{
    public class RequestFieldDescriptor
    {
        public RequestFieldDescriptor(string fieldName)
        {
            RequestFieldName = fieldName;
            FieldName = fieldName;
        }

        public RequestFieldDescriptor(string requestFieldName, string fieldName)
        {
            RequestFieldName = requestFieldName;
            FieldName = fieldName;
        }

        public string RequestFieldName { get; set; }
        public string FieldName { get; set; }

        public static implicit operator RequestFieldDescriptor(string fieldName) => new RequestFieldDescriptor(fieldName);
    }
}
