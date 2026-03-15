using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Contracts.Cards;
using SmartTasksAPI.Services;

namespace SmartTasksAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class CardsController(ICardService cardService) : ControllerBase
    {
        [HttpGet("lists/{listId:guid}/cards")]
        public async Task<IActionResult> GetByList(Guid listId)
        {
            try
            {
                var cards = await cardService.GetByListIdAsync(listId);
                return Ok(cards);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("cards/{cardId:guid}")]
        public async Task<IActionResult> GetById(Guid cardId)
        {
            var card = await cardService.GetByIdAsync(cardId);
            return card is null ? NotFound() : Ok(card);
        }

        [HttpPost("lists/{listId:guid}/cards")]
        public async Task<IActionResult> Create(Guid listId, [FromBody] CreateCardRequest request)
        {
            try
            {
                var card = await cardService.CreateAsync(listId, request.Title, request.Description, request.DueDateUtc);
                return CreatedAtAction(nameof(GetById), new { cardId = card.Id }, card);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("cards/{cardId:guid}")]
        public async Task<IActionResult> Update(Guid cardId, [FromBody] UpdateCardRequest request)
        {
            var updated = await cardService.UpdateAsync(cardId, request.Title, request.Description, request.Position, request.DueDateUtc);
            return updated ? NoContent() : NotFound();
        }

        [HttpPatch("cards/{cardId:guid}/move")]
        public async Task<IActionResult> Move(Guid cardId, [FromBody] MoveCardRequest request)
        {
            try
            {
                var moved = await cardService.MoveAsync(cardId, request.TargetListId, request.TargetPosition);
                return moved ? NoContent() : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("cards/{cardId:guid}")]
        public async Task<IActionResult> Delete(Guid cardId)
        {
            var deleted = await cardService.DeleteAsync(cardId);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("cards/{cardId:guid}/assignments/{userId:guid}")]
        public async Task<IActionResult> Assign(Guid cardId, Guid userId)
        {
            try
            {
                var assigned = await cardService.AssignUserAsync(cardId, userId);
                return assigned ? NoContent() : NotFound();
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

        [HttpDelete("cards/{cardId:guid}/assignments/{userId:guid}")]
        public async Task<IActionResult> Unassign(Guid cardId, Guid userId)
        {
            var unassigned = await cardService.UnassignUserAsync(cardId, userId);
            return unassigned ? NoContent() : NotFound();
        }
    }
}
