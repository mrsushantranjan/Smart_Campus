using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController]
[Route("api/books")]
[Produces("application/json")]
public class BooksApiController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public BooksApiController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category)
    {
        var books = await _libraryService.GetBooksAsync(search, category);
        return Ok(books);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var book = await _libraryService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound(new { message = $"Book with ID '{id}' not found." });
        }
        return Ok(book);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _libraryService.CreateBookAsync(dto);
        return Created($"/api/books/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBookDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _libraryService.UpdateBookAsync(id, dto);
        if (!success)
        {
            return NotFound(new { message = $"Book with ID '{id}' not found." });
        }
        return Ok(new { message = "Book updated successfully." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _libraryService.DeleteBookAsync(id);
        if (!success)
        {
            return NotFound(new { message = $"Book with ID '{id}' not found." });
        }
        return Ok(new { message = "Book deleted successfully." });
    }
}

