namespace SchoolPortal.Application.Wrappers
{
    public class FailedTransactionResult : ITransactionResult
    {
        public FailedTransactionResult(string message = "failed transaction.")
        {
            IsSuccessful = false;
            Message = message;
        }

        public bool IsSuccessful { get; }

        public string Message { get; }
    }
}
