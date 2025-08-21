using System.ComponentModel.DataAnnotations;

namespace BookLibwithSub.Service.Models
{
    public class CreateBookRequest
    {
        [Required, StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string AuthorName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Isbn { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Publisher { get; set; }

        [Range(0, 3000)]
        public int PublishedYear { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }
    }

    public class UpdateBookRequest : CreateBookRequest { }

    public class PatchBookRequest
    {
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(255)]
        public string? AuthorName { get; set; }

        [StringLength(20)]
        public string? Isbn { get; set; }

        [StringLength(255)]
        public string? Publisher { get; set; }

        [Range(0, 3000)]
        public int? PublishedYear { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalCopies { get; set; }

        [Range(0, int.MaxValue)]
        public int? AvailableCopies { get; set; }
    }

    public record BookResponse(
        int BookID,
        string Title,
        string AuthorName,
        string Isbn,
        string? Publisher,
        int PublishedYear,
        int TotalCopies,
        int AvailableCopies
    );

    public record BookDetailResponse(
        int BookID,
        string Title,
        string AuthorName,
        string Isbn,
        string? Publisher,
        int PublishedYear,
        int TotalCopies,
        int AvailableCopies,
        byte[]? CoverImage,
        string? CoverImageContentType
    );
}
