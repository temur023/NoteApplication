using Clean.Application.Dtos.Notification;
using Clean.Application.Filters;
using Clean.Application.Services;
using Clean.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/notes")]
public class NoteController(INoteService service):ControllerBase
{
    [PermissionAuthorize(PermissionConstants.Notes.View)]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] NoteFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }
    [PermissionAuthorize(PermissionConstants.Notes.View)]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await service.GetById(id));
    }
    [PermissionAuthorize(PermissionConstants.Notes.Manage)]
    [HttpPost("create")]
    public async Task<IActionResult> Create( NoteCreateDto note)
    {
        return Ok(await service.Create(note));
    }
    [PermissionAuthorize(PermissionConstants.Notes.Manage)]
    [HttpPut("update")]
    public async Task<IActionResult> Update(NoteUpdateDto note)
    {
        return Ok(await service.Update(note));
    }
    [PermissionAuthorize(PermissionConstants.Notes.Manage)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}