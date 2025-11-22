using Clean.Application.Dtos.Reminder;
using Clean.Application.Filters;
using Clean.Application.Services;
using Clean.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/reminder")]
public class ReminderController(IReminderService service):ControllerBase
{
    [PermissionAuthorize(PermissionConstants.Reminders.View)]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] ReminderFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }
    [PermissionAuthorize(PermissionConstants.Reminders.View)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await service.GetById(id));
    }
    [PermissionAuthorize(PermissionConstants.Reminders.Manage)]
    [HttpPost("create")]
    public async Task<IActionResult> Create(ReminderCreateDto dto)
    {
        return Ok(await service.Create(dto));
    }
    [PermissionAuthorize(PermissionConstants.Reminders.Manage)]
    [HttpPut("update")]
    public async Task<IActionResult> Update(ReminderUpdateDto dto)
    {
        return Ok(await service.Update(dto));
    }
    [PermissionAuthorize(PermissionConstants.Reminders.Manage)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}   