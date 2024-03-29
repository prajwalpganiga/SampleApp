﻿using System;
using System.Collections.Generic;
using SampleApp.API.Dtos;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SampleApp.API.Data.Interfaces;
using SampleApp.API.Helpers;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using SampleApp.API.Models;
using System.Linq;

namespace SampleApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        public PhotosController(IUnitOfWork unitOfWork, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account
            {
                Cloud = _cloudinaryConfig.Value.CloudName,
                ApiKey = _cloudinaryConfig.Value.ApiKey,
                ApiSecret = _cloudinaryConfig.Value.ApiSecret
            };

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name ="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _unitOfWork.userService.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,[FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var userFromRepo = await _unitOfWork.userService.GetUser(userId);

            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);
            
            if(await _unitOfWork.Complete())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id }, photoToReturn);
            }
            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var user = await _unitOfWork.userService.GetUser(userId);
            if(!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }
            var photoFromRepo = await _unitOfWork.userService.GetPhoto(id);
            if(photoFromRepo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }
            Photo currentMainPhoto = await _unitOfWork.userService.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;
            if(await _unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var user = await _unitOfWork.userService.GetUser(userId);
            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }
            var photoFromRepo = await _unitOfWork.userService.GetPhoto(id);
            if (photoFromRepo.IsMain)
            {
                return BadRequest("Can't delete main photo!");
            }
            if(photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if (result.Result == "ok")
                {
                    _unitOfWork.userService.Delete(photoFromRepo);
                }
                else
                {
                    return BadRequest("Something went wrong!\nPlease try again later");
                }
            }
            if(photoFromRepo.PublicId == null)
            {
                _unitOfWork.userService.Delete(photoFromRepo);
            }
            if(await _unitOfWork.Complete())
            {
                return Ok();
            }
            return BadRequest("Failed to Delete");
        }

    }
}