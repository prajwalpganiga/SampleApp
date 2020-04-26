using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.API.Dtos
{
    public class PhotoForCreationDto
    {
        public string Url { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; }
        public DateTime DateAddes { get; set; }
        public string PublicId { get; set; }
        public PhotoForCreationDto()
        {
            DateAddes = DateTime.Now;
        }
    }
}
