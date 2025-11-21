using Clean.Application.Dtos.User;
using Clean.Application.Filters;
using Clean.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace NoteApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService service):Controller
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] UserFilter filter)
    {
        return Ok(await service.GetAll(filter));
    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await service.GetById(id));
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
    {
        return Ok(await service.Update(dto));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        return Ok(await service.Delete(id));
    }
}