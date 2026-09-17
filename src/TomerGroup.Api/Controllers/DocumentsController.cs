using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Manager,Operations,Sales")]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentDto request)
    {
        var userId = GetUserId();
        var result = await _documentService.UploadAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerDocuments(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _documentService.GetCustomerDocumentsAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _documentService.GetByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _documentService.GetContentAsync(id, userId, role);
        if (!result.Success || result.Data == null)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _documentService.DeleteAsync(id, userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}

