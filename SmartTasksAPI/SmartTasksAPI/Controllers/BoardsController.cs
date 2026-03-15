using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Contracts.Boards;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Controllers
{

    [ApiController]
    [Route("api/boards")]
    public class BoardsController(IBoardService boardService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var boards = await boardService.GetAllAsync();
            return Ok(boards);
        }

        [HttpGet("{boardId:guid}")]
        public async Task<IActionResult> GetById(Guid boardId)
        {
            var board = await boardService.GetByIdAsync(boardId);
            return board is null ? NotFound() : Ok(board);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBoardRequest request)
        {
            try
            {
                var board = await boardService.CreateAsync(request.Name, request.Description, request.OwnerId);
                return CreatedAtAction(nameof(GetById), new { boardId = board.Id }, board);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{boardId:guid}")]
        public async Task<IActionResult> Update(Guid boardId, [FromBody] UpdateBoardRequest request)
        {
            var updated = await boardService.UpdateAsync(boardId, request.Name, request.Description);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{boardId:guid}")]
        public async Task<IActionResult> Delete(Guid boardId)
        {
            var deleted = await boardService.DeleteAsync(boardId);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{boardId:guid}/members")]
        public async Task<IActionResult> AddMember(Guid boardId, [FromBody] AddBoardMemberRequest request)
        {
            try
            {
                var added = await boardService.AddMemberAsync(boardId, request.UserId);
                return added ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{boardId:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid boardId, Guid userId)
        {
            try
            {
                var removed = await boardService.RemoveMemberAsync(boardId, userId);
                return removed ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }

}
