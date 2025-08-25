using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookLibwithSub.Repo.Entities;
using BookLibwithSub.Service.DTOs;
using BookLibwithSub.Service.Interfaces;

namespace BookLibwithSub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _svc;

        public BooksController(IBookService svc) => _svc = svc;

        
        [HttpPost]

        public async Task<IActionResult> Add([FromBody] BookCreateDto dto)
        {
            if (dto == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.AuthorName) ||
                string.IsNullOrWhiteSpace(dto.ISBN))
                return BadRequest(new { message = "Title, AuthorName, and ISBN are required." });

            var entity = new Book
            {
                Title = dto.Title.Trim(),
                AuthorName = dto.AuthorName.Trim(),
                ISBN = dto.ISBN.Trim(),
                Publisher = dto.Publisher?.Trim(),
                PublishedYear = dto.PublishedYear,
                TotalCopies = dto.TotalCopies,
                AvailableCopies = dto.AvailableCopies,
                CoverImageUrl = dto.CoverImageUrl?.Trim()
            };

            try
            {
                var created = await _svc.CreateAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.BookID }, Shape(created));
            }
            catch (System.InvalidOperationException ex) when (ex.Message.Contains("ISBN"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpPut("{id:int}")]

        public async Task<IActionResult> Change([FromRoute] int id, [FromBody] BookUpdateDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = new Book
            {
                Title = (dto.Title ?? "").Trim(),
                AuthorName = (dto.AuthorName ?? "").Trim(),
                ISBN = (dto.ISBN ?? "").Trim(),
                Publisher = dto.Publisher?.Trim(),
                PublishedYear = dto.PublishedYear,
                TotalCopies = dto.TotalCopies,
                AvailableCopies = dto.AvailableCopies,
                CoverImageUrl = dto.CoverImageUrl?.Trim()
            };

            try
            {
                var result = await _svc.UpdateAsync(id, updated);
                if (result == null) return NotFound();
                return Ok(Shape(result));
            }
            catch (System.InvalidOperationException ex) when (ex.Message.Contains("ISBN"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var b = await _svc.GetByIdAsync(id);
            return b == null ? NotFound() : Ok(Shape(b));
        }

        
        
        [HttpGet("sorted")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSorted([FromQuery] string? order = "desc")
        {
            var items = await _svc.GetAllAsync();
            var sorted = (order?.ToLowerInvariant() == "asc")
                ? items.OrderBy(b => b.PublishedYear).ThenBy(b => b.Title)
                : items.OrderByDescending(b => b.PublishedYear).ThenBy(b => b.Title);

            return Ok(sorted.Select(Shape));
        }

        
        [HttpDelete("{id:int}")]

        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        
        private static object Shape(Book b) => new
        {
            id = b.BookID,
            title = b.Title,
            authorName = b.AuthorName,
            isbn = b.ISBN,
            publisher = b.Publisher,
            publishedYear = b.PublishedYear,
            totalCopies = b.TotalCopies,
            availableCopies = b.AvailableCopies,
            coverImageUrl = b.CoverImageUrl
        };
    }
}
