using ColorBook.Data.Models;

namespace ColorBook.Validators;

public interface IBookValidator
{
    (bool IsValid, string? ErrorMessage) Validate(LibraryBookItem libraryBook);
}

public class BookValidator : IBookValidator
{
    public (bool IsValid, string? ErrorMessage) Validate(LibraryBookItem libraryBook)
    {
        if (string.IsNullOrWhiteSpace(libraryBook.Title))
        {
            return (false, "Title is required");
        }

        if (libraryBook.TotalPages <= 0)
        {
            return (false, "TotalPages must be greater than 0");
        }

        return (true, null);
    }
}
