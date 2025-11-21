using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/reminder")]
public class ReminderController(IReminderService service):ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll(ReminderFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await service.GetById(id));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(ReminderCreateDto dto)
    {
        return Ok(await service.Create(dto));
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update(ReminderUpdateDto dto)
    {
        return Ok(await service.Update(dto));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}   