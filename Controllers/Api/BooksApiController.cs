using Microsoft.AspNetCore.Mvc;
using SmartCampus.DTOs;
using SmartCampus.Services;

namespace SmartCampus.Controllers.Api;

[ApiController, Route("api/books"), Produces("application/json")]
public class BooksApiController(ILibraryService libraryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? category) =>
        Ok(await libraryService.GetBooksAsync(search, category));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id) =>
        await libraryService.GetBookByIdAsync(id) is { } b ? Ok(b) : NotFound(new { message = $"Book with ID '{id}' not found." });

    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await libraryService.CreateBookAsync(dto);
        return Created($"/api/books/{created.Id}", created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBookDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return await libraryService.UpdateBookAsync(id, dto) ? Ok(new { message = "Book updated successfully." }) : NotFound(new { message = $"Book with ID '{id}' not found." });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id) =>
        await libraryService.DeleteBookAsync(id) ? Ok(new { message = "Book deleted successfully." }) : NotFound(new { message = $"Book with ID '{id}' not found." });
}
