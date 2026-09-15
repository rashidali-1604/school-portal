namespace SchoolPortal.Application.Wrappers
{
    public interface ITransactionResult
    {
        bool IsSuccessful { get; }

        string Message { get; }
    }
}
