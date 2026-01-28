using MediatR;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Blog.Commands;

public record DeleteBlogCommand(int Id) : IRequest<bool>;

public class DeleteBlogCommandHandler : IRequestHandler<DeleteBlogCommand, bool>
{
    private readonly IBlogRepository _blogRepository;

    public DeleteBlogCommandHandler(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    public async Task<bool> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return false;

        _blogRepository.Delete(blog);
        await _blogRepository.SaveChangesAsync();
        return true;
    }
}
