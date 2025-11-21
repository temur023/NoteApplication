using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[Authorize]
[ApiController]
[Route("api/notes")]
public class NoteController(INoteService service):ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] NoteFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await service.GetById(id));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create( NoteCreateDto note)
    {
        return Ok(await service.Create(note));
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update(NoteUpdateDto note)
    {
        return Ok(await service.Update(note));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}