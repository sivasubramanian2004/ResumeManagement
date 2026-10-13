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

        [HttpGet("GetApplicants")]
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

        [HttpGet("{id:int}/DownLoadresume")]
        public async Task<IActionResult> DownloadApplicantResume(int id)
        {
            var file = await _applicantService.DownloadApplicantResumeAsync(id);

            //Response.Headers.ContentDisposition = "inline";
            // document view only
           // return File(file.FileStream, file.ContentType);


            //document view and download only

            return File(  file.FileStream, file.ContentType,file.FileName);
                        
        }

        [HttpGet("export/excel{id:int}")]
        public async Task<IActionResult> ExportApplicantsToExcel(int id)
        {
            var excelBytes = await _applicantService.ExportApplicantsToExcelAsync(id);

            var fileName = $"Applicants_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);

        }
        [HttpGet("Download/excel")]
        public async Task<IActionResult> ExportApplicantsToExcel()
        {
            var excelBytes = await _applicantService.ExportApplicantsToExcelAsync();

            var fileName = $"Applicants_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);

        }
        [HttpDelete("DeleteApplication{id:int}")]
        public async Task<IActionResult> DeleteApplicant(int id) {

            await _applicantService.DeleteApplicantAsync(id);

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Application DeletedSuccesffuly",
                Data = null,
                Errors = null,
                StatusCode = 200
            };

            return Ok(response);
        
        }
        [HttpPut("UpdateApplicant{id:int}")]
        public async Task<IActionResult> UpdateApplicant(int id, [FromForm] CreateApplicantDto dto) {

            await _applicantService.UpdateApplicantAsync(id, dto);

            var response = new ApiResponse<object>
            {

                Success = true,
                Message = "Application Updated Succesffuly",
                Data = null,
                Errors = null,
                StatusCode = 200
            };

            return Ok(response);

        }
    }
}
