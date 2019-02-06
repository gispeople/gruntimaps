namespace GruntiMaps.Domain.Common.Exceptions
{
    public class ValidatorError
    {
        public string Field { get; }
        public string Message { get; }

        public ValidatorError(string message) :
            this("Errors", message)
        {
        }

        public ValidatorError(string field, string message)
        {
            Field = field;
            Message = message;
        }
    }
}
