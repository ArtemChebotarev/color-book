using ColorBook.Models;

namespace ColorBook.Validators;

public interface IBookValidator
{
    (bool IsValid, string? ErrorMessage) Validate(BookItem book);
}

public class BookValidator : IBookValidator
{
    public (bool IsValid, string? ErrorMessage) Validate(BookItem book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            return (false, "Title is required");
        }

        if (book.TotalPages <= 0)
        {
            return (false, "TotalPages must be greater than 0");
        }

        return (true, null);
    }
}

