using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JealPrototype.Application.DTOs.Blog;
using JealPrototype.Application.UseCases.Blog.Commands;
using JealPrototype.Application.UseCases.Blog.Queries;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BlogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<BlogDto>>> GetAll()
    {
        var blogs = await _mediator.Send(new GetAllBlogsQuery());
        return Ok(blogs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogDto>> GetById(int id)
    {
        var blog = await _mediator.Send(new GetBlogByIdQuery(id));
        if (blog == null)
            return NotFound();
        return Ok(blog);
    }

    [HttpGet("dealership/{dealershipId}")]
    public async Task<ActionResult<List<BlogDto>>> GetByDealership(int dealershipId)
    {
        var blogs = await _mediator.Send(new GetBlogsByDealershipQuery(dealershipId));
        return Ok(blogs);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BlogDto>> Create([FromBody] CreateBlogDto dto)
    {
        var blog = await _mediator.Send(new CreateBlogCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = blog.Id }, blog);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<BlogDto>> Update(int id, [FromBody] UpdateBlogDto dto)
    {
        var blog = await _mediator.Send(new UpdateBlogCommand(id, dto));
        if (blog == null)
            return NotFound();
        return Ok(blog);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteBlogCommand(id));
        if (!result)
            return NotFound();
        return NoContent();
    }
}
