using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Services;
using Clean.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService service):ControllerBase
{
    [PermissionAuthorize(PermissionConstants.Users.View)]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] UserFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }
    [PermissionAuthorize(PermissionConstants.Users.View)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await service.GetById(id));
    }
    [PermissionAuthorize(PermissionConstants.Users.Manage)]
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
    {
        return Ok(await service.Update(dto));
    }
    [PermissionAuthorize(PermissionConstants.Users.Manage)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}