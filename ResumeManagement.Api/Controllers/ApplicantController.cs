using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using ResumeManagement.Api.DTOs;
using ResumeManagement.Api.Interfaces;
using ResumeManagement.Api.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ResumeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicantController:ControllerBase
    {
        private readonly IApplicantService _applicantService;

        public ApplicantController(IApplicantService applicantService)
        {
            _applicantService = applicantService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplicant([FromForm] CreateApplicantDto dto) 
        {
            await _applicantService.CreateApplicantAsync(dto);

            var response = new ApiResponse<object>
            {

                Success = true,
                Message = "Application Submitted Succesffuly",
                Data = null,
                Errors = null,
                StatusCode = 201
            };

            return StatusCode(201, response);
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicant() {

            var result = await _applicantService.GetAllApplicantsAsync();

            var response = new ApiResponse<List<ApplicantResponseDto>>
            {

                Success = true,
                Message = "Application Retrieved Succesffuly",
                Data = result,
                Errors = null,
                StatusCode = 200
            };

            return Ok(response);

        }

        [HttpGet("{id:int}/resume")]
        public async Task<IActionResult> DownloadApplicantResume(int id)
        {
            var file = await _applicantService.DownloadApplicantResumeAsync(id);

            return File(
                file.FileStream,
                file.ContentType,
                file.FileName);
        }
    }
}
