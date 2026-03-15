using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Contracts.Comments;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Controllers
{

    [ApiController]
    [Route("api")]
    public class CommentsController(ICommentService commentService) : ControllerBase
    {
        [HttpGet("cards/{cardId:guid}/comments")]
        public async Task<IActionResult> GetByCard(Guid cardId)
        {
            try
            {
                var comments = await commentService.GetByCardIdAsync(cardId);
                return Ok(comments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("cards/{cardId:guid}/comments")]
        public async Task<IActionResult> Create(Guid cardId, [FromBody] CreateCommentRequest request)
        {
            try
            {
                var comment = await commentService.CreateAsync(cardId, request.AuthorId, request.Message);
                return Created($"/api/comments/{comment.Id}", comment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("comments/{commentId:guid}")]
        public async Task<IActionResult> Delete(Guid commentId)
        {
            var deleted = await commentService.DeleteAsync(commentId);
            return deleted ? NoContent() : NotFound();
        }
    }
}
