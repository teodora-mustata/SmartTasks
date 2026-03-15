using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Contracts.Lists;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ListsController(IListService listService) : ControllerBase
    {
        [HttpGet("boards/{boardId:guid}/lists")]
        public async Task<IActionResult> GetByBoardId(Guid boardId)
        {
            try
            {
                var lists = await listService.GetByBoardIdAsync(boardId);
                return Ok(lists);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("lists/{listId:guid}")]
        public async Task<IActionResult> GetById(Guid listId)
        {
            var list = await listService.GetByIdAsync(listId);
            return list is null ? NotFound() : Ok(list);
        }

        [HttpPost("boards/{boardId:guid}/lists")]
        public async Task<IActionResult> Create(Guid boardId, [FromBody] CreateListRequest request)
        {
            try
            {
                var list = await listService.CreateAsync(boardId, request.Name);
                return CreatedAtAction(nameof(GetById), new { listId = list.Id }, list);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("lists/{listId:guid}")]
        public async Task<IActionResult> Update(Guid listId, [FromBody] UpdateListRequest request)
        {
            var updated = await listService.UpdateAsync(listId, request.Name, request.Position);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("lists/{listId:guid}")]
        public async Task<IActionResult> Delete(Guid listId)
        {
            var deleted = await listService.DeleteAsync(listId);
            return deleted ? NoContent() : NotFound();
        }
    }
}
