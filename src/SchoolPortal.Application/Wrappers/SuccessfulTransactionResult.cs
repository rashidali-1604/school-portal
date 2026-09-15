namespace SchoolPortal.Application.Wrappers
{
    public class SuccessfulTransactionResult : ITransactionResult
    {
        public SuccessfulTransactionResult(string message = "successful transaction.")
        {
            IsSuccessful = true;
            Message = message;
        }

        public bool IsSuccessful { get; }

        public string Message { get; }
    }
}
