using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.Constants;
using BookLibwithSub.Service.Interfaces;
using BookLibwithSub.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;
        public BooksController(IBookService service) => _service = service;

        private static BookResponse ToResponse(Book b) =>
            new(b.BookID, b.Title, b.AuthorName, b.ISBN, b.Publisher, b.PublishedYear, b.TotalCopies, b.AvailableCopies);

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<BookResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var books = await _service.GetAllAsync();
            return Ok(books.Select(ToResponse));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var book = await _service.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(ToResponse(book));
        }

        [HttpGet("by-title")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BookResponse[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTitle([FromQuery] string title)
        {
            var (items, _) = await _service.SearchAsync(title, 1, 100);
            var matches = items
                .Where(b => string.Equals(b.Title, title, StringComparison.OrdinalIgnoreCase))
                .Select(ToResponse)
                .ToArray();
            return Ok(matches);
        }
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] CreateBookRequest req, IFormFile? cover)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new Book
            {
                Title = req.Title,
                AuthorName = req.AuthorName,
                ISBN = req.Isbn,
                Publisher = req.Publisher,
                PublishedYear = req.PublishedYear,
                TotalCopies = req.TotalCopies,
                AvailableCopies = req.AvailableCopies
            };

            byte[]? coverBytes = null;
            string? contentType = null;

            if (cover is not null && cover.Length > 0)
            {
                using var ms = new MemoryStream();
                await cover.CopyToAsync(ms);
                coverBytes = ms.ToArray();
                contentType = string.IsNullOrWhiteSpace(cover.ContentType) ? "application/octet-stream" : cover.ContentType;
            }

            try
            {
                var created = await _service.CreateAsync(entity, coverBytes, contentType);

                return Created($"/api/books/{created.BookID}", ToResponse(created));
            }
            catch (Exception ex)
            {
                return Problem(title: "Failed to create book", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBookRequest req, IFormFile? cover)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = req.Title;
            existing.AuthorName = req.AuthorName;
            existing.ISBN = req.Isbn;
            existing.Publisher = req.Publisher;
            existing.PublishedYear = req.PublishedYear;
            existing.TotalCopies = req.TotalCopies;
            existing.AvailableCopies = req.AvailableCopies;

            byte[]? coverBytes = null;
            string? contentType = null;

            if (cover is not null && cover.Length > 0)
            {
                using var ms = new MemoryStream();
                await cover.CopyToAsync(ms);
                coverBytes = ms.ToArray();
                contentType = string.IsNullOrWhiteSpace(cover.ContentType) ? "application/octet-stream" : cover.ContentType;
            }

            try
            {
                await _service.UpdateAsync(existing, coverBytes, contentType);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(title: "Failed to update book", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
