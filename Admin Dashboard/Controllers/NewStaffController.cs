﻿using Admin_Dashboard.Models;
using AutoMapper;
using Core.Entites;
using dmin_Dashboard.Helpers;
using Infrastracture.Interfaces;
using Infrastracture.Specification.RequestSpecifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Hepler;

namespace Admin_Dashboard.Controllers
{
    [Authorize]

    public class NewStaffController(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IConfiguration configuration) : Controller
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<IActionResult> Index()
        {
            var tripsEvent = await _unitOfWork.Reposatory<NewStaff>().GetAllAsync();
            var mappedTripsEvent = _mapper.Map<IReadOnlyList<NewStaffViewModel>>(tripsEvent);
            return View(mappedTripsEvent);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var specs = new RequestSpecifications(id);
            var request = await _unitOfWork.Reposatory<Requests>().GetByIdWithSpecificationsAsync(specs);
            var mappedTripsEvent = _mapper.Map<NewStaffViewModel>(request.NewStaff.FirstOrDefault());
            return View(mappedTripsEvent);


        }

        public async Task<IActionResult> Delete(int id)
        {
            var specs = new RequestSpecifications(id);
            var request = await _unitOfWork.Reposatory<Requests>().GetByIdWithSpecificationsAsync(specs);
            var newstaff = request.NewStaff.FirstOrDefault();
            if(newstaff != null)
            {
                 await DeleteFileAsync(newstaff.CVUrl);


                await DeleteFileAsync(newstaff.PictureUrl);


                _unitOfWork.Reposatory<Requests>().Delete(request);
                await _unitOfWork.Complete();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Accept(int id)
        {
            var specs = new RequestSpecifications(id);
            var request = await _unitOfWork.Reposatory<Requests>().GetByIdWithSpecificationsAsync(specs);
            
            return RedirectToAction(nameof(SendApprovelEmail), request.NewStaff.FirstOrDefault());
        }
        public IActionResult SendApprovelEmail(NewStaffViewModel tripsEvents)
        {
            var emailAddress = new Email()
            {
                To = tripsEvents.Email,
                Subject = "New Staff Approval",
                Body = $"Hi {tripsEvents.Name},\n\nCongratulations! We are thrilled to inform you that you have been accepted to join the jungle jamboree family. We were very impressed with your skills and enthusiasm, and we believe you’ll be a fantastic addition to our team.\n\nWe can’t wait to start this exciting journey with you. Please expect further details about your onboarding process soon.\n\nWelcome aboard!\n\nBest regards,\nJungle jamboree Team"
            };
            _emailService.SendEmail(emailAddress); // No await here

            return RedirectToAction("Index");
        }



        public async Task<HttpResponseMessage> DeleteFileAsync(string imageUrl)
        {
            var requestUrl = _configuration["DeleteFile"];

            var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Add("ImageUrl", imageUrl);

            using HttpClient client = new();
            var response = await client.SendAsync(request); // await here
            return response;
        }


    }
}
